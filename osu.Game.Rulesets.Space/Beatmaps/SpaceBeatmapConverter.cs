using System;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Space.UI;
using osuTK;
using System.Linq;

namespace osu.Game.Rulesets.Space.Beatmaps
{
    public class SpaceBeatmapConverter : BeatmapConverter<SpaceHitObject>
    {
        public SpaceBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        protected override Beatmap<SpaceHitObject> CreateBeatmap() => new SpaceBeatmap();

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is SpaceHitObject || (h is IHasXPosition && h is IHasYPosition));

        private Vector2? prevOsuPos;
        private (int col, int row) prevGridPos;
        private (int col, int row) ppGridPos; // prev-prev grid pos
        private double prevTime;
        private int currentIndex;

        protected override IEnumerable<SpaceHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var osuPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            double time = original.StartTime;

            int targetCol, targetRow;
            if (prevOsuPos == null || (time - prevTime) > 1000)
            {
                var initPos = getGridPosition(original);
                targetCol = (int)initPos.col;
                targetRow = (int)initPos.row;
                ppGridPos = (-1, -1);
                prevGridPos = (-1, -1);
            }
            else
            {
                var diff = osuPos - prevOsuPos.Value;
                float dist = diff.Length;
                float osuAngle = MathF.Atan2(diff.Y, diff.X);

                int step = 1;
                if (dist < 10) step = 0;
                else if (dist > 180) step = 2;

                var candidates = new List<((int col, int row) pos, double score)>();

                double dt = time - prevTime;
                bool isStream = dt < 150;
                bool isHighBpm = dt < 90;

                if (isHighBpm && step > 1) step = 1;

                for (int c = 0; c <= 2; c++)
                {
                    for (int r = 0; r <= 2; r++)
                    {
                        int dc = c - prevGridPos.col;
                        int dr = r - prevGridPos.row;
                        int gridDist = Math.Max(Math.Abs(dc), Math.Abs(dr));

                        double score = 0;

                        if (step == 0)
                        {
                            if (gridDist == 0) score += 100;
                            else score -= 100;
                        }
                        else
                        {
                            if (gridDist == 0)
                            {
                                score -= 50;
                                if (isStream) score -= 100;
                            }
                            else if (step == 2)
                            {
                                if (gridDist >= 2) score += 20;
                                else score -= 10;
                            }
                            else
                            {
                                if (gridDist == 1) score += 20;
                                else score -= 5;
                            }
                        }

                        if (gridDist > 0)
                        {
                            float gridAngle = MathF.Atan2(dr, dc);
                            float dAngle = osuAngle - gridAngle;

                            if (float.IsNaN(dAngle) || float.IsInfinity(dAngle))
                                dAngle = 0f;

                            while (dAngle > MathF.PI) dAngle -= 2 * MathF.PI;
                            while (dAngle < -MathF.PI) dAngle += 2 * MathF.PI;

                            dAngle = Math.Abs(dAngle);

                            score += (1.0 - (dAngle / MathF.PI)) * 30;

                            if (isStream && prevGridPos != ppGridPos && ppGridPos.col != -1 && step < 2)
                            {
                                int prevDc = prevGridPos.col - ppGridPos.col;
                                int prevDr = prevGridPos.row - ppGridPos.row;
                                float prevAngle = MathF.Atan2(prevDr, prevDc);

                                float flowAngle = prevAngle - gridAngle;

                                if (float.IsNaN(flowAngle) || float.IsInfinity(flowAngle))
                                    flowAngle = 0f;

                                while (flowAngle > MathF.PI) flowAngle -= 2 * MathF.PI;
                                while (flowAngle < -MathF.PI) flowAngle += 2 * MathF.PI;

                                flowAngle = Math.Abs(flowAngle);

                                if (flowAngle > MathF.PI / 2 + 0.1f) score -= 20;
                                else score += 10;
                            }
                        }

                        if (step > 0 && c == ppGridPos.col && r == ppGridPos.row && ppGridPos.col != -1)
                        {
                            score -= 200;
                        }

                        double noise = (Math.Sin(time * 0.1 + c * 13 + r * 7) + 1) * 2;
                        score += noise;

                        candidates.Add(((c, r), score));
                    }
                }

                var best = candidates.OrderByDescending(p => p.score).First();
                targetCol = best.pos.col;
                targetRow = best.pos.row;
            }

            ppGridPos = prevGridPos;
            prevOsuPos = osuPos;
            prevGridPos = (targetCol, targetRow);
            prevTime = time;

            yield return new Note
            {
                Index = currentIndex++,
                Samples = original.Samples,
                StartTime = original.StartTime,
                X = (targetCol + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                Y = (targetRow + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                oX = targetCol,
                oY = targetRow
            };
        }

        private static (float col, float row) getGridPosition(HitObject hitObject)
        {
            float x = ((IHasXPosition)hitObject).X;
            float y = ((IHasYPosition)hitObject).Y;
            return (
                (int)Math.Clamp(x / (SpacePlayfield.BASE_SIZE / 3f), 0, 2),
                (int)Math.Clamp(y / (SpacePlayfield.BASE_SIZE / 3f), 0, 2)
                );
        }
    }
}

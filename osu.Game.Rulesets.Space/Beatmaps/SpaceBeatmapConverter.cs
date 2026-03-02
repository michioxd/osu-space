
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
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Space.Beatmaps
{
    public class SpaceBeatmapConverter : BeatmapConverter<SpaceHitObject>
    {
        public SpaceBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        protected override Beatmap<SpaceHitObject> CreateBeatmap() => new SpaceBeatmap();

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasXPosition && h is IHasYPosition);

        private Vector2? prevPos;
        private (int c, int r) prevGridPos;
        private (int c, int r) prevPrevGridPos;
        private double prevTime;
        private int currentIndex;

        protected override IEnumerable<SpaceHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            bool isSpace = beatmap.BeatmapInfo.Ruleset.ShortName == "osuspaceruleset";

            // If it's already a Space map, use the existing logic (absolute positioning)
            if (isSpace)
            {
                var (col, row) = getGridPosition(original, true);
                yield return new SpaceHitObject
                {
                    Index = currentIndex++, // Keep consistent indexing
                    Samples = original.Samples,
                    StartTime = original.StartTime,
                    X = (col + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                    Y = (row + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                    oX = col,
                    oY = row
                };
                yield break;
            }

            // For conversion from osu! or other rulesets
            var osuPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            var time = original.StartTime;

            int newCol, newRow;

            // Reset context if too much time passed (break) or first object
            if (prevPos == null || (time - prevTime) > 1000)
            {
                // Use absolute mapping as anchor
                var abs = getGridPosition(original, false);
                newCol = (int)abs.col;
                newRow = (int)abs.row;
                prevPrevGridPos = (newCol, newRow);
            }
            else
            {
                // Relative mapping based on flow
                var diff = osuPos - prevPos.Value;
                float dist = diff.Length;
                float osuAngle = MathF.Atan2(diff.Y, diff.X); // Radians

                // Target distance in grid units (Chebyshev)
                // 0 = stack, 1 = adjacent/diagonal, 2 = jump
                int targetStep = 1;
                // osu! stacks are very precise, usually < 2-3 units.
                // Streams are usually ~15-30 units.
                if (dist < 10) targetStep = 0;
                else if (dist > 180) targetStep = 2;

                var candidates = new List<((int c, int r) pos, double score)>();

                // Flow heuristic: Check if this is a high-density stream
                double timeDelta = time - prevTime;
                bool isStream = timeDelta < 150;
                bool isHighBpmStream = timeDelta < 90; // > 166 BPM 1/4

                if (isHighBpmStream && targetStep > 1) targetStep = 1; // Cap jumps at high BPM

                for (int c = 0; c <= 2; c++)
                {
                    for (int r = 0; r <= 2; r++)
                    {
                        int dc = c - prevGridPos.c;
                        int dr = r - prevGridPos.r;
                        int gridDist = Math.Max(Math.Abs(dc), Math.Abs(dr));

                        // 1. Distance Score
                        double score = 0;
                        if (targetStep == 0)
                        {
                            if (gridDist == 0) score += 100;
                            else score -= 100;
                        }
                        else
                        {
                            if (gridDist == 0)
                            {
                                score -= 50; // Avoid stacks if not intended
                                if (isStream) score -= 100; // Heavily penalize stacks during streams (force movement)
                            }
                            else if (targetStep == 2)
                            {
                                if (gridDist >= 2) score += 20;
                                else score -= 10;
                            }
                            else // targetStep == 1
                            {
                                if (gridDist == 1) score += 20;
                                else score -= 5;
                            }
                        }

                        // 2. Angle Score (only if moving)
                        if (gridDist > 0)
                        {
                            // Vector from prevGrid to candidate
                            // Y is down in osu! and grid, so (dr) is correct for Y component
                            // However, grid is small, so we treat it as vector
                            float gridAngle = MathF.Atan2(dr, dc);

                            // Difference between osu! angle and grid angle
                            float angleDiff = Math.Abs(osuAngle - gridAngle);
                            while (angleDiff > MathF.PI) angleDiff -= 2 * MathF.PI;
                            while (angleDiff < -MathF.PI) angleDiff += 2 * MathF.PI;
                            angleDiff = Math.Abs(angleDiff);

                            // Closer angle is better.
                            // Score ranges from roughly +15 (match) to -15 (opposite)
                            score += (1.0 - (angleDiff / MathF.PI)) * 30;

                            // 2.1 Flow Angle Bonus (High BPM)
                            // Only apply strict flow to streams (targetStep < 2).
                            // For jumps (targetStep >= 2), allow more angular variance (sharp turns) if the map calls for it.
                            if (isStream && prevGridPos != prevPrevGridPos && targetStep < 2)
                            {
                                int prevDc = prevGridPos.c - prevPrevGridPos.c;
                                int prevDr = prevGridPos.r - prevPrevGridPos.r;
                                float prevMoveAngle = MathF.Atan2(prevDr, prevDc);

                                float flowAngleDiff = Math.Abs(prevMoveAngle - gridAngle);
                                while (flowAngleDiff > MathF.PI) flowAngleDiff -= 2 * MathF.PI;
                                while (flowAngleDiff < -MathF.PI) flowAngleDiff += 2 * MathF.PI;
                                flowAngleDiff = Math.Abs(flowAngleDiff);

                                // If flow angle > 90 deg (sharp turn), penalize
                                if (flowAngleDiff > MathF.PI / 2 + 0.1f)
                                {
                                    score -= 20;
                                }
                                else
                                {
                                    score += 10; // Bonus for smooth flow
                                }
                            }
                        }

                        // 3. Backtracking Penalty (Anti-Spam)
                        // If we go back to exactly where we were 2 notes ago, and it's not a stack pattern
                        if (targetStep > 0 && c == prevPrevGridPos.c && r == prevPrevGridPos.r)
                        {
                            score -= 200; // Heavy penalty to prevent A-B-A trills/spams
                        }

                        // 4. Variety/Randomness tie-breaker (deterministically based on time/index)
                        // This helps avoid always picking the same diagonal for the same angle
                        double noise = (Math.Sin(time * 0.1 + c * 13 + r * 7) + 1) * 2;
                        score += noise;

                        candidates.Add(((c, r), score));
                    }
                }

                // Select best candidate
                var best = candidates.OrderByDescending(x => x.score).First();
                newCol = best.pos.c;
                newRow = best.pos.r;
            }

            prevPrevGridPos = prevGridPos;
            prevPos = osuPos;
            prevGridPos = (newCol, newRow);
            prevTime = time;

            yield return new SpaceHitObject
            {
                Index = currentIndex++,
                Samples = original.Samples,
                StartTime = original.StartTime,
                X = (newCol + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                Y = (newRow + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                oX = newCol,
                oY = newRow
            };
        }

        private static (float col, float row) getGridPosition(HitObject hitObject, bool isOSpaceBeatmap = false)
        {
            if (isOSpaceBeatmap)
            {
                return (((IHasXPosition)hitObject).X / 1e4f, ((IHasYPosition)hitObject).Y / 1e4f);
            }
            float x = ((IHasXPosition)hitObject).X;
            float y = ((IHasYPosition)hitObject).Y;
            return (
                (int)Math.Clamp(x / (SpacePlayfield.BASE_SIZE / 3f), 0, 2),
                (int)Math.Clamp(y / (SpacePlayfield.BASE_SIZE / 3f), 0, 2)
                );
        }
    }
}

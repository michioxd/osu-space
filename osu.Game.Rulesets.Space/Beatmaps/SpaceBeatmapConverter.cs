
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

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is IHasXPosition && h is IHasYPosition);

        protected override IEnumerable<SpaceHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var (col, row) = getGridPosition(original);

            int index = -1;
            if (beatmap.HitObjects is IList<HitObject> list)
            {
                index = list.IndexOf(original);
            }
            else
            {
                for (int i = 0; i < beatmap.HitObjects.Count; i++)
                {
                    if (beatmap.HitObjects[i] == original)
                    {
                        index = i;
                        break;
                    }
                }
            }

            if (beatmap.BeatmapInfo.Ruleset.ShortName != "osuspaceruleset")
            {
                int streak = 0;
                const float epsilon = 0.2f;
                if (index > 0)
                {
                    double lastTime = original.StartTime;
                    for (int i = index - 1; i >= 0; i--)
                    {
                        var prevObj = beatmap.HitObjects[i];
                        var prevPos = getGridPosition(prevObj);
                        if (Math.Abs(prevPos.col - col) > epsilon || Math.Abs(prevPos.row - row) > epsilon)
                            break;

                        if (lastTime - prevObj.StartTime > 1000)
                            break;

                        streak++;
                        lastTime = prevObj.StartTime;
                    }
                }

                if (streak > 0)
                {
                    var ringPath = new List<(float c, float r)>
                    {
                        (0.5f, 2.5f), (1.5f, 2.5f), (2.5f, 2.5f),
                        (2.5f, 1.5f),
                        (2.5f, 0.5f), (1.5f, 0.5f), (0.5f, 0.5f),
                        (0.5f, 1.5f)
                    };

                    int startIndex = closestIndex(ringPath, col, row, epsilon);

                    if (startIndex != -1)
                    {
                        var newPos = ringPath[(startIndex + streak) % ringPath.Count];
                        col = newPos.c;
                        row = newPos.r;
                    }
                    else
                    {
                        var newPos = ringPath[(streak - 1) % ringPath.Count];
                        col = newPos.c;
                        row = newPos.r;
                    }
                }
            }

            yield return new SpaceHitObject
            {
                Index = index,
                Samples = original.Samples,
                StartTime = original.StartTime,
                X = (col + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                Y = (row + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                oX = col,
                oY = row
            };
        }

        private static int closestIndex(List<(float c, float r)> path, float col, float row, float epsilon)
        {
            for (int i = 0; i < path.Count; i++)
            {
                if (Math.Abs(path[i].c - col) < epsilon && Math.Abs(path[i].r - row) < epsilon)
                    return i;
            }
            return -1;
        }

        private static (float col, float row) getGridPosition(HitObject hitObject)
        {
            float x = ((IHasXPosition)hitObject).X;
            float y = ((IHasYPosition)hitObject).Y;
            return (
                Math.Clamp(x / (SpacePlayfield.BASE_SIZE / 3f), 0, 2),
                Math.Clamp(y / (SpacePlayfield.BASE_SIZE / 3f), 0, 2)
                );
        }
    }
}

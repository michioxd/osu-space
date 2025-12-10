
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

            int streak = 0;
            if (index > 0)
            {
                for (int i = index - 1; i >= 0; i--)
                {
                    var prevObj = beatmap.HitObjects[i];
                    if (original.StartTime - prevObj.StartTime > 1000) break;

                    var prevPos = getGridPosition(prevObj);
                    if (prevPos.col == col && prevPos.row == row)
                    {
                        streak++;
                    }
                    else
                    {
                        break;
                    }
                }
            }

            if (streak > 0)
            {
                var ringPath = new List<(int c, int r)>
                {
                    (0, 2), (1, 2), (2, 2),
                    (2, 1),
                    (2, 0), (1, 0), (0, 0),
                    (0, 1)
                };

                int startIndex = ringPath.IndexOf((col, row));

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

            yield return new SpaceHitObject
            {
                Index = index,
                Samples = original.Samples,
                StartTime = original.StartTime,
                X = (col + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                Y = (row + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
            };
        }

        private (int col, int row) getGridPosition(HitObject hitObject)
        {
            float x = ((IHasXPosition)hitObject).X;
            float y = ((IHasYPosition)hitObject).Y;
            int col = Math.Clamp((int)(x / (SpacePlayfield.BASE_SIZE / 3f)), 0, 2);
            int row = Math.Clamp((int)(y / (SpacePlayfield.BASE_SIZE / 3f)), 0, 2);
            return (col, row);
        }
    }
}

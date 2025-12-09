
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
            float x = ((IHasXPosition)original).X;
            float y = ((IHasYPosition)original).Y;

            int col = Math.Clamp((int)(x / (SpacePlayfield.BASE_SIZE.X / 3f)), 0, 2);
            int row = Math.Clamp((int)(y / (SpacePlayfield.BASE_SIZE.Y / 3f)), 0, 2);

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

            yield return new SpaceHitObject
            {
                Index = index,
                Samples = original.Samples,
                StartTime = original.StartTime,
                X = (col + 0.5f) * (SpacePlayfield.BASE_SIZE.X / 3f),
                Y = (row + 0.5f) * (SpacePlayfield.BASE_SIZE.Y / 3f),
            };
        }
    }
}

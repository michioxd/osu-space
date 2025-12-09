
using System;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Space.UI;
using osuTK;

namespace osu.Game.Rulesets.Space.Beatmaps
{
    public class SpaceBeatmapConverter : BeatmapConverter<SpaceHitObject>
    {
        public SpaceBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        // todo: Check for conversion types that should be supported (ie. Beatmap.HitObjects.Any(h => h is IHasXPosition))
        // https://github.com/ppy/osu/tree/master/osu.Game/Rulesets/Objects/Types
        public override bool CanConvert() => true;

        protected override IEnumerable<SpaceHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            float x = ((IHasXPosition)original).X;
            float y = ((IHasYPosition)original).Y;

            int col = Math.Clamp((int)(x / (SpacePlayfield.BASE_SIZE.X / 3f)), 0, 2);
            int row = Math.Clamp((int)(y / (SpacePlayfield.BASE_SIZE.Y / 3f)), 0, 2);

            yield return new SpaceHitObject
            {
                Samples = original.Samples,
                StartTime = original.StartTime,
                X = (col + 0.5f) * (SpacePlayfield.BASE_SIZE.X / 3f),
                Y = (row + 0.5f) * (SpacePlayfield.BASE_SIZE.Y / 3f),
            };
        }
    }
}

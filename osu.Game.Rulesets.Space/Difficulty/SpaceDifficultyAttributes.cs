using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Space.Difficulty
{
    public class SpaceDifficultyAttributes : DifficultyAttributes
    {
        public double AimDifficulty;
        public double ReadingDifficulty;

        public SpaceDifficultyAttributes(Mod[] mods, double starRating)
            : base(mods, starRating) { }

        public override IEnumerable<(int attributeId, object value)> ToDatabaseAttributes()
        {
            foreach (var v in base.ToDatabaseAttributes())
                yield return v;

            yield return (11, AimDifficulty);
            yield return (13, ReadingDifficulty);
        }

        public override void FromDatabaseAttributes(
            IReadOnlyDictionary<int, double> values,
            IBeatmapOnlineInfo onlineInfo
        )
        {
            base.FromDatabaseAttributes(values, onlineInfo);

            AimDifficulty = values.GetValueOrDefault(11);
            ReadingDifficulty = values.GetValueOrDefault(13);
        }
    }
}

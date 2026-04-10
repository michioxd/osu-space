using System;
using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Space.Difficulty;
using osu.Game.Rulesets.Space.Mods;
using osu.Game.Scoring;

namespace osu.Game.Rulesets.Space
{
    public class SpacePerformanceCalculator : PerformanceCalculator
    {
        public SpacePerformanceCalculator()
            : base(new SpaceRuleset()) { }

        protected override PerformanceAttributes CreatePerformanceAttributes(
            ScoreInfo score,
            DifficultyAttributes attributes
        )
        {
            var spaceAttributes = (SpaceDifficultyAttributes)attributes;

            double multiplier = 1.12;

            if (score.Mods.Any(m => m is SpaceModNoFail))
                multiplier *= Math.Max(
                    0.90,
                    1.0 - 0.02 * score.Statistics.GetValueOrDefault(HitResult.Miss)
                );

            if (score.Mods.Any(m => m is SpaceModAutoplay))
                multiplier = 1.0;

            double aimValue = computeAimValue(score, spaceAttributes);
            double readingValue = computeReadingValue(score, spaceAttributes);
            double accuracyValue = computeAccuracyValue(score, spaceAttributes);

            double totalValue =
                Math.Pow(
                    Math.Pow(aimValue, 1.1)
                        + Math.Pow(readingValue, 1.1)
                        + Math.Pow(accuracyValue, 1.1),
                    1.0 / 1.1
                ) * multiplier;

            return new PerformanceAttributes { Total = totalValue };
        }

        private double computeAimValue(ScoreInfo score, SpaceDifficultyAttributes attributes)
        {
            double aimValue = Math.Pow(attributes.AimDifficulty, 2.5) * 1.5;

            if (attributes.MaxCombo > 0)
                aimValue *= Math.Min(
                    Math.Pow(score.MaxCombo / (double)attributes.MaxCombo, 0.8),
                    1.0
                );

            if (score.Statistics.TryGetValue(HitResult.Miss, out int missCount) && missCount > 0)
                aimValue *= 0.95 * Math.Pow(0.90, missCount - 1);

            return aimValue;
        }

        private double computeReadingValue(ScoreInfo score, SpaceDifficultyAttributes attributes)
        {
            double readingValue = Math.Pow(attributes.ReadingDifficulty, 2.5) * 1.5;

            if (attributes.MaxCombo > 0)
                readingValue *= Math.Min(
                    Math.Pow(score.MaxCombo / (double)attributes.MaxCombo, 0.8),
                    1.0
                );

            if (score.Statistics.TryGetValue(HitResult.Miss, out int missCount) && missCount > 0)
                readingValue *= 0.95 * Math.Pow(0.90, missCount - 1);

            return readingValue;
        }

        private double computeAccuracyValue(ScoreInfo score, SpaceDifficultyAttributes attributes)
        {
            return Math.Pow(attributes.StarRating, 2.0) * Math.Pow(score.Accuracy, 5.0) * 2.5;
        }
    }
}

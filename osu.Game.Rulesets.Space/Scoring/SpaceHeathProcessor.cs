using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Space.Scoring
{
    public partial class SpaceHealthProcessor : HealthProcessor
    {
        public SpaceHealthProcessor() { }

        protected override void Reset(bool storeResults)
        {
            base.Reset(storeResults);
            Health.Value = 1.0;
        }

        protected override double GetHealthIncreaseFor(JudgementResult result)
        {
            if (result.IsHit)
                return 0.1;
            else if (result.Type == HitResult.Miss)
                return -0.2;

            return 0;
        }
    }
}

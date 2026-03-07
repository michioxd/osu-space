using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;

namespace osu.Game.Rulesets.Space.Judgements
{
    public class SpaceJudgement : Judgement
    {
        protected override double HealthIncreaseFor(HitResult result)
        {
            switch (result)
            {
                case HitResult.Miss:
                    return -0.2;
                case HitResult.Perfect:
                    return DEFAULT_MAX_HEALTH_INCREASE;

                default:
                    return base.HealthIncreaseFor(result);
            }
        }
    }
}

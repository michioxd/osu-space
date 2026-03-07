using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Space.Judgements;

namespace osu.Game.Rulesets.Space.Objects
{
    public class Note : SpaceHitObject
    {
        public override Judgement CreateJudgement() => new SpaceJudgement();
    }
}

using osu.Framework.Graphics;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Space.Objects;
using osuTK;

namespace osu.Game.Rulesets.Space.UI
{
    public partial class DrawableSpaceJudgement : DrawableJudgement
    {
        private Vector2? screenSpacePosition;

        public override void Apply(JudgementResult result, DrawableHitObject? judgedObject)
        {
            base.Apply(result, judgedObject);

            if (judgedObject?.HitObject is SpaceHitObject)
            {
                screenSpacePosition = judgedObject.ToScreenSpace(judgedObject.OriginPosition);
            }
            else
            {
                screenSpacePosition = null;
            }
        }

        protected override void PrepareForUse()
        {
            base.PrepareForUse();

            if (screenSpacePosition != null && Parent != null)
                Position = Parent!.ToLocalSpace(screenSpacePosition.Value);
        }

        protected override Drawable CreateDefaultJudgement(HitResult result) =>
            new SpaceJudgementPiece(result);
    }
}

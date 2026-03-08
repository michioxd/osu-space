using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Sprites;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Space.UI
{
    public partial class SpaceJudgementPiece : CompositeDrawable, IAnimatableJudgement
    {
        protected readonly HitResult Result;
        protected SpriteText JudgementText { get; private set; }

        public SpaceJudgementPiece(HitResult result)
        {
            Result = result;
            AutoSizeAxes = Axes.Both;
            Origin = Anchor.Centre;
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            if (Result.IsMiss())
            {
                InternalChild = new SpriteIcon
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Icon = FontAwesome.Solid.Times,
                    Size = new Vector2(40),
                    Colour = Colour4.FromHex("#f5655b"),
                };
            }
        }

        public virtual void PlayAnimation()
        {
            if (Result.IsMiss())
            {
                this.ScaleTo(1.2f);
                this.ScaleTo(1, 100, Easing.In);

                this.MoveTo(Vector2.Zero);

                this.RotateTo(0);
                this.RotateTo(5f, 500, Easing.InQuint);
                this.ScaleTo(0.8f, 500, Easing.InQuint);
                this.FadeOutFromOne(500);
            }
        }

        public Drawable? GetAboveHitObjectsProxiedContent() => null;
    }
}

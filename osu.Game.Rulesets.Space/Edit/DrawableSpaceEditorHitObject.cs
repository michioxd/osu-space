using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Space.Objects;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class DrawableSpaceEditorHitObject : DrawableHitObject<SpaceHitObject>
    {
        private Container content;
        private Container borderContainer;
        private Box innerBox;
        private Container approachSquare;
        private OsuSpriteText indexText;

        [Resolved(CanBeNull = true)]
        private osu.Game.Screens.Edit.EditorBeatmap editorBeatmap { get; set; }

        public DrawableSpaceEditorHitObject(SpaceHitObject hitObject)
            : base(hitObject)
        {
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;

            Size = new Vector2(0.8f / 3f);

            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(6),
                Children =
                [
                    borderContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        BorderColour = Color4.LightPink,
                        BorderThickness = 2,
                        Alpha = 0.8f,
                        Children = new Drawable[]
                        {
                            innerBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.3f,
                                Colour = Color4.Pink
                            },
                            indexText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.Numeric.With(size: 17, family: "Torus Alternate"),
                                Colour = Color4.White
                            }
                        }
                    },
                    approachSquare = new Container
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        BorderThickness = 3,
                        Alpha = 0,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                ]
            });
        }

        private int? lastCellIndex;
        private int? lastIndex;

        [BackgroundDependencyLoader]
        private void load()
        {
        }

        protected override void Update()
        {
            base.Update();

            X = HitObject.X / UI.SpacePlayfield.BASE_SIZE;
            Y = HitObject.Y / UI.SpacePlayfield.BASE_SIZE;

            if (lastCellIndex != HitObject.CellIndex || lastIndex != HitObject.Index)
            {
                lastCellIndex = HitObject.CellIndex;
                lastIndex = HitObject.Index;

                bool isPink = HitObject.CellIndex % 2 == 0;
                borderContainer.BorderColour = isPink ? Color4.LightPink : Color4.LightCyan;
                innerBox.Colour = isPink ? Color4.Pink : Color4.Cyan;
                approachSquare.BorderColour = isPink ? Color4.LightPink : Color4.LightCyan;

                if (indexText != null)
                {
                    indexText.Text = HitObject.Index.ToString();
                    indexText.Colour = isPink ? Color4.LightPink : Color4.LightCyan;
                }
            }

            double timeRemaining = HitObject.StartTime - Time.Current;

            float targetAlpha;
            float targetApproachScale;

            double zoom = editorBeatmap?.TimelineZoom > 0 ? editorBeatmap.TimelineZoom : 1.0;
            double tPreempt = 1500 / zoom;
            double tFadeInSt = 500 / zoom;
            double tFadeOut = 200 / zoom;

            if (timeRemaining > tPreempt)
            {
                targetAlpha = 0f;
                targetApproachScale = 2.5f;
            }
            else if (timeRemaining > tFadeInSt)
            {
                targetAlpha = 1f - (float)((timeRemaining - tFadeInSt) / (tPreempt - tFadeInSt));
                targetApproachScale = 1f + (1.5f * (float)(timeRemaining / tPreempt));
            }
            else if (timeRemaining > 0)
            {
                targetAlpha = 1f;
                targetApproachScale = 1f + (1.5f * (float)(timeRemaining / tPreempt));
            }
            else if (timeRemaining > -tFadeOut)
            {
                targetAlpha = 1f - (float)(-timeRemaining / tFadeOut);
                targetApproachScale = 1f + (float)(-timeRemaining / tFadeOut) * 0.15f;
            }
            else
            {
                targetAlpha = 0f;
                targetApproachScale = 1.15f;
            }

            content.Alpha = targetAlpha;
            approachSquare.Alpha = targetAlpha;
            approachSquare.Scale = new Vector2(targetApproachScale);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
                ApplyMaxResult();
        }

        private const double lifetime_end_buffer = 1500;

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                case ArmedState.Miss:
                    LifetimeEnd = HitObject.StartTime + lifetime_end_buffer;
                    break;

                default:
                    LifetimeEnd = double.MaxValue;
                    break;
            }
        }
    }
}

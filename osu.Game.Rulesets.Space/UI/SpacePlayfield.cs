#nullable enable
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;
using osu.Game.Rulesets.Space.UI.Cursor;
using osuTK;
using osu.Game.Rulesets.Space.Configuration;
using osu.Framework.Bindables;

namespace osu.Game.Rulesets.Space.UI
{
    [Cached]
    public partial class SpacePlayfield : Playfield
    {
        private readonly PlayfieldBorder playfieldBorder;
        private readonly SpaceGrid grid;
        public readonly Container contentContainer;
        private readonly Bindable<float> parallaxStrength = new();
        private readonly Bindable<bool> enableGrid = new();
        private readonly Bindable<float> scalePlayfield = new();
        public static readonly float BASE_SIZE = 512;
        protected override GameplayCursorContainer CreateCursor() => new SpaceCursorContainer
        {
            RelativeSizeAxes = Axes.Both
        };

        public SpacePlayfield()
        {
            Origin = Anchor.Centre;
            InternalChildren =
            [
                contentContainer = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Masking = false,
                    Size = new Vector2(
                        0.6f // initial
                    ),
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 1,
                    Children =
                    [
                        playfieldBorder = new PlayfieldBorder
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        grid = new SpaceGrid(),
                        HitObjectContainer
                    ]
                }
            ];
        }

        protected override void Update()
        {
            base.Update();

            if (Cursor?.ActiveCursor != null)
            {
                Vector2 cursorPosition = ToLocalSpace(Cursor.ActiveCursor.ScreenSpaceDrawQuad.Centre);
                Vector2 center = DrawSize / 2;
                Vector2 offset = (cursorPosition - center) * (0.025f * parallaxStrength.Value);

                contentContainer.Position = -offset;
                contentContainer.Size = new Vector2(scalePlayfield.Value);
            }
        }

        [BackgroundDependencyLoader]
        private void load(SpaceRulesetConfigManager? config)
        {
            config?.BindWith(SpaceRulesetSetting.PlayfieldBorderStyle, playfieldBorder.PlayfieldBorderStyle);
            config?.BindWith(SpaceRulesetSetting.Parallax, parallaxStrength);
            config?.BindWith(SpaceRulesetSetting.ScalePlayfield, scalePlayfield);
            config?.BindWith(SpaceRulesetSetting.EnableGrid, enableGrid);

            enableGrid.BindValueChanged(e => grid.FadeTo(e.NewValue ? 1 : 0, 200), true);
        }

        public new Vector2 GamefieldToScreenSpace(Vector2 point)
        {
            Vector2 normalized = new Vector2(point.X / BASE_SIZE, point.Y / BASE_SIZE);
            return HitObjectContainer.ToScreenSpace(normalized * HitObjectContainer.DrawSize);
        }

        public new Vector2 ScreenSpaceToGamefield(Vector2 screenSpacePosition)
        {
            Vector2 local = HitObjectContainer.ToLocalSpace(screenSpacePosition);
            Vector2 normalized = new Vector2(local.X / HitObjectContainer.DrawSize.X, local.Y / HitObjectContainer.DrawSize.Y);
            return normalized * BASE_SIZE;
        }
    }

    public partial class SpaceGrid : CompositeDrawable
    {
        public SpaceGrid()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
            Masking = true;

            AddInternal(new DashedLine(Axes.Y) { RelativePositionAxes = Axes.Both, X = 1f / 3f });
            AddInternal(new DashedLine(Axes.Y) { RelativePositionAxes = Axes.Both, X = 2f / 3f });
            AddInternal(new DashedLine(Axes.X) { RelativePositionAxes = Axes.Both, Y = 1f / 3f });
            AddInternal(new DashedLine(Axes.X) { RelativePositionAxes = Axes.Both, Y = 2f / 3f });
        }
    }

    public partial class DashedLine : CompositeDrawable
    {
        public DashedLine(Axes axis)
        {
            RelativeSizeAxes = axis;
            float thickness = 2f;
            float dashLength = 10f;
            float gapLength = 10f;

            if (axis == Axes.Y)
            {
                Width = thickness;
                Anchor = Anchor.TopLeft;
                Origin = Anchor.TopCentre;
            }
            else
            {
                Height = thickness;
                Anchor = Anchor.TopLeft;
                Origin = Anchor.CentreLeft;
            }

            var flow = new FillFlowContainer
            {
                RelativeSizeAxes = Axes.Both,
                Direction = axis == Axes.Y ? FillDirection.Vertical : FillDirection.Horizontal,
                Spacing = new Vector2(gapLength),
            };

            for (int i = 0; i < 50; i++)
            {
                flow.Add(new Box
                {
                    RelativeSizeAxes = axis == Axes.Y ? Axes.X : Axes.Y,
                    Size = new Vector2(axis == Axes.Y ? 1 : dashLength, axis == Axes.Y ? dashLength : 1),
                    Colour = Color4.White,
                    Alpha = 0.3f
                });
            }

            InternalChild = flow;
        }
    }
}

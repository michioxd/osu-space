using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Space.UI;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Space.Edit
{
    [Cached]
    public partial class SpaceEditorPlayfield : Playfield
    {
        public readonly Container contentContainer;

        protected override GameplayCursorContainer CreateCursor() => new SpaceEditorCursorContainer
        {
            RelativeSizeAxes = Axes.Both
        };

        private partial class SpaceEditorCursorContainer : GameplayCursorContainer
        {
            protected override Drawable CreateCursor() => new Circle
            {
                Size = new Vector2(40),
                Origin = Anchor.Centre,
                Colour = Color4.White,
                Alpha = 0.4f
            };
        }

        public SpaceEditorPlayfield()
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
                        0.6f
                    ),
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 1,
                    Children =
                    [
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions =
                            [
                                new Dimension(GridSizeMode.Relative, 1f / 3f),
                                new Dimension(GridSizeMode.Relative, 1f / 3f),
                                new Dimension(GridSizeMode.Relative, 1f / 3f)
                            ],
                            ColumnDimensions =
                            [
                                new Dimension(GridSizeMode.Relative, 1f / 3f),
                                new Dimension(GridSizeMode.Relative, 1f / 3f),
                                new Dimension(GridSizeMode.Relative, 1f / 3f)
                            ],
                            Content = new[]
                            {
                                [createGridBox(), createGridBox(), createGridBox()],
                                [createGridBox(), createGridBox(), createGridBox()],
                                new Drawable[] { createGridBox(), createGridBox(), createGridBox() },
                            }
                        },
                        HitObjectContainer,
                    ]
                }
            ];
        }

        private Drawable createGridBox()
        {
            return new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(3),
                Alpha = 0.5f,
                Child = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    CornerRadius = 6,
                    BorderColour = Color4.Gray,
                    BorderThickness = 2,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Black,
                        Alpha = 1f,
                    }
                }
            };
        }

        public new Vector2 GamefieldToScreenSpace(Vector2 point)
        {
            Vector2 normalized = new Vector2(point.X / SpacePlayfield.BASE_SIZE, point.Y / SpacePlayfield.BASE_SIZE);
            return HitObjectContainer.ToScreenSpace(normalized * HitObjectContainer.DrawSize);
        }

        public new Vector2 ScreenSpaceToGamefield(Vector2 screenSpacePosition)
        {
            Vector2 local = HitObjectContainer.ToLocalSpace(screenSpacePosition);
            Vector2 normalized = new Vector2(local.X / HitObjectContainer.DrawSize.X, local.Y / HitObjectContainer.DrawSize.Y);
            return normalized * SpacePlayfield.BASE_SIZE;
        }
    }
}

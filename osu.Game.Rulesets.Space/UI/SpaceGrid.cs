using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osuTK.Graphics;
using osuTK;

namespace osu.Game.Rulesets.Space.UI
{
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

            AddInternal(new GridIntersection { RelativePositionAxes = Axes.Both, Position = new Vector2(1f / 3f, 1f / 3f) });
            AddInternal(new GridIntersection { RelativePositionAxes = Axes.Both, Position = new Vector2(2f / 3f, 1f / 3f) });
            AddInternal(new GridIntersection { RelativePositionAxes = Axes.Both, Position = new Vector2(1f / 3f, 2f / 3f) });
            AddInternal(new GridIntersection { RelativePositionAxes = Axes.Both, Position = new Vector2(2f / 3f, 2f / 3f) });
        }
    }

    public partial class GridIntersection : CompositeDrawable
    {
        public GridIntersection()
        {
            Origin = Anchor.Centre;
            Size = new Vector2(15);

            InternalChildren = new Drawable[]
            {
                new Circle
                {
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Scale = new Vector2(0.5f),
                    Colour = Color4.White,
                    Alpha = 0.8f
                }
            };
        }
    }

    public partial class DashedLine : CompositeDrawable
    {
        public DashedLine(Axes axis)
        {
            RelativeSizeAxes = axis;
            float thickness = 2f;
            float dashLength = 7f;
            float gapLength = 12f;

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

            for (int i = 0; i < 60; i++)
            {
                flow.Add(new Circle
                {
                    RelativeSizeAxes = axis == Axes.Y ? Axes.X : Axes.Y,
                    Size = new Vector2(axis == Axes.Y ? 1 : dashLength, axis == Axes.Y ? dashLength : 1),
                    Colour = Color4.White,
                    Alpha = 0.6f
                });
            }

            InternalChild = flow;
        }
    }
}

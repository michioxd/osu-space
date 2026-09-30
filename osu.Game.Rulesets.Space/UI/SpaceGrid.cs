using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Space.UI
{
    public partial class SpaceGrid : CompositeDrawable
    {
        public SpaceGrid()
        {
            RelativeSizeAxes = Axes.Both;
            Alpha = 0;
            Masking = true;

            AddInternal(new GridLine(Axes.Y) { RelativePositionAxes = Axes.Both, X = 1f / 3f });
            AddInternal(new GridLine(Axes.Y) { RelativePositionAxes = Axes.Both, X = 2f / 3f });
            AddInternal(new GridLine(Axes.X) { RelativePositionAxes = Axes.Both, Y = 1f / 3f });
            AddInternal(new GridLine(Axes.X) { RelativePositionAxes = Axes.Both, Y = 2f / 3f });
        }
    }

    public partial class GridLine : Box
    {
        public GridLine(Axes axis)
        {
            RelativeSizeAxes = axis;
            Size = axis == Axes.Y ? new Vector2(2, 1) : new Vector2(1, 2);
            Colour = Color4.White;
            Alpha = 0.6f;
        }
    }
}

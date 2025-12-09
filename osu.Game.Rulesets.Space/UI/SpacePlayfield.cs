using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
using osuTK.Graphics;
using osu.Game.Rulesets.Space.UI.Cursor;
using osuTK;

namespace osu.Game.Rulesets.Space.UI
{
    [Cached]
    public partial class SpacePlayfield : Playfield
    {
        public static readonly Vector2 BASE_SIZE = new(512, 512);

        protected override GameplayCursorContainer CreateCursor() => new SpaceCursorContainer
        {
            RelativeSizeAxes = Axes.Both
        };

        [BackgroundDependencyLoader]
        private void load()
        {
            AddRangeInternal(
            [
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 5f,
                    BorderColour = Color4.White,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.Transparent,
                        Alpha = 255
                    }
                },
                HitObjectContainer,
            ]);
        }
    }
}

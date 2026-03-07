using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Graphics;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Space.Edit.Blueprints.Components
{
    public partial class EditNotePiece : CompositeDrawable
    {
        private readonly Container border;
        private readonly Box box;

        public EditNotePiece()
        {
            InternalChildren = new Drawable[]
            {
                border = new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 3,
                    Child = new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Alpha = 0,
                        AlwaysPresent = true,
                    },
                },
                box = new Box
                {
                    RelativeSizeAxes = Axes.X,
                    Height = 3,
                    Anchor = Anchor.BottomCentre,
                    Origin = Anchor.BottomCentre,
                },
            };
        }

        [BackgroundDependencyLoader]
        private void load(OsuColour colours)
        {
            border.BorderColour = colours.YellowDark;
            box.Colour = colours.YellowLight;
        }

        protected override void Update()
        {
            base.Update();

        }
    }
}

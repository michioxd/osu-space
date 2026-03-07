using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osuTK.Graphics;

namespace osu.Game.Rulesets.Space.Edit.Blueprints.Components
{
    public partial class EditNotePiece : CompositeDrawable
    {
        public EditNotePiece()
        {
            Origin = Anchor.Centre;
            Size = new Vector2(80);

            InternalChild = new Container
            {
                RelativeSizeAxes = Axes.Both,
                BorderColour = Color4.SkyBlue,
                BorderThickness = 2,
                CornerRadius = 10,
                Masking = true,
                Children = [
                    new Box
                    {
                        RelativeSizeAxes = Axes.Both,
                        Colour = Color4.DarkBlue,
                        Alpha = 0.5f,
                    }
                ]
            };
        }
    }
}

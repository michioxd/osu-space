using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;

namespace osu.Game.Rulesets.Space.UI
{
    public partial class SpaceMiss : CompositeDrawable
    {
        private readonly MissIcon[] icons = new MissIcon[9];

        public SpaceMiss()
        {
            RelativeSizeAxes = Axes.Both;

            for (int i = 0; i < 9; i++)
            {
                int col = i % 3;
                int row = i / 3;

                AddInternal(icons[i] = new MissIcon
                {
                    RelativePositionAxes = Axes.Both,
                    Position = new Vector2((col + 0.5f) / 3f, (row + 0.5f) / 3f),
                });
            }
        }

        public void ShowMiss(float col, float row)
        {
            if (col < 0 || col > 2 || row < 0 || row > 2) return;
            int index = (int)(row * 3 + col);
            icons[index].Show();
        }
    }

    public partial class MissIcon : CompositeDrawable
    {
        public MissIcon()
        {
            Origin = Anchor.Centre;
            Size = new Vector2(25);
            Alpha = 0;
            Rotation = 3;
            InternalChild = new SpriteIcon
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                RelativeSizeAxes = Axes.Both,
                Icon = FontAwesome.Solid.Times,
                Colour = Colour4.FromHex("#f5655b"),
            };
        }

        public new void Show()
        {
            ClearTransforms();
            Alpha = 1;
            this.Delay(500).FadeOut(300);
        }
    }
}

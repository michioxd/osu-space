using osu.Framework.Graphics;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Containers;
using osuTK.Graphics;
using osuTK;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Sprites;
using System;

namespace osu.Game.Rulesets.Space.UI
{
    public partial class SpaceMiss : CompositeDrawable
    {
        public SpaceMiss()
        {
            RelativeSizeAxes = Axes.Both;
        }

        public void ShowMiss(float col, float row)
        {
            var missIcon = new MissIcon
            {
                RelativePositionAxes = Axes.Both,
                Position = new Vector2((col + 0.5f) / 3f, (row + 0.5f) / 3f),
            };

            AddInternal(missIcon);
            missIcon.Show();

            // throw it away after it's done
            missIcon.Delay(800).Expire();
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

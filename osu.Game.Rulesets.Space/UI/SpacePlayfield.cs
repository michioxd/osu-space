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

namespace osu.Game.Rulesets.Space.UI
{
    [Cached]
    public partial class SpacePlayfield : Playfield
    {
        private readonly PlayfieldBorder playfieldBorder;
        public static readonly Vector2 BASE_SIZE = new(512, 512);

        protected override GameplayCursorContainer CreateCursor() => new SpaceCursorContainer
        {
            RelativeSizeAxes = Axes.Both
        };

        public SpacePlayfield()
        {
            Origin = Anchor.Centre;
            InternalChildren =
            [
                new Container
                {
                    RelativeSizeAxes = Axes.Both,
                    Masking = true,
                    BorderThickness = 5f,
                    BorderColour = Color4.White,
                    Child = playfieldBorder = new PlayfieldBorder
                    {
                        RelativeSizeAxes = Axes.Both,
                    }
                },
                HitObjectContainer,
            ];
        }

        [BackgroundDependencyLoader]
        private void load(SpaceRulesetConfigManager? config)
        {
            config?.BindWith(SpaceRulesetSetting.PlayfieldBorderStyle, playfieldBorder.PlayfieldBorderStyle);
        }
    }
}

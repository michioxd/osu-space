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
        private readonly Container contentContainer;
        private readonly Bindable<float> parallaxStrength = new();
        public static readonly float BASE_SIZE = 512;

        public static readonly float PLAYFIELD_SIZE = 0.6f;

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
                        0.6f
                    ),
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 1,
                    Children =
                    [
                        playfieldBorder = new PlayfieldBorder
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
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
            }
        }

        [BackgroundDependencyLoader]
        private void load(SpaceRulesetConfigManager? config)
        {
            config?.BindWith(SpaceRulesetSetting.PlayfieldBorderStyle, playfieldBorder.PlayfieldBorderStyle);
            config?.BindWith(SpaceRulesetSetting.Parallax, parallaxStrength);
        }
    }
}

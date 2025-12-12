#nullable enable
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Game.Rulesets.UI;
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
        private readonly SpaceGrid grid;
        public readonly Container contentContainer;
        public readonly SpaceMiss spaceMiss;
        private readonly Bindable<float> parallaxStrength = new();
        private readonly Bindable<bool> enableGrid = new();
        private readonly Bindable<float> scalePlayfield = new();
        public static readonly float BASE_SIZE = 512;
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
                        0.6f // initial
                    ),
                    FillMode = FillMode.Fit,
                    FillAspectRatio = 1,
                    Children =
                    [
                        playfieldBorder = new PlayfieldBorder
                        {
                            RelativeSizeAxes = Axes.Both,
                        },
                        grid = new SpaceGrid(),
                        spaceMiss = new SpaceMiss(),
                        HitObjectContainer,
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
            config?.BindWith(SpaceRulesetSetting.ScalePlayfield, scalePlayfield);
            config?.BindWith(SpaceRulesetSetting.EnableGrid, enableGrid);

            grid.Alpha = enableGrid.Value ? 1 : 0;
            contentContainer.Size = new Vector2(scalePlayfield.Value);

            enableGrid.BindValueChanged(e => grid.FadeTo(e.NewValue ? 1 : 0, 100), true);
            scalePlayfield.BindValueChanged(s => contentContainer.ResizeTo(s.NewValue, 200, Easing.OutQuint), true);
        }

        public new Vector2 GamefieldToScreenSpace(Vector2 point)
        {
            Vector2 normalized = new Vector2(point.X / BASE_SIZE, point.Y / BASE_SIZE);
            return HitObjectContainer.ToScreenSpace(normalized * HitObjectContainer.DrawSize);
        }

        public new Vector2 ScreenSpaceToGamefield(Vector2 screenSpacePosition)
        {
            Vector2 local = HitObjectContainer.ToLocalSpace(screenSpacePosition);
            Vector2 normalized = new Vector2(local.X / HitObjectContainer.DrawSize.X, local.Y / HitObjectContainer.DrawSize.Y);
            return normalized * BASE_SIZE;
        }
    }
}

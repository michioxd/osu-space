// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Localisation;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Space
{
    public partial class SpaceSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!space";

        public SpaceSettingsSubsection(SpaceRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load(GameHost host)
        {
            var config = (SpaceRulesetConfigManager)Config;

            Children = new Drawable[]
            {
                new SettingsCheckbox
                {
                    LabelText = "osu!space by michioxd ฅ^>//<^ฅ. Based on Sound Space (Roblox) Gameplay",
                },
                new SettingsButton
                {
                    Text = "GitHub Repository",
                    Action = () => host.OpenUrlExternally("https://github.com/michioxd/osu-space")
                },
                new SettingsButton
                {
                    Text = "Original Sound Space",
                    Action = () => host.OpenUrlExternally("https://www.roblox.com/games/2677609345/Sound-Space-Rhythm-Game")
                },
                new SettingsButton
                {
                    Text = "Sound Space Plus (Rhythia)",
                    Action = () => host.OpenUrlExternally("https://github.com/David20122/sound-space-plus"),
                },
                new SettingsEnumDropdown<PlayfieldBorderStyle>
                {
                    LabelText = RulesetSettingsStrings.PlayfieldBorderStyle,
                    Current = config.GetBindable<PlayfieldBorderStyle>(SpaceRulesetSetting.PlayfieldBorderStyle),
                },
                new SettingsSlider<float>
                {
                    LabelText = "Note Thickness",
                    TooltipText = "Thickness of the notes' borders",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.NoteThickness),
                    KeyboardStep = 0.5f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "Note Corner Radius",
                    TooltipText = "Roundness of the notes' corners",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.NoteCornerRadius),
                    KeyboardStep = 0.5f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "Note Opacity",
                    TooltipText = "How opaque/transparent/visible the note appears",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.noteOpacity),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<float>
                {
                    LabelText = "Note Scale",
                    TooltipText = "The visual size of the notes (doesn't affect hitboxes)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.noteScale),
                    KeyboardStep = 0.05f
                },
                new SettingsSlider<float>
                {
                    LabelText = "Approach Rate",
                    TooltipText = "The speed that note move toward the grid (m/s)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.approachRate),
                    KeyboardStep = 1f
                },
                new SettingsSlider<float>
                {
                    LabelText = "Spawn Distance",
                    TooltipText = "Distance from the grid that note spawn (m)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.spawnDistance),
                    KeyboardStep = 1f
                },
                new SettingsSlider<float>
                {
                    LabelText = "Fade Length",
                    TooltipText = "Percentage of the spawn distance that notes take to fade from invisible to fully opaque",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.fadeLength),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = "Do not push back",
                    TooltipText = "While enabled, notes will go past the grid when you miss, instead of always vanishing 0.1 units past the grid",
                    Keywords = new[] { "miss", "push", "back" },
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.doNotPushBack)
                },
                new SettingsCheckbox
                {
                    LabelText = "Half ghost",
                    TooltipText = "Useful for patterns that fill the whole screen",
                    Keywords = new[] { "ghost", "transparency", "alpha" },
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.halfGhost)
                }
            };
        }

        // private partial class SpaceScrollSlider : RoundedSliderBar<double>
        // {
        //     public override LocalisableString TooltipText => RulesetSettingsStrings.ScrollSpeedTooltip((int)DrawableSpaceRuleset.ComputeScrollTime(Current.Value), Current.Value);
        // }
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Framework.Configuration.Tracking;
using osu.Game.Configuration;
using osu.Game.Localisation;
using osu.Game.Rulesets.Configuration;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Space.Configuration
{
    public class SpaceRulesetConfigManager : RulesetConfigManager<SpaceRulesetSetting>
    {
        public SpaceRulesetConfigManager(SettingsStore? settings, RulesetInfo ruleset, int? variant = null)
            : base(settings, ruleset, variant)
        {
        }

        protected override void InitialiseDefaults()
        {
            base.InitialiseDefaults();
            SetDefault(SpaceRulesetSetting.SnakingInSliders, true);
            SetDefault(SpaceRulesetSetting.SnakingOutSliders, true);
            SetDefault(SpaceRulesetSetting.ShowCursorTrail, true);
            SetDefault(SpaceRulesetSetting.PlayfieldBorderStyle, PlayfieldBorderStyle.Full);
            SetDefault(SpaceRulesetSetting.noteOpacity, 1f, 0f, 1f, 0.01f);
            SetDefault(SpaceRulesetSetting.noteScale, 1.0f, 0.5f, 4.0f, 0.05f);
            SetDefault(SpaceRulesetSetting.approachRate, 40f, 1f, 200f, 1f);
            SetDefault(SpaceRulesetSetting.spawnDistance, 40f, 1f, 400f, 1f);
            SetDefault(SpaceRulesetSetting.fadeLength, 0.5f, 0f, 1f, 0.01f);
            SetDefault(SpaceRulesetSetting.doNotPushBack, true);
            SetDefault(SpaceRulesetSetting.halfGhost, false);
            SetDefault(SpaceRulesetSetting.NoteThickness, 5f, 0.5f, 10f, 0.5f);
            SetDefault(SpaceRulesetSetting.NoteCornerRadius, 7f, 0.5f, 9f, 0.5f);
            SetDefault(SpaceRulesetSetting.Palette, SpacePalette.White);
            SetDefault(SpaceRulesetSetting.Parallax, 5f, 0.0f, 25f, 0.1f);
            SetDefault(SpaceRulesetSetting.ScalePlayfield, 1f, 0.2f, 2f, 0.1f);
        }
    }

    public enum SpaceRulesetSetting
    {
        SnakingInSliders,
        SnakingOutSliders,
        ShowCursorTrail,
        PlayfieldBorderStyle,
        noteOpacity,
        noteScale,
        approachRate,
        spawnDistance,
        fadeLength,
        doNotPushBack,
        halfGhost,
        NoteThickness,
        NoteCornerRadius,
        Palette,
        Parallax,
        ScalePlayfield
    }
}

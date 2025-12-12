// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

#nullable enable

using osu.Game.Configuration;
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
            SetDefault(SpaceRulesetSetting.PlayfieldBorderStyle, PlayfieldBorderStyle.Full);
            SetDefault(SpaceRulesetSetting.EnableGrid, false);
            SetDefault(SpaceRulesetSetting.noteOpacity, 1f, 0f, 1f, 0.01f);
            SetDefault(SpaceRulesetSetting.noteScale, 1.0f, 0.5f, 4.0f, 0.05f);
            SetDefault(SpaceRulesetSetting.approachRate, 40f, 1f, 125f, 1f);
            SetDefault(SpaceRulesetSetting.spawnDistance, 40f, 5f, 250f, 1f);
            SetDefault(SpaceRulesetSetting.fadeLength, 0.5f, 0f, 1f, 0.01f);
            SetDefault(SpaceRulesetSetting.doNotPushBack, true);
            SetDefault(SpaceRulesetSetting.halfGhost, false);
            SetDefault(SpaceRulesetSetting.NoteThickness, 8f, 0.5f, 10f, 0.1f);
            SetDefault(SpaceRulesetSetting.NoteCornerRadius, 8f, 0.5f, 9f, 0.1f);
            SetDefault(SpaceRulesetSetting.Palette, SpacePalette.White);
            SetDefault(SpaceRulesetSetting.Parallax, 2f, 0.0f, 20f, 0.1f);
            SetDefault(SpaceRulesetSetting.ScalePlayfield, 0.6f, 0.2f, 0.95f, 0.05f);
            SetDefault(SpaceRulesetSetting.GameplayCursorSize, 1.0f, 0.1f, 6f, 0.01f);
            SetDefault(SpaceRulesetSetting.ShowCursorTrail, true);
            SetDefault(SpaceRulesetSetting.Bloom, false);
            SetDefault(SpaceRulesetSetting.BloomStrength, 1.0f, 0.1f, 10f, 0.01f);
        }
    }

    public enum SpaceRulesetSetting
    {
        ShowCursorTrail,
        PlayfieldBorderStyle,
        EnableGrid,
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
        ScalePlayfield,
        GameplayCursorSize,
        Bloom,
        BloomStrength,
    }
}

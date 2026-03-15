#nullable enable
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Space.Beatmaps;
using osu.Game.Rulesets.Space.Mods;
using osu.Game.Rulesets.Space.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Configuration;
using osu.Game.Configuration;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Space.Scoring;
using osu.Game.Skinning;
using osu.Game.Rulesets.Space.Skinning.Legacy;
using osu.Game.Rulesets.Space.Skinning.Argon;
using osu.Game.Rulesets.Space.Beatmaps.Formats;
using osu.Game.Screens.Edit.Setup;
using osu.Game.Rulesets.Space.Edit;
using osu.Game.Rulesets.Edit;

namespace osu.Game.Rulesets.Space
{
    public partial class SpaceRuleset : Ruleset
    {
        public override string Description => "osu!space";
        public override string ShortName => "osuspaceruleset";
        public const string VERSION_STRING = "2026.316.0";
        public SpaceRuleset()
        {
            SpaceLegacyBeatmapDecoder.Register();

            // lol who fkin knows
            RulesetInfo.OnlineID = 727;
        }

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) =>
            new DrawableSpaceRuleset(this, beatmap, mods);

        public override HitObjectComposer CreateHitObjectComposer() => new SpaceHitObjectComposer(this);

        public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new SpaceHealthProcessor();

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new SpaceBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) =>
            new SpaceDifficultyCalculator(RulesetInfo, beatmap);

        public override PerformanceCalculator CreatePerformanceCalculator() => new SpacePerformanceCalculator();

        public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new SpaceRulesetConfigManager(settings, RulesetInfo);

        public override RulesetSettingsSubsection CreateSettings() => new SpaceSettingsSubsection(this);

        public override IEnumerable<Drawable> CreateEditorSetupSections() =>
        [
            new MetadataSection(),
            new ResourcesSection(),
        ];

        public override IEnumerable<HitResult> GetValidHitResults()
        {
            return
            [
                HitResult.Perfect,
                HitResult.Miss
            ];
        }

        public override ISkin? CreateSkinTransformer(ISkin skin, IBeatmap beatmap)
        {
            switch (skin)
            {
                case LegacySkin:
                    return new SpaceLegacySkinTransformer(skin);

                case ArgonSkin:
                    return new SpaceArgonSkinTransformer(skin);
            }

            return null;
        }

        public override IEnumerable<Mod> GetModsFor(ModType type)
        {
            switch (type)
            {
                case ModType.DifficultyReduction:
                    return
                    [
                        new SpaceModNoFail(),
                        new MultiMod(new SpaceModHalfTime(), new SpaceModDaycore()),
                    ];

                case ModType.DifficultyIncrease:
                    return
                    [
                        new MultiMod(new SpaceModPerfect()),
                        new MultiMod(new SpaceModDoubleTime(), new SpaceModNightcore()),
                        new ModAccuracyChallenge(),
                    ];

                // case ModType.Conversion:
                //     return new Mod[]
                //     {

                //     };

                case ModType.Automation:
                    return
                    [
                        new MultiMod(new SpaceModAutoplay(), new SpaceModCinema()),
                    ];

                case ModType.Fun:
                    return
                    [
                        new MultiMod(new ModWindUp(), new ModWindDown()),
                        new SpaceModMuted(),
                        new ModAdaptiveSpeed(),
                        new SpaceModNoScope(),
                    ];
                // case ModType.System:
                //     return new Mod[]
                //     {
                //     };

                default:
                    return [];
            }
        }

        public override IEnumerable<KeyBinding> GetDefaultKeyBindings(int variant = 0) => [];

        public override Drawable CreateIcon() => new SpaceRulesetIcon(this);

        // only notes is enough
        public override IEnumerable<RulesetBeatmapAttribute> GetBeatmapAttributesForDisplay(IBeatmapInfo beatmapInfo, IReadOnlyCollection<Mod> mods) => [];

        // Leave this line intact. It will bake the correct version into the ruleset on each build/release.
        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;
    }
}

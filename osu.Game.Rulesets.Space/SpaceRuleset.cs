#nullable enable
using System.Collections.Generic;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Difficulty;
using osu.Game.Rulesets.Space.Beatmaps;
using osu.Game.Rulesets.Space.Mods;
using osu.Game.Rulesets.Space.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.UI;
using osuTK;
using osuTK.Graphics;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Configuration;
using osu.Game.Configuration;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Space.Scoring;
using osu.Game.Skinning;
using osu.Game.Rulesets.Space.Skinning.Legacy;
using osu.Game.Rulesets.Space.Skinning.Argon;

namespace osu.Game.Rulesets.Space
{
    public partial class SpaceRuleset : Ruleset
    {
        public override string Description => "osu!space";
        public override string ShortName => "osuspaceruleset";
        public const string VERSION_STRING = "2025.1212.2";

        public override DrawableRuleset CreateDrawableRulesetWith(IBeatmap beatmap, IReadOnlyList<Mod>? mods = null) =>
            new DrawableSpaceRuleset(this, beatmap, mods);

        public override HealthProcessor CreateHealthProcessor(double drainStartTime) => new SpaceHealthProcessor();

        public override IBeatmapConverter CreateBeatmapConverter(IBeatmap beatmap) =>
            new SpaceBeatmapConverter(beatmap, this);

        public override DifficultyCalculator CreateDifficultyCalculator(IWorkingBeatmap beatmap) =>
            new SpaceDifficultyCalculator(RulesetInfo, beatmap);

        public override IRulesetConfigManager CreateConfig(SettingsStore? settings) => new SpaceRulesetConfigManager(settings, RulesetInfo);

        public override RulesetSettingsSubsection CreateSettings() => new SpaceSettingsSubsection(this);

        protected override IEnumerable<HitResult> GetValidHitResults()
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

        public override Drawable CreateIcon() => new Icon(ShortName[0]);

        public partial class Icon : CompositeDrawable
        {
            public Icon(char c)
            {
                Size = new Vector2(50);
                InternalChildren =
                [
                    new CircularContainer
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        BorderThickness = 6f,
                        BorderColour = Color4.White,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        }
                    },
                    new Container
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Size = new Vector2(28),
                        Rotation = -25,
                        Masking = true,
                        CornerRadius = 8,
                        BorderThickness = 6f,
                        BorderColour = Color4.White,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true,
                        }
                    }
                ];
            }
        }

        // Leave this line intact. It will bake the correct version into the ruleset on each build/release.
        public override string RulesetAPIVersionSupported => CURRENT_RULESET_API_VERSION;
    }
}

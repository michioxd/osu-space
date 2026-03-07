using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Screens.Edit.Compose;
using osu.Game.Rulesets.Space.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;

using osu.Framework.Allocation;

namespace osu.Game.Rulesets.Space.Edit
{
    [Cached]
    public partial class SpaceHitObjectComposer : HitObjectComposer<SpaceHitObject>
    {
        private DrawableSpaceEditorRuleset drawableRuleset = null!;

        public SpaceHitObjectComposer(SpaceRuleset ruleset)
            : base(ruleset)
        {
        }

        protected override IReadOnlyList<CompositionTool> CompositionTools =>
        [
            new NoteCompositionTool()
        ];

        protected override DrawableRuleset<SpaceHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
        drawableRuleset = new DrawableSpaceEditorRuleset(ruleset, beatmap, mods);

        protected override ComposeBlueprintContainer CreateBlueprintContainer()
            => new SpaceBlueprintContainer(this);

        public SpaceEditorPlayfield EditorPlayfield => (SpaceEditorPlayfield)drawableRuleset.Playfield;
    }
}

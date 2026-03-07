using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Space.Edit.Blueprints;
using osu.Game.Rulesets.Space.Objects;

namespace osu.Game.Rulesets.Space.Edit
{
    public class NoteCompositionTool : CompositionTool
    {
        public NoteCompositionTool()
            : base(nameof(Note))
        {
        }

        public override PlacementBlueprint CreatePlacementBlueprint() => new NotePlacementBlueprint();
    }
}

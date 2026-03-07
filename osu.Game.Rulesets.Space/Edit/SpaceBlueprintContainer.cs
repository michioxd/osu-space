using System.Collections.Generic;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Space.Edit.Blueprints;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class SpaceBlueprintContainer : ComposeBlueprintContainer
    {
        public SpaceBlueprintContainer(SpaceHitObjectComposer composer)
            : base(composer)
        {
        }

        protected override SelectionHandler<HitObject> CreateSelectionHandler() => new SpaceSelectionHandler();

        public override HitObjectSelectionBlueprint? CreateHitObjectBlueprintFor(HitObject hitObject)
        {
            switch (hitObject)
            {
                case Note note:
                    return new NoteSelectionBlueprint(note);
            }

            return base.CreateHitObjectBlueprintFor(hitObject);
        }

        protected override bool TryMoveBlueprints(DragEvent e, IList<(SelectionBlueprint<HitObject> blueprint, Vector2[] originalSnapPositions)> blueprints)
        {
            return false;
        }
    }
}

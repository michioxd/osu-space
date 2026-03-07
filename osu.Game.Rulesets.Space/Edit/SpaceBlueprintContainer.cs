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
            var playfield = ((SpaceHitObjectComposer)Composer).EditorPlayfield;

            Vector2 targetGamefieldPos = playfield.ScreenSpaceToGamefield(e.ScreenSpaceMousePosition);
            bool snapToGrid = !e.ShiftPressed;
            float cellSize = UI.SpacePlayfield.BASE_SIZE / 3f;

            Vector2 gridTarget = new Vector2(
                (targetGamefieldPos.X / cellSize) - 0.5f,
                (targetGamefieldPos.Y / cellSize) - 0.5f
            );

            if (snapToGrid)
            {
                gridTarget = new Vector2(
                    (float)System.Math.Round(gridTarget.X),
                    (float)System.Math.Round(gridTarget.Y)
                );
            }

            gridTarget = new Vector2(
                System.Math.Clamp(gridTarget.X, 0f, 2f),
                System.Math.Clamp(gridTarget.Y, 0f, 2f)
            );

            if (blueprints.Count > 0)
            {
                var firstOriginalScreen = blueprints[0].originalSnapPositions[0];
                Vector2 originalGrid = new Vector2(
                    (firstOriginalScreen.X / cellSize) - 0.5f,
                    (firstOriginalScreen.Y / cellSize) - 0.5f
                );

                Vector2 dragDelta = gridTarget - originalGrid;

                foreach (var b in blueprints)
                {
                    if (b.blueprint.Item is SpaceHitObject spaceObject)
                    {
                        Vector2 objOriginalGrid = new Vector2(
                            (b.originalSnapPositions[0].X / cellSize) - 0.5f,
                            (b.originalSnapPositions[0].Y / cellSize) - 0.5f
                        );

                        Vector2 newGridPos = objOriginalGrid + dragDelta;

                        if (snapToGrid)
                        {
                            newGridPos = new Vector2(
                                (float)System.Math.Round(newGridPos.X),
                                (float)System.Math.Round(newGridPos.Y)
                            );
                        }

                        newGridPos = new Vector2(
                            System.Math.Clamp(newGridPos.X, 0f, 2f),
                            System.Math.Clamp(newGridPos.Y, 0f, 2f)
                        );

                        spaceObject.oX = newGridPos.X;
                        spaceObject.oY = newGridPos.Y;
                    }
                }
            }

            return true;
        }
    }
}

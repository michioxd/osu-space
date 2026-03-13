using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Space.Edit.Blueprints;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Screens.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class SpaceBlueprintContainer : ComposeBlueprintContainer
    {
        [Resolved(CanBeNull = true)]
        private EditorBeatmap editorBeatmap { get; set; }

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

        protected override void UpdateSelectionFromDragBox(HashSet<HitObject> selectionBeforeDrag)
        {
            var quad = DragBox.Box.ScreenSpaceDrawQuad;

            foreach (var blueprint in SelectionBlueprints)
            {
                bool shouldBeSelected = selectionBeforeDrag.Contains(blueprint.Item)
                                        || (blueprint.IsSelectable
                                            && isWithinVisibleRange(blueprint.Item)
                                            && quad.Contains(blueprint.ScreenSpaceSelectionPoint));

                if (blueprint.IsSelected)
                {
                    if (!shouldBeSelected)
                        blueprint.Deselect();
                }
                else if (shouldBeSelected)
                {
                    blueprint.Select();
                }
            }
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
                Vector2 firstOriginalGamefield = playfield.ScreenSpaceToGamefield(firstOriginalScreen);
                Vector2 originalGrid = new Vector2(
                    (firstOriginalGamefield.X / cellSize) - 0.5f,
                    (firstOriginalGamefield.Y / cellSize) - 0.5f
                );

                Vector2 dragDelta = gridTarget - originalGrid;

                foreach (var b in blueprints)
                {
                    if (b.blueprint.Item is SpaceHitObject spaceObject)
                    {
                        Vector2 originalGamefield = playfield.ScreenSpaceToGamefield(b.originalSnapPositions[0]);
                        Vector2 objOriginalGrid = new Vector2(
                            (originalGamefield.X / cellSize) - 0.5f,
                            (originalGamefield.Y / cellSize) - 0.5f
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

        private bool isWithinVisibleRange(HitObject hitObject)
        {
            if (hitObject is not SpaceHitObject spaceHitObject)
                return false;

            double zoom = editorBeatmap?.TimelineZoom > 0 ? editorBeatmap.TimelineZoom : 1.0;
            double preempt = 1500 / zoom;
            double fadeOut = 200 / zoom;

            double visibleStartTime = spaceHitObject.StartTime - preempt;
            double visibleEndTime = spaceHitObject.StartTime + fadeOut;
            double currentTime = EditorClock.CurrentTime;

            return visibleStartTime <= currentTime && currentTime <= visibleEndTime;
        }
    }
}

using System;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Screens.Edit.Compose.Components;
using osuTK.Input;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class SpaceSelectionHandler : EditorSelectionHandler
    {
        [Resolved]
        private HitObjectComposer composer { get; set; } = null!;

        private const int max_column = 2;

        private bool nudgeMovementActive;

        protected override void OnSelectionChanged()
        {
            base.OnSelectionChanged();

            var selectedObjects = SelectedItems.OfType<SpaceHitObject>().ToArray();

            SelectionBox.CanFlipX = canFlipX(selectedObjects);
            SelectionBox.CanFlipY = canFlipY(selectedObjects);
            SelectionBox.CanReverse = EditorBeatmap.SelectedHitObjects.Count > 1;
        }

        #region Keyboard nudge (Ctrl+Arrow)

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (e.ShiftPressed)
                return false;

            if (e.ControlPressed)
            {
                switch (e.Key)
                {
                    case Key.Left:
                        return nudgeSelection(-1, 0);

                    case Key.Right:
                        return nudgeSelection(1, 0);

                    case Key.Up:
                        return nudgeSelection(0, -1);

                    case Key.Down:
                        return nudgeSelection(0, 1);
                }
            }

            return false;
        }

        protected override void OnKeyUp(KeyUpEvent e)
        {
            base.OnKeyUp(e);

            if (nudgeMovementActive && !e.ControlPressed)
            {
                EditorBeatmap.EndChange();
                nudgeMovementActive = false;
            }
        }

        private bool nudgeSelection(int deltaX, int deltaY)
        {
            if (!nudgeMovementActive)
            {
                nudgeMovementActive = true;
                EditorBeatmap.BeginChange();
            }

            var selectedObjects = EditorBeatmap.SelectedHitObjects.OfType<SpaceHitObject>().ToArray();

            if (selectedObjects.Length == 0)
                return false;

            int minX = selectedObjects.Min(o => (int)Math.Round(o.oX));
            int maxX = selectedObjects.Max(o => (int)Math.Round(o.oX));
            int minY = selectedObjects.Min(o => (int)Math.Round(o.oY));
            int maxY = selectedObjects.Max(o => (int)Math.Round(o.oY));

            deltaX = Math.Clamp(deltaX, -minX, max_column - maxX);
            deltaY = Math.Clamp(deltaY, -minY, max_column - maxY);

            if (deltaX == 0 && deltaY == 0)
                return true;

            foreach (var obj in selectedObjects)
            {
                obj.oX = Math.Clamp(obj.oX + deltaX, 0f, max_column);
                obj.oY = Math.Clamp(obj.oY + deltaY, 0f, max_column);
            }

            return true;
        }

        #endregion

        #region HandleMovement (drag)

        public override bool HandleMovement(MoveSelectionEvent<HitObject> moveEvent)
        {
            var editorPlayfield = ((SpaceHitObjectComposer)composer).EditorPlayfield;

            var screenPos = moveEvent.Blueprint.ScreenSpaceSelectionPoint + moveEvent.ScreenSpaceDelta;
            var gamefieldPos = editorPlayfield.ScreenSpaceToGamefield(screenPos);
            float cellSize = UI.SpacePlayfield.BASE_SIZE / 3f;

            int targetCol = (int)Math.Round(gamefieldPos.X / cellSize - 0.5f);
            int targetRow = (int)Math.Round(gamefieldPos.Y / cellSize - 0.5f);

            targetCol = Math.Clamp(targetCol, 0, max_column);
            targetRow = Math.Clamp(targetRow, 0, max_column);

            var blueprint = (HitObjectSelectionBlueprint)moveEvent.Blueprint;
            var draggedObject = (SpaceHitObject)blueprint.Item;

            int lastCol = (int)Math.Round(draggedObject.oX);
            int lastRow = (int)Math.Round(draggedObject.oY);

            int deltaCol = targetCol - lastCol;
            int deltaRow = targetRow - lastRow;

            if (deltaCol == 0 && deltaRow == 0)
                return true;

            var selectedObjects = EditorBeatmap.SelectedHitObjects.OfType<SpaceHitObject>().ToArray();

            int minCol = selectedObjects.Min(o => (int)Math.Round(o.oX));
            int maxCol = selectedObjects.Max(o => (int)Math.Round(o.oX));
            int minRow = selectedObjects.Min(o => (int)Math.Round(o.oY));
            int maxRow = selectedObjects.Max(o => (int)Math.Round(o.oY));

            deltaCol = Math.Clamp(deltaCol, -minCol, max_column - maxCol);
            deltaRow = Math.Clamp(deltaRow, -minRow, max_column - maxRow);

            foreach (var obj in selectedObjects)
            {
                obj.oX = Math.Clamp(obj.oX + deltaCol, 0f, max_column);
                obj.oY = Math.Clamp(obj.oY + deltaRow, 0f, max_column);
            }

            return true;
        }

        #endregion

        #region Flip

        public override bool HandleFlip(Direction direction, bool flipOverOrigin)
        {
            var selectedObjects = SelectedItems.OfType<SpaceHitObject>().ToArray();

            if (selectedObjects.Length == 0)
                return false;

            switch (direction)
            {
                case Direction.Horizontal:
                    if (!canFlipX(selectedObjects))
                        return false;

                    float firstCol = flipOverOrigin ? 0 : selectedObjects.Min(o => o.oX);
                    float lastCol = flipOverOrigin ? max_column : selectedObjects.Max(o => o.oX);

                    foreach (var obj in selectedObjects)
                        obj.oX = Math.Clamp(firstCol + (lastCol - obj.oX), 0f, max_column);

                    return true;

                case Direction.Vertical:
                    if (!canFlipY(selectedObjects))
                        return false;

                    float firstRow = flipOverOrigin ? 0 : selectedObjects.Min(o => o.oY);
                    float lastRow = flipOverOrigin ? max_column : selectedObjects.Max(o => o.oY);

                    foreach (var obj in selectedObjects)
                        obj.oY = Math.Clamp(firstRow + (lastRow - obj.oY), 0f, max_column);

                    return true;

                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, "Cannot flip over the supplied direction.");
            }
        }

        #endregion

        #region Reverse

        public override bool HandleReverse()
        {
            var hitObjects = EditorBeatmap.SelectedHitObjects
                                          .OfType<SpaceHitObject>()
                                          .OrderBy(obj => obj.StartTime)
                                          .ToList();

            if (hitObjects.Count < 2)
                return false;

            double startTime = hitObjects.Min(h => h.StartTime);
            double endTime = hitObjects.Max(h => h.GetEndTime());

            foreach (var h in hitObjects)
                h.StartTime = endTime - (h.GetEndTime() - startTime);

            return true;
        }

        #endregion

        #region Helpers

        private static bool canFlipX(SpaceHitObject[] selectedObjects)
            => selectedObjects.Select(o => Math.Round(o.oX)).Distinct().Count() > 1;

        private static bool canFlipY(SpaceHitObject[] selectedObjects)
            => selectedObjects.Select(o => Math.Round(o.oY)).Distinct().Count() > 1;

        #endregion
    }
}

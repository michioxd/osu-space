using System;
using osu.Framework.Allocation;
using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Space.Edit.Blueprints.Components;
using osu.Game.Rulesets.Space.Objects;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Space.Edit.Blueprints
{
    public partial class NotePlacementBlueprint : HitObjectPlacementBlueprint
    {
        public new Note HitObject => (Note)base.HitObject;

        private readonly EditNotePiece piece;

        [Resolved(CanBeNull = false)]
        private SpaceHitObjectComposer composer { get; set; } = null!;

        private SpaceEditorPlayfield playfield => composer.EditorPlayfield;

        public NotePlacementBlueprint()
            : base(new Note())
        {
            InternalChild = piece = new EditNotePiece();
        }

        protected override bool OnMouseDown(MouseDownEvent e)
        {
            if (e.Button == MouseButton.Left)
            {
                EndPlacement(true);
                return true;
            }

            return base.OnMouseDown(e);
        }

        public override SnapResult UpdateTimeAndPosition(Vector2 snapPosition, double time)
        {
            var result = base.UpdateTimeAndPosition(snapPosition, time);

            HitObject.StartTime = time;

            var gamefieldPos = playfield.ScreenSpaceToGamefield(snapPosition);

            float cellSize = UI.SpacePlayfield.BASE_SIZE / 3f;

            bool snapToGrid = !GetContainingInputManager().CurrentState.Keyboard.ShiftPressed;

            Vector2 gridTarget = new Vector2(
                (gamefieldPos.X / cellSize) - 0.5f,
                (gamefieldPos.Y / cellSize) - 0.5f
            );

            if (snapToGrid)
            {
                gridTarget = new Vector2(
                    (float)Math.Round(gridTarget.X),
                    (float)Math.Round(gridTarget.Y)
                );
            }

            gridTarget = new Vector2(
                Math.Clamp(gridTarget.X, 0f, 2f),
                Math.Clamp(gridTarget.Y, 0f, 2f)
            );

            HitObject.oX = gridTarget.X;
            HitObject.oY = gridTarget.Y;

            piece.Position = ToLocalSpace(playfield.GamefieldToScreenSpace(HitObject.Position));

            return result;
        }
    }
}

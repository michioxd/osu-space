using osu.Framework.Input.Events;
using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Space.Edit.Blueprints.Components;
using osu.Game.Rulesets.Space.Objects;
using osuTK.Input;

namespace osu.Game.Rulesets.Space.Edit.Blueprints
{
    public partial class NotePlacementBlueprint : HitObjectPlacementBlueprint
    {
        public new Note HitObject => (Note)base.HitObject;

        private readonly EditNotePiece piece;

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

        public override SnapResult UpdateTimeAndPosition(osuTK.Vector2 snapPosition, double time)
        {
            var result = base.UpdateTimeAndPosition(snapPosition, time);

            HitObject.StartTime = time;

            HitObject.Position = ToLocalSpace(snapPosition);
            piece.Position = HitObject.Position;

            return result;
        }
    }
}

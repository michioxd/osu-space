using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Space.Objects;

namespace osu.Game.Rulesets.Space.Edit.Blueprints
{
    public partial class NoteSelectionBlueprint : HitObjectSelectionBlueprint<Note>
    {
        public NoteSelectionBlueprint(Note hitObject)
            : base(hitObject)
        {
        }

        protected override void Update()
        {
            base.Update();

            Position = HitObject.Position;
        }

        public override osuTK.Vector2 ScreenSpaceSelectionPoint => Position;
    }
}

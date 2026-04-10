using osu.Game.Rulesets.Edit;
using osu.Game.Rulesets.Space.Objects;
using osuTK;

namespace osu.Game.Rulesets.Space.Edit.Blueprints
{
    public partial class NoteSelectionBlueprint : HitObjectSelectionBlueprint<Note>
    {
        public NoteSelectionBlueprint(Note hitObject)
            : base(hitObject) { }

        protected override void Update()
        {
            base.Update();

            if (Parent != null)
                Position = Parent.ToLocalSpace(DrawableObject.ScreenSpaceDrawQuad.Centre);
        }
    }
}

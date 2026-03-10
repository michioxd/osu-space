using osu.Game.Rulesets.Edit.Tools;
using osu.Game.Rulesets.Space.Objects;
using System.Collections.Generic;
using osu.Game.Rulesets.Edit;
using osu.Game.Screens.Edit.Compose.Components;
using osu.Game.Rulesets.UI;
using osu.Game.Rulesets.Mods;
using osu.Game.Beatmaps;
using osu.Game.Screens.Edit;

using osu.Framework.Allocation;
using osu.Game.Rulesets.Space.Edit.Compose.Components;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Space.Edit
{
    [Cached]
    public partial class SpaceHitObjectComposer : HitObjectComposer<SpaceHitObject>
    {
        private DrawableSpaceEditorRuleset drawableRuleset = null!;

        [Resolved(CanBeNull = true)]
        private EditorBeatmap editorBeatmap { get; set; }

        public SpaceHitObjectComposer(SpaceRuleset ruleset)
            : base(ruleset)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(new SpaceHoldToExitOverlay());

            if (editorBeatmap != null)
            {
                editorBeatmap.HitObjectAdded += onHitObjectChanged;
                editorBeatmap.HitObjectRemoved += onHitObjectChanged;
                editorBeatmap.HitObjectUpdated += onHitObjectChanged;
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            base.Dispose(isDisposing);

            if (editorBeatmap != null)
            {
                editorBeatmap.HitObjectAdded -= onHitObjectChanged;
                editorBeatmap.HitObjectRemoved -= onHitObjectChanged;
                editorBeatmap.HitObjectUpdated -= onHitObjectChanged;
            }
        }

        private bool cacheInvalidated = true;

        private void onHitObjectChanged(Rulesets.Objects.HitObject obj)
        {
            cacheInvalidated = true;
        }

        protected override void Update()
        {
            base.Update();

            if (cacheInvalidated && editorBeatmap != null)
            {
                var hitObjects = editorBeatmap.HitObjects;
                int[,] counts = new int[3, 3];

                for (int i = 0; i < hitObjects.Count; i++)
                {
                    if (hitObjects[i] is SpaceHitObject spaceHo)
                    {
                        int cx = (int)System.Math.Clamp(System.Math.Round(spaceHo.oX), 0, 2);
                        int cy = (int)System.Math.Clamp(System.Math.Round(spaceHo.oY), 0, 2);

                        int currentCount = counts[cx, cy];

                        if (spaceHo.CellIndex != currentCount)
                            spaceHo.CellIndex = currentCount;

                        counts[cx, cy] = currentCount + 1;

                        if (spaceHo.Index != i + 1)
                            spaceHo.Index = i + 1;
                    }
                }

                cacheInvalidated = false;
            }
        }

        protected override IReadOnlyList<CompositionTool> CompositionTools =>
        [
            new NoteCompositionTool()
        ];

        protected override Drawable CreateHitObjectInspector() => new SpaceHitObjectInspector();

        protected override DrawableRuleset<SpaceHitObject> CreateDrawableRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
        drawableRuleset = new DrawableSpaceEditorRuleset(ruleset, beatmap, mods);

        protected override ComposeBlueprintContainer CreateBlueprintContainer()
            => new SpaceBlueprintContainer(this);

        public SpaceEditorPlayfield EditorPlayfield => (SpaceEditorPlayfield)drawableRuleset.Playfield;
    }
}

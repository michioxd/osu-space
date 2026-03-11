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
using osu.Framework.Input.Events;
using osu.Framework.Logging;
using osuTK.Input;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using System;
using osu.Framework.Platform;
using osu.Game.Database;

namespace osu.Game.Rulesets.Space.Edit
{
    [Cached]
    public partial class SpaceHitObjectComposer : HitObjectComposer<SpaceHitObject>
    {
        private DrawableSpaceEditorRuleset drawableRuleset = null!;
        private SpaceSaveFilePickerScreen saveFilePickerOverlay = null!;

        [Resolved(CanBeNull = true)]
        private EditorBeatmap editorBeatmap { get; set; }

        public SpaceHitObjectComposer(SpaceRuleset ruleset)
            : base(ruleset)
        {
        }

        [Resolved(CanBeNull = true)]
        private INotificationOverlay notifications { get; set; }

        [Resolved(CanBeNull = true)]
        private IDialogOverlay dialogOverlay { get; set; }

        [Resolved(CanBeNull = true)]
        private BeatmapManager beatmapManager { get; set; }

        [Resolved(CanBeNull = true)]
        private RealmAccess realm { get; set; }

        [Resolved(CanBeNull = true)]
        private Storage storage { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(saveFilePickerOverlay = new SpaceSaveFilePickerScreen(saveBeatmapToPath, getDefaultSaveFileName, false, canDirectSaveToRealm, requestDirectSaveToRealm));
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

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.ControlPressed && !e.AltPressed && !e.SuperPressed && e.Key == Key.S)
            {
                saveFilePickerOverlay.Show();
                return true;
            }

            return base.OnKeyDown(e);
        }

        private string getDefaultSaveFileName()
            => SpaceBeatmapFileHandler.GetDefaultSaveFileName(editorBeatmap);

        private void saveBeatmapToPath(string path)
        {
            if (editorBeatmap == null || string.IsNullOrWhiteSpace(path))
                return;

            try
            {
                SpaceBeatmapFileHandler.SaveBeatmapToPath(editorBeatmap, path);

                Logger.Log($"Saved beatmap to: {path}");
                notifications?.Post(new SimpleNotification
                {
                    Text = $"Beatmap saved to {System.IO.Path.GetFileName(path)}"
                });
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to save beatmap to: {path}");
                notifications?.Post(new SimpleNotification
                {
                    Text = $"Failed to save beatmap: {ex.Message}"
                });
            }
        }

        private bool canDirectSaveToRealm()
        {
            if (editorBeatmap == null || beatmapManager == null)
                return false;

            return beatmapManager.QueryBeatmap(b => b.ID == editorBeatmap.BeatmapInfo.ID) != null;
        }

        private void requestDirectSaveToRealm()
        {
            if (!canDirectSaveToRealm())
                return;

            Action onConfirm = () =>
            {
                try
                {
                    directSaveToRealm();
                    notifications?.Post(new SimpleNotification
                    {
                        Text = "Beatmap was saved directly to local database."
                    });
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, "Direct beatmap save to database failed.");
                    notifications?.Post(new SimpleNotification
                    {
                        Text = $"Direct save failed: {ex.Message}"
                    });
                }
            };

            if (dialogOverlay != null)
            {
                dialogOverlay.Push(new ConfirmDialog("This feature is experimental. Use at your own risk. Continue?", onConfirm));
                return;
            }

            onConfirm();
        }

        private void directSaveToRealm()
        {
            if (editorBeatmap == null || realm == null || storage == null)
                throw new InvalidOperationException("Required dependencies are unavailable for direct realm save.");

            SpaceBeatmapFileHandler.DirectSaveToRealm(editorBeatmap, realm, storage);
        }
    }
}

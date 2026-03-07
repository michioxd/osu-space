using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Space.Objects;
using osuTK;
using osuTK.Graphics;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Game.Screens.Edit;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class DrawableSpaceEditorHitObject : DrawableHitObject<SpaceHitObject>
    {
        private Container content;
        private Container borderContainer;
        private Box innerBox;
        private Container approachSquare;
        private OsuSpriteText indexText;

        [Resolved(CanBeNull = true)]
        private EditorBeatmap editorBeatmap { get; set; }

        public DrawableSpaceEditorHitObject(SpaceHitObject hitObject)
            : base(hitObject)
        {
            Origin = Anchor.Centre;
            RelativePositionAxes = Axes.Both;
            RelativeSizeAxes = Axes.Both;

            Size = new Vector2(0.8f / 3f);

            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Padding = new MarginPadding(6),
                Children =
                [
                    borderContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        BorderColour = Color4.LightPink,
                        BorderThickness = 2,
                        Alpha = 0.8f,
                        Children = new Drawable[]
                        {
                            innerBox = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Alpha = 0.3f,
                                Colour = Color4.Pink
                            },
                            indexText = new OsuSpriteText
                            {
                                Anchor = Anchor.Centre,
                                Origin = Anchor.Centre,
                                Font = OsuFont.Numeric.With(size: 20, weight: FontWeight.Bold),
                                Colour = Color4.White
                            }
                        }
                    },
                    approachSquare = new Container
                    {
                        Origin = Anchor.Centre,
                        Anchor = Anchor.Centre,
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        BorderThickness = 3,
                        Alpha = 0,
                        Child = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0,
                            AlwaysPresent = true
                        }
                    }
                ]
            });
        }

        private class SharedEditorCache
        {
            public EditorBeatmap Beatmap;
            public readonly Dictionary<SpaceHitObject, int> CellIndexes = new Dictionary<SpaceHitObject, int>();
            public bool IsValid;

            public void Rebuild(EditorBeatmap beatmap)
            {
                Beatmap = beatmap;
                CellIndexes.Clear();
                IsValid = true;

                if (beatmap == null) return;

                var positionCounts = new Dictionary<(float, float), int>();

                var hitObjects = beatmap.HitObjects;
                int count = hitObjects.Count;
                for (int i = 0; i < count; i++)
                {
                    if (hitObjects[i] is SpaceHitObject spaceHo)
                    {
                        var key = (spaceHo.oX, spaceHo.oY);
                        positionCounts.TryGetValue(key, out int currentCount);
                        CellIndexes[spaceHo] = currentCount;
                        positionCounts[key] = currentCount + 1;
                    }
                }
            }
        }

        private static readonly SharedEditorCache shared_cache = new();

        private int? cachedCellIndex;

        [BackgroundDependencyLoader]
        private void load()
        {
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

        private void onHitObjectChanged(osu.Game.Rulesets.Objects.HitObject hitObject)
        {
            cachedCellIndex = null;
            shared_cache.IsValid = false;
        }

        protected override void Update()
        {
            base.Update();

            if (X != HitObject.X / UI.SpacePlayfield.BASE_SIZE)
                X = HitObject.X / UI.SpacePlayfield.BASE_SIZE;

            if (Y != HitObject.Y / UI.SpacePlayfield.BASE_SIZE)
                Y = HitObject.Y / UI.SpacePlayfield.BASE_SIZE;

            if (!cachedCellIndex.HasValue)
            {
                int cellIndex = 0;
                if (editorBeatmap != null)
                {
                    if (!shared_cache.IsValid || shared_cache.Beatmap != editorBeatmap)
                    {
                        shared_cache.Rebuild(editorBeatmap);
                    }

                    if (HitObject is SpaceHitObject spaceHo && shared_cache.CellIndexes.TryGetValue(spaceHo, out int index))
                    {
                        cellIndex = index;
                    }
                }
                else if (Parent is Container<DrawableHitObject> container)
                {
                    var hitObjects = container.Children;
                    int count = hitObjects.Count;
                    for (int i = 0; i < count; i++)
                    {
                        var dho = hitObjects[i];
                        if (dho is DrawableSpaceEditorHitObject other &&
                            other.HitObject.oX == HitObject.oX &&
                            other.HitObject.oY == HitObject.oY)
                        {
                            if (other.HitObject.StartTime < HitObject.StartTime)
                                cellIndex++;
                            else if (other.HitObject.StartTime == HitObject.StartTime && other.HitObject.GetHashCode() < HitObject.GetHashCode())
                                cellIndex++;
                        }
                    }
                }

                cachedCellIndex = cellIndex;
                bool isPink = cellIndex % 2 == 0;
                borderContainer.BorderColour = isPink ? Color4.LightPink : Color4.LightCyan;
                innerBox.Colour = isPink ? Color4.Pink : Color4.Cyan;
                approachSquare.BorderColour = isPink ? Color4.LightPink : Color4.LightCyan;

                if (indexText != null)
                {
                    indexText.Text = HitObject.Index.ToString();
                    indexText.Colour = isPink ? Color4.LightPink : Color4.LightCyan;
                }
            }

            double timeRemaining = HitObject.StartTime - Time.Current;

            float targetAlpha;
            float targetApproachScale;

            if (timeRemaining > 1500)
            {
                targetAlpha = 0f;
                targetApproachScale = 2.5f;
            }
            else if (timeRemaining > 500)
            {
                targetAlpha = 1f - (float)((timeRemaining - 500) / 1000);
                targetApproachScale = 1f + (1.5f * (float)(timeRemaining / 1500));
            }
            else if (timeRemaining > 0)
            {
                targetAlpha = 1f;
                targetApproachScale = 1f + (1.5f * (float)(timeRemaining / 1500));
            }
            else if (timeRemaining > -200)
            {
                targetAlpha = 1f - (float)(-timeRemaining / 200);
                targetApproachScale = 1f + (float)(-timeRemaining / 200) * 0.15f;
            }
            else
            {
                targetAlpha = 0f;
                targetApproachScale = 1.15f;
            }

            if (content.Alpha != targetAlpha)
                content.Alpha = targetAlpha;

            if (approachSquare.Alpha != targetAlpha)
                approachSquare.Alpha = targetAlpha;

            if (approachSquare.Scale.X != targetApproachScale)
                approachSquare.Scale = new Vector2(targetApproachScale);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
                ApplyMaxResult();
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            base.UpdateHitStateTransforms(state);

            switch (state)
            {
                case ArmedState.Hit:
                case ArmedState.Miss:
                    this.FadeOut(200, Easing.OutQuint).Expire();
                    break;
            }
        }
    }
}

using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Space.Objects;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Allocation;
using osu.Game.Screens.Edit;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class DrawableSpaceEditorHitObject : DrawableHitObject<SpaceHitObject>
    {
        private Container content;
        private Container borderContainer;
        private Box innerBox;
        private Container approachSquare;

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
                Children = new Drawable[]
                {
                    borderContainer = new Container
                    {
                        RelativeSizeAxes = Axes.Both,
                        Masking = true,
                        CornerRadius = 6,
                        BorderColour = Color4.LightPink,
                        BorderThickness = 2,
                        Alpha = 0.8f,
                        Child = innerBox = new Box
                        {
                            RelativeSizeAxes = Axes.Both,
                            Alpha = 0.3f,
                            Colour = Color4.Pink
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
                }
            });
        }

        protected override void Update()
        {
            base.Update();

            X = HitObject.X / UI.SpacePlayfield.BASE_SIZE;
            Y = HitObject.Y / UI.SpacePlayfield.BASE_SIZE;

            int cellIndex = 0;
            if (editorBeatmap != null)
            {
                foreach (var ho in editorBeatmap.HitObjects)
                {
                    if (ho is SpaceHitObject other &&
                        other.oX == HitObject.oX &&
                        other.oY == HitObject.oY)
                    {
                        if (other.StartTime < HitObject.StartTime)
                            cellIndex++;
                        else if (other.StartTime == HitObject.StartTime && other.GetHashCode() < HitObject.GetHashCode())
                            cellIndex++;
                    }
                }
            }
            else if (Parent is Container<DrawableHitObject> container)
            {
                foreach (var dho in container)
                {
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

            bool isPink = cellIndex % 2 == 0;
            borderContainer.BorderColour = isPink ? Color4.LightPink : Color4.LightCyan;
            innerBox.Colour = isPink ? Color4.Pink : Color4.Cyan;
            approachSquare.BorderColour = isPink ? Color4.LightPink : Color4.LightCyan;

            double timeRemaining = HitObject.StartTime - Time.Current;

            if (timeRemaining > 0)
            {
                if (timeRemaining > 1500)
                {
                    content.Alpha = 0;
                    approachSquare.Alpha = 0;
                }
                else
                {
                    float fadeProgress = 1f - (float)(timeRemaining / 1500);
                    content.Alpha = fadeProgress;

                    float approachScale = 1f + (1.5f * (float)(timeRemaining / 1500));
                    approachSquare.Scale = new Vector2(approachScale);
                    approachSquare.Alpha = fadeProgress;
                }
            }
            else
            {
                content.Alpha = 0;
            }

        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
                ApplyMaxResult();
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
        }
    }
}

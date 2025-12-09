
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.Graphics.Textures;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Space.UI;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Graphics.Colour;

namespace osu.Game.Rulesets.Space.Objects.Drawables
{
    public partial class DrawableSpaceHitObject : DrawableHitObject<SpaceHitObject>
    {
        private Container content;
        private Box box;

        public DrawableSpaceHitObject(SpaceHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(120);
            Origin = Anchor.Centre;
            Scale = Vector2.Zero;
            Alpha = 0;
        }

        [Resolved]
        private DrawableSpaceRuleset ruleset { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = 15,
                BorderThickness = 20,
                BorderColour = Color4.White,
                Child = box = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true,
                }
            });
            double index = HitObject.StartTime;
            if (index % 3 == 1)
                content.BorderColour = Colour4.FromHex("#BADFDB");
            else if (index % 3 == 2)
                content.BorderColour = Colour4.FromHex("#FFBDBD");
            else
                content.BorderColour = Color4.White;
        }

        public override IEnumerable<HitSampleInfo> GetSamples() => new[]
        {
            new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
        };

        protected override void Update()
        {
            base.Update();

            if (Judged) return;

            double timeRemaining = HitObject.StartTime - Time.Current;

            if (timeRemaining > HitObject.TimePreempt)
            {
                Alpha = 0;
                return;
            }

            double preempt = HitObject.TimePreempt;

            const float start_z = 50f;
            const float hit_z = 1f;

            float timeProgress = (float)(timeRemaining / preempt);
            float z = hit_z + (start_z - hit_z) * timeProgress;

            if (z <= 0f)
            {
                Alpha = 0;
                return;
            }

            float scale = hit_z / z;

            Scale = new Vector2(scale);

            Vector2 targetRelative = new Vector2(HitObject.X / 512f, HitObject.Y / 384f);
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 offset = targetRelative - center;

            Position = center + offset * scale;
            RelativePositionAxes = Axes.Both;

            float fadeInStart = start_z;
            float fadeInEnd = start_z * 0.6f;

            float alpha = 1f;

            if (z > fadeInEnd)
            {
                alpha = (fadeInStart - z) / (fadeInStart - fadeInEnd);
            }

            Alpha = System.Math.Clamp(alpha, 0, 1);
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (timeOffset >= 0)
            {
                // testing only
                ApplyMaxResult();
                return;

            }
            if (Judged) return;

            bool isHit = false;

            var cursor = ruleset.Cursor?.ActiveCursor;
            if (cursor != null)
            {
                if (ScreenSpaceDrawQuad.Contains(cursor.ScreenSpaceDrawQuad.Centre))
                {
                    isHit = true;
                }
            }
            else if (IsHovered)
            {
                isHit = true;
            }

            if (isHit && timeOffset >= -HitObject.HitWindows.WindowFor(HitResult.Great) && timeOffset <= HitObject.HitWindows.WindowFor(HitResult.Great))
            {
                ApplyResult(HitResult.Great);
                return;
            }

            if (!HitObject.HitWindows.CanBeHit(timeOffset))
            {
                ApplyResult(HitResult.Miss);
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    this.ScaleTo(1.5f, 500, Easing.OutQuint).FadeOut(500, Easing.OutQuint).Expire();
                    break;

                case ArmedState.Miss:
                    this.FadeColour(Color4.Red, 500, Easing.OutQuint).FadeOut(500, Easing.InQuint).Expire();
                    break;
            }
        }
    }
}

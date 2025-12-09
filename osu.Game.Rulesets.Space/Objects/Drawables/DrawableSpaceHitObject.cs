
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Space.Mods;
using osu.Game.Rulesets.Space.UI;
using osuTK;
using osuTK.Graphics;

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

            const float camera_z = 3.75f;

            float ar = Math.Max(1f, ruleset.Beatmap.Difficulty.ApproachRate);
            float speed = ar * 10f;

            float current_dist = speed * (float)(timeRemaining / 1000);
            float z = camera_z + current_dist;

            float scale = camera_z / z;
            Scale = new Vector2(scale);

            Vector2 targetRelative = new Vector2(HitObject.X / SpacePlayfield.BASE_SIZE.X, HitObject.Y / SpacePlayfield.BASE_SIZE.Y);
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 offset = targetRelative - center;

            Position = center + offset * scale;
            RelativePositionAxes = Axes.Both;

            float spawn_distance = speed * (float)(preempt / 1000);
            float fade_in_end = speed * 0.1f;

            float alpha = 1f;
            if (current_dist > fade_in_end)
            {
                float fadeProgress = (spawn_distance - current_dist) / (spawn_distance - fade_in_end);
                alpha = MathF.Pow(Math.Clamp(fadeProgress, 0, 1), 1.3f);
            }

            Alpha = alpha;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (ruleset.Mods.Any(m => m is SpaceModAutoplay) && timeOffset >= 0)
            {
                ApplyMaxResult();
                return;
            }
            if (Judged) return;

            bool isHit = false;

            var cursor = ruleset.Playfield.Cursor?.ActiveCursor;
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

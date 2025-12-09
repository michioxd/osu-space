
using System;
using System.Collections.Generic;
using System.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Space.Configuration;
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

        private readonly Bindable<float> noteOpacity = new Bindable<float>();
        private readonly Bindable<float> noteScale = new Bindable<float>();
        private readonly Bindable<float> approachRate = new Bindable<float>();
        private readonly Bindable<float> spawnDistance = new Bindable<float>();
        private readonly Bindable<float> fadeLength = new Bindable<float>();
        private readonly Bindable<bool> doNotPushBack = new Bindable<bool>();
        private readonly Bindable<bool> halfGhost = new Bindable<bool>();
        private readonly Bindable<float> noteThickness = new Bindable<float>();
        private readonly Bindable<float> noteCornerRadius = new Bindable<float>();

        public DrawableSpaceHitObject(SpaceHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(SpacePlayfield.BASE_SIZE.X / 3f);
            Origin = Anchor.Centre;
            Scale = Vector2.Zero;
            Alpha = 0;
        }

        [Resolved]
        private DrawableSpaceRuleset ruleset { get; set; }

        [BackgroundDependencyLoader(true)]
        private void load(SpaceRulesetConfigManager config)
        {
            config?.BindWith(SpaceRulesetSetting.noteOpacity, noteOpacity);
            config?.BindWith(SpaceRulesetSetting.noteScale, noteScale);
            config?.BindWith(SpaceRulesetSetting.approachRate, approachRate);
            config?.BindWith(SpaceRulesetSetting.spawnDistance, spawnDistance);
            config?.BindWith(SpaceRulesetSetting.fadeLength, fadeLength);
            config?.BindWith(SpaceRulesetSetting.doNotPushBack, doNotPushBack);
            config?.BindWith(SpaceRulesetSetting.halfGhost, halfGhost);
            config?.BindWith(SpaceRulesetSetting.NoteThickness, noteThickness);
            config?.BindWith(SpaceRulesetSetting.NoteCornerRadius, noteCornerRadius);

            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = SpacePlayfield.BASE_SIZE.X / 3f / 3f,
                BorderThickness = SpacePlayfield.BASE_SIZE.X / 3f / 5.5f,
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

            if (Judged && Result?.Type != HitResult.Miss) return;

            // int noteIndex = HitObject.Index;

            content.BorderThickness = SpacePlayfield.BASE_SIZE.X / 3f / (11f - noteThickness.Value);
            content.CornerRadius = SpacePlayfield.BASE_SIZE.X / 3f / (11f - noteCornerRadius.Value);

            float userNoteOpacity = noteOpacity.Value;
            float userNoteScale = noteScale.Value;
            float userAr = approachRate.Value;
            float userSpawnDistance = spawnDistance.Value;
            float userFadeLength = fadeLength.Value;
            bool userDoNotPushBack = doNotPushBack.Value;
            bool userHalfGhost = halfGhost.Value;

            double timeRemaining = HitObject.StartTime - Time.Current;
            float speed = userAr;
            float current_dist = speed * (float)(timeRemaining / 1000);

            if (!Judged && current_dist > userSpawnDistance)
            {
                Alpha = 0;
                return;
            }

            if (!userDoNotPushBack && current_dist < -0.1f)
            {
                Alpha = 0;
                return;
            }

            const float camera_z = 3.75f;
            float z = camera_z + current_dist;

            if (z < 0.1f) return;

            float scale = camera_z / z;
            Scale = new Vector2(scale * userNoteScale);

            Vector2 targetRelative = new Vector2(HitObject.X / SpacePlayfield.BASE_SIZE.X, HitObject.Y / SpacePlayfield.BASE_SIZE.Y);
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 offset = targetRelative - center;

            Position = center + offset * scale;
            RelativePositionAxes = Axes.Both;

            if (!Judged)
            {
                float alpha = 1f;

                float fade_in_start = userSpawnDistance;
                float fade_in_end = userSpawnDistance * (1.0f - userFadeLength);

                if (current_dist > fade_in_end)
                {
                    float fadeProgress = (fade_in_start - current_dist) / (fade_in_start - fade_in_end);
                    alpha = MathF.Pow(Math.Clamp(fadeProgress, 0, 1), 1.3f);
                }

                if (userHalfGhost)
                {
                    float fade_out_start = (12.0f / 50f) * userAr;
                    float fade_out_end = (3.0f / 50f) * userAr;
                    float fade_out_base = 0.8f;

                    float fadeOutProgress = (current_dist - fade_out_end) / (fade_out_start - fade_out_end);
                    float fadeOutAlpha = (1 - fade_out_base) + (MathF.Pow(Math.Clamp(fadeOutProgress, 0, 1), 1.3f) * fade_out_base);

                    alpha = Math.Min(alpha, fadeOutAlpha);
                }

                Alpha = alpha * userNoteOpacity;
            }
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
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

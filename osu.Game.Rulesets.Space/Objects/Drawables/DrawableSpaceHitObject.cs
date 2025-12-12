using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Scoring;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Rulesets.Space.UI;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Extensions.PolygonExtensions;
using System.Linq;

namespace osu.Game.Rulesets.Space.Objects.Drawables
{
    public partial class DrawableSpaceHitObject : DrawableHitObject<SpaceHitObject>
    {
        private Container content;

        private readonly Bindable<float> noteOpacity = new();
        private readonly Bindable<float> noteScale = new();
        private readonly Bindable<float> approachRate = new();
        private readonly Bindable<float> spawnDistance = new();
        private readonly Bindable<float> fadeLength = new();
        private readonly Bindable<bool> doNotPushBack = new();
        private readonly Bindable<bool> halfGhost = new();
        private readonly Bindable<float> noteThickness = new();
        private readonly Bindable<float> noteCornerRadius = new();
        private readonly Bindable<SpacePalette> palette = new();
        private readonly Bindable<float> scalePlayfield = new();
        private readonly Bindable<bool> bloom = new();
        private readonly Bindable<float> bloomStrength = new();
        private readonly Bindable<float> hitWindow = new();
        public DrawableSpaceHitObject(SpaceHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(SpacePlayfield.BASE_SIZE / 3f);
            Origin = Anchor.Centre;
            Scale = Vector2.Zero;
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
            config?.BindWith(SpaceRulesetSetting.Palette, palette);
            config?.BindWith(SpaceRulesetSetting.ScalePlayfield, scalePlayfield);
            config?.BindWith(SpaceRulesetSetting.Bloom, bloom);
            config?.BindWith(SpaceRulesetSetting.BloomStrength, bloomStrength);
            config?.BindWith(SpaceRulesetSetting.HitWindow, hitWindow);

            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = SpacePlayfield.BASE_SIZE / 3f / 3f,
                BorderThickness = SpacePlayfield.BASE_SIZE / 3f / 5.5f,
                BorderColour = Color4.White,
                Child = new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Alpha = 0,
                    AlwaysPresent = true,
                }
            });

            palette.BindValueChanged(_ =>
            {
                updateColor();
                updateBloom();
            }, true);
            bloom.BindValueChanged(_ => updateBloom(), true);
            bloomStrength.BindValueChanged(_ => updateBloom(), true);
        }

        public override IEnumerable<HitSampleInfo> GetSamples() => new[]
        {
            new HitSampleInfo(HitSampleInfo.HIT_NORMAL)
        };

        private void updateColor()
        {
            var colors = SpacePaletteHelper.GetColors(palette.Value);
            content.Colour = colors[HitObject.Index % colors.Length];
        }

        private void updateBloom()
        {
            if (bloom.Value == true && bloomStrength.Value > 0)
            {
                content.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = ((Color4)content.Colour).Opacity(0.5f),
                    Radius = bloomStrength.Value * 10f,
                    Roundness = content.CornerRadius,
                    Hollow = true,
                };
            }
            else
            {
                content.EdgeEffect = default;
            }
        }

        protected override void Update()
        {
            base.Update();

            var playfield = (SpacePlayfield)ruleset.Playfield;
            float base_size = playfield.contentContainer.DrawSize.X / 3f;
            Size = new Vector2(base_size);

            content.BorderThickness = base_size / 3f / (10f - noteThickness.Value);
            content.CornerRadius = base_size / 3f / (10f - noteCornerRadius.Value);

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

            if (!userDoNotPushBack && current_dist < -0.2f)
            {
                Alpha = 0;
                return;
            }

            const float camera_z = 3.75f;
            float z = camera_z + current_dist;

            if (z < 0.1f) return;

            float rawScale = camera_z / z;

            if (rawScale >= 1f && !HitObject.IsHitOk)
            {
                playfield.spaceMiss.ShowMiss(HitObject.col, HitObject.row);
            }

            if (rawScale >= 2f && HitObject.row == 1 || (rawScale >= 1f && HitObject.IsHitOk))
            {
                Alpha = 0;
                Scale = new Vector2(2f);
                return;
            }

            Scale = new Vector2(rawScale * userNoteScale);

            Vector2 targetRelative = new Vector2((HitObject.col + 0.5f) / 3f, (HitObject.row + 0.5f) / 3f);
            Vector2 center = new Vector2(0.5f, 0.5f);
            Vector2 offset = targetRelative - center;

            Position = center + offset * rawScale;
            RelativePositionAxes = Axes.Both;


            float alpha = 1f;

            float fade_in_start = userSpawnDistance;
            float fade_in_end = userSpawnDistance - (userFadeLength * userAr);

            if (current_dist > fade_in_end)
            {
                float fadeProgress = (fade_in_start - current_dist) / (fade_in_start - fade_in_end);
                alpha = MathF.Pow(Math.Clamp(fadeProgress, 0, 1), 1.3f);
            }

            if (userHalfGhost)
            {
                float fade_out_start = 12.0f / 50f * userAr;
                float fade_out_end = 3.0f / 50f * userAr;
                float fade_out_base = 0.8f;

                float fadeOutProgress = (current_dist - fade_out_end) / (fade_out_start - fade_out_end);
                float fadeOutAlpha = 1 - fade_out_base + (MathF.Pow(Math.Clamp(fadeOutProgress, 0, 1), 1.3f) * fade_out_base);

                alpha = Math.Min(alpha, fadeOutAlpha);
            }

            Alpha = alpha * userNoteOpacity;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Judged) return;

            bool isHit = false;

            var cursor = ruleset.Playfield.Cursor?.ActiveCursor;
            if (cursor != null)
            {
                var playfield = (SpacePlayfield)ruleset.Playfield;
                float cellSize = SpacePlayfield.BASE_SIZE / 3f;

                float cellX = HitObject.X - cellSize / 2;
                float cellY = HitObject.Y - cellSize / 2;

                Vector2 tl = playfield.GamefieldToScreenSpace(new Vector2(cellX, cellY));
                Vector2 tr = playfield.GamefieldToScreenSpace(new Vector2(cellX + cellSize, cellY));
                Vector2 bl = playfield.GamefieldToScreenSpace(new Vector2(cellX, cellY + cellSize));
                Vector2 br = playfield.GamefieldToScreenSpace(new Vector2(cellX + cellSize, cellY + cellSize));

                var cellQuad = new Quad(tl, tr, bl, br);

                if (cellQuad.Intersects(cursor.ScreenSpaceDrawQuad))
                {
                    isHit = true;
                }
            }

            if (!HitObject.HitWindows.CanBeHit(timeOffset))
            {
                ApplyResult(HitResult.Miss);
                return;
            }

            if (isHit && timeOffset >= -hitWindow.Value && timeOffset <= hitWindow.Value)
            {
                ApplyResult(HitResult.Perfect);
                HitObject.IsHitOk = true;
                return;
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Miss:
                    this.FadeOut(0, Easing.OutQuint).Expire();
                    break;
            }
        }
    }
}

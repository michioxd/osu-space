using System;
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Game.Audio;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Rulesets.Space.UI;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Extensions.Color4Extensions;
using osu.Framework.Graphics.Effects;
using osu.Framework.Graphics.Primitives;
using osu.Framework.Extensions.PolygonExtensions;

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
        private readonly Bindable<bool> glow = new();
        private readonly Bindable<float> glowStrength = new();
        private readonly Bindable<float> hitWindow = new();

        private SpacePlayfield cachedPlayfield;
        private float cachedTargetRelX;
        private float cachedTargetRelY;
        private float cachedOffsetX;
        private float cachedOffsetY;
        private float cachedCellLeft;
        private float cachedCellTop;
        private float cachedCellRight;
        private float cachedCellBottom;
        private float cachedNoteOpacity;
        private float cachedNoteScale;
        private float cachedAr;
        private float cachedSpawnDist;
        private float cachedFadeLen;
        private bool cachedDoNotPushBack;
        private bool cachedHalfGhost;
        private float cachedHitWindow;
        private const float camera_z = 3.75f;
        private static readonly float cell_size = SpacePlayfield.BASE_SIZE / 3f;
        private const float inv3 = 1f / 3f;

        public DrawableSpaceHitObject(SpaceHitObject hitObject)
            : base(hitObject)
        {
            Size = new Vector2(cell_size);
            Origin = Anchor.Centre;
            Scale = Vector2.Zero;

            cacheHitObjectGeometry(hitObject);
        }

        private void cacheHitObjectGeometry(SpaceHitObject ho)
        {
            cachedTargetRelX = (ho.oX + 0.5f) * inv3;
            cachedTargetRelY = (ho.oY + 0.5f) * inv3;
            cachedOffsetX = cachedTargetRelX - 0.5f;
            cachedOffsetY = cachedTargetRelY - 0.5f;

            cachedCellLeft = ho.X - cell_size * 0.5f;
            cachedCellTop = ho.Y - cell_size * 0.5f;
            cachedCellRight = cachedCellLeft + cell_size;
            cachedCellBottom = cachedCellTop + cell_size;
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
            config?.BindWith(SpaceRulesetSetting.Glow, glow);
            config?.BindWith(SpaceRulesetSetting.GlowStrength, glowStrength);
            config?.BindWith(SpaceRulesetSetting.HitWindow, hitWindow);

            AddInternal(content = new Container
            {
                RelativeSizeAxes = Axes.Both,
                Masking = true,
                CornerRadius = cell_size / 3f,
                BorderThickness = cell_size / 5.5f,
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
                updateGlow();
            }, true);
            glow.BindValueChanged(_ => updateGlow(), true);
            glowStrength.BindValueChanged(_ => updateGlow(), true);

            noteOpacity.BindValueChanged(e => cachedNoteOpacity = e.NewValue, true);
            noteScale.BindValueChanged(e => cachedNoteScale = e.NewValue, true);
            approachRate.BindValueChanged(e => cachedAr = e.NewValue, true);
            spawnDistance.BindValueChanged(e => cachedSpawnDist = e.NewValue, true);
            fadeLength.BindValueChanged(e => cachedFadeLen = e.NewValue, true);
            doNotPushBack.BindValueChanged(e => cachedDoNotPushBack = e.NewValue, true);
            halfGhost.BindValueChanged(e => cachedHalfGhost = e.NewValue, true);
            hitWindow.BindValueChanged(e => cachedHitWindow = e.NewValue, true);
            noteThickness.BindValueChanged(_ => updateContentShape(), true);
            noteCornerRadius.BindValueChanged(_ => updateContentShape(), true);
        }

        private float lastBaseSize;

        private void updateContentShape()
        {
            if (lastBaseSize <= 0) return;

            float unit = lastBaseSize * inv3;
            content.BorderThickness = unit / (10f - noteThickness.Value);
            content.CornerRadius = unit / (10f - noteCornerRadius.Value);
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

        private void updateGlow()
        {
            if (glow.Value && glowStrength.Value > 0)
            {
                content.EdgeEffect = new EdgeEffectParameters
                {
                    Type = EdgeEffectType.Glow,
                    Colour = ((Color4)content.Colour).Opacity(0.5f),
                    Radius = glowStrength.Value * 10f,
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

            cachedPlayfield ??= (SpacePlayfield)ruleset.Playfield;
            float currentOX = HitObject.oX, currentOY = HitObject.oY;
            float expectedRelX = (currentOX + 0.5f) * inv3;

            if (Math.Abs(expectedRelX - cachedTargetRelX) > 0.001f ||
                Math.Abs((currentOY + 0.5f) * inv3 - cachedTargetRelY) > 0.001f)
            {
                cacheHitObjectGeometry(HitObject);
            }

            float baseSize = cachedPlayfield.contentContainer.DrawSize.X * inv3;

            if (Math.Abs(baseSize - lastBaseSize) > 0.01f)
            {
                lastBaseSize = baseSize;
                Size = new Vector2(baseSize);
                updateContentShape();
            }

            double timeRemaining = HitObject.StartTime - Time.Current;
            float currentDist = cachedAr * (float)((timeRemaining + cachedHitWindow) / 1000.0);

            if (!Judged && currentDist > cachedSpawnDist)
            {
                Alpha = 0;
                return;
            }

            if (!cachedDoNotPushBack && currentDist < -0.2f)
            {
                Alpha = 0;
                return;
            }

            float z = camera_z + currentDist;

            if (z < 0.1f)
            {
                Alpha = 0;
                return;
            }

            float rawScale = camera_z / z;
            float finalScale = rawScale * cachedNoteScale;

            Scale = new Vector2(finalScale);

            Position = new Vector2(
                0.5f + cachedOffsetX * rawScale,
                0.5f + cachedOffsetY * rawScale
            );
            RelativePositionAxes = Axes.Both;

            float alpha = 1f;
            float fadeInEnd = cachedSpawnDist - cachedFadeLen * cachedAr;

            if (currentDist > fadeInEnd)
            {
                float range = cachedSpawnDist - fadeInEnd;
                float fadeProgress = (cachedSpawnDist - currentDist) / range;
                alpha = MathF.Pow(Math.Clamp(fadeProgress, 0f, 1f), 1.3f);
            }

            if (cachedHalfGhost)
            {
                float fadeOutStart = 0.24f * cachedAr;
                float fadeOutEnd = 0.06f * cachedAr;
                const float fade_out_base = 0.8f;

                float fadeOutProgress = (currentDist - fadeOutEnd) / (fadeOutStart - fadeOutEnd);
                float fadeOutAlpha = 1f - fade_out_base + MathF.Pow(Math.Clamp(fadeOutProgress, 0f, 1f), 1.3f) * fade_out_base;

                if (fadeOutAlpha < alpha)
                    alpha = fadeOutAlpha;
            }

            alpha *= cachedNoteOpacity;

            if ((rawScale >= 2f && currentOX >= 1f && currentOX <= 1.5f) ||
                (rawScale >= 1f && HitObject.IsHitOk))
            {
                Alpha = 0;
                Scale = new Vector2(2f);
                return;
            }

            Alpha = alpha;
        }

        protected override void CheckForResult(bool userTriggered, double timeOffset)
        {
            if (Judged) return;

            if (!HitObject.HitWindows.CanBeHit(timeOffset) || timeOffset > cachedHitWindow)
            {
                HitObject.IsHitOk = false;
                ApplyMinResult();
                return;
            }

            if (timeOffset < -cachedHitWindow || timeOffset > cachedHitWindow)
                return;

            HitObject.IsHitOk = false;

            var cursor = cachedPlayfield?.Cursor?.ActiveCursor;

            if (cursor == null)
                return;

            Vector2 tl = cachedPlayfield.GamefieldToScreenSpace(new Vector2(cachedCellLeft, cachedCellTop));
            Vector2 tr = cachedPlayfield.GamefieldToScreenSpace(new Vector2(cachedCellRight, cachedCellTop));
            Vector2 bl = cachedPlayfield.GamefieldToScreenSpace(new Vector2(cachedCellLeft, cachedCellBottom));
            Vector2 br = cachedPlayfield.GamefieldToScreenSpace(new Vector2(cachedCellRight, cachedCellBottom));

            if (new Quad(tl, tr, bl, br).Intersects(cursor.ScreenSpaceDrawQuad))
            {
                ApplyMaxResult();
                HitObject.IsHitOk = true;
            }
        }

        protected override void UpdateHitStateTransforms(ArmedState state)
        {
            switch (state)
            {
                case ArmedState.Hit:
                    this.FadeOut(100, Easing.OutQuint).Expire();
                    break;

                case ArmedState.Miss:
                    this.FadeOut(50, Easing.OutQuint).Expire();
                    break;
            }
        }
    }
}

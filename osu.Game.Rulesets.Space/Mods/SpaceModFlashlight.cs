// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osuTK;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Input;
using osu.Framework.Input.Events;
using osu.Framework.Utils;
using osu.Game.Configuration;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Rulesets.Space.Objects;

namespace osu.Game.Rulesets.Space.Mods;

public partial class SpaceModFlashlight : ModFlashlight<SpaceHitObject>
{
    public override double ScoreMultiplier => UsesDefaultConfiguration ? 1.12 : 1;

    private const double default_follow_delay = 120;

    [SettingSource("Follow delay", "Milliseconds until the flashlight reaches the cursor")]
    public BindableNumber<double> FollowDelay { get; } = new BindableDouble(default_follow_delay)
    {
        MinValue = default_follow_delay,
        MaxValue = default_follow_delay * 10,
        Precision = default_follow_delay,
    };

    public override BindableFloat SizeMultiplier { get; } = new BindableFloat(1)
    {
        MinValue = 0.5f,
        MaxValue = 2f,
        Precision = 0.1f
    };

    public override BindableBool ComboBasedSize { get; } = new BindableBool(true);

    public override float DefaultFlashlightSize => 125;

    private SpaceFlashlight flashlight = null!;

    protected override Flashlight CreateFlashlight() => flashlight = new SpaceFlashlight(this);

    private partial class SpaceFlashlight : Flashlight, IRequireHighFrequencyMousePosition
    {
        private readonly double followDelay;

        public SpaceFlashlight(SpaceModFlashlight modFlashlight)
            : base(modFlashlight)
        {
            followDelay = modFlashlight.FollowDelay.Value;

            FlashlightSize = new Vector2(0, GetSize());
            FlashlightSmoothness = 1.4f;
        }

        protected override bool OnMouseMove(MouseMoveEvent e)
        {
            var position = FlashlightPosition;
            var destination = e.MousePosition;

            FlashlightPosition = Interpolation.ValueAt(
                Math.Min(Math.Abs(Clock.ElapsedFrameTime), followDelay), position, destination, 0, followDelay, Easing.Out);

            return base.OnMouseMove(e);
        }

        protected override void UpdateFlashlightSize(float size)
        {
            float fieldSize = SpaceRulesetConfigManager.FieldSize?.Value ?? 0.6f;
            this.TransformTo(nameof(FlashlightSize), new Vector2(0, size * (fieldSize + 0.1f)), FLASHLIGHT_FADE_DURATION);
        }

        protected override string FragmentShader => "CircularFlashlight";
    }
}

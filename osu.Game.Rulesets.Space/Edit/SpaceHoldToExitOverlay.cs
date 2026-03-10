using System;
using osu.Framework.Allocation;
using osu.Framework.Audio;
using osu.Framework.Audio.Sample;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Bindings;
using osu.Framework.Input.Events;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Game.Input.Bindings;
using osu.Game.Screens.Edit;
using osu.Framework.Screens;
using osu.Framework.Utils;
using osuTK;
using osuTK.Graphics;
using osu.Framework.Logging;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class SpaceHoldToExitOverlay : HoldToConfirmContainer, IKeyBindingHandler<GlobalAction>
    {
        private const double hold_duration = 2000;

        protected override bool AllowMultipleFires => true;

        private double lastTickPlaybackTime;

        private Box overlay;
        private Container textContainer;
        private readonly BindableDouble audioVolume = new BindableDouble(1);

        private Sample tickSample;
        private int lastDisplayedSecond = -1;

        private Box progressBarFill;

        [Resolved]
        private AudioManager audio { get; set; }

        [Resolved]
        private Editor editor { get; set; }

        public SpaceHoldToExitOverlay()
            : base(isDangerousAction: true)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            RelativeSizeAxes = Axes.Both;
            AlwaysPresent = true;

            Action = () => editor.Exit();

            tickSample = audio.Samples.Get(@"UI/dialog-dangerous-tick");

            Children = new Drawable[]
            {
                overlay = new Box
                {
                    Alpha = 0,
                    Colour = Color4.Black,
                    RelativeSizeAxes = Axes.Both,
                },
                textContainer = new Container
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    AutoSizeAxes = Axes.Both,
                    Alpha = 0,
                    Children = new Drawable[]
                    {
                        new Container
                        {
                            RelativeSizeAxes = Axes.Both,
                            Masking = true,
                            CornerRadius = 10,
                            Child = new Box
                            {
                                RelativeSizeAxes = Axes.Both,
                                Colour = Color4.Black,
                                Alpha = 0.6f,
                            }
                        },
                        new FillFlowContainer
                        {
                            AutoSizeAxes = Axes.Both,
                            Direction = FillDirection.Vertical,
                            Padding = new MarginPadding { Horizontal = 24, Vertical = 12 },
                            Spacing = new Vector2(0, 8),
                            Children = new Drawable[]
                            {
                                new Container
                                {
                                    AutoSizeAxes = Axes.Both,
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    Children = new Drawable[]
                                    {
                                        new OsuSpriteText
                                        {
                                            Anchor = Anchor.Centre,
                                            Origin = Anchor.Centre,
                                            Font = OsuFont.Torus.With(size: 28, weight: FontWeight.Bold),
                                            Colour = Color4.White,
                                            Text = "Hold to exit"
                                        }
                                    }
                                },
                                new Container
                                {
                                    Anchor = Anchor.TopCentre,
                                    Origin = Anchor.TopCentre,
                                    RelativeSizeAxes = Axes.None,
                                    Width = 280,
                                    Height = 4,
                                    Masking = true,
                                    CornerRadius = 2,
                                    Children = new Drawable[]
                                    {
                                        new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4.White,
                                            Alpha = 0.2f,
                                        },
                                        progressBarFill = new Box
                                        {
                                            RelativeSizeAxes = Axes.Both,
                                            Colour = Color4.White,
                                            Width = 0,
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };

            Progress.ValueChanged += p =>
            {
                double target = p.NewValue * 0.8;
                audioVolume.Value = 1 - target;
                overlay.Alpha = (float)target;

                if (!(Clock.CurrentTime - lastTickPlaybackTime < 40))
                {
                    var channel = tickSample.GetChannel();
                    channel.Frequency.Value = 1 + p.NewValue;
                    channel.Volume.Value = 0.1f + p.NewValue / 2f;

                    channel.Play();
                    lastTickPlaybackTime = Clock.CurrentTime;
                }

                if (p.NewValue > 0 && p.NewValue < 1)
                {
                    int secondsLeft = (int)Math.Ceiling(hold_duration / 1000.0 * (1.0 - p.NewValue));
                    secondsLeft = Math.Max(secondsLeft, 0);

                    progressBarFill.ResizeWidthTo((float)p.NewValue, 100, Easing.OutQuint);

                    if (p.NewValue > 0.2)
                    {
                        float t = (float)((p.NewValue - 0.2) / 0.8);
                        progressBarFill.FadeColour(new Color4(
                            1f,
                            1f - t * 0.7f,
                            1f - t * 0.7f,
                            1f
                        ), 100, Easing.OutQuint);
                    }
                    else
                        progressBarFill.FadeColour(Color4.White, 100, Easing.OutQuint);

                    if (secondsLeft != lastDisplayedSecond)
                        lastDisplayedSecond = secondsLeft;
                    if (p.NewValue > 0.6)
                    {
                        float shakeIntensity = (float)((p.NewValue - 0.6) / 0.4);
                        float amplitude = 8 * shakeIntensity;
                        textContainer.MoveTo(new Vector2(
                            RNG.NextSingle(-amplitude, amplitude),
                            RNG.NextSingle(-amplitude, amplitude)
                        ), 30, Easing.OutQuint);
                    }
                    else
                        textContainer.MoveTo(Vector2.Zero, 100, Easing.OutQuint);

                    textContainer.FadeIn(300, Easing.OutQuint);
                }
                else
                {
                    textContainer.MoveTo(Vector2.Zero, 100, Easing.OutQuint);
                    textContainer.FadeOut(300, Easing.OutQuint);
                    progressBarFill.ResizeWidthTo(0, 300, Easing.OutQuint);
                    lastDisplayedSecond = -1;
                }
            };

            audio.Tracks.AddAdjustment(AdjustableProperty.Volume, audioVolume);
        }

        protected override void LoadComplete()
        {
            base.LoadComplete();

            ((Bindable<double>)HoldActivationDelay).Value = hold_duration;
        }

        protected override void AbortConfirm()
        {
            base.AbortConfirm();
            lastDisplayedSecond = -1;
        }

        public bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Repeat)
                return false;

            if (e.Action == GlobalAction.Back)
            {
                BeginConfirm();
                return true;
            }

            return false;
        }

        public void OnReleased(KeyBindingReleaseEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.Back && !Fired)
                AbortConfirm();
        }

        protected override void Dispose(bool isDisposing)
        {
            tickSample?.Dispose();
            audio?.Tracks.RemoveAdjustment(AdjustableProperty.Volume, audioVolume);
            base.Dispose(isDisposing);
        }
    }
}

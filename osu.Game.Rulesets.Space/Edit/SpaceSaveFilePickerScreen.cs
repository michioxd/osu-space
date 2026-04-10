using System;
using System.Collections.Generic;
using System.IO;
using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Input.Events;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.Sprites;
using osu.Game.Graphics.UserInterface;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Input.Bindings;
using osu.Game.Overlays;
using osu.Game.Overlays.Settings;
using osu.Game.Screens.OnlinePlay.Match.Components;
using osuTK;
using osuTK.Input;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class SpaceSaveFilePickerScreen : OsuFocusedOverlayContainer
    {
        private readonly Action<string> onPathSelected;
        private readonly Func<string> defaultFileNameProvider;
        private readonly bool isImport;
        private readonly Func<bool> showDirectSaveButtonProvider;
        private readonly Action onDirectSaveRequested;

        protected override bool DimMainContent => false;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(
            OverlayColourScheme.Blue
        );
        private Container contentContainer = null!;
        private Container selectorHost = null!;
        private Container loadingContainer = null!;
        private LoadingSpinner loadingSpinner = null!;
        private RoundedButton saveButton = null!;
        private FormTextBox fileNameTxt = null!;

        private const float duration = 250;
        private static readonly string[] import_file_extensions = { ".osu", ".sspm", ".txt" };

        private OsuDirectorySelector directorySelector;
        private OsuFileSelector fileSelector;
        private bool selectorLoadRequested;

        public SpaceSaveFilePickerScreen(
            Action<string> onPathSelected,
            Func<string> defaultFileNameProvider,
            bool isImport = false,
            Func<bool> showDirectSaveButtonProvider = null,
            Action onDirectSaveRequested = null
        )
        {
            this.onPathSelected = onPathSelected;
            this.defaultFileNameProvider = defaultFileNameProvider;
            this.isImport = isImport;
            this.showDirectSaveButtonProvider = showDirectSaveButtonProvider;
            this.onDirectSaveRequested = onDirectSaveRequested;
            RelativeSizeAxes = Axes.Both;
            Depth = float.MinValue;
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            InternalChildren = new Drawable[]
            {
                new Box
                {
                    RelativeSizeAxes = Axes.Both,
                    Colour = Colour4.Black,
                    Alpha = 0.5f,
                },
                contentContainer = new Container
                {
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.6f, 0.9f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colourProvider.Background5,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            Padding = new MarginPadding(0),
                            RowDimensions = new[]
                            {
                                new Dimension(GridSizeMode.AutoSize),
                                new Dimension(GridSizeMode.Relative, 0.68f),
                                new Dimension(GridSizeMode.AutoSize),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Padding = new MarginPadding(20),
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = isImport
                                            ? "Please select a beatmap file to import:"
                                            : "Please select where to save this beatmap:",
                                        Font = OsuFont.Default.With(size: 20),
                                    },
                                },
                                new Drawable[]
                                {
                                    selectorHost = new Container
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                        Children = new Drawable[]
                                        {
                                            loadingContainer = new Container
                                            {
                                                RelativeSizeAxes = Axes.Both,
                                                Children = new Drawable[]
                                                {
                                                    loadingSpinner = new LoadingSpinner
                                                    {
                                                        Anchor = Anchor.Centre,
                                                        Origin = Anchor.Centre,
                                                    },
                                                },
                                            },
                                        },
                                    },
                                },
                                new Drawable[] { createFooter() },
                            },
                        },
                    },
                },
            };

            loadingSpinner.Show();

            if (!isImport)
                fileNameTxt.Current.BindValueChanged(_ => updateActionButtonState(), true);

            updateActionButtonState();
        }

        protected override void Update()
        {
            base.Update();
        }

        protected override void PopIn()
        {
            if (!selectorLoadRequested)
            {
                selectorLoadRequested = true;
                loadSelector();
            }

            if (!isImport)
                fileNameTxt.Current.Value = defaultFileNameProvider?.Invoke() ?? string.Empty;

            contentContainer.ScaleTo(0.96f).ScaleTo(1, duration, Easing.OutQuint);
            this.FadeInFromZero(duration);
        }

        protected override void PopOut()
        {
            contentContainer.ScaleTo(0.98f, duration, Easing.OutQuint);
            this.FadeOut(duration, Easing.OutQuint);
        }

        protected override bool OnKeyDown(KeyDownEvent e)
        {
            if (!e.Repeat && e.Key == Key.Escape)
            {
                Hide();
                return true;
            }

            return base.OnKeyDown(e);
        }

        private void confirmSelection()
        {
            if (isImport)
            {
                string selectedFile = fileSelector?.CurrentFile.Value?.FullName;

                if (string.IsNullOrWhiteSpace(selectedFile))
                    return;

                onPathSelected(selectedFile);
                Hide();
                return;
            }

            string fileName = fileNameTxt.Current.Value;
            string path = directorySelector?.CurrentPath.Value?.ToString();

            if (string.IsNullOrWhiteSpace(fileName) || string.IsNullOrWhiteSpace(path))
                return;

            onPathSelected(Path.Combine(path, fileName));
            Hide();
        }

        private Drawable createFooter()
        {
            List<Drawable> children = new List<Drawable>();

            if (!isImport)
            {
                children.Add(
                    new SettingsItemV2(
                        fileNameTxt = new FormTextBox
                        {
                            Caption = "File name",
                            RelativeSizeAxes = Axes.X,
                            Anchor = Anchor.Centre,
                            Origin = Anchor.Centre,
                        }
                    )
                );
            }

            List<Drawable> actionButtons = new List<Drawable>();

            actionButtons.Add(
                new PurpleRoundedButton
                {
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Width = 80,
                    Text = "Cancel",
                    Action = Hide,
                }
            );

            saveButton = new RoundedButton
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                Width = 80,
                Text = isImport ? "Import" : "Save",
                Action = confirmSelection,
                Enabled = { Value = false },
            };
            actionButtons.Add(saveButton);

            if (!isImport && (showDirectSaveButtonProvider?.Invoke() ?? false))
            {
                actionButtons.Add(
                    new PurpleRoundedButton
                    {
                        Anchor = Anchor.Centre,
                        Origin = Anchor.Centre,
                        Width = 240,
                        Text = "Save directly (NOT RECOMMENDED)",
                        Action = () =>
                        {
                            Hide();
                            onDirectSaveRequested?.Invoke();
                        },
                    }
                );
            }

            children.Add(
                new FillFlowContainer
                {
                    AutoSizeAxes = Axes.Y,
                    Direction = FillDirection.Horizontal,
                    RelativeSizeAxes = Axes.X,
                    Spacing = new Vector2(10),
                    Children = actionButtons.ToArray(),
                }
            );

            return new FillFlowContainer
            {
                Anchor = Anchor.Centre,
                Origin = Anchor.Centre,
                AutoSizeAxes = Axes.Y,
                Direction = FillDirection.Vertical,
                RelativeSizeAxes = Axes.X,
                Spacing = new Vector2(10),
                Children = children.ToArray(),
            };
        }

        private void loadSelector()
        {
            if (isImport)
                loadFileSelector();
            else
                loadDirectorySelector();
        }

        private void loadDirectorySelector()
        {
            if (directorySelector != null)
                return;

            saveButton.Enabled.Value = false;

            LoadComponentAsync(
                new OsuDirectorySelector { RelativeSizeAxes = Axes.Both },
                selector =>
                {
                    Schedule(() =>
                    {
                        if (IsDisposed)
                            return;

                        selectorHost.Child = directorySelector = selector;
                        directorySelector.CurrentPath.BindValueChanged(
                            _ => updateActionButtonState(),
                            true
                        );
                        loadingContainer.Hide();
                        updateActionButtonState();
                    });
                }
            );
        }

        private void loadFileSelector()
        {
            if (fileSelector != null)
                return;

            saveButton.Enabled.Value = false;

            LoadComponentAsync(
                new OsuFileSelector(validFileExtensions: import_file_extensions)
                {
                    RelativeSizeAxes = Axes.Both,
                },
                selector =>
                {
                    Schedule(() =>
                    {
                        if (IsDisposed)
                            return;

                        selectorHost.Child = fileSelector = selector;
                        fileSelector.CurrentFile.BindValueChanged(
                            _ => updateActionButtonState(),
                            true
                        );
                        loadingContainer.Hide();
                        updateActionButtonState();
                    });
                }
            );
        }

        private void updateActionButtonState()
        {
            if (saveButton == null)
                return;

            if (isImport)
            {
                saveButton.Enabled.Value = fileSelector?.CurrentFile.Value != null;
                return;
            }

            saveButton.Enabled.Value =
                directorySelector?.CurrentPath.Value != null
                && !string.IsNullOrWhiteSpace(fileNameTxt?.Current.Value);
        }

        public override bool OnPressed(KeyBindingPressEvent<GlobalAction> e)
        {
            if (e.Action == GlobalAction.Select && saveButton.Enabled.Value)
            {
                confirmSelection();
                return true;
            }

            return base.OnPressed(e);
        }
    }
}

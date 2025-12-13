#nullable enable

using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Screens;
using osu.Game.Graphics;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Overlays;
using osu.Game.Screens;
using osuTK;
using osu.Framework.Platform;
using osu.Game.Overlays.Notifications;
using osu.Game.Graphics.Sprites;
using osu.Framework.Graphics.Sprites;
using System.Threading.Tasks;
using osu.Game.Beatmaps;
using System;
using osu.Game.Overlays.Dialog;
using System.Collections.Immutable;

namespace osu.Game.Rulesets.Space.Extension.SSPM
{
    public partial class SSPMImportScreen : OsuScreen
    {
        public override bool DisallowExternalBeatmapRulesetChanges => true;
        public override bool HideOverlaysOnEnter => true;

        [Cached]
        private OverlayColourProvider colourProvider = new OverlayColourProvider(OverlayColourScheme.Blue);

        [Resolved]
        private OsuColour? colours { get; set; }

        [Resolved]
        private Storage? storage { get; set; }

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private OsuGame? game { get; set; }

        [Resolved]
        private BeatmapManager? beatmapManager { get; set; }

        [Resolved]
        private RulesetStore? rulesets { get; set; }

        private OsuDirectorySelector? directorySelector;

        [BackgroundDependencyLoader]
        private void load()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string userHomePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            string initialPath = userHomePath;

            string sspMapsPath = System.IO.Path.Combine(appData, "SoundSpacePlus", "maps");

            if (System.IO.Directory.Exists(sspMapsPath))
            {
                dialogOverlay?.Push(new ImportConfirmationDialog(sspMapsPath, () => startImport(sspMapsPath), () => { }));
            }

            InternalChildren =
            [
                new Container
                {
                    Masking = true,
                    CornerRadius = 10,
                    RelativeSizeAxes = Axes.Both,
                    Anchor = Anchor.Centre,
                    Origin = Anchor.Centre,
                    Size = new Vector2(0.5f, 0.8f),
                    Children = new Drawable[]
                    {
                        new Box
                        {
                            Colour = colours.GreySeaFoamDark,
                            RelativeSizeAxes = Axes.Both,
                        },
                        new GridContainer
                        {
                            RelativeSizeAxes = Axes.Both,
                            RowDimensions = new[]
                            {
                                new Dimension(),
                                new Dimension(GridSizeMode.Relative, 0.8f),
                                new Dimension(),
                            },
                            Content = new[]
                            {
                                new Drawable[]
                                {
                                    new OsuSpriteText
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Text = "Please select a folder containing .sspm files",
                                        Font = OsuFont.Default.With(size: 20)
                                    },
                                },
                                new Drawable[]
                                {
                                    directorySelector = new OsuDirectorySelector(initialPath)
                                    {
                                        RelativeSizeAxes = Axes.Both,
                                    }
                                },
                                new Drawable[]
                                {
                                    new FillFlowContainer
                                    {
                                        Anchor = Anchor.Centre,
                                        Origin = Anchor.Centre,
                                        Direction = FillDirection.Horizontal,
                                        Spacing = new Vector2(20),
                                        Children = new Drawable[]
                                        {
                                            new RoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 300,
                                                Text = "Import",
                                                Action = import
                                            },
                                        }
                                    }
                                }
                            }
                        }
                    },
                }
            ];
        }

        private void import() => startImport(directorySelector?.CurrentPath.Value?.FullName);

        private void startImport(string? path)
        {
            if (string.IsNullOrEmpty(path) || !System.IO.Directory.Exists(path))
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "Please select a valid directory.",
                    Icon = FontAwesome.Solid.ExclamationTriangle,
                });
                return;
            }

            var notification = new ProgressNotification
            {
                Text = "Importing Sound Space Plus map files...",
                CompletionText = "Import Sound Space Plus map complete!",
                State = ProgressNotificationState.Active,
            };
            notifications?.Post(notification);

            Task.Run(() =>
            {
                var importer = new SSPMConverter(beatmapManager!, rulesets!);
                importer.ImportFromDirectory(path, notification.CancellationToken, (current, total, failed, done, noFile) =>
                {
                    if (notification.State == ProgressNotificationState.Cancelled)
                        return;

                    if (noFile)
                    {
                        notification.State = ProgressNotificationState.Cancelled;
                        notification.Text = "No .sspm files found to import.";
                        return;
                    }

                    notification.Text = $"Importing Sound Space Plus map files ({current}/{total})...";
                    notification.Progress = (float)current / total;
                    if (done)
                    {
                        notification.State = ProgressNotificationState.Completed;
                        notification.Text = failed > 0 ?
                            $"Import completed with {failed} failed imports." :
                            "Import completed successfully!";
                    }
                });
            });

            this.Exit();
        }

        private partial class ImportConfirmationDialog : PopupDialog
        {
            public ImportConfirmationDialog(string path, Action onConfirm, Action onCancel)
            {
                HeaderText = "Sound Space Plus maps folder detected";
                BodyText = $"We found a maps folder at:\n{path}\nDo you want to import from here?";
                Icon = FontAwesome.Solid.QuestionCircle;
                Buttons =
                [
                    new PopupDialogOkButton
                    {
                        Text = "Yes, import these maps",
                        Action = onConfirm
                    },
                    new PopupDialogCancelButton
                    {
                        Text = "No, I'll select manually",
                        Action = onCancel
                    }
                ];
            }
        }
    }
}

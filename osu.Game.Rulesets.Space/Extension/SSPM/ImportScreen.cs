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
using osu.Game.Database;
using System.Linq;
using System.Drawing;
using osu.Game.Screens.OnlinePlay.Match.Components;
using System.IO;
using osu.Game.Screens.SelectV2;

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

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        private OsuDirectorySelector? directorySelector;

        [BackgroundDependencyLoader]
        private void load()
        {

            realm.Write(r =>
            {
                var badRulesets = r.All<RulesetInfo>().Where(rs => rs.ShortName == "osuspaceruleset" && rs.OnlineID != 727);
                if (badRulesets.Any())
                {
                    r.RemoveRange(badRulesets);
                    dialogOverlay?.Push(new ConfirmRebootToApply(() =>
                    {
                        this.Exit();
                    }));
                }
            });

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
                                    directorySelector = new OsuDirectorySelector
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
                                                Width = 150,
                                                Text = "Import",
                                                Action = import
                                            },
                                            new PurpleRoundedButton
                                            {
                                                Anchor = Anchor.Centre,
                                                Origin = Anchor.Centre,
                                                Width = 300,
                                                Text = "Try to locate the Rhythia (SSP) folder",
                                                Action = scanAndImportFromSSP
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

        private void scanAndImportFromSSP()
        {
            string appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
            string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string[] candidatePaths =
            [
                Path.Combine(appData, "SoundSpacePlus", "maps"),
                Path.Combine(localAppData, "SoundSpacePlus", "maps")
            ];

            foreach (string path in candidatePaths)
            {
                if (tryCheckThePath(path))
                {
                    dialogOverlay?.Push(new ImportConfirmationDialog(path, () => startImport(path), () => { }));
                    return;
                }
            }

            notifications?.Post(new SimpleNotification
            {
                Text = "No Sound Space Plus maps folder was detected, or no .sspm files were found inside that. Please select the folder manually.",
                Icon = FontAwesome.Solid.ExclamationTriangle,
            });
        }

        private bool tryCheckThePath(string path)
        {
            if (!Directory.Exists(path))
                return false;
            string[] files = Directory.GetFiles(path, "*.sspm");
            if (files.Length > 0)
            {
                return true;
            }
            return false;
        }

        private void startImport(string? path)
        {
            if (string.IsNullOrEmpty(path) || !Directory.Exists(path))
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "Please select a valid directory.",
                    Icon = FontAwesome.Solid.ExclamationTriangle,
                });
                return;
            }

            if (!tryCheckThePath(path))
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "No .sspm files found in the selected directory. Please select a different folder.",
                    Icon = FontAwesome.Solid.ExclamationTriangle,
                });
                return;
            }

            var notification = new ProgressNotification
            {
                Text = "Importing Sound Space Plus map files...",
                CompletionText = "Import Sound Space Plus map complete!",
                State = ProgressNotificationState.Active,
                CompletionClickAction = () =>
                {
                    game?.PerformFromScreen(s => s.Push(new SoloSongSelect()));
                    return true;
                }
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

        private partial class ConfirmRebootToApply : PopupDialog
        {
            public ConfirmRebootToApply(Action onCancel)
            {
                HeaderText = "osu!space need you to restart the game";
                BodyText = "Since you have previously installed an earlier version of osu!space (below 2025.1214.0), and this update includes breaking changes. To use this feature, a game restart is required to apply the fixes. Please restart the game to ensure everything works correctly.";
                Icon = FontAwesome.Solid.ExclamationTriangle;
                Buttons =
                [
                    new PopupDialogOkButton
                    {
                        Text = "Restart now (quit the game)",
                        Action = () => {
                            Environment.Exit(0);
                        }
                    },
                    new PopupDialogCancelButton
                    {
                        Text = "Later",
                        Action = onCancel
                    }
                ];
            }
        }
    }
}

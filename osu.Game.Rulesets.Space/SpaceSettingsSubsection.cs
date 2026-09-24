#nullable enable

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Linq;
using Newtonsoft.Json.Linq;
using osu.Framework.Allocation;
using osu.Framework.Bindables;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osu.Framework.Graphics.Sprites;
using osu.Framework.IO.Network;
using osu.Framework.Localisation;
using osu.Framework.Logging;
using osu.Framework.Platform;
using osu.Framework.Screens;
using osu.Game.Beatmaps;
using osu.Game.Database;
using osu.Game.Graphics;
using osu.Game.Graphics.Containers;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Localisation;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Notifications;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Rulesets.Space.Extension.SSPM;
using osu.Game.Rulesets.Space.Localisation;
using osu.Game.Rulesets.Space.UI;
using osu.Game.Rulesets.UI;
using osu.Game.Screens.Menu;
using osuTK;

namespace osu.Game.Rulesets.Space
{
    public partial class SpaceSettingsSubsection : RulesetSettingsSubsection
    {
        public SpaceSettingsSubsection(SpaceRuleset ruleset)
            : base(ruleset) { }

        private SettingsButtonV2 checkForUpdatesButton = null!;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved(CanBeNull = true)]
        private UserProfileOverlay? userProfile { get; set; }

        [Resolved]
        private Storage storage { get; set; } = null!;

        [Resolved]
        private GameHost host { get; set; } = null!;

        [Resolved(CanBeNull = true)]
        private OsuGame? game { get; set; }

        [Resolved]
        private OsuColour colours { get; set; } = null!;

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private Bindable<WorkingBeatmap> currentBeatmap { get; set; } = null!;

        [Resolved]
        private BeatmapManager beatmapManager { get; set; } = null!;

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SpaceRulesetConfigManager)Config;

            var header = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: 14))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Horizontal = 13, Vertical = 6 },
            };

            header.AddText("by ");
            header.AddLink(
                "michioxd",
                () => userProfile?.ShowUser(new APIUser { Id = 16149043 }),
                "View profile"
            );
            header.AddText(" ฅ^>//<^ฅ ");
            header.AddLink(
                "v" + SpaceRuleset.VERSION_STRING,
                "https://github.com/michioxd/osu-space/releases/tag/" + SpaceRuleset.VERSION_STRING
            );
            header.AddText(". Thanks to ");
            header.AddLink(
                "all contributors",
                "https://github.com/michioxd/osu-space/graphs/contributors"
            );
            header.AddText(".");

            Children =
            [
                header,
                new SettingsButtonV2
                {
                    Text = SpaceStrings.Get("GitHub Repository"),
                    Action = () => host.OpenUrlExternally("https://github.com/michioxd/osu-space"),
                    BackgroundColour = colours.YellowDark,
                },
                checkForUpdatesButton = new SettingsButtonV2
                {
                    Text = SpaceStrings.Get("Check for Updates"),
                    Action = checkRulesetUpdate,
                    BackgroundColour = colours.BlueDark,
                },
                new SettingsButtonV2
                {
                    Text = SpaceStrings.Get("Import Sound Space Plus map (.sspm) (WIP)"),
                    Action = importSSPM,
                },
                new DangerousSettingsButtonV2
                {
                    Text = SpaceStrings.Get("Delete all osu!space beatmaps"),
                    Action = deleteAllBeatmaps,
                },
                new CreateHeader(SpaceStrings.Get("Playfield")),
                new SettingsItemV2(
                    new FormEnumDropdown<PlayfieldBorderStyle>
                    {
                        Caption = RulesetSettingsStrings.PlayfieldBorderStyle,
                        Current = config.GetBindable<PlayfieldBorderStyle>(
                            SpaceRulesetSetting.PlayfieldBorderStyle
                        ),
                    }
                ),
                new SettingsItemV2(
                    new FormCheckBox
                    {
                        Caption = SpaceStrings.Get("Enable Grid"),
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.EnableGrid),
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Playfield Scale"),
                        HintText = SpaceStrings.Get(
                            "Scale of the playfield (higher values = larger playfield)"
                        ),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.ScalePlayfield),
                        KeyboardStep = 0.05f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SkinSettingsStrings.GameplayCursorSize,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.GameplayCursorSize),
                        KeyboardStep = 0.01f,
                    }
                ),
                new SettingsItemV2(
                    new FormCheckBox
                    {
                        Caption = SpaceStrings.Get("Show Cursor Trail"),
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.ShowCursorTrail),
                    }
                ),
                new SettingsItemV2(
                    new FormEnumDropdown<SpaceTouchInputType>
                    {
                        Caption = SpaceStrings.Get("Touch Input Type"),
                        HintText = SpaceStrings.Get(
                            "Only for touch devices. Relative: Touch input moves the cursor relative to its current position. Absolute: Touch input sets the cursor position directly to the touched position."
                        ),
                        Current = config.GetBindable<SpaceTouchInputType>(
                            SpaceRulesetSetting.TouchInputType
                        ),
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Touch Sensitivity"),
                        HintText = SpaceStrings.Get(
                            "Only for touch devices and Touch Input Type is set to Relative. Sensitivity of touch input (higher values = more sensitive)."
                        ),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.TouchSensitivity),
                        TransferValueOnCommit = true,
                        LabelFormat = v => $@"{v:0.##}x",
                        KeyboardStep = 0.1f,
                        TooltipFormat = v => $@"{v:0.##}x",
                    }
                ),
                new CreateHeader(SpaceStrings.Get("Notes")),
                new SettingsItemV2(
                    new FormEnumDropdown<SpacePalette>
                    {
                        Caption = SpaceStrings.Get("Note Color Palette"),
                        HintText = SpaceStrings.Get(
                            "Changes the colors of the notes. Some colors extracted from Sound Space Plus (Rhythia)"
                        ),
                        Current = config.GetBindable<SpacePalette>(SpaceRulesetSetting.Palette),
                    }
                ),
                new PalettePreview(config),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Note Thickness"),
                        HintText = SpaceStrings.Get("Thickness of the notes' borders"),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.NoteThickness),
                        KeyboardStep = 0.5f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Note Corner Radius"),
                        HintText = SpaceStrings.Get("Roundness of the notes' corners"),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.NoteCornerRadius),
                        KeyboardStep = 0.5f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Note Opacity"),
                        HintText = SpaceStrings.Get(
                            "How opaque/transparent/visible the note appears"
                        ),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.noteOpacity),
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Note Scale"),
                        HintText = SpaceStrings.Get(
                            "The visual size of the notes (doesn't affect hitboxes)"
                        ),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.noteScale),
                        KeyboardStep = 0.05f,
                    }
                ),
                new SettingsItemV2(
                    new FormCheckBox
                    {
                        Caption = SpaceStrings.Get("Note Glow"),
                        HintText = SpaceStrings.Get(
                            "Enables a glow effect on notes. Best used with 100% background dim and light note colors."
                        ),
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.Glow),
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Glow Strength"),
                        HintText = SpaceStrings.Get("Strength of the glow effect on notes"),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.GlowStrength),
                        KeyboardStep = 0.01f,
                    }
                ),
                new CreateHeader(SpaceStrings.Get("Gameplay")),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Approach Rate"),
                        HintText = SpaceStrings.Get(
                            "The speed that note move toward the grid (m/s)"
                        ),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.approachRate),
                        KeyboardStep = 1f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Spawn Distance"),
                        HintText = SpaceStrings.Get("Distance from the grid that note spawn (m)"),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.spawnDistance),
                        KeyboardStep = 1f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Fade Length"),
                        HintText = SpaceStrings.Get(
                            "Percentage of the spawn distance that notes take to fade from invisible to fully opaque"
                        ),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.fadeLength),
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true,
                    }
                ),
                new SettingsItemV2(
                    new FormCheckBox
                    {
                        Caption = SpaceStrings.Get("Do not push back"),
                        HintText = SpaceStrings.Get(
                            "While enabled, notes will go past the grid when you miss, instead of always vanishing 0.2 units past the grid"
                        ),
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.doNotPushBack),
                    }
                ),
                new SettingsItemV2(
                    new FormCheckBox
                    {
                        Caption = SpaceStrings.Get("Half ghost"),
                        HintText = SpaceStrings.Get(
                            "Useful for patterns that fill the whole screen"
                        ),
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.halfGhost),
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Parallax Strength"),
                        HintText = SpaceStrings.Get(
                            "Strength of the parallax effect on the playfield (higher values = stronger effect, 0 = disable)"
                        ),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.Parallax),
                        KeyboardStep = 0.1f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceStrings.Get("Hit Window"),
                        HintText = SpaceStrings.Get(
                            "The length of time notes can be hit after reaching the grid (default 25ms, rhythia def 55ms)"
                        ),
                        Current = config.GetBindable<float>(SpaceRulesetSetting.HitWindow),
                        KeyboardStep = 1f,
                    }
                ),
            ];
        }

        private void importSSPM()
        {
            game?.PerformFromScreen(s => s.Push(new SSPMImportScreen()));
        }

        private void goHome(Action execute)
        {
            game?.PerformFromScreen(s =>
            {
                if (s is MainMenu || s is IntroScreen)
                {
                    execute?.Invoke();
                    return;
                }

                s.Exit();
                Scheduler.AddDelayed(() => goHome(execute), 100);
            });
        }

        private void deleteAllBeatmaps()
        {
            dialogOverlay?.Push(
                new DeleteAllBeatmapDialog(() =>
                {
                    goHome(() =>
                    {
                        if (
                            currentBeatmap.Value.BeatmapInfo?.Ruleset?.ShortName
                            == "osuspaceruleset"
                        )
                        {
                            currentBeatmap.Value = (WorkingBeatmap)beatmapManager.DefaultBeatmap;
                        }
                        realm.Write(r =>
                        {
                            var beatmapsToDelete = r.All<BeatmapInfo>()
                                .Where(b => b.Ruleset != null)
                                .ToList()
                                .Where(b => b.Ruleset.ShortName == "osuspaceruleset")
                                .ToList();

                            foreach (var beatmap in beatmapsToDelete)
                            {
                                var parentSet = beatmap.BeatmapSet;

                                if (parentSet != null)
                                {
                                    parentSet.Beatmaps.Remove(beatmap);
                                    r.Remove(beatmap);

                                    if (parentSet.Beatmaps.Count == 0)
                                    {
                                        parentSet.DeletePending = true;
                                    }
                                }
                            }
                        });
                        notifications?.Post(
                            new SimpleNotification
                            {
                                Text = SpaceStrings.Get(
                                    "All osu!space beatmaps added to deletion queue."
                                ),
                                Icon = FontAwesome.Solid.Trash,
                            }
                        );
                    });
                })
            );
        }

        private new partial class CreateHeader : LinkFlowContainer
        {
            public CreateHeader(LocalisableString text)
                : base(t => t.Font = OsuFont.GetFont(size: 16))
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Horizontal = 13, Vertical = 6 };
                Text = text;
            }
        }

        private void checkRulesetUpdate()
        {
            checkForUpdatesButton.Enabled.Value = false;
            checkForUpdatesButton.Text = SpaceStrings.Get("Checking...");
            try
            {
                var req = new JsonWebRequest<JObject>("https://michioxd.ch/osu-space/update.json");
                req.Finished += () =>
                {
                    Schedule(() =>
                    {
                        try
                        {
                            var response = req.ResponseObject;

                            string? version = response["version"]?.ToString();
                            string? downloadUrl = response["download"]?.ToString();
                            string? releaseUrl = response["release"]?.ToString();

                            if (
                                string.IsNullOrWhiteSpace(version)
                                || string.IsNullOrWhiteSpace(downloadUrl)
                                || string.IsNullOrWhiteSpace(releaseUrl)
                            )
                                throw new InvalidOperationException(
                                    "Update response is missing required fields."
                                );

                            if (
                                System.Version.TryParse(version, out var latestVersion)
                                && System.Version.TryParse(
                                    SpaceRuleset.VERSION_STRING,
                                    out var currentVersion
                                )
                            )
                            {
                                if (latestVersion > currentVersion)
                                {
                                    dialogOverlay?.Push(
                                        new UpdateDialog(version, releaseUrl, downloadUrl, host)
                                    );
                                }
                                else
                                {
                                    notifications?.Post(
                                        new SimpleNotification
                                        {
                                            Text = SpaceStrings.Get(
                                                "You are running the latest version of osu!space!"
                                            ),
                                            Icon = FontAwesome.Solid.CheckCircle,
                                        }
                                    );
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            notifications?.Post(
                                new SimpleNotification
                                {
                                    Text = SpaceStrings.Get(
                                        "Failed to check for updates. Please check your internet connection."
                                    ),
                                    Icon = FontAwesome.Solid.TimesCircle,
                                }
                            );

                            Logger.Error(e, "Failed to check for updates", "osu!space");
                        }
                        finally
                        {
                            checkForUpdatesButton.Enabled.Value = true;
                            checkForUpdatesButton.Text = SpaceStrings.Get("Check for Updates");
                        }
                    });
                };
                req.PerformAsync();
            }
            catch (System.Exception e)
            {
                notifications?.Post(
                    new SimpleNotification
                    {
                        Text = SpaceStrings.Get("Failed to check for updates."),
                        Icon = FontAwesome.Solid.TimesCircle,
                    }
                );
                Logger.Error(e, "Failed to check for updates", "osu!space");
                checkForUpdatesButton.Enabled.Value = true;
                checkForUpdatesButton.Text = SpaceStrings.Get("Check for Updates");
            }
        }

        private partial class UpdateDialog : PopupDialog
        {
            public UpdateDialog(
                string version,
                string releaseUrl,
                string downloadUrl,
                GameHost host
            )
            {
                HeaderText = SpaceStrings.Get("New version of osu!space are available!");
                BodyText = SpaceStrings.Format(
                    "Your current version is {0} and the latest version is {1}. Do you want to download it or visit the release page of this version?",
                    SpaceRuleset.VERSION_STRING,
                    version
                );

                Icon = FontAwesome.Solid.Download;

                Buttons =
                [
                    new PopupDialogOkButton
                    {
                        Text = SpaceStrings.Get("View Release"),
                        Action = () => host.OpenUrlExternally(releaseUrl),
                    },
                    new PopupDialogOkButton
                    {
                        Text = SpaceStrings.Get("Download"),
                        Action = () => host.OpenUrlExternally(downloadUrl),
                    },
                    new PopupDialogCancelButton { Text = SpaceStrings.Get("Cancel") },
                ];
            }
        }

        private partial class DeleteAllBeatmapDialog : PopupDialog
        {
            public DeleteAllBeatmapDialog(Action delete)
            {
                HeaderText = SpaceStrings.Get("Delete all osu!space beatmaps?");
                BodyText = SpaceStrings.Get(
                    "Are you sure you want to delete all osu!space beatmaps? This action cannot be undone."
                );

                Icon = FontAwesome.Solid.Trash;

                Buttons =
                [
                    new PopupDialogDangerousButton
                    {
                        Text = SpaceStrings.Get("Delete All Beatmaps"),
                        Action = delete,
                    },
                    new PopupDialogCancelButton { Text = SpaceStrings.Get("Lemme think again...") },
                ];
            }
        }

        private partial class PalettePreview : CompositeDrawable
        {
            private readonly Bindable<SpacePalette> palette = new Bindable<SpacePalette>();
            private readonly FillFlowContainer flow;

            public PalettePreview(SpaceRulesetConfigManager config)
            {
                RelativeSizeAxes = Axes.X;
                AutoSizeAxes = Axes.Y;
                Padding = new MarginPadding { Horizontal = 20, Vertical = 0 };

                InternalChild = flow = new FillFlowContainer
                {
                    RelativeSizeAxes = Axes.X,
                    AutoSizeAxes = Axes.Y,
                    Spacing = new Vector2(2),
                    Direction = FillDirection.Full,
                };

                config.BindWith(SpaceRulesetSetting.Palette, palette);
            }

            protected override void LoadComplete()
            {
                base.LoadComplete();
                palette.BindValueChanged(p => updateColors(p.NewValue), true);
            }

            private void updateColors(SpacePalette p)
            {
                flow.Clear();
                var colors = SpacePaletteHelper.GetColors(p);
                foreach (var color in colors)
                {
                    flow.Add(new Box { Size = new Vector2(35), Colour = color });
                }
            }
        }
    }
}

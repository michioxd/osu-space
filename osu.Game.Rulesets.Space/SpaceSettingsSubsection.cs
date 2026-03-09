// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Allocation;
using osu.Framework.Platform;
using osu.Framework.Graphics.Containers;
using osu.Framework.Graphics.Shapes;
using osuTK;
using osu.Framework.Bindables;
using osu.Framework.Localisation;
using osu.Game.Overlays.Settings;
using osu.Game.Rulesets.Space.Configuration;
using osu.Game.Rulesets.UI;
using osu.Game.Localisation;
using osu.Framework.Graphics;
using osu.Framework.Graphics.Sprites;
using osu.Game.Graphics;
using osu.Framework.IO.Network;
using Newtonsoft.Json.Linq;
using osu.Framework.Logging;
using osu.Game.Overlays;
using osu.Game.Overlays.Dialog;
using osu.Game.Overlays.Notifications;
using osu.Game.Graphics.Containers;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets.Space.Extension.SSPM;
using osu.Framework.Screens;
using System;
using osu.Game.Database;
using osu.Game.Beatmaps;
using System.Linq;
using osu.Game.Screens.Menu;
using osu.Game.Graphics.UserInterfaceV2;
using osu.Game.Rulesets.Space.UI;

namespace osu.Game.Rulesets.Space
{
    public partial class SpaceSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!space";

        public SpaceSettingsSubsection(SpaceRuleset ruleset)
            : base(ruleset)
        {
        }

        private SettingsButtonV2 checkForUpdatesButton;
        private FormSliderBar<float> touchSensitivitySlider;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved(CanBeNull = true)]
        private UserProfileOverlay? userProfile { get; set; }

        [Resolved]
        private Storage storage { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        [Resolved(CanBeNull = true)]
        private OsuGame game { get; set; }

        [Resolved]
        private OsuColour colours { get; set; }

        [Resolved]
        private RealmAccess realm { get; set; } = null!;

        [Resolved]
        private Bindable<WorkingBeatmap> currentBeatmap { get; set; }

        [Resolved]
        private BeatmapManager beatmapManager { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SpaceRulesetConfigManager)Config;

            var header = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: 14))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Horizontal = 13, Vertical = 6 }
            };

            header.AddText("by ");
            header.AddLink("michioxd", () => userProfile?.ShowUser(new APIUser { Id = 16149043 }), "View profile");
            header.AddText(" ฅ^>//<^ฅ ");
            header.AddLink("v" + SpaceRuleset.VERSION_STRING, "https://github.com/michioxd/osu-space/releases/tag/" + SpaceRuleset.VERSION_STRING);
            header.AddText(". Thanks to ");
            header.AddLink("all contributors", "https://github.com/michioxd/osu-space/graphs/contributors");
            header.AddText(".");

            Children =
            [
                header,
                new SettingsButtonV2
                {
                    Text = "GitHub Repository",
                    Action = () => host.OpenUrlExternally("https://github.com/michioxd/osu-space"),
                    BackgroundColour = colours.YellowDark,
                },
                checkForUpdatesButton = new SettingsButtonV2
                {
                    Text = "Check for Updates",
                    Action = checkRulesetUpdate,
                    BackgroundColour = colours.BlueDark,
                },
                new SettingsButtonV2
                {
                    Text = "Import Sound Space Plus map (.sspm) (WIP)",
                    Action = importSSPM,
                },
                new DangerousSettingsButtonV2
                {
                    Text = "Delete all osu!space beatmaps",
                    Action = deleteAllBeatmaps,
                },
                new CreateHeader("Playfield"),
                new SettingsItemV2(new FormEnumDropdown<PlayfieldBorderStyle>
                {
                    Caption = RulesetSettingsStrings.PlayfieldBorderStyle,
                    Current = config.GetBindable<PlayfieldBorderStyle>(SpaceRulesetSetting.PlayfieldBorderStyle),
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Enable Grid",
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.EnableGrid),
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Playfield Scale",
                    HintText = "Scale of the playfield (higher values = larger playfield)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.ScalePlayfield),
                    KeyboardStep = 0.05f,
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = SkinSettingsStrings.GameplayCursorSize,
                    Current = config.GetBindable<float>(SpaceRulesetSetting.GameplayCursorSize),
                    KeyboardStep = 0.01f,
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Show Cursor Trail",
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.ShowCursorTrail),
                }),
                new SettingsItemV2(new FormEnumDropdown<SpaceTouchInputType>
                {
                    Caption = "Touch Input Type",
                    HintText = "Only for touch devices. Relative: Touch input moves the cursor relative to its current position. Absolute: Touch input sets the cursor position directly to the touched position.",
                    Current = config.GetBindable<SpaceTouchInputType>(SpaceRulesetSetting.TouchInputType),
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Touch Sensitivity",
                    HintText = "Only for touch devices and Touch Input Type is set to Relative. Sensitivity of touch input (higher values = more sensitive).",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.TouchSensitivity),
                    TransferValueOnCommit = true,
                    LabelFormat = v => $@"{v:0.##}x",
                    KeyboardStep = 0.1f,
                    TooltipFormat = v => $@"{v:0.##}x",
                }),
                new CreateHeader("Notes"),
                new SettingsItemV2(new FormEnumDropdown<SpacePalette>
                {
                    Caption = "Note Color Palette",
                    HintText = "Changes the colors of the notes. Some colors extracted from Sound Space Plus (Rhythia)",
                    Current = config.GetBindable<SpacePalette>(SpaceRulesetSetting.Palette),
                }),
                new PalettePreview(config),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Note Thickness",
                    HintText = "Thickness of the notes' borders",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.NoteThickness),
                    KeyboardStep = 0.5f,
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Note Corner Radius",
                    HintText = "Roundness of the notes' corners",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.NoteCornerRadius),
                    KeyboardStep = 0.5f,
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Note Opacity",
                    HintText = "How opaque/transparent/visible the note appears",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.noteOpacity),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Note Scale",
                    HintText = "The visual size of the notes (doesn't affect hitboxes)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.noteScale),
                    KeyboardStep = 0.05f
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Note Glow",
                    HintText = "Enables a glow effect on notes. Best used with 100% background dim and light note colors.",
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.Glow)
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Glow Strength",
                    HintText = "Strength of the glow effect on notes",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.GlowStrength),
                    KeyboardStep = 0.01f,
                }),
                new CreateHeader("Gameplay"),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Approach Rate",
                    HintText = "The speed that note move toward the grid (m/s)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.approachRate),
                    KeyboardStep = 1f
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Spawn Distance",
                    HintText = "Distance from the grid that note spawn (m)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.spawnDistance),
                    KeyboardStep = 1f
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Fade Length",
                    HintText = "Percentage of the spawn distance that notes take to fade from invisible to fully opaque",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.fadeLength),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Do not push back",
                    HintText = "While enabled, notes will go past the grid when you miss, instead of always vanishing 0.2 units past the grid",
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.doNotPushBack)
                }),
                new SettingsItemV2(new FormCheckBox
                {
                    Caption = "Half ghost",
                    HintText = "Useful for patterns that fill the whole screen",
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.halfGhost)
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Parallax Strength",
                    HintText = "Strength of the parallax effect on the playfield (higher values = stronger effect, 0 = disable)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.Parallax),
                    KeyboardStep = 0.1f,
                }),
                new SettingsItemV2(new FormSliderBar<float>
                {
                    Caption = "Hit Window",
                    HintText = "The length of time notes can be hit after reaching the grid (default 25ms, rhythia def 55ms)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.HitWindow),
                    KeyboardStep = 1f,
                }),
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
            dialogOverlay?.Push(new DeleteAllBeatmapDialog(() =>
            {
                goHome(() =>
                {
                    if (currentBeatmap.Value.BeatmapInfo?.Ruleset?.ShortName == "osuspaceruleset")
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
                    notifications?.Post(new SimpleNotification
                    {
                        Text = "All osu!space beatmaps added to deletion queue.",
                        Icon = FontAwesome.Solid.Trash,
                    });
                });
            }));
        }

        private new partial class CreateHeader : LinkFlowContainer
        {
            public CreateHeader(string text) : base(t => t.Font = OsuFont.GetFont(size: 16))
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
            checkForUpdatesButton.Text = "Checking...";
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
                            string version = response["version"].ToString();
                            string downloadUrl = response["download"].ToString();
                            string releaseUrl = response["release"].ToString();

                            if (System.Version.TryParse(version, out var latestVersion)
                                && System.Version.TryParse(SpaceRuleset.VERSION_STRING, out var currentVersion))
                            {
                                if (latestVersion > currentVersion)
                                {
                                    dialogOverlay?.Push(new UpdateDialog(version, releaseUrl, downloadUrl, host));
                                }
                                else
                                {
                                    notifications?.Post(new SimpleNotification
                                    {
                                        Text = "You are running the latest version of osu!space!",
                                        Icon = FontAwesome.Solid.CheckCircle,
                                    });
                                }
                            }
                        }
                        catch (System.Exception e)
                        {
                            notifications?.Post(new SimpleNotification
                            {
                                Text = "Failed to check for updates. Please check your internet connection.",
                                Icon = FontAwesome.Solid.TimesCircle,
                            });

                            Logger.Error(e, "Failed to check for updates", "osu!space");
                        }
                        finally
                        {
                            checkForUpdatesButton.Enabled.Value = true;
                            checkForUpdatesButton.Text = "Check for Updates";
                        }
                    });
                };
                req.PerformAsync();
            }
            catch (System.Exception e)
            {
                notifications?.Post(new SimpleNotification
                {
                    Text = "Failed to check for updates.",
                    Icon = FontAwesome.Solid.TimesCircle,
                });
                Logger.Error(e, "Failed to check for updates", "osu!space");
                checkForUpdatesButton.Enabled.Value = true;
                checkForUpdatesButton.Text = "Check for Updates";
            }
        }

        private partial class UpdateDialog : PopupDialog
        {
            public UpdateDialog(string version, string releaseUrl, string downloadUrl, GameHost host)
            {
                HeaderText = $"New version of osu!space are available!";
                BodyText = $"Your current version is {SpaceRuleset.VERSION_STRING} and the latest version is {version}. Do you want to download it or visit the release page of this version?";

                Icon = FontAwesome.Solid.Download;

                Buttons =
                [
                    new PopupDialogOkButton
                    {
                        Text = "View Release",
                        Action = () => host.OpenUrlExternally(releaseUrl)
                    },
                    new PopupDialogOkButton
                    {
                        Text = "Download",
                        Action = () => host.OpenUrlExternally(downloadUrl)
                    },
                    new PopupDialogCancelButton
                    {
                        Text = "Cancel"
                    },
                ];
            }
        }

        private partial class DeleteAllBeatmapDialog : PopupDialog
        {
            public DeleteAllBeatmapDialog(Action delete)
            {
                HeaderText = $"Delete all osu!space beatmaps?";
                BodyText = $"Are you sure you want to delete all osu!space beatmaps? This action cannot be undone.";

                Icon = FontAwesome.Solid.Trash;

                Buttons =
                [
                    new PopupDialogDangerousButton
                    {
                        Text = "Delete All Beatmaps",
                        Action = delete
                    },
                    new PopupDialogCancelButton
                    {
                        Text = "Lemme think again..."
                    },
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
                    flow.Add(new Box
                    {
                        Size = new Vector2(35),
                        Colour = color,
                    });
                }
            }
        }
    }
}

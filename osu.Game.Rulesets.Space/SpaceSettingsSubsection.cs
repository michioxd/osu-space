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
        protected override LocalisableString Header => "osu!space";

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

            header.AddText(SpaceSettingsSubsectionStrings.HeaderBy);
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
            header.AddText(SpaceSettingsSubsectionStrings.HeaderThanksTo);
            header.AddLink(
                SpaceSettingsSubsectionStrings.AllContributors,
                "https://github.com/michioxd/osu-space/graphs/contributors"
            );
            header.AddText(".");

            Children =
            [
                header,
                new SettingsButtonV2
                {
                    Text = SpaceSettingsSubsectionStrings.GitHubRepository,
                    Action = () => host.OpenUrlExternally("https://github.com/michioxd/osu-space"),
                    BackgroundColour = colours.YellowDark,
                },
                checkForUpdatesButton = new SettingsButtonV2
                {
                    Text = SpaceSettingsSubsectionStrings.CheckForUpdates,
                    Action = checkRulesetUpdate,
                    BackgroundColour = colours.BlueDark,
                },
                new SettingsButtonV2
                {
                    Text = SpaceSettingsSubsectionStrings.ImportSspmMap,
                    Action = importSSPM,
                },
                new DangerousSettingsButtonV2
                {
                    Text = SpaceSettingsSubsectionStrings.DeleteAllBeatmaps,
                    Action = deleteAllBeatmaps,
                },
                new CreateHeader(SpaceSettingsSubsectionStrings.HeaderPlayfield),
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
                        Caption = SpaceSettingsSubsectionStrings.EnableGrid,
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.EnableGrid),
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.PlayfieldScale,
                        HintText = SpaceSettingsSubsectionStrings.PlayfieldScaleTooltip,
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
                        Caption = SpaceSettingsSubsectionStrings.ShowCursorTrail,
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.ShowCursorTrail),
                    }
                ),
                new SettingsItemV2(
                    new FormEnumDropdown<SpaceTouchInputType>
                    {
                        Caption = SpaceSettingsSubsectionStrings.TouchInputType,
                        HintText = SpaceSettingsSubsectionStrings.TouchInputTypeTooltip,
                        Current = config.GetBindable<SpaceTouchInputType>(
                            SpaceRulesetSetting.TouchInputType
                        ),
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.TouchSensitivity,
                        HintText = SpaceSettingsSubsectionStrings.TouchSensitivityTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.TouchSensitivity),
                        TransferValueOnCommit = true,
                        LabelFormat = v => $@"{v:0.##}x",
                        KeyboardStep = 0.1f,
                        TooltipFormat = v => $@"{v:0.##}x",
                    }
                ),
                new CreateHeader(SpaceSettingsSubsectionStrings.HeaderNotes),
                new SettingsItemV2(
                    new FormEnumDropdown<SpacePalette>
                    {
                        Caption = SpaceSettingsSubsectionStrings.NoteColorPalette,
                        HintText = SpaceSettingsSubsectionStrings.NoteColorPaletteTooltip,
                        Current = config.GetBindable<SpacePalette>(SpaceRulesetSetting.Palette),
                    }
                ),
                new PalettePreview(config),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.NoteThickness,
                        HintText = SpaceSettingsSubsectionStrings.NoteThicknessTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.NoteThickness),
                        KeyboardStep = 0.5f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.NoteCornerRadius,
                        HintText = SpaceSettingsSubsectionStrings.NoteCornerRadiusTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.NoteCornerRadius),
                        KeyboardStep = 0.5f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.NoteOpacity,
                        HintText = SpaceSettingsSubsectionStrings.NoteOpacityTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.noteOpacity),
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.NoteScale,
                        HintText = SpaceSettingsSubsectionStrings.NoteScaleTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.noteScale),
                        KeyboardStep = 0.05f,
                    }
                ),
                new SettingsItemV2(
                    new FormCheckBox
                    {
                        Caption = SpaceSettingsSubsectionStrings.NoteGlow,
                        HintText = SpaceSettingsSubsectionStrings.NoteGlowTooltip,
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.Glow),
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.GlowStrength,
                        HintText = SpaceSettingsSubsectionStrings.GlowStrengthTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.GlowStrength),
                        KeyboardStep = 0.01f,
                    }
                ),
                new CreateHeader(SpaceSettingsSubsectionStrings.HeaderGameplay),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.ApproachRate,
                        HintText = SpaceSettingsSubsectionStrings.ApproachRateTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.approachRate),
                        KeyboardStep = 1f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.SpawnDistance,
                        HintText = SpaceSettingsSubsectionStrings.SpawnDistanceTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.spawnDistance),
                        KeyboardStep = 1f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.FadeLength,
                        HintText = SpaceSettingsSubsectionStrings.FadeLengthTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.fadeLength),
                        KeyboardStep = 0.01f,
                        DisplayAsPercentage = true,
                    }
                ),
                new SettingsItemV2(
                    new FormCheckBox
                    {
                        Caption = SpaceSettingsSubsectionStrings.DoNotPushBack,
                        HintText = SpaceSettingsSubsectionStrings.DoNotPushBackTooltip,
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.doNotPushBack),
                    }
                ),
                new SettingsItemV2(
                    new FormCheckBox
                    {
                        Caption = SpaceSettingsSubsectionStrings.HalfGhost,
                        HintText = SpaceSettingsSubsectionStrings.HalfGhostTooltip,
                        Current = config.GetBindable<bool>(SpaceRulesetSetting.halfGhost),
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.ParallaxStrength,
                        HintText = SpaceSettingsSubsectionStrings.ParallaxStrengthTooltip,
                        Current = config.GetBindable<float>(SpaceRulesetSetting.Parallax),
                        KeyboardStep = 0.1f,
                    }
                ),
                new SettingsItemV2(
                    new FormSliderBar<float>
                    {
                        Caption = SpaceSettingsSubsectionStrings.HitWindow,
                        HintText = SpaceSettingsSubsectionStrings.HitWindowTooltip,
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
                                Text = SpaceSettingsSubsectionStrings.DeletionQueued,
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
            checkForUpdatesButton.Text = SpaceSettingsSubsectionStrings.Checking;
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
                                            Text =
                                                SpaceSettingsSubsectionStrings.LatestVersionNotification,
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
                                    Text =
                                        SpaceSettingsSubsectionStrings.UpdateCheckFailedConnection,
                                    Icon = FontAwesome.Solid.TimesCircle,
                                }
                            );

                            Logger.Error(e, "Failed to check for updates", "osu!space");
                        }
                        finally
                        {
                            checkForUpdatesButton.Enabled.Value = true;
                            checkForUpdatesButton.Text =
                                SpaceSettingsSubsectionStrings.CheckForUpdates;
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
                        Text = SpaceSettingsSubsectionStrings.UpdateCheckFailed,
                        Icon = FontAwesome.Solid.TimesCircle,
                    }
                );
                Logger.Error(e, "Failed to check for updates", "osu!space");
                checkForUpdatesButton.Enabled.Value = true;
                checkForUpdatesButton.Text = "Check for Updates";
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
                HeaderText = SpaceSettingsSubsectionStrings.UpdateAvailableHeader;
                BodyText = SpaceSettingsSubsectionStrings.UpdateAvailableBody(
                    SpaceRuleset.VERSION_STRING,
                    version
                );

                Icon = FontAwesome.Solid.Download;

                Buttons =
                [
                    new PopupDialogOkButton
                    {
                        Text = SpaceSettingsSubsectionStrings.ViewRelease,
                        Action = () => host.OpenUrlExternally(releaseUrl),
                    },
                    new PopupDialogOkButton
                    {
                        Text = SpaceSettingsSubsectionStrings.Download,
                        Action = () => host.OpenUrlExternally(downloadUrl),
                    },
                    new PopupDialogCancelButton { Text = SpaceSettingsSubsectionStrings.Cancel },
                ];
            }
        }

        private partial class DeleteAllBeatmapDialog : PopupDialog
        {
            public DeleteAllBeatmapDialog(Action delete)
            {
                HeaderText = SpaceSettingsSubsectionStrings.DeleteAllHeader;
                BodyText = SpaceSettingsSubsectionStrings.DeleteAllBody;

                Icon = FontAwesome.Solid.Trash;

                Buttons =
                [
                    new PopupDialogDangerousButton
                    {
                        Text = SpaceSettingsSubsectionStrings.DeleteAllConfirm,
                        Action = delete,
                    },
                    new PopupDialogCancelButton
                    {
                        Text = SpaceSettingsSubsectionStrings.DeleteAllCancel,
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
                    flow.Add(new Box { Size = new Vector2(35), Colour = color });
                }
            }
        }
    }
}

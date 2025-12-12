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
using osu.Game.Overlays.Settings.Sections;
using osu.Game.Overlays.Settings.Sections.Maintenance;

namespace osu.Game.Rulesets.Space
{
    public partial class SpaceSettingsSubsection : RulesetSettingsSubsection
    {
        protected override LocalisableString Header => "osu!space";

        public SpaceSettingsSubsection(SpaceRuleset ruleset)
            : base(ruleset)
        {
        }

        private SettingsEnumDropdown<SpacePalette> paletteSelector;

        private SettingsButton checkForUpdatesButton;

        [Resolved]
        private IDialogOverlay? dialogOverlay { get; set; }

        [Resolved]
        private INotificationOverlay? notifications { get; set; }

        [Resolved(CanBeNull = true)]
        private UserProfileOverlay? userProfile { get; set; }

        [Resolved]
        private GameHost host { get; set; }

        [BackgroundDependencyLoader]
        private void load()
        {
            var config = (SpaceRulesetConfigManager)Config;

            var header = new LinkFlowContainer(t => t.Font = OsuFont.GetFont(size: 14))
            {
                RelativeSizeAxes = Axes.X,
                AutoSizeAxes = Axes.Y,
                Padding = new MarginPadding { Horizontal = 20, Vertical = 0 }
            };

            header.AddText("osu!space by ");
            header.AddLink("michioxd", () => userProfile?.ShowUser(new APIUser { Id = 16149043 }), "View profile");
            header.AddText(" ฅ^>//<^ฅ v");
            header.AddLink(SpaceRuleset.VERSION_STRING, "https://github.com/michioxd/osu-space/releases/tag/" + SpaceRuleset.VERSION_STRING);

            Children = new Drawable[]
            {
                header,
                new SettingsButton
                {
                    Text = "GitHub Repository",
                    Action = () => host.OpenUrlExternally("https://github.com/michioxd/osu-space")
                },
                checkForUpdatesButton = new SettingsButton
                {
                    Text = "Check for Updates",
                    Action = checkRulesetUpdate
                },
                new SettingsEnumDropdown<PlayfieldBorderStyle>
                {
                    LabelText = RulesetSettingsStrings.PlayfieldBorderStyle,
                    Current = config.GetBindable<PlayfieldBorderStyle>(SpaceRulesetSetting.PlayfieldBorderStyle),
                },
                new SettingsCheckbox
                {
                    LabelText = "Enable Grid",
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.EnableGrid),
                },
                new SettingsSlider<float>
                {
                    LabelText = "Playfield Scale",
                    TooltipText = "Scale of the playfield (higher values = larger playfield)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.ScalePlayfield),
                    KeyboardStep = 0.05f,
                },
                new SettingsSlider<float, SizeSlider<float>>
                {
                    LabelText = SkinSettingsStrings.GameplayCursorSize,
                    Current = config.GetBindable<float>(SpaceRulesetSetting.GameplayCursorSize),
                    KeyboardStep = 0.01f
                },
                new SettingsCheckbox
                {
                    LabelText = "Show Cursor Trail",
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.ShowCursorTrail),
                },
                paletteSelector = new SettingsEnumDropdown<SpacePalette>
                {
                    LabelText = "Note Color Palette",
                    Current = config.GetBindable<SpacePalette>(SpaceRulesetSetting.Palette),
                },
                new PalettePreview(config),
                new SettingsSlider<float>
                {
                    LabelText = "Note Thickness",
                    TooltipText = "Thickness of the notes' borders",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.NoteThickness),
                    KeyboardStep = 0.5f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "Note Corner Radius",
                    TooltipText = "Roundness of the notes' corners",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.NoteCornerRadius),
                    KeyboardStep = 0.5f,
                },
                new SettingsSlider<float>
                {
                    LabelText = "Note Opacity",
                    TooltipText = "How opaque/transparent/visible the note appears",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.noteOpacity),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsSlider<float>
                {
                    LabelText = "Note Scale",
                    TooltipText = "The visual size of the notes (doesn't affect hitboxes)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.noteScale),
                    KeyboardStep = 0.05f
                },
                new SettingsSlider<float>
                {
                    LabelText = "Approach Rate",
                    TooltipText = "The speed that note move toward the grid (m/s)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.approachRate),
                    KeyboardStep = 1f
                },
                new SettingsSlider<float>
                {
                    LabelText = "Spawn Distance",
                    TooltipText = "Distance from the grid that note spawn (m)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.spawnDistance),
                    KeyboardStep = 1f
                },
                new SettingsSlider<float>
                {
                    LabelText = "Fade Length",
                    TooltipText = "Percentage of the spawn distance that notes take to fade from invisible to fully opaque",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.fadeLength),
                    KeyboardStep = 0.01f,
                    DisplayAsPercentage = true
                },
                new SettingsCheckbox
                {
                    LabelText = "Do not push back",
                    TooltipText = "While enabled, notes will go past the grid when you miss, instead of always vanishing 0.1 units past the grid",
                    Keywords = new[] { "miss", "push", "back" },
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.doNotPushBack)
                },
                new SettingsCheckbox
                {
                    LabelText = "Half ghost",
                    TooltipText = "Useful for patterns that fill the whole screen",
                    Keywords = new[] { "ghost", "transparency", "alpha" },
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.halfGhost)
                },
                new SettingsSlider<float>
                {
                    LabelText = "Parallax Strength",
                    TooltipText = "Strength of the parallax effect on the playfield (higher values = stronger effect, 0 = disable)",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.Parallax),
                    KeyboardStep = 0.1f,
                },
                new SettingsCheckbox
                {
                    LabelText = "Enable Bloom",
                    TooltipText = "Enables a glow effect on notes. Best used with 100% background dim and light note colors.",
                    Current = config.GetBindable<bool>(SpaceRulesetSetting.Bloom)
                },
                new SettingsSlider<float>
                {
                    LabelText = "Bloom Strength",
                    TooltipText = "Strength of the bloom effect on notes",
                    Current = config.GetBindable<float>(SpaceRulesetSetting.BloomStrength),
                    KeyboardStep = 0.01f,
                },
            };


            paletteSelector.SetNoticeText("Some colors extracted from Sound Space Plus (Rhythia)");
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

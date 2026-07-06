// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Space.Localisation
{
    public static class SpaceSettingsSubsectionStrings
    {
        private const string prefix =
            @"osu.Game.Rulesets.Space.Resources.Localisation.SpaceSettingsSubsectionStrings";

        // Header sentence ("by {author} .... Thanks to {contributors}.")
        public static LocalisableString HeaderBy =>
            new TranslatableString(getKey(@"header_by"), @"by ");
        public static LocalisableString HeaderThanksTo =>
            new TranslatableString(getKey(@"header_thanks_to"), @". Thanks to ");

        // Header links / action buttons
        public static LocalisableString AllContributors =>
            new TranslatableString(getKey(@"all_contributors"), @"all contributors");
        public static LocalisableString GitHubRepository =>
            new TranslatableString(getKey(@"github_repository"), @"GitHub Repository");
        public static LocalisableString CheckForUpdates =>
            new TranslatableString(getKey(@"check_for_updates"), @"Check for Updates");
        public static LocalisableString Checking =>
            new TranslatableString(getKey(@"checking"), @"Checking...");
        public static LocalisableString ImportSspmMap =>
            new TranslatableString(
                getKey(@"import_sspm_map"),
                @"Import Sound Space Plus map (.sspm) (WIP)"
            );
        public static LocalisableString DeleteAllBeatmaps =>
            new TranslatableString(
                getKey(@"delete_all_beatmaps"),
                @"Delete all osu!space beatmaps"
            );

        // Section headers
        public static LocalisableString HeaderPlayfield =>
            new TranslatableString(getKey(@"header_playfield"), @"Playfield");
        public static LocalisableString HeaderNotes =>
            new TranslatableString(getKey(@"header_notes"), @"Notes");
        public static LocalisableString HeaderGameplay =>
            new TranslatableString(getKey(@"header_gameplay"), @"Gameplay");

        // Playfield settings
        public static LocalisableString EnableGrid =>
            new TranslatableString(getKey(@"enable_grid"), @"Enable Grid");
        public static LocalisableString PlayfieldScale =>
            new TranslatableString(getKey(@"playfield_scale"), @"Playfield Scale");
        public static LocalisableString PlayfieldScaleTooltip =>
            new TranslatableString(
                getKey(@"playfield_scale_tooltip"),
                @"Scale of the playfield (higher values = larger playfield)"
            );
        public static LocalisableString ShowCursorTrail =>
            new TranslatableString(getKey(@"show_cursor_trail"), @"Show Cursor Trail");
        public static LocalisableString TouchInputType =>
            new TranslatableString(getKey(@"touch_input_type"), @"Touch Input Type");
        public static LocalisableString TouchInputTypeTooltip =>
            new TranslatableString(
                getKey(@"touch_input_type_tooltip"),
                @"Only for touch devices. Relative: Touch input moves the cursor relative to its current position. Absolute: Touch input sets the cursor position directly to the touched position."
            );
        public static LocalisableString TouchSensitivity =>
            new TranslatableString(getKey(@"touch_sensitivity"), @"Touch Sensitivity");
        public static LocalisableString TouchSensitivityTooltip =>
            new TranslatableString(
                getKey(@"touch_sensitivity_tooltip"),
                @"Only for touch devices and Touch Input Type is set to Relative. Sensitivity of touch input (higher values = more sensitive)."
            );

        // Note settings
        public static LocalisableString NoteColorPalette =>
            new TranslatableString(getKey(@"note_color_palette"), @"Note Color Palette");
        public static LocalisableString NoteColorPaletteTooltip =>
            new TranslatableString(
                getKey(@"note_color_palette_tooltip"),
                @"Changes the colors of the notes. Some colors extracted from Sound Space Plus (Rhythia)"
            );
        public static LocalisableString NoteThickness =>
            new TranslatableString(getKey(@"note_thickness"), @"Note Thickness");
        public static LocalisableString NoteThicknessTooltip =>
            new TranslatableString(
                getKey(@"note_thickness_tooltip"),
                @"Thickness of the notes' borders"
            );
        public static LocalisableString NoteCornerRadius =>
            new TranslatableString(getKey(@"note_corner_radius"), @"Note Corner Radius");
        public static LocalisableString NoteCornerRadiusTooltip =>
            new TranslatableString(
                getKey(@"note_corner_radius_tooltip"),
                @"Roundness of the notes' corners"
            );
        public static LocalisableString NoteOpacity =>
            new TranslatableString(getKey(@"note_opacity"), @"Note Opacity");
        public static LocalisableString NoteOpacityTooltip =>
            new TranslatableString(
                getKey(@"note_opacity_tooltip"),
                @"How opaque/transparent/visible the note appears"
            );
        public static LocalisableString NoteScale =>
            new TranslatableString(getKey(@"note_scale"), @"Note Scale");
        public static LocalisableString NoteScaleTooltip =>
            new TranslatableString(
                getKey(@"note_scale_tooltip"),
                @"The visual size of the notes (doesn't affect hitboxes)"
            );
        public static LocalisableString NoteGlow =>
            new TranslatableString(getKey(@"note_glow"), @"Note Glow");
        public static LocalisableString NoteGlowTooltip =>
            new TranslatableString(
                getKey(@"note_glow_tooltip"),
                @"Enables a glow effect on notes. Best used with 100% background dim and light note colors."
            );
        public static LocalisableString GlowStrength =>
            new TranslatableString(getKey(@"glow_strength"), @"Glow Strength");
        public static LocalisableString GlowStrengthTooltip =>
            new TranslatableString(
                getKey(@"glow_strength_tooltip"),
                @"Strength of the glow effect on notes"
            );

        // Gameplay settings
        public static LocalisableString ApproachRate =>
            new TranslatableString(getKey(@"approach_rate"), @"Approach Rate");
        public static LocalisableString ApproachRateTooltip =>
            new TranslatableString(
                getKey(@"approach_rate_tooltip"),
                @"The speed that note move toward the grid (m/s)"
            );
        public static LocalisableString SpawnDistance =>
            new TranslatableString(getKey(@"spawn_distance"), @"Spawn Distance");
        public static LocalisableString SpawnDistanceTooltip =>
            new TranslatableString(
                getKey(@"spawn_distance_tooltip"),
                @"Distance from the grid that note spawn (m)"
            );
        public static LocalisableString FadeLength =>
            new TranslatableString(getKey(@"fade_length"), @"Fade Length");
        public static LocalisableString FadeLengthTooltip =>
            new TranslatableString(
                getKey(@"fade_length_tooltip"),
                @"Percentage of the spawn distance that notes take to fade from invisible to fully opaque"
            );
        public static LocalisableString DoNotPushBack =>
            new TranslatableString(getKey(@"do_not_push_back"), @"Do not push back");
        public static LocalisableString DoNotPushBackTooltip =>
            new TranslatableString(
                getKey(@"do_not_push_back_tooltip"),
                @"While enabled, notes will go past the grid when you miss, instead of always vanishing 0.2 units past the grid"
            );
        public static LocalisableString HalfGhost =>
            new TranslatableString(getKey(@"half_ghost"), @"Half ghost");
        public static LocalisableString HalfGhostTooltip =>
            new TranslatableString(
                getKey(@"half_ghost_tooltip"),
                @"Useful for patterns that fill the whole screen"
            );
        public static LocalisableString ParallaxStrength =>
            new TranslatableString(getKey(@"parallax_strength"), @"Parallax Strength");
        public static LocalisableString ParallaxStrengthTooltip =>
            new TranslatableString(
                getKey(@"parallax_strength_tooltip"),
                @"Strength of the parallax effect on the playfield (higher values = stronger effect, 0 = disable)"
            );
        public static LocalisableString HitWindow =>
            new TranslatableString(getKey(@"hit_window"), @"Hit Window");
        public static LocalisableString HitWindowTooltip =>
            new TranslatableString(
                getKey(@"hit_window_tooltip"),
                @"The length of time notes can be hit after reaching the grid (default 25ms, rhythia def 55ms)"
            );

        // Update-check notifications
        public static LocalisableString LatestVersionNotification =>
            new TranslatableString(
                getKey(@"latest_version_notification"),
                @"You are running the latest version of osu!space!"
            );
        public static LocalisableString UpdateCheckFailedConnection =>
            new TranslatableString(
                getKey(@"update_check_failed_connection"),
                @"Failed to check for updates. Please check your internet connection."
            );
        public static LocalisableString UpdateCheckFailed =>
            new TranslatableString(getKey(@"update_check_failed"), @"Failed to check for updates.");
        public static LocalisableString DeletionQueued =>
            new TranslatableString(
                getKey(@"deletion_queued"),
                @"All osu!space beatmaps added to deletion queue."
            );

        // Update-available dialog
        public static LocalisableString UpdateAvailableHeader =>
            new TranslatableString(
                getKey(@"update_available_header"),
                @"New version of osu!space are available!"
            );

        public static LocalisableString UpdateAvailableBody(
            string currentVersion,
            string latestVersion
        ) =>
            new TranslatableString(
                getKey(@"update_available_body"),
                @"Your current version is {0} and the latest version is {1}. Do you want to download it or visit the release page of this version?",
                currentVersion,
                latestVersion
            );

        public static LocalisableString ViewRelease =>
            new TranslatableString(getKey(@"view_release"), @"View Release");
        public static LocalisableString Download =>
            new TranslatableString(getKey(@"download"), @"Download");
        public static LocalisableString Cancel =>
            new TranslatableString(getKey(@"cancel"), @"Cancel");

        // Delete-all-beatmaps dialog
        public static LocalisableString DeleteAllHeader =>
            new TranslatableString(getKey(@"delete_all_header"), @"Delete all osu!space beatmaps?");
        public static LocalisableString DeleteAllBody =>
            new TranslatableString(
                getKey(@"delete_all_body"),
                @"Are you sure you want to delete all osu!space beatmaps? This action cannot be undone."
            );
        public static LocalisableString DeleteAllConfirm =>
            new TranslatableString(getKey(@"delete_all_confirm"), @"Delete All Beatmaps");
        public static LocalisableString DeleteAllCancel =>
            new TranslatableString(getKey(@"delete_all_cancel"), @"Lemme think again...");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Space.Localisation.Mods
{
    public static class SpaceModNoScopeStrings
    {
        private const string prefix =
            @"osu.Game.Rulesets.Space.Resources.Localisation.Mods.SpaceModNoScopeStrings";

        public static LocalisableString ModDescription =>
            new TranslatableString(getKey(@"mod_description"), @"Where's the cursor?");

        private static string getKey(string key) => $@"{prefix}:{key}";
    }
}

#nullable enable

using System;
using System.Globalization;
using System.Linq;
using System.Resources;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Space.Localisation
{
    public static class SpaceStrings
    {
        private static readonly System.Collections.Generic.Dictionary<string, ResourceManager> resources =
            typeof(SpaceStrings).Assembly.GetManifestResourceNames()
                .Where(name => name.StartsWith(typeof(SpaceStrings).FullName + ".", StringComparison.Ordinal)
                    && name.EndsWith(".resources", StringComparison.Ordinal)
                    && name.Length > typeof(SpaceStrings).FullName!.Length + ".resources".Length + 1)
                .ToDictionary(
                    name => name.Substring(typeof(SpaceStrings).FullName!.Length + 1, name.Length - typeof(SpaceStrings).FullName!.Length - ".resources".Length - 1),
                    name => new ResourceManager(name.Substring(0, name.Length - ".resources".Length), typeof(SpaceStrings).Assembly),
                    StringComparer.OrdinalIgnoreCase
                );

        public static LocalisableString Get(string english) =>
            new LocalisableString(new SpaceString(english));

        public static LocalisableString Format(string format, params object[] args) =>
            new LocalisableString(new FormattedSpaceString(format, args));

        private sealed class FormattedSpaceString : ILocalisableStringData
        {
            private readonly string format;
            private readonly object[] args;

            public FormattedSpaceString(string format, object[] args) =>
                (this.format, this.args) = (format, args);

            public string GetLocalised(LocalisationParameters parameters) =>
                string.Format(
                    CultureInfo.InvariantCulture,
                    new SpaceString(format).GetLocalised(parameters),
                    args
                );

            public bool Equals(ILocalisableStringData? other) =>
                other is FormattedSpaceString value
                && format == value.format
                && args.AsSpan().SequenceEqual(value.args);
        }

        private sealed class SpaceString : ILocalisableStringData
        {
            private readonly string english;

            public SpaceString(string english) => this.english = english;

            public string GetLocalised(LocalisationParameters parameters) =>
                parameters.Store != null
                && resources.TryGetValue(parameters.Store.EffectiveCulture.TwoLetterISOLanguageName, out var resource)
                    ? resource.GetString(english, CultureInfo.InvariantCulture) ?? english
                    : english;

            public bool Equals(ILocalisableStringData? other) =>
                other is SpaceString value && english == value.english;

            public override string ToString() => english;
        }
    }
}

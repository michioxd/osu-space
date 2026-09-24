#nullable enable

using System;
using System.Globalization;
using osu.Framework.Localisation;

namespace osu.Game.Rulesets.Space.Localisation
{
    public static partial class SpaceStrings
    {
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
                parameters.Store?.EffectiveCulture.TwoLetterISOLanguageName switch
                {
                    "vi" when vietnamese.TryGetValue(english, out string? viTranslation) =>
                        viTranslation,
                    "ja" when japanese.TryGetValue(english, out string? jaTranslation) =>
                        jaTranslation,
                    _ => english,
                };

            public bool Equals(ILocalisableStringData? other) =>
                other is SpaceString value && english == value.english;

            public override string ToString() => english;
        }
    }
}

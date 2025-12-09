using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Space.Configuration
{
    public static class SpacePaletteHelper
    {
        public static Colour4[] GetColors(SpacePalette palette)
        {
            switch (palette)
            {
                case SpacePalette.White: return new[] { Colour4.FromHex("#FFFFFF") };
                case SpacePalette.Purple: return new[] { Colour4.FromHex("#9A5EF9") };
                case SpacePalette.Vortex: return new[] { Colour4.FromHex("#000000"), Colour4.FromHex("#381E42") };
                case SpacePalette.CottonCandy: return new[] { Colour4.FromHex("#00FFED"), Colour4.FromHex("#00FFED") };
                case SpacePalette.VeggieStraws: return new[] { Colour4.FromHex("#FFCC4D"), Colour4.FromHex("#FF7892"), Colour4.FromHex("#FF7892") };
                case SpacePalette.EverybodyVotesChannel: return new[] { Colour4.FromHex("#FC94F2"), Colour4.FromHex("#96FC94") };
                case SpacePalette.RedAndBlue: return new[] { Colour4.FromHex("#FC4441"), Colour4.FromHex("#4151FC") };
                case SpacePalette.Pastel: return new[] { Colour4.FromHex("#5BCEFA"), Colour4.FromHex("#F5A9B8"), Colour4.FromHex("#FFFFFF") };
                case SpacePalette.WiiPlayers: return new[] { Colour4.FromHex("#008CFF"), Colour4.FromHex("#ED3434"), Colour4.FromHex("#10BD0D"), Colour4.FromHex("#FFB300") };
                case SpacePalette.HueWheel:
                    return new[] {
                    Colour4.FromHex("#E95F5F"), Colour4.FromHex("#E88D5F"), Colour4.FromHex("#E8BA5F"), Colour4.FromHex("#E8E85F"),
                    Colour4.FromHex("#BAE85F"), Colour4.FromHex("#8DE85F"), Colour4.FromHex("#5FE85F"), Colour4.FromHex("#5FE88D"),
                    Colour4.FromHex("#5FE8BA"), Colour4.FromHex("#5FE8E8"), Colour4.FromHex("#5FBAE8"), Colour4.FromHex("#5F8DE8"),
                    Colour4.FromHex("#5F8DE8"), Colour4.FromHex("#8D5FE8"), Colour4.FromHex("#BA5FE8"), Colour4.FromHex("#E85FE8"),
                    Colour4.FromHex("#E85FA4"), Colour4.FromHex("#E85F8D"), Colour4.FromHex("#7E7E7E"), Colour4.FromHex("#6D6D6E")
                };
                case SpacePalette.HueWheelUltra:
                    return new[] {
                    Colour4.FromHex("#E75354"), Colour4.FromHex("#EF7D5D"), Colour4.FromHex("#F19C5B"), Colour4.FromHex("#F5B95B"), Colour4.FromHex("#F5CE5C"), Colour4.FromHex("#EED75A"),
                    Colour4.FromHex("#EBE858"), Colour4.FromHex("#D1E75A"), Colour4.FromHex("#ACE15F"), Colour4.FromHex("#92DC64"), Colour4.FromHex("#75D86B"), Colour4.FromHex("#80D86B"),
                    Colour4.FromHex("#61D565"), Colour4.FromHex("#5BD887"), Colour4.FromHex("#61DAA5"), Colour4.FromHex("#66DBBE"), Colour4.FromHex("#65DBD4"), Colour4.FromHex("#66E1DD"),
                    Colour4.FromHex("#64E2ED"), Colour4.FromHex("#6ABBE7"), Colour4.FromHex("#6A9EE3"), Colour4.FromHex("#6C89E2"), Colour4.FromHex("#7077E1"), Colour4.FromHex("#6F66E0"),
                    Colour4.FromHex("#715AE0"), Colour4.FromHex("#845CE0"), Colour4.FromHex("#9E5EE0"), Colour4.FromHex("#B361E0"), Colour4.FromHex("#C462E0"), Colour4.FromHex("#D164DF"),
                    Colour4.FromHex("#E167DF"), Colour4.FromHex("#E36AC4"), Colour4.FromHex("#E66D9E"), Colour4.FromHex("#E66F85"), Colour4.FromHex("#E77175"), Colour4.FromHex("#E8746F")
                };
                default: return new[] { Colour4.White };
            }
        }
    }
}

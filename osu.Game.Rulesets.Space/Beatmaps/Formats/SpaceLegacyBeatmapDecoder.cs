using System;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;

namespace osu.Game.Rulesets.Space.Beatmaps.Formats;

public class SpaceLegacyBeatmapDecoder : LegacyBeatmapDecoder
{
    public new const int LATEST_VERSION = 1;

    public new static void Register()
    {
        AddDecoder<Beatmap>("osuspaceruleset file format v", m => new SpaceLegacyBeatmapDecoder(Parsing.ParseInt(m.Split('v').Last())));
        SetFallbackDecoder<Beatmap>(() => new SpaceLegacyBeatmapDecoder());
    }

    public SpaceLegacyBeatmapDecoder(int version = LATEST_VERSION)
        : base(version)
    {
    }

    protected override void ParseLine(Beatmap beatmap, Section section, string line)
    {
        switch (section)
        {
            case Section.General:
                if (line.StartsWith("Mode", StringComparison.Ordinal))
                {
                    beatmap.BeatmapInfo.Ruleset = new SpaceRuleset().RulesetInfo;
                    return;
                }
                break;
        }

        base.ParseLine(beatmap, section, line);
    }
}

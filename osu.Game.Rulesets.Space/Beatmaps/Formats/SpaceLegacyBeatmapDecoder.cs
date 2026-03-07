using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using osu.Game.Audio;
using osu.Game.Beatmaps;
using osu.Game.Beatmaps.Formats;
using osu.Game.Rulesets.Space.Objects;

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
            case Section.HitObjects:
                string[] split = line.Split(',');
                if (split.Length >= 3 &&
                    float.TryParse(split[0], NumberStyles.Float, CultureInfo.InvariantCulture, out float x) &&
                    float.TryParse(split[1], NumberStyles.Float, CultureInfo.InvariantCulture, out float y) &&
                    double.TryParse(split[2], NumberStyles.Float, CultureInfo.InvariantCulture, out double time))
                {
                    var (cx, cy) = (Math.Clamp(x / 1e4f, 0f, 2f), Math.Clamp(y / 1e4f, 0f, 2f));
                    var (ccx, ccy) = ((cx + 0.5f) * (UI.SpacePlayfield.BASE_SIZE / 3f), (cy + 0.5f) * (UI.SpacePlayfield.BASE_SIZE / 3f));
                    beatmap.HitObjects.Add(new Note
                    {
                        Index = beatmap.HitObjects.Count + 1,
                        StartTime = time,
                        Samples = new List<HitSampleInfo> { new HitSampleInfo(HitSampleInfo.HIT_NORMAL, HitSampleInfo.BANK_NORMAL) },
                        X = ccx,
                        Y = ccy,
                        oX = cx,
                        oY = cy
                    });
                    return;
                }
                break;
        }

        base.ParseLine(beatmap, section, line);
    }
}

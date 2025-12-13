// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.Linq;
using osu.Game.Beatmaps;
using osu.Game.Localisation;
using osu.Game.Rulesets.Space.Objects;

namespace osu.Game.Rulesets.Space.Beatmaps
{
    public class SpaceBeatmap : Beatmap<SpaceHitObject>
    {
        public override IEnumerable<BeatmapStatistic> GetStatistics()
        {
            int notes = HitObjects.Count(s => s is not null);

            return
            [
                new BeatmapStatistic
                {
                    Name = BeatmapStatisticStrings.Notes,
                    CreateIcon = () => new BeatmapStatisticIcon(BeatmapStatisticsIconType.Circles),
                    Content = notes.ToString(),
                    BarDisplayLength = notes,
                },
            ];
        }
    }
}

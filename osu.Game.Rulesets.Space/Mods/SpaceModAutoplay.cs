
using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Space.Replays;
using osu.Game.Rulesets.Mods;

namespace osu.Game.Rulesets.Space.Mods
{
    public class SpaceModAutoplay : ModAutoplay
    {
        public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods)
            => new ModReplayData(new SpaceAutoGenerator(beatmap).Generate(), new ModCreatedUser { Username = "osu!space bot" });
    }
}

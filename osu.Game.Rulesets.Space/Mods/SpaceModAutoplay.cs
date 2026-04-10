using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Space.Replays;

namespace osu.Game.Rulesets.Space.Mods
{
    public class SpaceModAutoplay : ModAutoplay
    {
        public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new(
                new SpaceAutoGenerator(beatmap, mods).Generate(),
                new ModCreatedUser { Username = "Autoplay" }
            );
    }
}

// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Rulesets.Space.Replays;

namespace osu.Game.Rulesets.Space.Mods
{
    public class SpaceModCinema : ModCinema<SpaceHitObject>
    {
        public override ModReplayData CreateReplayData(IBeatmap beatmap, IReadOnlyList<Mod> mods) =>
            new(
                new SpaceAutoGenerator(beatmap, mods).Generate(),
                new ModCreatedUser { Username = "Autoplay" }
            );
    }
}

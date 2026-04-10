using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Space.Replays
{
    public class SpaceReplayFrame : ReplayFrame
    {
        public Vector2 Position;

        public override bool IsEquivalentTo(ReplayFrame other) =>
            other is SpaceReplayFrame spaceFrame
            && Time == spaceFrame.Time
            && Position == spaceFrame.Position;
    }
}


using System.Collections.Generic;
using System.Linq;
using osu.Game.Rulesets.Replays;
using osuTK;

namespace osu.Game.Rulesets.Space.Replays
{
    public class SpaceReplayFrame : ReplayFrame
    {
        public List<SpaceAction> Actions = new List<SpaceAction>();
        public Vector2 Position;

        public SpaceReplayFrame(SpaceAction? button = null)
        {
            if (button.HasValue)
                Actions.Add(button.Value);
        }

        public override bool IsEquivalentTo(ReplayFrame other)
            => other is SpaceReplayFrame freeformFrame && Time == freeformFrame.Time && Position == freeformFrame.Position && Actions.SequenceEqual(freeformFrame.Actions);
    }
}

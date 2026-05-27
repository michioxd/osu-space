using System.Collections.Generic;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Space.Replays;
using osu.Game.Rulesets.UI;
using osu.Game.Scoring;
using osuTK;

namespace osu.Game.Rulesets.Space.UI
{
    public partial class SpaceReplayRecorder(Score score) : ReplayRecorder<SpaceAction>(score)
    {
        protected override ReplayFrame HandleFrame(
            Vector2 mousePosition,
            List<SpaceAction> actions,
            ReplayFrame previousFrame
        ) => new SpaceReplayFrame(Time.Current, mousePosition);
    }
}

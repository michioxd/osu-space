using System.Collections.Generic;
using osu.Framework.Input.StateChanges;
using osu.Framework.Utils;
using osu.Game.Replays;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Space.Replays
{
    public class SpaceFramedReplayInputHandler : FramedReplayInputHandler<SpaceReplayFrame>
    {
        public SpaceFramedReplayInputHandler(Replay replay)
            : base(replay) { }

        protected override bool IsImportant(SpaceReplayFrame frame) => true;

        protected override void CollectReplayInputs(List<IInput> inputs)
        {
            var position = Interpolation.ValueAt(
                CurrentTime,
                StartFrame.Position,
                EndFrame.Position,
                StartFrame.Time,
                EndFrame.Time
            );

            inputs.Add(
                new MousePositionAbsoluteInput { Position = GamefieldToScreenSpace(position) }
            );
        }
    }
}

#nullable enable
using osu.Game.Beatmaps;
using osu.Game.Replays.Legacy;
using osu.Game.Rulesets.Replays;
using osu.Game.Rulesets.Replays.Types;
using osuTK;

namespace osu.Game.Rulesets.Space.Replays
{
    public class SpaceReplayFrame : ReplayFrame, IConvertibleReplayFrame
    {
        public Vector2 Position;

        public SpaceReplayFrame() { }

        public SpaceReplayFrame(double time, Vector2 position)
            : base(time)
        {
            Position = position;
        }

        public void FromLegacy(
            LegacyReplayFrame currentFrame,
            IBeatmap beatmap,
            ReplayFrame? lastFrame = null
        )
        {
            Position = currentFrame.Position;
        }

        public LegacyReplayFrame ToLegacy(IBeatmap beatmap)
        {
            return new LegacyReplayFrame(Time, Position.X, Position.Y, ReplayButtonState.None);
        }

        public override bool IsEquivalentTo(ReplayFrame other) =>
            other is SpaceReplayFrame spaceFrame
            && Time == spaceFrame.Time
            && Position == spaceFrame.Position;
    }
}

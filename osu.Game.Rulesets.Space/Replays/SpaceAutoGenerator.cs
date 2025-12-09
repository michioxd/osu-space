
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Rulesets.Replays;

namespace osu.Game.Rulesets.Space.Replays
{
    public class SpaceAutoGenerator : AutoGenerator<SpaceReplayFrame>
    {
        public new Beatmap<SpaceHitObject> Beatmap => (Beatmap<SpaceHitObject>)base.Beatmap;

        public SpaceAutoGenerator(IBeatmap beatmap)
            : base(beatmap)
        {
        }

        protected override void GenerateFrames()
        {
            Frames.Add(new SpaceReplayFrame());

            foreach (SpaceHitObject hitObject in Beatmap.HitObjects)
            {
                Frames.Add(new SpaceReplayFrame
                {
                    Time = hitObject.StartTime,
                    Position = hitObject.Position,
                    // todo: add required inputs and extra frames.
                });
            }
        }
    }
}

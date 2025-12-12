using osu.Game.Beatmaps;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Rulesets.Replays;
using System.Collections.Generic;
using osu.Game.Rulesets.Mods;
using osu.Framework.Utils;
using osu.Framework.Graphics;
using osuTK;
using System;

using osu.Game.Rulesets.Space.UI;

namespace osu.Game.Rulesets.Space.Replays
{
    public class SpaceAutoGenerator : AutoGenerator<SpaceReplayFrame>
    {
        public new Beatmap<SpaceHitObject> Beatmap => (Beatmap<SpaceHitObject>)base.Beatmap;

        public SpaceAutoGenerator(IBeatmap beatmap, IReadOnlyList<Mod> mods)
            : base(beatmap)
        {
        }

        protected override void GenerateFrames()
        {
            if (Beatmap.HitObjects.Count == 0)
                return;

            Frames.Add(new SpaceReplayFrame { Position = new Vector2(SpacePlayfield.BASE_SIZE / 2) });

            foreach (SpaceHitObject hitObject in Beatmap.HitObjects)
            {
                moveToHitObject(hitObject);

                Frames.Add(new SpaceReplayFrame
                {
                    Time = hitObject.StartTime,
                    Position = hitObject.Position,
                });
            }
        }

        private void moveToHitObject(SpaceHitObject h)
        {
            if (Frames.Count == 0) return;

            SpaceReplayFrame lastFrame = (SpaceReplayFrame)Frames[Frames.Count - 1];
            Vector2 startPosition = lastFrame.Position;
            Vector2 endPosition = h.Position;

            double reactionTime = 100;

            double moveStartTime = h.StartTime - h.TimePreempt + reactionTime;

            moveStartTime = Math.Max(moveStartTime, lastFrame.Time);
            moveStartTime = Math.Min(moveStartTime, h.StartTime);

            double frameInterval = 1000.0 / 60.0;

            for (double t = lastFrame.Time + frameInterval; t < moveStartTime; t += frameInterval)
            {
                Frames.Add(new SpaceReplayFrame
                {
                    Time = t,
                    Position = startPosition
                });
            }

            for (double t = moveStartTime; t < h.StartTime; t += frameInterval)
            {
                Vector2 pos = Interpolation.ValueAt(t, startPosition, endPosition, moveStartTime, h.StartTime, Easing.Out);
                pos = Vector2.Clamp(pos, Vector2.Zero, new Vector2(SpacePlayfield.BASE_SIZE));
                Frames.Add(new SpaceReplayFrame
                {
                    Time = t,
                    Position = pos
                });
            }
        }
    }
}

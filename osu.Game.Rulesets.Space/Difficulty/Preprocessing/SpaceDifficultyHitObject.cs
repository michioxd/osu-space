using System;
using osu.Game.Rulesets.Difficulty.Preprocessing;
using osu.Game.Rulesets.Space.Objects;
using osuTK;

namespace osu.Game.Rulesets.Space.Difficulty.Preprocessing
{
    public class SpaceDifficultyHitObject : DifficultyHitObject
    {
        public new SpaceHitObject BaseObject => (SpaceHitObject)base.BaseObject;

        public double JumpDistance { get; private set; }

        public double? Angle { get; private set; }

        public SpaceDifficultyHitObject(
            SpaceHitObject hitObject,
            SpaceHitObject lastObject,
            SpaceHitObject lastLastObject,
            double clockRate,
            System.Collections.Generic.List<DifficultyHitObject> objects,
            int index
        )
            : base(hitObject, lastObject, clockRate, objects, index)
        {
            JumpDistance = Vector2.Distance(hitObject.Position, lastObject.Position);
            if (lastLastObject != null)
            {
                Vector2 v1 = lastObject.Position - lastLastObject.Position;
                Vector2 v2 = hitObject.Position - lastObject.Position;

                float dot = Vector2.Dot(v1, v2);
                float det = v1.X * v2.Y - v1.Y * v2.X;

                Angle = Math.Abs(Math.Atan2(det, dot));
            }
        }
    }
}

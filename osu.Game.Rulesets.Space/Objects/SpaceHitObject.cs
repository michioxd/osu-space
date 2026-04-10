using osu.Game.Beatmaps;
using osu.Game.Beatmaps.ControlPoints;
using osu.Game.Rulesets.Judgements;
using osu.Game.Rulesets.Objects;
using osuTK;

namespace osu.Game.Rulesets.Space.Objects
{
    public class SpaceHitObject : HitObject
    {
        public bool IsHitOk { get; set; } = false;
        public float X { get; set; }
        public float Y { get; set; }

        public float oX
        {
            get => (X / (UI.SpacePlayfield.BASE_SIZE / 3f)) - 0.5f;
            set => X = (value + 0.5f) * (UI.SpacePlayfield.BASE_SIZE / 3f);
        }

        public float oY
        {
            get => (Y / (UI.SpacePlayfield.BASE_SIZE / 3f)) - 0.5f;
            set => Y = (value + 0.5f) * (UI.SpacePlayfield.BASE_SIZE / 3f);
        }

        public override Judgement CreateJudgement() => new Judgement();

        public Vector2 Position
        {
            get => new Vector2(X, Y);
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public int Index { get; set; }
        public int CellIndex { get; set; }

        public double TimePreempt = 600;

        protected override void ApplyDefaultsToSelf(
            ControlPointInfo controlPointInfo,
            IBeatmapDifficultyInfo difficulty
        )
        {
            base.ApplyDefaultsToSelf(controlPointInfo, difficulty);

            TimePreempt = (float)
                IBeatmapDifficultyInfo.DifficultyRange(difficulty.ApproachRate, 3500, 2500, 1500);
        }
    }
}

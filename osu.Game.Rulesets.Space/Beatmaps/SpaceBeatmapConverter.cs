using System;
using System.Collections.Generic;
using System.Threading;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Objects.Types;
using osu.Game.Rulesets.Space.UI;
using osuTK;
using System.Linq;

namespace osu.Game.Rulesets.Space.Beatmaps
{
    public class SpaceBeatmapConverter : BeatmapConverter<SpaceHitObject>
    {
        public SpaceBeatmapConverter(IBeatmap beatmap, Ruleset ruleset)
            : base(beatmap, ruleset)
        {
        }

        protected override Beatmap<SpaceHitObject> CreateBeatmap() => new SpaceBeatmap();

        public override bool CanConvert() => Beatmap.HitObjects.All(h => h is SpaceHitObject || (h is IHasXPosition && h is IHasYPosition));

        #region Tuning constants

        private const double gap_threshold_ms = 800;
        private const int history_length = 6;

        private const float velocity_stack = 0.08f;
        private const float velocity_normal = 0.38f;
        private const float velocity_jump = 0.82f;

        private const double stream_threshold_ms = 160;
        private const double burst_threshold_ms = 100;

        private const double w_direction = 35;
        private const double w_distance = 25;
        private const double w_recency_base = -50;
        private const double w_flow = 20;
        private const double w_anti_jack = -250;
        private const double w_variety = 3;
        private const double w_jump_preference = 42;
        private const double w_jack_preference = 15;
        private const double w_stream_preference = 7;
        private const double w_pattern_repeat_penalty = -12;
        private const double w_stream_repeat_penalty = -28;
        private const double w_pattern_switch_bonus = 14;

        #endregion

        #region Conversion state

        private enum PatternType
        {
            Jack,
            Stream,
            Jump,
        }

        private Vector2? prevOsuPos;
        private Vector2 prevOsuEndPos;
        private double prevTime;
        private int currentIndex;

        private readonly (int col, int row)[] history = new (int col, int row)[history_length];
        private int historyCount;

        #endregion

        #region History helpers

        private void resetHistory()
        {
            historyCount = 0;

            for (int i = 0; i < history_length; i++)
                history[i] = (-1, -1);
        }

        private void pushHistory(int col, int row)
        {
            for (int i = history_length - 1; i > 0; i--)
                history[i] = history[i - 1];

            history[0] = (col, row);

            if (historyCount < history_length)
                historyCount++;
        }

        private (int col, int row) getHistory(int stepsBack)
        {
            return stepsBack < historyCount ? history[stepsBack] : (-1, -1);
        }

        #endregion

        protected override Beatmap<SpaceHitObject> ConvertBeatmap(IBeatmap original, CancellationToken cancellationToken)
        {
            var beatmap = base.ConvertBeatmap(original, cancellationToken);

            for (int i = 0; i < beatmap.HitObjects.Count; i++)
            {
                var h = beatmap.HitObjects[i];

                if (h.GetType() == typeof(Note))
                {
                    beatmap.HitObjects[i] = new Note
                    {
                        StartTime = h.StartTime,
                        X = h.X,
                        Y = h.Y,
                        Index = h.Index,
                        CellIndex = h.CellIndex,
                        Samples = h.Samples,
                    };
                }
                else if (h.GetType() == typeof(SpaceHitObject))
                {
                    beatmap.HitObjects[i] = new SpaceHitObject
                    {
                        StartTime = h.StartTime,
                        X = h.X,
                        Y = h.Y,
                        Index = h.Index,
                        CellIndex = h.CellIndex,
                        Samples = h.Samples,
                    };
                }
            }

            return beatmap;
        }

        protected override IEnumerable<SpaceHitObject> ConvertHitObject(HitObject original, IBeatmap beatmap, CancellationToken cancellationToken)
        {
            var osuPos = (original as IHasPosition)?.Position ?? Vector2.Zero;
            double time = original.StartTime;

            Vector2 effectiveEndPos = osuPos;
            double? spanDuration = null;
            int repeatCount = 0;

            if (original is IHasPath pathObj)
            {
                Vector2 pathEnd = pathObj.Path.PositionAt(1);
                bool returnToStart = original is IHasRepeats rep && (rep.RepeatCount + 1) % 2 == 0;
                effectiveEndPos = returnToStart ? osuPos : osuPos + pathEnd;

                if (original is IHasRepeats repeats && original is IHasDuration duration)
                {
                    repeatCount = repeats.RepeatCount;
                    spanDuration = duration.Duration / (repeatCount + 1);
                }
            }

            yield return createConvertedNote(original, time, osuPos, original.Samples, resetFromOriginal: true, stateEndPos: osuPos);

            if (original is IHasPath sliderPath && original is IHasDuration sliderDuration)
            {
                Vector2 pathEnd = sliderPath.Path.PositionAt(1);

                if (original is IHasRepeats repeatsObj && spanDuration.HasValue && repeatCount > 0)
                {
                    for (int i = 1; i <= repeatCount; i++)
                    {
                        Vector2 reversePos = i % 2 == 1 ? osuPos + pathEnd : osuPos;

                        yield return createConvertedNote(
                            original,
                            time + spanDuration.Value * i,
                            reversePos,
                            i < repeatsObj.NodeSamples.Count ? repeatsObj.NodeSamples[i] : original.Samples,
                            resetFromOriginal: false,
                            stateEndPos: reversePos);
                    }
                }

                int tailNodeIndex = repeatCount + 1;

                yield return createConvertedNote(
                    original,
                    time + sliderDuration.Duration,
                    effectiveEndPos,
                    original is IHasRepeats tailRepeats && tailNodeIndex < tailRepeats.NodeSamples.Count
                        ? tailRepeats.NodeSamples[tailNodeIndex]
                        : original.Samples,
                    resetFromOriginal: false,
                    stateEndPos: effectiveEndPos);
            }
            else
                prevOsuEndPos = effectiveEndPos;
        }

        private Note createConvertedNote(HitObject original, double time, Vector2 osuPos, IList<osu.Game.Audio.HitSampleInfo> samples, bool resetFromOriginal, Vector2 stateEndPos)
        {
            int targetCol;
            int targetRow;
            double dt = time - prevTime;

            if (prevOsuPos == null || dt > gap_threshold_ms)
            {
                var initPos = resetFromOriginal ? mapOsuPositionToGrid(original) : mapOsuPositionToGrid(osuPos);
                targetCol = initPos.col;
                targetRow = initPos.row;
                resetHistory();
            }
            else
            {
                (targetCol, targetRow) = computeBestPosition(osuPos, time, dt);
            }

            pushHistory(targetCol, targetRow);
            prevOsuEndPos = stateEndPos;
            prevOsuPos = osuPos;
            prevTime = time;

            return new Note
            {
                Index = currentIndex++,
                Samples = samples,
                StartTime = time,
                X = (targetCol + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                Y = (targetRow + 0.5f) * (SpacePlayfield.BASE_SIZE / 3f),
                oX = targetCol,
                oY = targetRow
            };
        }

        private (int col, int row) computeBestPosition(Vector2 osuPos, double time, double dt)
        {
            Vector2 diff = osuPos - prevOsuEndPos;
            float dist = diff.Length;
            float velocity = dt > 0 ? dist / (float)dt : 0;
            float moveAngle = MathF.Atan2(diff.Y, diff.X);

            bool isStream = dt < stream_threshold_ms;
            bool isBurst = dt < burst_threshold_ms;

            float targetGridDist;

            if (velocity < velocity_stack)
                targetGridDist = 0;
            else if (velocity < velocity_normal)
                targetGridDist = 1f;
            else if (velocity < velocity_jump)
                targetGridDist = 1.41f;
            else
                targetGridDist = 2f;

            if (isBurst && targetGridDist > 1.41f)
                targetGridDist = 1.41f;

            var prev = getHistory(0);
            var prevPrev = getHistory(1);

            int bestCol = prev.col;
            int bestRow = prev.row;
            double bestScore = double.MinValue;

            for (int c = 0; c <= 2; c++)
            {
                for (int r = 0; r <= 2; r++)
                {
                    double score = scoreCandidate(
                        c, r, prev, prevPrev,
                        moveAngle, dist, targetGridDist,
                        isStream, isBurst, time, dt);

                    if (score > bestScore)
                    {
                        bestScore = score;
                        bestCol = c;
                        bestRow = r;
                    }
                }
            }

            return (bestCol, bestRow);
        }

        private double scoreCandidate(
            int c, int r,
            (int col, int row) prev,
            (int col, int row) prevPrev,
            float moveAngle, float dist, float targetGridDist,
            bool isStream, bool isBurst, double time, double dt)
        {
            double score = 0;

            int dc = c - prev.col;
            int dr = r - prev.row;
            float gridDist = MathF.Sqrt(dc * dc + dr * dr);
            int chebyshev = Math.Max(Math.Abs(dc), Math.Abs(dr));
            PatternType candidateType = classifyPattern(dc, dr);

            float distError = Math.Abs(gridDist - targetGridDist);
            score += w_distance * Math.Max(0, 1.0 - distError / 2.0);
            score += getPatternPreferenceScore(candidateType, dist, dt, targetGridDist, isStream, isBurst);

            if (dist > 8 && chebyshev > 0)
            {
                float gridAngle = MathF.Atan2(dr, dc);
                float angleDiff = normalizeAngle(moveAngle - gridAngle);
                score += w_direction * (1.0 - angleDiff / MathF.PI);
            }

            for (int h = 0; h < historyCount; h++)
            {
                var hist = getHistory(h);

                if (hist.col == c && hist.row == r)
                {
                    float decay = MathF.Pow(0.45f, h);
                    score += w_recency_base * decay;
                }
            }

            if (chebyshev == 0 && (isStream || isBurst))
            {
                double jackSuitability = Math.Clamp(1.0 - dist / 45f, 0, 1);

                score += w_anti_jack * (1.0 - jackSuitability);

                if (isBurst)
                    score += w_anti_jack * 0.6 * (1.0 - jackSuitability);
            }

            if (isStream && historyCount >= 2 && chebyshev > 0 && prevPrev.col >= 0)
            {
                int prevDc = prev.col - prevPrev.col;
                int prevDr = prev.row - prevPrev.row;

                if (prevDc != 0 || prevDr != 0)
                {
                    float prevAngle = MathF.Atan2(prevDr, prevDc);
                    float gridAngle = MathF.Atan2(dr, dc);
                    float flowDiff = normalizeAngle(gridAngle - prevAngle);

                    if (flowDiff < MathF.PI * 0.5f)
                        score += w_flow * (1.0 - flowDiff / (MathF.PI * 0.5f));
                    else if (flowDiff > MathF.PI * 0.67f)
                        score -= w_flow * 0.6;
                }
            }

            PatternType? previousPattern = getPreviousPatternType();

            if (previousPattern.HasValue)
            {
                int streak = getRecentPatternStreak(previousPattern.Value);

                if (candidateType == previousPattern.Value)
                {
                    double repeatPenalty = candidateType == PatternType.Stream ? w_stream_repeat_penalty : w_pattern_repeat_penalty;
                    score += repeatPenalty * Math.Min(streak, 3);
                }
                else if (streak >= 2)
                {
                    score += w_pattern_switch_bonus * Math.Min(streak, 3);
                }
            }

            score += Math.Sin(time * 0.073 + c * 17.3 + r * 11.7) * w_variety;

            return score;
        }

        private static PatternType classifyPattern(int dc, int dr)
        {
            if (dc == 0 && dr == 0)
                return PatternType.Jack;

            if (Math.Abs(dc) + Math.Abs(dr) == 1)
                return PatternType.Stream;

            return PatternType.Jump;
        }

        private double getPatternPreferenceScore(PatternType patternType, float dist, double dt, float targetGridDist, bool isStream, bool isBurst)
        {
            double jackIntent = Math.Max(
                Math.Clamp(1.0 - dist / 42f, 0, 1),
                Math.Clamp((0.6f - targetGridDist) / 0.6f, 0, 1));

            double streamIntent = Math.Clamp(1.0 - Math.Abs(targetGridDist - 1f), 0, 1) * 0.75;

            if (isStream)
                streamIntent = Math.Min(1.0, streamIntent + 0.12);

            if (isBurst)
                streamIntent = Math.Min(1.0, streamIntent + 0.05);

            double jumpIntent = Math.Max(
                Math.Clamp((targetGridDist - 0.9f) / 0.7f, 0, 1),
                Math.Clamp((dist - 60f) / 120f, 0, 1));

            jumpIntent = Math.Max(jumpIntent, Math.Clamp((dt - 120) / 160.0, 0, 1));

            return patternType switch
            {
                PatternType.Jack => jackIntent * w_jack_preference,
                PatternType.Stream => streamIntent * w_stream_preference,
                PatternType.Jump => jumpIntent * w_jump_preference,
                _ => 0,
            };
        }

        private PatternType? getPreviousPatternType()
        {
            if (historyCount < 2)
                return null;

            var current = getHistory(0);
            var previous = getHistory(1);

            if (previous.col < 0 || previous.row < 0)
                return null;

            return classifyPattern(current.col - previous.col, current.row - previous.row);
        }

        private int getRecentPatternStreak(PatternType patternType)
        {
            int streak = 0;

            for (int i = 0; i + 1 < historyCount; i++)
            {
                var newer = getHistory(i);
                var older = getHistory(i + 1);

                if (older.col < 0 || older.row < 0)
                    break;

                if (classifyPattern(newer.col - older.col, newer.row - older.row) != patternType)
                    break;

                streak++;
            }

            return streak;
        }

        private static float normalizeAngle(float angle)
        {
            angle %= (2 * MathF.PI);

            if (angle > MathF.PI) angle -= 2 * MathF.PI;
            else if (angle < -MathF.PI) angle += 2 * MathF.PI;

            return Math.Abs(angle);
        }

        private static (int col, int row) mapOsuPositionToGrid(HitObject hitObject)
        {
            float x = ((IHasXPosition)hitObject).X;
            float y = ((IHasYPosition)hitObject).Y;
            float cellSize = SpacePlayfield.BASE_SIZE / 3f;

            return (
                (int)Math.Clamp(x / cellSize, 0, 2),
                (int)Math.Clamp(y / cellSize, 0, 2)
            );
        }

        private static (int col, int row) mapOsuPositionToGrid(Vector2 position)
        {
            float cellSize = SpacePlayfield.BASE_SIZE / 3f;

            return (
                (int)Math.Clamp(position.X / cellSize, 0, 2),
                (int)Math.Clamp(position.Y / cellSize, 0, 2)
            );
        }
    }
}

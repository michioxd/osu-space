
using System.Collections.Generic;
using osu.Framework.Allocation;
using osu.Framework.Input;
using osu.Game.Beatmaps;
using osu.Game.Input.Handlers;
using osu.Game.Replays;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Rulesets.Space.Objects.Drawables;
using osu.Game.Rulesets.Space.Replays;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;

namespace osu.Game.Rulesets.Space.UI
{
    [Cached]
    public partial class DrawableSpaceRuleset : DrawableRuleset<SpaceHitObject>
    {
        public DrawableSpaceRuleset(SpaceRuleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod> mods = null)
            : base(ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new SpacePlayfield
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both,
        };

        private SpaceFramedReplayInputHandler replayInputHandler;

        protected override ReplayInputHandler CreateReplayInputHandler(Replay replay) => replayInputHandler = new SpaceFramedReplayInputHandler(replay);

        public override DrawableHitObject<SpaceHitObject> CreateDrawableRepresentation(SpaceHitObject h) => new DrawableSpaceHitObject(h);

        protected override PassThroughInputManager CreateInputManager() => new SpaceInputManager(Ruleset?.RulesetInfo);

        protected override void LoadComplete()
        {
            base.LoadComplete();
            if (replayInputHandler != null)
            {
                if (Playfield is SpacePlayfield spacePlayfield)
                    replayInputHandler.GamefieldToScreenSpace = spacePlayfield.GamefieldToScreenSpace;
            }
        }
    }
}

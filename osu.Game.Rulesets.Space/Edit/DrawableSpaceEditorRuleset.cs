using System.Collections.Generic;
using osu.Game.Beatmaps;
using osu.Game.Rulesets.Mods;
using osu.Game.Rulesets.Space.UI;
using osu.Game.Rulesets.UI;
using osu.Framework.Graphics;
using osu.Game.Rulesets.Objects.Drawables;
using osu.Game.Rulesets.Space.Objects;

namespace osu.Game.Rulesets.Space.Edit
{
    public partial class DrawableSpaceEditorRuleset : DrawableSpaceRuleset
    {
        public DrawableSpaceEditorRuleset(Ruleset ruleset, IBeatmap beatmap, IReadOnlyList<Mod>? mods)
            : base((SpaceRuleset)ruleset, beatmap, mods)
        {
        }

        protected override Playfield CreatePlayfield() => new SpaceEditorPlayfield()
        {
            Anchor = Anchor.Centre,
            Origin = Anchor.Centre,
            RelativeSizeAxes = Axes.Both
        };

        public override DrawableHitObject<SpaceHitObject> CreateDrawableRepresentation(SpaceHitObject h)
            => new DrawableSpaceEditorHitObject(h);
    }
}

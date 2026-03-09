using osu.Framework.Allocation;
using osu.Framework.Graphics;
using osu.Framework.Input.Bindings;
using osu.Game.Rulesets.Space.UI;
using osu.Game.Rulesets.UI;

namespace osu.Game.Rulesets.Space
{
    public partial class SpaceInputManager : RulesetInputManager<SpaceAction>
    {
        public SpaceInputManager(RulesetInfo ruleset)
            : base(ruleset, 0, SimultaneousBindingMode.Unique)
        {
        }

        [BackgroundDependencyLoader]
        private void load()
        {
            Add(new SpaceTouchInputMapper(this) { RelativeSizeAxes = Axes.Both });
        }
    }

    public enum SpaceAction
    {
    }
}

using System.Linq;
using osu.Game.Rulesets.Objects;
using osu.Game.Rulesets.Space.Objects;
using osu.Game.Screens.Edit.Compose.Components;

namespace osu.Game.Rulesets.Space.Edit.Compose.Components
{
    public partial class SpaceHitObjectInspector : HitObjectInspector
    {
        protected override void AddInspectorValues(HitObject[] objects)
        {
            switch (objects.Length)
            {
                case 0:
                    AddValue("No objects selected");
                    break;

                case 1:
                    if (objects[0] is not SpaceHitObject selected)
                    {
                        AddValue("Selected object is not a Space hit object");
                        break;
                    }

                    AddHeader("Time");
                    AddValue($"{selected.StartTime:#,0.##}ms");

                    AddHeader("Position");
                    AddValue($"x:{selected.oX:#,0.##}");
                    AddValue($"y:{selected.oY:#,0.##}");

                    break;

                default:
                    AddHeader("Selected Objects");
                    AddValue($"{objects.Length:#,0.##}");

                    AddHeader("Start Time");
                    AddValue($"{objects.Min(o => o.StartTime):#,0.##}ms");

                    AddHeader("End Time");
                    AddValue($"{objects.Max(o => o.GetEndTime()):#,0.##}ms");
                    break;
            }
        }
    }
}

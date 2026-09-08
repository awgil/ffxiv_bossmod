using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;
using System.Reflection;

namespace BossMod.Stormblood.Ultimate.UCOB;

[ConfigDisplay(Order = 0x200, Parent = typeof(StormbloodConfig))]
public class UCOBConfig() : ConfigNode()
{
    [PropertyDisplay("P1 Fireball 1: add stack-avoid hints for tanks and healer")]
    public bool P1Fireball1LBHints = true;

    [PropertyDisplay("P3 Quickmarch/Heavensfall Trio: safespot assignments (assuming bahamut is relative north/up, group L goes left; L1/R1 closest to bosses)")]
    [GroupDetails(["L1", "L2", "L3", "L4", "R1", "R2", "R3", "R4"])]
    [GroupPreset("Hector: THMR", [0, 4, 1, 5, 2, 6, 3, 7])]
    [GroupPreset("LPDU: HTTH/RMMR", [1, 2, 0, 3, 5, 6, 4, 7])]
    public GroupAssignmentUnique P3QuickmarchTrioAssignments = new() { Assignments = [0, 4, 1, 5, 2, 6, 3, 7] };

    [PropertyDisplay("P3 Heavensfall Trio: tower priority, CW starting from nael")]
    [GroupDetails(["0", "1", "2", "3", "4", "5", "6", "7"])]
    [GroupPreset("Hector: THMR, G1 CCW, G2 CW", [7, 0, 6, 1, 5, 2, 4, 3])]
    public GroupAssignmentUnique P3HeavensfallTrioTowers = new() { Assignments = [7, 0, 6, 1, 5, 2, 4, 3] };

    [SectionStart("AI-only settings")]
    [PropertyDisplay("P1: players assigned to soak Plummet (for LB)", customRenderer: typeof(RolesRenderer))]
    public BitMask P1PlummetTargets = new();
}

public class RolesRenderer : PropertyRenderer
{
    public override bool Draw(PropertyDisplayAttribute attrs, bool nested, ConfigNode node, FieldInfo member, object value, ConfigRoot root, UITree tree, WorldState ws)
    {
        var cfg = (UCOBConfig)node;

        ConfigUI.DrawHelp(attrs.Tooltip, nested);

        var modified = false;

        foreach (var _ in tree.Node(attrs.Label, false))
        {
            using (ImRaii.PushIndent())
            {
                for (var i = 2; i < 8; i++)
                {
                    var assignment = (PartyRolesConfig.Assignment)i;
                    var isChecked = cfg.P1PlummetTargets[i];
                    if (ImGui.Checkbox($"{assignment}###plummet{i}", ref isChecked))
                    {
                        if (isChecked)
                            cfg.P1PlummetTargets.Set(i);
                        else
                            cfg.P1PlummetTargets.Clear(i);
                        modified = true;
                    }
                    if (i < 7)
                        ImGui.SameLine();
                }
            }
        }

        return modified;
    }
}

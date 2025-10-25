using Dalamud.Bindings.ImGui;

namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class Q01TheFinalVerseConfig : ConfigNode
{
    // TODO: fixed positions strat where everyone stacks at the end
    public enum SinBearer
    {
        [PropertyDisplay("Don't assume any order")]
        None,
        [PropertyDisplay("MMRR -> Roles -> MMRR (accelerated 1st pass)")]
        AccelFirst,
    }

    [PropertyDisplay("Sin Bearer pass order", tooltip: "Each party member needs a unique role in Party Roles Assignment for pass order to be calculated correctly")]
    public SinBearer SinBearerOrder = SinBearer.AccelFirst;

    public struct FlamebornAssignment
    {
        public Role NW;
        public Role NE;
        public Role Center;
        public Role SW;
        public Role SE;

        public static FlamebornAssignment Build(params Role[] roles) => new()
        {
            NW = roles.BoundSafeAt(0),
            NE = roles.BoundSafeAt(1),
            Center = roles.BoundSafeAt(2),
            SW = roles.BoundSafeAt(3),
            SE = roles.BoundSafeAt(4),
        };

        public readonly Role[] AsArray() => [NW, NE, Center, SW, SE];

        public static readonly FlamebornAssignment SingleMerge = Build(Role.Tank, Role.Melee, Role.Ranged, Role.Healer, Role.Ranged);
        public static readonly FlamebornAssignment FarmValley = Build(Role.Tank, Role.Ranged, Role.Healer, Role.Melee, Role.Ranged);
        public static readonly FlamebornAssignment DoubleCross = Build(Role.Tank, Role.Melee, Role.Tank, Role.Ranged, Role.Healer);
        public static readonly FlamebornAssignment ThreeWayMerge = Build(Role.Ranged, Role.Healer, Role.Tank, Role.Melee, Role.Healer);
    }

    public FlamebornAssignment FlamebornAssignments;

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        var modified = false;

        foreach (var _ in tree.Node("Fevered Flame assignments", contextMenu: () =>
        {
            if (ImGui.MenuItem("Single Merge (pastebin)"))
            {
                FlamebornAssignments = FlamebornAssignment.SingleMerge;
                modified = true;
            }
            if (ImGui.MenuItem("Merge Farm Merge Valley (pastebin)"))
            {
                FlamebornAssignments = FlamebornAssignment.FarmValley;
                modified = true;
            }
            if (ImGui.MenuItem("Double Cross (pastebin)"))
            {
                FlamebornAssignments = FlamebornAssignment.DoubleCross;
                modified = true;
            }
            if (ImGui.MenuItem("Triple Merge"))
            {
                FlamebornAssignments = FlamebornAssignment.ThreeWayMerge;
                modified = true;
            }
            ImGui.Separator();
            if (ImGui.MenuItem("Reset to default"))
            {
                FlamebornAssignments = default;
                modified = true;
            }
        }))
        {
            modified |= UICombo.Enum("Northwest", ref FlamebornAssignments.NW);
            modified |= UICombo.Enum("Northeast", ref FlamebornAssignments.NE);
            modified |= UICombo.Enum("Center", ref FlamebornAssignments.Center);
            modified |= UICombo.Enum("Southwest", ref FlamebornAssignments.SW);
            modified |= UICombo.Enum("Southeast", ref FlamebornAssignments.SE);
        }

        if (modified)
            Modified.Fire();
    }
}

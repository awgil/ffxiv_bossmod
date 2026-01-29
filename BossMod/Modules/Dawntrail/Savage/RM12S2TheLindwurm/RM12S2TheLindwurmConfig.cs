using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

[ConfigDisplay(Parent = typeof(DawntrailConfig), Order = -1)]
public class RM12S2TheLindwurmConfig : ConfigNode
{
    public Replication2Tethers Rep2Assignments = Replication2Tethers.DN;

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        Rep2Assignments.DrawCustom(tree, Modified);
    }
}

public class Replication2Tethers
{
    public enum Role
    {
        Boss,
        None,
        Cone1,
        Cone2,
        Stack1,
        Stack2,
        Defam1,
        Defam2,
    }

    public Role[] RolesOrdered = new Role[8];
    public Order RelativeNorth = Order.N;

    public bool IsValid() => RolesOrdered.Distinct().Count() == 8;

    public enum Order
    {
        [PropertyDisplay("North")]
        N,
        [PropertyDisplay("Northeast")]
        NE,
        [PropertyDisplay("East")]
        E,
        [PropertyDisplay("Southeast")]
        SE,
        [PropertyDisplay("South")]
        S,
        [PropertyDisplay("Southwest")]
        SW,
        [PropertyDisplay("West")]
        W,
        [PropertyDisplay("Northwest")]
        NW
    }

    public void DrawCustom(UITree tree, Event modified)
    {
        foreach (var _ in tree.Node("Replication 2: clone <-> tether assignments", contextMenu: () => DrawContextMenu(modified)))
        {
            if (UICombo.Enum("Relative north (for tether priority)", ref RelativeNorth))
                modified.Fire();

            using (var table = ImRaii.Table("tab2", 10, ImGuiTableFlags.SizingFixedFit))
            {
                if (table)
                {
                    foreach (var r in typeof(Order).GetEnumValues())
                        ImGui.TableSetupColumn(r.ToString(), ImGuiTableColumnFlags.None, 25);
                    ImGui.TableSetupColumn("Role");
                    ImGui.TableHeadersRow();

                    foreach (var r in typeof(Role).GetEnumValues().Cast<Role>())
                    {
                        ImGui.TableNextRow();
                        for (var i = 0; i < RolesOrdered.Length; i++)
                        {
                            ImGui.TableNextColumn();
                            if (ImGui.RadioButton($"###r{r}_i{i}", RolesOrdered[i] == r))
                            {
                                RolesOrdered[i] = r;
                                modified.Fire();
                            }
                        }
                        ImGui.TableNextColumn();
                        ImGui.TextUnformatted(r.ToString().Replace("1", " (CW)", StringComparison.InvariantCultureIgnoreCase).Replace("2", " (CCW)", StringComparison.InvariantCultureIgnoreCase));
                    }
                }
            }

            if (IsValid())
            {
                using (ImRaii.PushColor(ImGuiCol.Text, 0xFF00FF00))
                    ImGui.TextUnformatted("All good!");
            }
            else
            {
                using (ImRaii.PushColor(ImGuiCol.Text, 0xFF00FFFF))
                    ImGui.TextUnformatted("Invalid assignments: roles must be unique");
            }
        }
    }

    void DrawContextMenu(Event modified)
    {
        if (ImGui.MenuItem("DN: true North; stack paired with stack"))
        {
            Array.Copy(DN.RolesOrdered, RolesOrdered, 8);
            RelativeNorth = DN.RelativeNorth;
            modified.Fire();
        }
        if (ImGui.MenuItem("Banana Codex: West-relative; stack paired with defam"))
        {
            Array.Copy(BC.RolesOrdered, RolesOrdered, 8);
            RelativeNorth = BC.RelativeNorth;
            modified.Fire();
        }
    }

    public static readonly Replication2Tethers DN = new()
    {
        RolesOrdered = [Role.Boss, Role.Cone1, Role.Stack1, Role.Defam1, Role.None, Role.Defam2, Role.Stack2, Role.Cone2],
        RelativeNorth = Order.N
    };

    public static readonly Replication2Tethers BC = new()
    {
        RolesOrdered = [Role.Boss, Role.Stack1, Role.Cone1, Role.Defam1, Role.None, Role.Defam2, Role.Cone2, Role.Stack2],
        RelativeNorth = Order.W
    };
}

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

public enum Clockspot
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

public enum Replication2Role
{
    [PropertyDisplay("Boss")]
    Boss,
    [PropertyDisplay("None")]
    None,
    [PropertyDisplay("Cone (CW)")]
    Cone1,
    [PropertyDisplay("Cone (CCW)")]
    Cone2,
    [PropertyDisplay("Stack (CW)")]
    Stack1,
    [PropertyDisplay("Stack (CCW)")]
    Stack2,
    [PropertyDisplay("Defam (CW)")]
    Defam1,
    [PropertyDisplay("Defam (CCW)")]
    Defam2,
}

public class Replication2Tethers
{
    public Replication2Role[] RolesOrdered = new Replication2Role[8];
    public Clockspot RelativeNorth = Clockspot.N;

    public Replication2Role this[int index] => IsValid() ? RolesOrdered[index] : Replication2Role.None;
    public Replication2Role this[Clockspot index] => IsValid() ? RolesOrdered[(int)index] : Replication2Role.None;

    public bool IsValid() => RolesOrdered.Distinct().Count() == 8;

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
                    foreach (var r in typeof(Clockspot).GetEnumValues())
                        ImGui.TableSetupColumn(r.ToString(), ImGuiTableColumnFlags.None, 25);
                    ImGui.TableSetupColumn("Role");
                    ImGui.TableHeadersRow();

                    foreach (var r in typeof(Replication2Role).GetEnumValues().Cast<Replication2Role>())
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
                        ImGui.TextUnformatted(UICombo.EnumString(r));
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
        RolesOrdered = [Replication2Role.Boss, Replication2Role.Cone1, Replication2Role.Stack1, Replication2Role.Defam1, Replication2Role.None, Replication2Role.Defam2, Replication2Role.Stack2, Replication2Role.Cone2],
        RelativeNorth = Clockspot.N
    };

    public static readonly Replication2Tethers BC = new()
    {
        RolesOrdered = [Replication2Role.Boss, Replication2Role.Stack1, Replication2Role.Cone1, Replication2Role.Defam1, Replication2Role.None, Replication2Role.Defam2, Replication2Role.Cone2, Replication2Role.Stack2],
        RelativeNorth = Clockspot.W
    };
}

#pragma warning disable CA1708 // false positive
public static class WurmExtensions
{
    extension(Clockspot c)
    {
        public Angle Angle => AngleExtensions.Degrees(c switch
        {
            Clockspot.N => 180,
            Clockspot.NE => 135,
            Clockspot.E => 90,
            Clockspot.SE => 45,
            Clockspot.SW => -45,
            Clockspot.W => -90,
            Clockspot.NW => -135,
            _ => 0
        });

        public int SpawnOrder => c switch
        {
            Clockspot.N or Clockspot.S => 0,
            Clockspot.NE or Clockspot.SW => 1,
            Clockspot.E or Clockspot.W => 2,
            Clockspot.SE or Clockspot.NW => 3,
            _ => -1
        };

        public int Group => c switch
        {
            Clockspot.N or Clockspot.NE or Clockspot.E or Clockspot.SE => 0,
            _ => 1
        };

        public string HumanReadable => (c.Group == 0 ? "N" : "S") + (c.SpawnOrder + 1).ToString();
    }

    extension(Replication2Role r)
    {
        public bool IsDefam => r is Replication2Role.Defam1 or Replication2Role.Defam2;
        public bool IsStack => r is Replication2Role.Stack1 or Replication2Role.Stack2;
        public bool IsCone => r is Replication2Role.Cone1 or Replication2Role.Cone2;
    }
}

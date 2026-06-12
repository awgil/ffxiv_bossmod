using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility.Raii;

namespace BossMod;

[ConfigDisplay(Name = "Party Roles", Order = 2)]
public class PartyRolesConfig : ConfigNode
{
    public enum Assignment { MT, OT, H1, H2, M1, M2, R1, R2, Unassigned }

    public Dictionary<ulong, Assignment> Assignments = [];

    public Assignment this[ulong contentID] => Assignments.GetValueOrDefault(contentID, Assignment.Unassigned);

    // return either array of assigned roles per party slot (if each role is assigned exactly once) or empty array (if assignments are invalid)
    public Assignment[] AssignmentsPerSlot(PartyState party)
    {
        int[] counts = new int[(int)Assignment.Unassigned];
        Assignment[] res = Utils.MakeArray(PartyState.MaxPartySize, Assignment.Unassigned);
        for (int i = 0; i < PartyState.MaxPartySize; ++i)
        {
            var r = this[party.Members[i].ContentId];
            if (r == Assignment.Unassigned)
                return [];
            if (counts[(int)r]++ > 0)
                return [];
            res[i] = r;
        }
        return res;
    }

    // return either array of party slots per assigned role (if each role is assigned exactly once) or empty array (if assignments are invalid)
    public int[] SlotsPerAssignment(PartyState party)
    {
        int[] res = Utils.MakeArray((int)Assignment.Unassigned, PartyState.MaxPartySize);
        for (int i = 0; i < PartyState.MaxPartySize; ++i)
        {
            var r = this[party.Members[i].ContentId];
            if (r == Assignment.Unassigned)
                return [];
            if (res[(int)r] != PartyState.MaxPartySize)
                return [];
            res[(int)r] = i;
        }
        return res;
    }

    // return array of effective roles per party slot
    public Role[] EffectiveRolePerSlot(PartyState party)
    {
        var res = new Role[PartyState.MaxPartySize];
        for (int i = 0; i < PartyState.MaxPartySize; ++i)
        {
            res[i] = this[party.Members[i].ContentId] switch
            {
                Assignment.MT or Assignment.OT => Role.Tank,
                Assignment.H1 or Assignment.H2 => Role.Healer,
                Assignment.M1 or Assignment.M2 => Role.Melee,
                Assignment.R1 or Assignment.R2 => Role.Ranged,
                _ => party[i]?.Role ?? Role.None
            };
        }
        return res;
    }

    record struct PartyMember(ulong CID, string Name, Class Class, Assignment Assignment);

    public override void DrawCustom(UITree tree, WorldState ws)
    {
        List<PartyMember> party = [];
        using (var table = ImRaii.Table("tab2", 10, ImGuiTableFlags.SizingFixedFit))
        {
            if (table)
            {
                foreach (var r in typeof(Assignment).GetEnumValues())
                    ImGui.TableSetupColumn(r.ToString(), ImGuiTableColumnFlags.None, 25);
                ImGui.TableSetupColumn("Name");
                ImGui.TableHeadersRow();

                for (int i = 0; i < PartyState.MaxPartySize; ++i)
                {
                    ref var m = ref ws.Party.Members[i];
                    if (m.IsValid())
                        party.Add(new(m.ContentId, m.Name, ws.Party[i]?.Class ?? Class.None, this[m.ContentId]));
                }
                party.SortBy(e => e.Class.GetRole());

                foreach (var (contentID, name, classRole, assignment) in party)
                {
                    ImGui.TableNextRow();
                    foreach (var r in typeof(Assignment).GetEnumValues().Cast<Assignment>())
                    {
                        ImGui.TableNextColumn();
                        if (ImGui.RadioButton($"###{contentID:X}:{r}", assignment == r))
                        {
                            if (r != Assignment.Unassigned)
                                Assignments[contentID] = r;
                            else
                                Assignments.Remove(contentID);
                            Modified.Fire();
                        }
                    }
                    ImGui.TableNextColumn();
                    ImGui.TextUnformatted($"({classRole.GetRole().ToString()[0]}) {name}");
                }
            }
        }

        if (AssignmentsPerSlot(ws.Party).Length == 0)
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, 0xff00ffff);
            ImGui.TextUnformatted("Invalid assignments: there should be exactly one raid member per role");
        }
        else
        {
            using var color = ImRaii.PushColor(ImGuiCol.Text, 0xff00ff00);
            ImGui.TextUnformatted("All good!");
        }

        using var _ = ImRaii.Disabled(party.Count != 8);
        if (ImGui.Button("Auto-assign slots"))
            TryAutoAssign(party);
    }

    void TryAutoAssign(List<PartyMember> members)
    {
        // melee prio is invariably group-specific so we just assign it to both melees and make the user fix it
        Assignment[] assOrdered = [Assignment.MT, Assignment.OT, Assignment.H1, Assignment.H2, Assignment.M1, Assignment.M1, Assignment.R1, Assignment.R2];

        var (numMelee, numRanged) = members.Aggregate((0, 0), (acc, pm) => pm.Class.GetRole() switch
        {
            Role.Melee => (acc.Item1 + 1, acc.Item2),
            Role.Ranged => (acc.Item1, acc.Item2 + 1),
            _ => acc
        });

        foreach (var (m, a) in members.OrderBy(r => (r.Class.GetRole(), RolePrio(r.Class, numMelee == 1 && numRanged == 3))).Zip(assOrdered))
            Assignments[m.CID] = a;

        Modified.Fire();
    }

    static int RolePrio(Class c, bool doubleCaster) => (c.GetClassCategory(), c) switch
    {
        (_, Class.WAR) => 0,
        (_, Class.PLD or Class.GNB) => 1,
        (_, Class.DRK) => 2,

        (_, Class.AST or Class.WHM) => 0,
        (_, Class.SGE or Class.SCH) => 1,

        (ClassCategory.Melee, _) => 0,
        (_, Class.RDM or Class.BLM) when doubleCaster => 1,
        (ClassCategory.PhysRanged, _) => 2,
        (ClassCategory.Caster, _) => 3,

        _ => 10
    };
}

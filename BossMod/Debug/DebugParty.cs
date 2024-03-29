using Dalamud.Memory;
using FFXIVClientStructs.FFXIV.Client.Game.Group;
using ImGuiNET;

namespace BossMod;

class DebugParty
{
    PartyAlliance _alliance = new();

    public void DrawPartyDalamud()
    {
        // note: alliance doesn't seem to work correctly, IsAlliance is always false and AllianceMembers are not filled...
        // new member is always added to the end, if member is removed, remaining members are shifted to fill the gap
        ImGui.TextUnformatted($"Num members: {Service.PartyList.Length}, alliance={Service.PartyList.IsAlliance}, id={Service.PartyList.PartyId}");

        ImGui.BeginTable("party", 6);
        ImGui.TableSetupColumn("ContentID");
        ImGui.TableSetupColumn("ObjectID");
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Zone");
        ImGui.TableSetupColumn("World");
        ImGui.TableSetupColumn("Position");
        ImGui.TableHeadersRow();
        foreach (var member in Service.PartyList)
        {
            ImGui.TableNextRow();
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.ContentId:X}");
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.ObjectId:X}");
            ImGui.TableNextColumn(); ImGui.TextUnformatted(member.Name.ToString());
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.Territory.Id}");
            ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.World.Id}");
            ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.Vec3String(member.Position));
        }
        ImGui.EndTable();
    }

    public unsafe void DrawPartyCustom()
    {
        // note: alliance slots, unlike normal slots, are more permanent - if a player leaves, other players retain their indices (leaving gaps)
        // also content ID for all alliance members always seems to be 0; this isn't a huge deal, since alliance members are always in the same zone and thus have valid object IDs
        ImGui.TextUnformatted($"Num members: {_alliance.NumPartyMembers}, alliance={(_alliance.IsAlliance ? (_alliance.IsSmallGroupAlliance ? "small-group" : "yes") : "no")}");

        ImGui.BeginTable("party-custom", 7, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("Index");
        ImGui.TableSetupColumn("ContentID");
        ImGui.TableSetupColumn("ObjectID");
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Zone");
        ImGui.TableSetupColumn("World");
        ImGui.TableSetupColumn("Position");
        ImGui.TableHeadersRow();
        for (int i = 0; i < _alliance.NumPartyMembers; ++i)
            DrawPartyMember($"P{i}", _alliance.PartyMember(i));
        for (int i = 0; i < PartyAlliance.MaxAllianceMembers; ++i)
            DrawPartyMember($"A{i}", _alliance.AllianceMember(i));
        ImGui.EndTable();
    }

    private unsafe void DrawPartyMember(string index, PartyMember* member)
    {
        if (member == null)
            return;
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.TextUnformatted(index);
        ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member->ContentID:X}");
        ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member->ObjectID:X}");
        ImGui.TableNextColumn(); ImGui.TextUnformatted(MemoryHelper.ReadSeString((IntPtr)member->Name, 0x40).ToString());
        ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member->TerritoryType}");
        ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member->HomeWorld}");
        ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.Vec3String(new(member->X, member->Y, member->Z)));
    }
}

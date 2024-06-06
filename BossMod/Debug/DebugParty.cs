using FFXIVClientStructs.FFXIV.Client.Game.Group;
using FFXIVClientStructs.FFXIV.Client.Game.Object;
using FFXIVClientStructs.FFXIV.Client.Game.UI;
using ImGuiNET;

namespace BossMod;

class DebugParty
{
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

    public unsafe void DrawPartyCustom(bool secondary)
    {
        // note: alliance slots, unlike normal slots, are more permanent - if a player leaves, other players retain their indices (leaving gaps)
        // also content ID for all alliance members always seems to be 0; this isn't a huge deal, since alliance members are always in the same zone and thus have valid object IDs
        var gm = GroupManager.Instance();
        if (secondary)
            gm += 1;
        var ui = UIState.Instance();
        ImGui.TextUnformatted($"Num members: {gm->MemberCount}, alliance={(!gm->IsAlliance ? "no" : gm->IsSmallGroupAlliance ? "small-group" : "yes")}, has-helpers={ui->Buddy.DutyHelperInfo.HasHelpers}");

        ImGui.BeginTable("party-custom", 7, ImGuiTableFlags.Resizable);
        ImGui.TableSetupColumn("Index");
        ImGui.TableSetupColumn("ContentId");
        ImGui.TableSetupColumn("EntityId");
        ImGui.TableSetupColumn("Name");
        ImGui.TableSetupColumn("Zone");
        ImGui.TableSetupColumn("World");
        ImGui.TableSetupColumn("Position");
        ImGui.TableHeadersRow();
        for (int i = 0; i < gm->MemberCount; ++i)
            DrawPartyMember($"P{i}", ref gm->PartyMembers[i]);
        for (int i = 0; i < gm->AllianceMembers.Length; ++i)
            if (gm->AllianceMembers[i].IsValidAllianceMember)
                DrawPartyMember($"A{i}", ref gm->AllianceMembers[i]);
        for (int i = 0; i < ui->Buddy.DutyHelperInfo.ENpcIds.Length; ++i)
        {
            var id = ui->Buddy.DutyHelperInfo.DutyHelpers[i].EntityId;
            if (id == 0xE0000000)
                continue;
            var obj = GameObjectManager.Instance()->Objects.GetObjectByEntityId(id);
            ImGui.TableNextRow();
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"B{i}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{ui->Buddy.DutyHelperInfo.ENpcIds[i]}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{id:X}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{obj->NameString}");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("---");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted("---");
            ImGui.TableNextColumn();
            ImGui.TextUnformatted($"{Utils.Vec3String(obj->Position)}");
        }
        ImGui.EndTable();
    }

    private unsafe void DrawPartyMember(string index, ref PartyMember member)
    {
        ImGui.TableNextRow();
        ImGui.TableNextColumn(); ImGui.TextUnformatted(index);
        ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.ContentId:X}");
        ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.EntityId:X}");
        ImGui.TableNextColumn(); ImGui.TextUnformatted(member.NameString);
        ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.TerritoryType}");
        ImGui.TableNextColumn(); ImGui.TextUnformatted($"{member.HomeWorld}");
        ImGui.TableNextColumn(); ImGui.TextUnformatted(Utils.Vec3String(new(member.X, member.Y, member.Z)));
    }
}

using Dalamud.Game.ClientState.Party;
using ImGuiNET;
using System;

namespace BossMod
{
    class DebugParty
    {
        public void DrawParty()
        {
            // note: alliance doesn't seem to work correctly, IsAlliance is always false and AllianceMembers are not filled...
            // new member is always added to the end, if member is removed, remaining members are shifted to fill the gap
            ImGui.Text($"Num members: {Service.PartyList.Length}, alliance={Service.PartyList.IsAlliance}, id={Service.PartyList.PartyId}");

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
                ImGui.TableNextColumn(); ImGui.Text($"{member.ContentId:X}");
                ImGui.TableNextColumn(); ImGui.Text($"{member.ObjectId:X}");
                ImGui.TableNextColumn(); ImGui.Text(member.Name.ToString());
                ImGui.TableNextColumn(); ImGui.Text($"{member.Territory.Id}");
                ImGui.TableNextColumn(); ImGui.Text($"{member.World.Id}");
                ImGui.TableNextColumn(); ImGui.Text(Utils.Vec3String(member.Position));
            }
            ImGui.EndTable();
        }
    }
}

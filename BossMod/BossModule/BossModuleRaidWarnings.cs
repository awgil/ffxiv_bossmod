using ImGuiNET;

namespace BossMod
{
    // TODO: this should really snap to player in party ui...
    class BossModuleRaidWarnings
    {
        public void Draw(BossModule module)
        {
            foreach ((int i, var player) in module.IterateRaidMembers())
            {
                var obj = Service.ObjectTable.SearchById(player.InstanceID);
                if (obj == null)
                    continue;

                var hints = module.CalculateHintsForRaidMember(i, player);
                if (hints.Count == 0)
                    continue;

                ImGui.Text($"{obj.Name}:");
                foreach (var hint in hints)
                {
                    ImGui.SameLine();
                    ImGui.Text(hint);
                }
            }
        }
    }
}

using ImGuiNET;

namespace BossMod
{
    // TODO: this should really snap to player in party ui...
    class BossModuleRaidWarnings
    {
        public void Draw(BossModule module)
        {
            var riskColor = ImGui.ColorConvertU32ToFloat4(0xff00ffff);
            var safeColor = ImGui.ColorConvertU32ToFloat4(0xff00ff00);
            foreach ((int i, var player) in module.IterateRaidMembers())
            {
                var obj = Service.ObjectTable.SearchById(player.InstanceID);
                if (obj == null)
                    continue;

                var hints = module.CalculateHintsForRaidMember(i, player);
                if (hints.Count == 0)
                    continue;

                ImGui.Text($"{obj.Name}:");
                foreach ((var hint, bool risk) in hints)
                {
                    ImGui.SameLine();
                    ImGui.TextColored(risk ? riskColor : safeColor, hint);
                }
            }
        }
    }
}

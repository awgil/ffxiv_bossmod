using ImGuiNET;
using System;

namespace BossMod
{
    public static class UICombo
    {
        public static bool Enum<T>(string label, ref T v) where T : Enum
        {
            bool res = false;
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo(label, v.ToString()))
            {
                foreach (var opt in System.Enum.GetValues(v.GetType()))
                {
                    if (ImGui.Selectable(opt.ToString(), opt.Equals(v)))
                    {
                        v = (T)opt;
                        res =  true;
                    }
                }
                ImGui.EndCombo();
            }
            return res;
        }
    }
}

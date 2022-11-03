using ImGuiNET;
using System;
using System.Reflection;

namespace BossMod
{
    public static class UICombo
    {
        public static string EnumString(Enum v)
        {
            var name = v.ToString();
            return v.GetType().GetField(name)?.GetCustomAttribute<PropertyDisplayAttribute>()?.Label ?? name;
        }

        public static bool Enum<T>(string label, ref T v) where T : Enum
        {
            bool res = false;
            ImGui.SetNextItemWidth(200);
            if (ImGui.BeginCombo(label, EnumString(v)))
            {
                foreach (var opt in System.Enum.GetValues(v.GetType()))
                {
                    if (ImGui.Selectable(EnumString((Enum)opt), opt.Equals(v)))
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

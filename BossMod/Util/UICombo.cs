using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using System.Reflection;

namespace BossMod;

public static class UICombo
{
    public static string EnumString(Enum v)
    {
        var name = v.ToString();
        return v.GetType().GetField(name)?.GetCustomAttribute<PropertyDisplayAttribute>()?.Label ?? name;
    }

    public static bool Enum<T>(string label, ref T v, Func<T, string>? print = null) where T : Enum
    {
        var et = v.GetType();
        var values = System.Enum.GetValues(et).Cast<T>().ToArray();
        var idxCur = Array.IndexOf(values, v);

        print ??= p => EnumString(p);

        var res = false;
        if (EnumIndex(label, v.GetType(), ref idxCur, idx => print(values[idx])))
        {
            v = values[idxCur];
            res = true;
        }
        return res;
    }

    public static bool EnumIndex(string label, Type type, ref int v, Func<int, string>? print = null)
    {
        var values = System.Enum.GetValues(type).Cast<Enum>().ToArray();
        print ??= p => EnumString(values[p]);
        var res = false;
        var width = 200 * ImGuiHelpers.GlobalScale;
        ImGui.SetNextItemWidth(width);

        var labelCur = print(v);
        var showLabelPopup = ImGui.CalcTextSize(labelCur).X > width;

        // draw combo without label so we can check if only the button itself is hovered
        if (ImGui.BeginCombo($"###{label}", print(v)))
        {
            showLabelPopup = false;
            for (var i = 0; i < values.Length; i++)
            {
                var opt = values[i];
                if (ImGui.Selectable(print(i), i == v))
                {
                    v = i;
                    res = true;
                }
            }
            ImGui.EndCombo();
        }
        if (showLabelPopup && ImGui.IsItemHovered())
            ImGui.SetTooltip(labelCur);
        ImGui.SameLine();
        ImGui.TextWrapped(label);
        return res;
    }

    public static bool Int(string label, string[] values, ref int v)
    {
        bool res = false;
        ImGui.SetNextItemWidth(200);
        if (ImGui.BeginCombo(label, v < values.Length ? values[v] : v.ToString()))
        {
            for (int i = 0; i < values.Length; ++i)
            {
                if (ImGui.Selectable(values[i], v == i))
                {
                    v = i;
                    res = true;
                }
            }
            ImGui.EndCombo();
        }
        return res;
    }

    public static bool Bool(string label, string[] values, ref bool v)
    {
        int val = v ? 1 : 0;
        if (!Int(label, values, ref val))
            return false;
        v = val != 0;
        return true;
    }
}

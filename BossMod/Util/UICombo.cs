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

    public static bool Enum<T>(string label, ref T v, Func<T, string>? print = null, Func<T, bool>? filter = null) where T : Enum
    {
        var et = v.GetType();
        var values = System.Enum.GetValues(et).Cast<T>().ToArray();
        var idxCur = Array.IndexOf(values, v);

        if (idxCur < 0)
            idxCur = 0;

        print ??= p => EnumString(p);
        filter ??= _ => true;

        var res = false;
        if (EnumIndex(label, v.GetType(), ref idxCur, idx => print(values[idx]), idx => filter(values[idx])))
        {
            v = values[idxCur];
            res = true;
        }
        return res;
    }

    public static bool EnumIndex(string label, Type type, ref int v, Func<int, string>? print = null, Func<int, bool>? filter = null)
    {
        var values = System.Enum.GetValues(type).Cast<Enum>().ToArray();
        print ??= p => EnumString(values[p]);
        filter ??= _ => true;
        var res = false;
        var width = 300 * ImGuiHelpers.GlobalScale;
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
                if (!filter(i))
                    continue;
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
        if (!label.StartsWith('#'))
        {
            ImGui.SameLine();
            ImGui.TextWrapped(label);
        }
        return res;
    }

    public static bool Radio(Type type, ref int v, bool oneLine, Func<int, string>? print = null)
    {
        var values = System.Enum.GetValues(type).Cast<Enum>().ToArray();
        print ??= p => EnumString(values[p]);
        var orig = v;
        var res = false;

        for (var i = 0; i < values.Length; i++)
        {
            var opt = values[i];
            if (ImGui.RadioButton(print(i), i == v))
            {
                v = i;
                res = i != orig;
            }
            if (oneLine && i + 1 < values.Length)
                ImGui.SameLine();
        }

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

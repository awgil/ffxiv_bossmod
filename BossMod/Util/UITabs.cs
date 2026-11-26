using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BossMod;

public sealed class UITabs
{
    private readonly List<(string Name, Action Tab, Func<bool>? Visible)> _tabs = [];
    private string _forceSelect = "";

    public void Add(string name, Action tab, Func<bool>? visible = null)
    {
        if (name.Length == 0 || _tabs.Any(t => t.Name == name))
            throw new ArgumentException($"Tab '{name}' has empty or duplicate name");
        _tabs.Add((name, tab, visible));
    }

    public void Select(string name) => _forceSelect = name;

    public void Draw()
    {
        using var tabs = ImRaii.TabBar("Tabs");
        if (!tabs)
            return;
        foreach (var t in _tabs.Where(t => t.Visible?.Invoke() ?? true))
            using (var tab = ImRaii.TabItem(t.Name, t.Name == _forceSelect ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None))
                if (tab)
                    t.Tab();
        _forceSelect = "";
    }
}

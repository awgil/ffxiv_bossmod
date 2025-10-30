using Dalamud.Interface.Utility.Raii;
using Dalamud.Bindings.ImGui;

namespace BossMod;

public sealed class UITabs
{
    private readonly List<(string Name, Action Tab)> _tabs = [];
    private string _forceSelect = "";

    public void Add(string name, Action tab)
    {
        if (name.Length == 0 || _tabs.Any(t => t.Name == name))
            throw new ArgumentException($"Tab '{name}' has empty or duplicate name");
        _tabs.Add((name, tab));
    }

    public void Select(string name) => _forceSelect = name;

    public void Draw()
    {
        using var tabs = ImRaii.TabBar("Tabs");
        if (!tabs)
            return;
        foreach (var t in _tabs)
            using (var tab = ImRaii.TabItem(t.Name, t.Name == _forceSelect ? ImGuiTabItemFlags.SetSelected : ImGuiTabItemFlags.None))
                if (tab)
                    t.Tab();
        _forceSelect = "";
    }
}

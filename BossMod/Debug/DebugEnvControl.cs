using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using ImGuiNET;
using System.Globalization;
using System.Runtime.InteropServices;

namespace BossMod;

unsafe class DebugEnvControl
{
    private delegate void ProcessEnvControlDelegate(void* self, uint index, ushort s1, ushort s2);
    private readonly ProcessEnvControlDelegate ProcessEnvControl = Marshal.GetDelegateForFunctionPointer<ProcessEnvControlDelegate>(Service.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8"));

    private readonly List<string> _history = [];
    private string _current = "";

    public void Draw()
    {
        ImGui.SetNextItemWidth(100);
        ImGui.InputText("ii.ssssssss", ref _current, 12);
        ImGui.SameLine();
        if (ImGui.Button("Execute"))
            ExecuteEnvControl();

        using var hist = ImRaii.ListBox("History");
        if (hist)
            foreach (var h in _history)
                if (ImGui.Selectable(h, h == _current))
                    _current = h;
    }

    private void ExecuteEnvControl()
    {
        var parts = _current.Split('.');
        if (parts.Length != 2 || !byte.TryParse(parts[0], NumberStyles.HexNumber, null, out var index) || !uint.TryParse(parts[1], NumberStyles.HexNumber, null, out var state))
            return;

        _history.Remove(_current);
        _history.Insert(0, _current);
        ProcessEnvControl(EventFramework.Instance()->DirectorModule.ActiveContentDirector, index, (ushort)(state & 0xFFFF), (ushort)(state >> 16));
    }
}

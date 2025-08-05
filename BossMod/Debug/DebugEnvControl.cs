using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using Dalamud.Bindings.ImGui;
using System.Globalization;
using System.Runtime.InteropServices;

namespace BossMod;

sealed unsafe class DebugEnvControl : IDisposable
{
    private delegate void ProcessEnvControlDelegate(void* self, uint index, ushort s1, ushort s2);
    private readonly ProcessEnvControlDelegate ProcessEnvControl = Marshal.GetDelegateForFunctionPointer<ProcessEnvControlDelegate>(Service.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8"));

    private readonly List<string> _history = [];
    private string _current = "";
    private bool deduplicate;

    private readonly EventSubscription _onEnvControl;

    public DebugEnvControl(WorldState ws)
    {
        _onEnvControl = ws.EnvControl.Subscribe(OnEventEnvControl);
    }

    public void Dispose() => _onEnvControl.Dispose();

    public void Draw()
    {
        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        ImGui.InputText("ii.ssssssss", ref _current, 12);
        ImGui.SameLine();
        if (ImGui.Button("Execute"))
            ExecuteEnvControl();
        ImGui.SameLine();
        if (ImGui.Button("Clear history"))
            _history.Clear();

        using var hist = ImRaii.ListBox("History");
        if (hist)
            foreach (var h in _history)
                if (ImGui.Selectable(h, h == _current))
                    _current = h;
    }

    private void OnEventEnvControl(WorldState.OpEnvControl ec)
    {
        if (deduplicate)
        {
            deduplicate = false;
            return;
        }
        _history.Insert(0, $"{ec.Index:X2}.{ec.State:X8}");
    }

    private void ExecuteEnvControl()
    {
        var parts = _current.Split('.');
        if (parts.Length != 2 || !byte.TryParse(parts[0], NumberStyles.HexNumber, null, out var index) || !uint.TryParse(parts[1], NumberStyles.HexNumber, null, out var state))
            return;

        _history.Remove(_current);
        _history.Insert(0, _current);
        var director = EventFramework.Instance()->DirectorModule.ActiveContentDirector;
        if (director == null)
        {
            Service.Log("No active content director, doing nothing");
            return;
        }
        deduplicate = true;
        ProcessEnvControl(director, index, (ushort)(state & 0xFFFF), (ushort)(state >> 16));
    }
}

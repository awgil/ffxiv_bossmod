using Dalamud.Bindings.ImGui;
using Dalamud.Interface.Utility;
using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using System.Globalization;
using System.Runtime.InteropServices;

namespace BossMod;

sealed unsafe class DebugMapEffect : IDisposable
{
    private readonly WorldStateGameSync.ProcessMapEffectDelegate ProcessMapEffect = Marshal.GetDelegateForFunctionPointer<WorldStateGameSync.ProcessMapEffectDelegate>(Service.SigScanner.ScanText("48 89 5C 24 ?? 48 89 6C 24 ?? 48 89 74 24 ?? 57 48 83 EC 20 8B FA 41 0F B7 E8"));

    private readonly List<string> _history = [];
    private string _current = "";
    private bool deduplicate;

    private readonly EventSubscriptions _subscriptions;

    public DebugMapEffect(WorldState ws)
    {
        _subscriptions = new(
            ws.MapEffect.Subscribe(OnMapEffect),
            ws.LegacyMapEffect.Subscribe(OnLegacyMapEffect)
        );
    }

    public void Dispose() => _subscriptions.Dispose();

    public void Draw()
    {
        if (ImGui.Button("Load effect list for current zone"))
        {
            var inst = EventFramework.Instance()->GetContentDirector();
            if (inst == null)
            {
                Service.Log($"no content director available");
                return;
            }

            for (var i = 0; i < inst->MapEffects->ItemCount; i++)
            {
                var item = inst->MapEffects->Items[i];
                var itemStr = $"LayoutId = {item.LayoutId:X}, State = {item.State:X}, Flags = {item.Flags:X}";
                Service.Log($"effect {i}: {{ {itemStr} }}");
            }
        }

        ImGui.SetNextItemWidth(100 * ImGuiHelpers.GlobalScale);
        ImGui.InputText("ii.ssssssss", ref _current, 12);
        ImGui.SameLine();
        if (ImGui.Button("Execute"))
            ApplyMapEffect();
        ImGui.SameLine();
        if (ImGui.Button("Clear history"))
            _history.Clear();

        using var hist = ImRaii.ListBox("History");
        if (hist)
            foreach (var h in _history)
                if (ImGui.Selectable(h, h == _current))
                    _current = h;
    }

    private void OnMapEffect(WorldState.OpMapEffect ec)
    {
        if (deduplicate)
        {
            deduplicate = false;
            return;
        }
        _history.Insert(0, $"{ec.Index:X2}.{ec.State:X8}");
    }

    private void OnLegacyMapEffect(WorldState.OpLegacyMapEffect ec)
    {
        _history.Insert(0, $"{ec.Sequence:X2} {ec.Param:X2} [{string.Join(" ", ec.Data.Select(d => d.ToString("X2")))}]");
    }

    private void ApplyMapEffect()
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
        ProcessMapEffect(director, index, (ushort)(state & 0xFFFF), (ushort)(state >> 16));
    }
}

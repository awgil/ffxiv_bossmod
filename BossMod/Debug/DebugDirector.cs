using Dalamud.Interface.Utility.Raii;
using FFXIVClientStructs.FFXIV.Client.Game.Event;
using ImGuiNET;

namespace BossMod.Debug;

unsafe class DebugDirector : IDisposable
{
    private delegate void* ProcessDirectorUpdateDelegate(void* self, uint directorID, uint updateID, uint param1, uint param2, uint param3, uint param4);

    private string _current = "";

    private readonly HookAddress<ProcessDirectorUpdateDelegate> _processHook;

    private readonly List<string> history = [];

    public DebugDirector()
    {
        _processHook = new("48 89 5C 24 ?? 57 48 83 EC 30 41 8B D9 41 8B F8 E8 ?? ?? ?? ?? 48 85 C0", ProcessDetour);
    }

    public void Draw()
    {
        ImGui.InputText("Update", ref _current, 256);

        if (ImGui.Button("Execute"))
        {
            var args = _current.Split(" ");
            if (args.Length != 6)
                return;

            List<uint> args2 = [];
            foreach (var arg in args)
            {
                if (!uint.TryParse(arg, System.Globalization.NumberStyles.HexNumber, null, out var p))
                    return;
                args2.Add(p);
            }

            var ef = EventFramework.Instance();
            if (ef == null)
            {
                Service.Log($"No EF instance, doing nothing.");
                return;
            }

            _processHook.Original(ef, args2[0], args2[1], args2[2], args2[3], args2[4], args2[5]);
        }

        if (ImGui.Button("Clear"))
            history.Clear();

        using (ImRaii.ListBox("###director"))
            foreach (var h in Enumerable.Reverse(history))
                if (ImGui.Selectable(h, h == _current))
                    _current = h;
    }

    void* ProcessDetour(void* self, uint directorID, uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        history.Add($"{directorID:X} {updateID:X} {param1:X} {param2:X} {param3:X} {param4:X}");
        return _processHook.Original(self, directorID, updateID, param1, param2, param3, param4);
    }

    public void Dispose()
    {
        _processHook.Dispose();
    }
}

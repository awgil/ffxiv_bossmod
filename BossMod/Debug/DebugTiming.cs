using ImGuiNET;

namespace BossMod;

public class DebugTiming
{
    uint _prevFrameCounter;
    ulong _prevQPC;

    public unsafe void Draw()
    {
        var fwk = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
        var qpf = Utils.FrameQPF();
        var qpc = Utils.FrameQPC();
        var dtRaw = Utils.FrameDurationRaw();
        var dtReal = (double)(qpc - _prevQPC) / qpf;
        ImGui.TextUnformatted($"Frame counter: {fwk->FrameCounter}");
        ImGui.TextUnformatted($"Frame time effective: {fwk->FrameDeltaTime}");
        ImGui.TextUnformatted($"Framerate: {fwk->FrameRate}");
        ImGui.TextUnformatted($"Forced frame duration: {Utils.ReadField<float>(fwk, 0x16C0)}");
        ImGui.TextUnformatted($"Forced next frame duration: {Utils.ReadField<float>(fwk, 0x17CC)}");
        ImGui.TextUnformatted($"Frame duration multiplier: {Utils.ReadField<float>(fwk, 0x16C4)}");
        ImGui.TextUnformatted($"Tick speed multiplier: {Utils.TickSpeedMultiplier()}");
        ImGui.TextUnformatted($"QPC freq: {qpf}");
        ImGui.TextUnformatted($"QPC value: {qpc}");
        ImGui.TextUnformatted($"dt raw: {dtRaw}");
        ImGui.TextUnformatted($"dt real: {dtReal} = raw + {dtReal - dtRaw}");
        ImGui.TextUnformatted($"dt ms granularity: {Utils.ReadField<long>(fwk, 0x16D0)} + {Utils.ReadField<float>(fwk, 0x16D8)}");
        ImGui.TextUnformatted($"dt us granularity: {Utils.ReadField<long>(fwk, 0x16E0)} + {Utils.ReadField<float>(fwk, 0x16E8)}");
        ImGui.TextUnformatted($"dt timer: {DateTime.UnixEpoch.AddSeconds(fwk->UtcTime.TimeStamp)}");
        _prevFrameCounter = fwk->FrameCounter;
        _prevQPC = qpc;
    }
}

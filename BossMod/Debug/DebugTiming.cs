using ImGuiNET;

namespace BossMod;

public class DebugTiming
{
    uint _prevFrameCounter;
    long _prevQPC;

    public unsafe void Draw()
    {
        var fwk = FFXIVClientStructs.FFXIV.Client.System.Framework.Framework.Instance();
        var dtReal = (double)(fwk->PerformanceCounterValue - _prevQPC) / fwk->PerformanceCounterFrequency;
        ImGui.TextUnformatted($"Frame counter: {fwk->FrameCounter}");
        ImGui.TextUnformatted($"Frame time effective: {fwk->FrameDeltaTime}");
        ImGui.TextUnformatted($"Framerate: {fwk->FrameRate}");
        ImGui.TextUnformatted($"Forced frame duration: {fwk->FrameDeltaTimeOverride}");
        ImGui.TextUnformatted($"Forced next frame duration: {fwk->NextFrameDeltaTimeOverride}");
        ImGui.TextUnformatted($"Frame duration multiplier: {fwk->FrameDeltaFactor}");
        ImGui.TextUnformatted($"Tick speed multiplier: {fwk->GameSpeedMultiplier}");
        ImGui.TextUnformatted($"QPC freq: {fwk->PerformanceCounterFrequency}");
        ImGui.TextUnformatted($"QPC value: {fwk->PerformanceCounterValue}");
        ImGui.TextUnformatted($"dt raw: {fwk->RealFrameDeltaTime}");
        ImGui.TextUnformatted($"dt real: {dtReal} = raw + {dtReal - fwk->RealFrameDeltaTime}");
        ImGui.TextUnformatted($"dt ms granularity: {fwk->FrameDeltaTimeMSInt} + {fwk->FrameDeltaTimeMSRem}");
        ImGui.TextUnformatted($"dt us granularity: {fwk->FrameDeltaTimeUSInt} + {fwk->FrameDeltaTimeUSRem}");
        ImGui.TextUnformatted($"dt timer: {DateTime.UnixEpoch.AddSeconds(fwk->UtcTime.TimeStamp)}");
        _prevFrameCounter = fwk->FrameCounter;
        _prevQPC = fwk->PerformanceCounterValue;
    }
}

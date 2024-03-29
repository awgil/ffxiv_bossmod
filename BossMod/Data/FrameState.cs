namespace BossMod;

// game updates its timings every frame (in the beginning of the tick)
// it's all based on QueryPerformanceCounter; we assume that its frequency can never change (indeed, game never tries to update it anyway, and OS guarantees that too)
// unfortunately, game does slightly weird thing - it samples QPC, calculates raw dt, then samples it again and stores second value as 'previous' QPC
// this means that raw dt is slightly smaller than d(QPC)/QPF
// then there is a frame duration override logic, which can modify dt used for further calculations (not sure when is it used)
// finally, there is additional multiplier for cooldown/status/etc. calculations (used by duty recorder replays)
public struct FrameState
{
    public DateTime Timestamp;
    public ulong QPC;
    public uint Index;
    public float DurationRaw;
    public float Duration;
    public float TickSpeedMultiplier;
}

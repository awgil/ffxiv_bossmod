using BossMod.Autorotation;

namespace BossMod.Services;

public interface IWorldStateGameSync : IDisposable
{
    void Update(TimeSpan prevFramePerf);
    void UpdateReplayState(RotationModuleManager rmm, IMovementOverride movement, AIHints hints);
}

internal sealed class MockWorldStateGameSync : IWorldStateGameSync
{
    public void Update(TimeSpan prevFramePerf) { }
    public void UpdateReplayState(RotationModuleManager rmm, IMovementOverride movement, AIHints hints) { }
    public void Dispose() { }
}

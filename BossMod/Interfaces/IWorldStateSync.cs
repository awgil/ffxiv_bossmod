using BossMod.Autorotation;

namespace BossMod.Services;

public interface IWorldStateGameSync
{
    void Update(TimeSpan prevFramePerf);
    void UpdateReplayState(RotationModuleManager rmm, IMovementOverride movement, AIHints hints);
    void RefreshInventory();
}

internal sealed class MockWorldStateGameSync : IWorldStateGameSync
{
    public void Update(TimeSpan prevFramePerf) { }
    public void UpdateReplayState(RotationModuleManager rmm, IMovementOverride movement, AIHints hints) { }
    public void RefreshInventory() { }
}

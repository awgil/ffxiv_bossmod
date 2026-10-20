using BossMod.Autorotation;

namespace BossMod.Services;

public interface IWorldStateGameSync : IDisposable
{
    void Update(TimeSpan prevFramePerf);
    void UpdateReplayState(RotationModuleManager rmm, IMovementOverride movement, AIHints hints, PartyRolesConfig roles);
}

internal sealed class MockWorldStateGameSync : IWorldStateGameSync
{
    public void Update(TimeSpan prevFramePerf) { }
    public void UpdateReplayState(RotationModuleManager rmm, IMovementOverride movement, AIHints hints, PartyRolesConfig roles) { }
    public void Dispose() { }
}

using BossMod.Interfaces;

namespace BossMod.Mocks;

sealed class MockWorldStateSync : IWorldStateSync
{
    public void Dispose() { }
    public void Update(TimeSpan prevFramePerf) { }
}

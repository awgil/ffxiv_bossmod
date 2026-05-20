namespace BossMod.Services;

public interface IWorldStateGameSync : IDisposable
{
    void Update(TimeSpan prevFramePerf);
}

internal sealed class MockWorldStateGameSync : IWorldStateGameSync
{
    public void Update(TimeSpan prevFramePerf) { }
    public void Dispose() { }
}

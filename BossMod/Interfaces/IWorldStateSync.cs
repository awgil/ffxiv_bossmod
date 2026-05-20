namespace BossMod.Services;

public interface IWorldStateGameSync : IDisposable
{
    void Update(TimeSpan ts);
}

internal sealed class MockWorldStateGameSync : IWorldStateGameSync
{
    public void Update(TimeSpan ts) { }
    public void Dispose() { }
}

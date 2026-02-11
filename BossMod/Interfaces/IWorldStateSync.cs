namespace BossMod.Interfaces;

public interface IWorldStateSync : IDisposable
{
    public delegate IWorldStateSync Factory(WorldState ws);

    public void Update(TimeSpan prevFramePerf);
}

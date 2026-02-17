namespace BossMod.Interfaces;

public interface IWorldStateSync : IDisposable
{
    public void Update(TimeSpan prevFramePerf);
}

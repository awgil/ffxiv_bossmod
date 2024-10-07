namespace BossMod;

public abstract class ZoneModule(WorldState ws) : IDisposable
{
    public WorldState WorldState = ws;

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
    }

    public virtual void Update() { }
    public virtual void CalculateAIHints(Actor player, AIHints hints) { } // note: this is called after framework automatically fills auto-detected hints
}

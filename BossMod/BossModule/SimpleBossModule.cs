namespace BossMod;

// base class for simple boss modules (hunts, fates, dungeons, etc.)
// these always center map around PC
public abstract class SimpleBossModule : BossModule
{
    public SimpleBossModule(WorldState ws, Actor primary)
        : base(ws, primary, new ArenaBoundsCircle(primary.Position, 30))
    {
    }

    protected override void UpdateModule()
    {
        var pc = WorldState.Party.Player();
        if (pc != null && Bounds.Center != pc.Position)
            Arena.Bounds = new ArenaBoundsCircle(pc.Position, 30);
    }
}

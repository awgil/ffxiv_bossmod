namespace BossMod;

// base class for simple boss modules (hunts, fates, dungeons, etc.)
// these always center map around PC
public abstract class SimpleBossModule(WorldState ws, Actor primary) : BossModule(ws, primary, primary.Position, new ArenaBoundsCircle(30))
{
    public override bool CheckReset() => !PrimaryActor.InCombat;
    protected override void UpdateModule() => Arena.Center = WorldState.Party.Player()?.Position ?? default;
}

namespace BossMod.Modules.Stormblood.Foray;

public abstract class BAModule(WorldState ws, Actor primary, WPos center, ArenaBounds bounds) : BossModule(ws, primary, center, bounds)
{
    public sealed override bool DrawAllPlayers => true;
}

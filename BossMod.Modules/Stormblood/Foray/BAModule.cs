namespace BossMod.Modules.Stormblood.Foray;

public abstract class BAModule(ModuleInit init, WPos center, ArenaBounds bounds) : BossModule(init, center, bounds)
{
    public sealed override bool DrawAllPlayers => true;
}

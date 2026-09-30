namespace BossMod.Dawntrail.Foray.CriticalEngagement;

public abstract class CEModule(ModuleInit init, WPos center, ArenaBounds bounds) : BossModule(init, center, bounds)
{
    // CE participants get 1778 Hoofing It, doesn't seem like there are any more specific statuses applied
    // mainly we want to avoid activating the module if the player incidentally moves too close to the arena, since it will disable generic hints, and the active module may also crash if helper actors disappear from the object table mid cast
    protected sealed override bool AllowedToActivate() => Raid.Player()?.FindStatus(1778) != null;

    public sealed override bool DrawAllPlayers => true;

    public override bool ShouldPrioritizeAllEnemies => true;
}

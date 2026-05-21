namespace BossMod.Dawntrail.Foray.CriticalEngagement;

public abstract class CEModule(WorldState ws, Actor primary, WPos center, ArenaBounds bounds) : BossModule(ws, primary, center, bounds)
{
    // CE participants get 1778 Hoofing It, doesn't seem like there are any more specific statuses applied
    // mainly we want to avoid activating the module if the player incidentally moves too close to the arena, since it will disable generic hints, and the active module may also crash if helper actors disappear from the object table mid cast
    protected override bool CheckPull() => WorldState.Party.Player()?.FindStatus(1778) != null;

    public sealed override bool DrawAllPlayers => true;
}

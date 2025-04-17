namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class StateOfShock(BossModule module) : Components.CastCounter(module, AID.StateOfShockSecond)
{
    public int NumStuns;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Stun)
            ++NumStuns;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Stun)
            --NumStuns;
    }
}

class HighestStakes(BossModule module) : Components.GenericTowers(module, AID.HighestStakesAOE)
{
    private BitMask _forbidden;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.HighestStakes)
        {
            Towers.Add(new(actor.Position, 6, 3, 3, _forbidden, WorldState.FutureTime(6.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            Towers.Clear();
            foreach (var t in spell.Targets)
                _forbidden.Set(Raid.FindSlot(t.ID));
        }
    }
}

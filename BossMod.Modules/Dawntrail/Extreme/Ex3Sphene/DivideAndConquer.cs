namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class DivideAndConquerBait(BossModule module) : Components.GenericBaitAway(module, AID.DivideAndConquerBait)
{
    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.DivideAndConquer && WorldState.Actors.Find(targetID) is var target && target != null)
            CurrentBaits.Add(new(actor, target, _shape, WorldState.FutureTime(3.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }
}

class DivideAndConquerAOE(BossModule module) : Components.StandardAOEs(module, AID.DivideAndConquerAOE, new AOEShapeRect(60, 2.5f));

namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class DivideAndConquer(BossModule module) : Components.GenericBaitAway(module)
{
    // line baits can be staggered so we can't use BaitAwayIcon which clears all at the same time
    // staggered waves always got 8 casts even if some players are dead, simultan waves got line baits on all alive players
    // drawing all 8 baits at the same time to make it easier to preposition for the 8 simultan casts
    private static readonly AOEShapeRect rect = new(100f, 2.5f);
    private int counter;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.LineBaits && CurrentBaits.Count == 0)
        {
            var raid = Raid.WithoutSlot(true, true, true);
            var len = raid.Length;
            var s = Module.PrimaryActor;
            var act = WorldState.FutureTime(3d);
            for (var i = 0; i < len; ++i)
            {
                CurrentBaits.Add(new(s, raid[i], rect, act));
            }
            counter = 8;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (CurrentBaits.Count == 0)
        {
            return;
        }
        if (spell.Action.ID == (uint)AID.DivideAndConquer)
        {
            if (--counter == 0)
            {
                CurrentBaits.Clear();
            }
            if (++NumCasts > 8)
            {
                CurrentBaits.Clear();
                NumCasts = 0;
            }
        }
    }
}

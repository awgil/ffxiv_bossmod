namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class WickedFire(BossModule module) : Components.StandardAOEs(module, AID.WickedFireAOE, 10);

class TwilightSabbath(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCone _shape = new(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Skip(NumCasts).Take(2);

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID != OID.WickedReplica)
            return;
        var (offset, delay) = id switch
        {
            0x11D6 => (-90.Degrees(), 7.1f),
            0x11D7 => (-90.Degrees(), 15.2f),
            0x11D8 => (90.Degrees(), 7.1f),
            0x11D9 => (90.Degrees(), 15.2f),
            _ => default
        };
        if (offset != default)
        {
            AOEs.Add(new(_shape, actor.Position, actor.Rotation + offset, WorldState.FutureTime(delay)));
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TwilightSabbathSidewiseSparkR or AID.TwilightSabbathSidewiseSparkL)
            ++NumCasts;
    }
}

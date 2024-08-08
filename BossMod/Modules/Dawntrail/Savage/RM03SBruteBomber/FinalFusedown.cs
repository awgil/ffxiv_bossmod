namespace BossMod.Dawntrail.Savage.RM03SBruteBomber;

class FinalFusedownSelfDestruct(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Skip(NumCasts).Take(4);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var delay = (SID)status.ID switch
        {
            SID.FinalFusedownFutureSelfDestructShort => 12.2f,
            SID.FinalFusedownFutureSelfDestructLong => 17.2f,
            _ => 0
        };
        if (delay > 0)
        {
            _aoes.Add(new(_shape, actor.Position, default, WorldState.FutureTime(delay)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FinalFusedownSelfDestructShort or AID.FinalFusedownSelfDestructLong)
            ++NumCasts;
    }
}

class FinalFusedownExplosion(BossModule module) : Components.GenericStackSpread(module, true)
{
    public int NumCasts;
    private readonly List<Spread> _spreads1 = [];
    private readonly List<Spread> _spreads2 = [];

    public void Show() => Spreads = _spreads1;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        (List<Spread>? list, float delay) = (SID)status.ID switch
        {
            SID.FinalFusedownFutureExplosionShort => (_spreads1, 12.2f),
            SID.FinalFusedownFutureExplosionLong => (_spreads2, 17.2f),
            _ => (null, 0)
        };
        list?.Add(new(actor, 6, WorldState.FutureTime(delay)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FinalFusedownExplosionShort:
                ++NumCasts;
                Spreads = _spreads2;
                break;
            case AID.FinalFusedownExplosionLong:
                ++NumCasts;
                Spreads.Clear();
                break;
        }
    }
}

namespace BossMod.Dawntrail.Savage.RM01SBlackCat;

class OneTwoPawBoss(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(100, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Skip(NumCasts).Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OneTwoPawBossAOERFirst or AID.OneTwoPawBossAOELSecond or AID.OneTwoPawBossAOELFirst or AID.OneTwoPawBossAOERSecond)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.OneTwoPawBossAOERFirst or AID.OneTwoPawBossAOELSecond or AID.OneTwoPawBossAOELFirst or AID.OneTwoPawBossAOERSecond)
            ++NumCasts;
    }
}

class OneTwoPawShade(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _firstDirection;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(100, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Skip(NumCasts).Take(2);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var dir = (AID)spell.Action.ID switch
        {
            AID.OneTwoPawBossRL => -90.Degrees(),
            AID.OneTwoPawBossLR => 90.Degrees(),
            _ => default
        };
        if (dir != default)
            _firstDirection = dir;
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Soulshade && _aoes.Count < 4)
        {
            _aoes.Add(new(_shape, source.Position, source.Rotation + _firstDirection, Module.WorldState.FutureTime(20.3f)));
            _aoes.Add(new(_shape, source.Position, source.Rotation - _firstDirection, Module.WorldState.FutureTime(23.3f)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.OneTwoPawShadeAOERFirst or AID.OneTwoPawShadeAOELSecond or AID.OneTwoPawShadeAOELFirst or AID.OneTwoPawShadeAOERSecond)
            ++NumCasts;
    }
}

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

class LeapingOneTwoPaw(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private Angle _leapDirection;
    private Angle _firstDirection;
    private Actor? _clone;

    private static readonly AOEShapeCone _shape = new(100, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Skip(NumCasts).Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (leapDir, firstDir) = (AID)spell.Action.ID switch
        {
            AID.LeapingOneTwoPawBossLRL => (90.Degrees(), -90.Degrees()),
            AID.LeapingOneTwoPawBossLLR => (90.Degrees(), 90.Degrees()),
            AID.LeapingOneTwoPawBossRRL => (-90.Degrees(), -90.Degrees()),
            AID.LeapingOneTwoPawBossRLR => (-90.Degrees(), 90.Degrees()),
            _ => default
        };
        if (leapDir == default)
            return;

        _leapDirection = leapDir;
        _firstDirection = firstDir;
        StartMechanic(caster.Position, spell.Rotation, Module.CastFinishAt(spell, 1.8f));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.LeapingOneTwoPawBossAOERFirst or AID.LeapingOneTwoPawBossAOELSecond or AID.LeapingOneTwoPawBossAOELFirst or AID.LeapingOneTwoPawBossAOERSecond
            or AID.LeapingOneTwoPawShadeAOERFirst or AID.LeapingOneTwoPawShadeAOELSecond or AID.LeapingOneTwoPawShadeAOELFirst or AID.LeapingOneTwoPawShadeAOERSecond)
        {
            ++NumCasts;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Soulshade)
        {
            if (_clone == null)
            {
                _clone = source;
            }
            else if (_clone == source)
            {
                // note: if this is second mechanic, we could activate it slightly earlier than tethers appear, as soon as first mechanic ends
                StartMechanic(source.Position, source.Rotation, Module.WorldState.FutureTime(16));
            }
            // else: second clone being tethered, wait...
        }
    }

    private void StartMechanic(WPos position, Angle rotation, DateTime activation)
    {
        var origin = position + 10 * (rotation + _leapDirection).ToDirection();
        AOEs.Add(new(_shape, origin, rotation + _firstDirection, activation));
        AOEs.Add(new(_shape, origin, rotation - _firstDirection, activation.AddSeconds(2)));
    }
}

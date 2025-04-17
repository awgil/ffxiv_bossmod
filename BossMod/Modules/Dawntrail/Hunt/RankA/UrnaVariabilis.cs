namespace BossMod.Dawntrail.Hunt.RankA.UrnaVariabilis;

public enum OID : uint
{
    Boss = 0x416F, // R3.500, x1
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    ProximityPlasma = 39106, // Boss->self, 5.0s cast, range 20 circle
    RingLightning = 39107, // Boss->self, 5.0s cast, range 8-60 donut
    Magnetron = 39108, // Boss->self, 3.0s cast, range 40 circle, raidwide + apply charge statuses
    MagnetoplasmaPositive = 39109, // Boss->self, 6.0s cast, range 60 circle (knockback 10 positive, attract 10 negative, followed by circle)
    MagnetoringPositive = 39110, // Boss->self, 6.0s cast, range 60 circle (knockback 10 positive, attract 10 negative, followed by donut)
    MagnetoplasmaNegative = 39111, // Boss->self, 6.0s cast, range 60 circle (knockback 10 negative, attract 10 positive, followed by circle)
    MagnetoringNegative = 39112, // Boss->self, 6.0s cast, range 60 circle (knockback 10 negative, attract 10 positive, followed by donut)
    ProximityPlasmaShort = 39113, // Boss->self, 1.0s cast, range 20 circle
    RingLightningShort = 39114, // Boss->self, 1.0s cast, range 8-60 donut
    ThunderousShower = 39115, // Boss->player, 5.0s cast, range 6 circle stack
    Electrowave = 39116, // Boss->self, 4.0s cast, range 60 circle, raidwide
}

public enum SID : uint
{
    PositiveCharge = 4071, // Boss->player, extra=0x0
    NegativeCharge = 4072, // Boss->player, extra=0x0
}

public enum IconID : uint
{
    Negative = 290, // Boss
    Positive = 291, // Boss
}

class ProximityPlasmaRingLightning(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance? _aoe;

    private static readonly AOEShape _shapeOut = new AOEShapeCircle(20);
    private static readonly AOEShape _shapeIn = new AOEShapeDonut(8, 60);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (shape, delay) = (AID)spell.Action.ID switch
        {
            AID.ProximityPlasma => (_shapeOut, 0),
            AID.RingLightning => (_shapeIn, 0),
            AID.MagnetoplasmaPositive or AID.MagnetoplasmaNegative => (_shapeOut, 2.5f),
            AID.MagnetoringPositive or AID.MagnetoringNegative => (_shapeIn, 2.5f),
            _ => (null, 0)
        };
        if (shape != null)
            _aoe = new(shape, caster.Position, default, Module.CastFinishAt(spell, delay));
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ProximityPlasma or AID.RingLightning or AID.ProximityPlasmaShort or AID.RingLightningShort)
            _aoe = null;
    }
}

class Magnetron(BossModule module) : Components.Knockback(module)
{
    private BitMask _positive;
    private BitMask _negative;
    private DateTime _activation;
    private bool _bossPositive;

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (_activation != default)
        {
            if (_positive[slot])
                yield return new(Module.PrimaryActor.Position, 10, _activation, Kind: _bossPositive ? Kind.AwayFromOrigin : Kind.TowardsOrigin);
            if (_negative[slot])
                yield return new(Module.PrimaryActor.Position, 10, _activation, Kind: _bossPositive ? Kind.TowardsOrigin : Kind.AwayFromOrigin);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.PositiveCharge:
                _positive.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.NegativeCharge:
                _negative.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.PositiveCharge:
                _positive.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.NegativeCharge:
                _negative.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.MagnetoplasmaPositive:
            case AID.MagnetoringPositive:
                _bossPositive = true;
                _activation = Module.CastFinishAt(spell);
                break;
            case AID.MagnetoplasmaNegative:
            case AID.MagnetoringNegative:
                _bossPositive = false;
                _activation = Module.CastFinishAt(spell);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.MagnetoplasmaPositive or AID.MagnetoringPositive or AID.MagnetoplasmaNegative or AID.MagnetoringNegative)
            _activation = default;
    }
}

class ThunderousShower(BossModule module) : Components.StackWithCastTargets(module, AID.ThunderousShower, 6, 4);
class Electrowave(BossModule module) : Components.RaidwideCast(module, AID.Electrowave);

class UrnaVariabilisStates : StateMachineBuilder
{
    public UrnaVariabilisStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ProximityPlasmaRingLightning>()
            .ActivateOnEnter<Magnetron>()
            .ActivateOnEnter<ThunderousShower>()
            .ActivateOnEnter<Electrowave>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Hunt, GroupID = (uint)BossModuleInfo.HuntRank.A, NameID = 13158)]
public class UrnaVariabilis(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

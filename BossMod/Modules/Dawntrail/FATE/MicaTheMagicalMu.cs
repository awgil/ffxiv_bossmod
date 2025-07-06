namespace BossMod.Dawntrail.FATE.MicaTheMagicalMu;

public enum OID : uint
{
    Boss = 0x43EB, // R7.999, x1
    Helper = 0x43EC, // R0.500, x0 (spawn during fight)
    CardHelper = 0x43ED, // R0.500, x0 (spawn during fight)
    MagicalHoop = 0x43F4, // R1.000, x0 (spawn during fight)
    Card1 = 0x43EE, // R1.000, x0 (spawn during fight)
    Card2 = 0x43EF, // R1.000, x0 (spawn during fight)
    Card3 = 0x43F0, // R1.000, x0 (spawn during fight)
    Card4 = 0x43F1, // R1.000, x0 (spawn during fight)
    Card5 = 0x43F2, // R1.000, x0 (spawn during fight)
    Card6 = 0x43F3, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 38709, // Boss->player, no cast, single-target
    Teleport = 38708, // Boss->location, no cast, single-target

    DealDrawStart = 38864, // Boss->location, no cast, single-target, visual (???)
    Deal = 38665, // Boss->self, 4.0s cast, single-target, visual (show pattern)
    DrawFirst1 = 38666, // Boss->self, 4.0s cast, single-target
    DrawFirst2 = 38667, // Boss->self, 4.0s cast, single-target
    DrawFirst3 = 38668, // Boss->self, 4.0s cast, single-target
    DrawFirst4 = 38669, // Boss->self, 4.0s cast, single-target
    DrawFirst5 = 38670, // Boss->self, 4.0s cast, single-target
    DrawFirst6 = 38671, // Boss->self, 4.0s cast, single-target
    DrawRest1 = 38672, // Boss->self, 1.5s cast, single-target
    DrawRest2 = 38673, // Boss->self, 1.5s cast, single-target
    DrawRest3 = 38674, // Boss->self, 1.5s cast, single-target
    DrawRest4 = 38675, // Boss->self, 1.5s cast, single-target
    DrawRest5 = 38676, // Boss->self, 1.5s cast, single-target
    DrawRest6 = 38677, // Boss->self, 1.5s cast, single-target
    CardTrick = 38678, // Boss->self, 4.0+0.5s cast, single-target, visual (explode squares)
    CardTrickAOEReal = 38679, // Helper->location, 1.5s cast, range 20 width 14 rect
    CardTrickAOEFake = 39156, // Helper->location, 1.5s cast, range 20 width 14 rect, visual (correct square)
    CardResolve1 = 38680, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve2 = 38681, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve3 = 38682, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve4 = 38683, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve5 = 38684, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)
    CardResolve6 = 38685, // CardHelper->location, no cast, range 40 width 42 rect, visual (remove card-drawn status)

    BewitchingBall = 38686, // Boss->self, 4.0s cast, single-target, visual (prepare for next mechanic)
    RollingStarlightFirst = 38687, // Boss->location, 7.0s cast, width 10 rect charge
    RollingStarlightRest = 38688, // Boss->location, no cast, width 10 rect charge
    RollingStarlightVisual1 = 38996, // Helper->location, 2.0s cast, range 20 width 10 rect
    RollingStarlightVisual2 = 38997, // Helper->location, 2.0s cast, range 29 width 10 rect
    RollingStarlightVisual3 = 38998, // Helper->location, 2.0s cast, range 43 width 10 rect
    RollingStarlightVisual4 = 38999, // Helper->location, 2.0s cast, range 52 width 10 rect
    BewitchingBallEnd = 38690, // Boss->self, no cast, single-target, visual (clear the status)

    MagicalHat = 38691, // Boss->self, 4.0+0.8s cast, single-target, visual (create hoops)
    TwinkleToss = 38692, // MagicalHoop->self, 3.0s cast, range 42 width 5 rect

    FlourishingBow = 38696, // Boss->self, 4.5+0.5s cast, single-target, visual (in/out)
    TwinklingFlourishLong = 38697, // Helper->location, 7.0s cast, range 10 circle
    TwinklingFlourishShort = 38698, // Helper->location, 5.0s cast, range 10 circle
    TwinklingRingLong = 38700, // Helper->location, 7.0s cast, range 10-30 donut
    TwinklingRingShort = 38701, // Helper->location, 5.0s cast, range 10-30 donut
    DoubleMisdirect = 38693, // Boss->self, 4.5+0.5s cast, single-target, visual (pizzas)
    DoubleMisdirectAOELong = 38694, // Helper->location, 7.0s cast, range 40 60-degree cone
    DoubleMisdirectAOEShort = 38695, // Helper->location, 5.0s cast, range 40 60-degree cone
    RoundOfApplause = 38699, // Boss->self, 4.5+0.5s cast, single-target, visual (in/out + pizzas)

    Shimmerstorm = 38702, // Boss->self, 2.5+0.5s cast, single-target, visual (puddles)
    ShimmerstormAOE = 38703, // Helper->location, 3.0s cast, range 6 circle
    Shimmerstrike = 38704, // Boss->self, 4.5+0.5s cast, single-target, visual (tankbuster)
    ShimmerstrikeAOE = 38705, // Helper->players, 5.0s cast, range 6 circle
    SparkOfImagination = 38706, // Boss->self, 4.5+0.5s cast, single-target, visual (raidwide)
    SparkOfImaginationAOE = 38707, // Helper->location, 4.0s cast, range 35 circle, raidwide
    End = 38710, // Boss->self, no cast, single-target, visual (end fight)
}

class Draw(BossModule module) : Components.GenericAOEs(module)
{
    private readonly IReadOnlyList<Actor>[] _cards = [
        module.Enemies(OID.Card1),
        module.Enemies(OID.Card2),
        module.Enemies(OID.Card3),
        module.Enemies(OID.Card4),
        module.Enemies(OID.Card5),
        module.Enemies(OID.Card6),
    ];
    private readonly List<int> _safeZones = [];
    private DateTime _activation = DateTime.MaxValue;

    private static readonly AOEShapeRect _shape = new(10, 7, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_safeZones.Count > 0)
            for (int i = 0; i < _cards.Length; ++i)
                if (i != _safeZones[0])
                    foreach (var a in _cards[i])
                        yield return new(_shape, a.Position, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DrawFirst1:
            case AID.DrawRest1:
                _safeZones.Add(0);
                break;
            case AID.DrawFirst2:
            case AID.DrawRest2:
                _safeZones.Add(1);
                break;
            case AID.DrawFirst3:
            case AID.DrawRest3:
                _safeZones.Add(2);
                break;
            case AID.DrawFirst4:
            case AID.DrawRest4:
                _safeZones.Add(3);
                break;
            case AID.DrawFirst5:
            case AID.DrawRest5:
                _safeZones.Add(4);
                break;
            case AID.DrawFirst6:
            case AID.DrawRest6:
                _safeZones.Add(5);
                break;
            case AID.CardTrick:
                _activation = Module.CastFinishAt(spell, 0.6f);
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CardTrickAOEFake)
        {
            _activation = DateTime.MaxValue;
            if (_safeZones.Count > 0)
                _safeZones.RemoveAt(0);
        }
    }
}

class FlourishingBow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeOut = new(10);
    private static readonly AOEShapeDonut _shapeIn = new(10, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = (AID)spell.Action.ID switch
        {
            AID.TwinklingFlourishLong or AID.TwinklingFlourishShort => _shapeOut,
            AID.TwinklingRingLong or AID.TwinklingRingShort => _shapeIn,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TwinklingFlourishLong or AID.TwinklingFlourishShort or AID.TwinklingRingLong or AID.TwinklingRingShort && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class DoubleMisdirect(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(40, 30.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DoubleMisdirectAOELong or AID.DoubleMisdirectAOEShort)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DoubleMisdirectAOELong or AID.DoubleMisdirectAOEShort && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class RollingStarlight(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape1 = new(20, 5);
    private static readonly AOEShapeRect _shape2 = new(29, 5);
    private static readonly AOEShapeRect _shape3 = new(43, 5);
    private static readonly AOEShapeRect _shape4 = new(52, 5);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = (AID)spell.Action.ID switch
        {
            AID.RollingStarlightVisual1 => _shape1,
            AID.RollingStarlightVisual2 => _shape2,
            AID.RollingStarlightVisual3 => _shape3,
            AID.RollingStarlightVisual4 => _shape4,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 5 + _aoes.Count * 0.6f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RollingStarlightFirst or AID.RollingStarlightRest && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class MagicalHat(BossModule module) : Components.StandardAOEs(module, AID.TwinkleToss, new AOEShapeRect(42, 2.5f), 4);
class Shimmerstorm(BossModule module) : Components.StandardAOEs(module, AID.ShimmerstormAOE, 6);
class Shimmerstrike(BossModule module) : Components.BaitAwayCast(module, AID.ShimmerstrikeAOE, new AOEShapeCircle(6), true);
class SparkOfImagination(BossModule module) : Components.RaidwideCast(module, AID.SparkOfImaginationAOE);

class MicaTheMagicalMuStates : StateMachineBuilder
{
    public MicaTheMagicalMuStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Draw>()
            .ActivateOnEnter<FlourishingBow>()
            .ActivateOnEnter<DoubleMisdirect>()
            .ActivateOnEnter<RollingStarlight>()
            .ActivateOnEnter<MagicalHat>()
            .ActivateOnEnter<Shimmerstorm>()
            .ActivateOnEnter<Shimmerstrike>()
            .ActivateOnEnter<SparkOfImagination>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1922, NameID = 13049)]
public class MicaTheMagicalMu(WorldState ws, Actor primary) : BossModule(ws, primary, new(791, 593), new ArenaBoundsRect(21, 20));

namespace BossMod.Dawntrail.FATE.Ttokrrone;

public enum OID : uint
{
    Boss = 0x41DF, // R13.000, x1
    Helper = 0x4225, // R0.500, x18 (spawn during fight)
    SandSphere = 0x425E, // R2.200, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 37342, // Boss->location, no cast, single-target
    Devour = 37327, // Boss->location, 3.5s cast, range 8 circle
    Touchdown = 37339, // Boss->self, 5.0s cast, range 60 circle, raidwide
    SummoningSands = 38647, // SandSphere->self, 6.3s cast, range 6 circle
    SandburstLong = 39245, // SandSphere->location, 8.0s cast, range 12 circle
    SandburstShort = 39246, // SandSphere->location, 6.0s cast, range 12 circle

    FangwardSandspout = 37313, // Boss->self, 5.2+0.8s cast, single-target, visual (forward cleave + out)
    FangwardSandspoutAOE = 39813, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakSandspoutF = 39841, // Helper->location, 2.1s cast, range 13 circle
    TailwardSandspout = 37314, // Boss->self, 5.2+0.8s cast, single-target, visual (backward cleave + out)
    TailwardSandspoutAOE = 39814, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakSandspoutB = 39842, // Helper->location, 2.1s cast, range 13 circle
    RightwardSandspout = 37315, // Boss->self, 5.2+0.8s cast, single-target, visual (right cleave + out)
    RightwardSandspoutAOE = 39815, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakSandspoutR = 39843, // Helper->location, 2.1s cast, range 13 circle
    LeftwardSandspout = 37316, // Boss->self, 5.2+0.8s cast, single-target, visual (left cleave + out)
    LeftwardSandspoutAOE = 39816, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakSandspoutL = 39844, // Helper->location, 2.1s cast, range 13 circle

    FangwardDustdevilCW = 37317, // Boss->self, 7.2+0.8s cast, single-target, visual (cleave front, rotate CW)
    TailwardDustdevilCW = 37318, // Boss->self, 7.2+0.8s cast, single-target, visual (cleave back, rotate CW)
    FangwardDustdevilCCW = 37321, // Boss->self, 7.2+0.8s cast, single-target, visual (cleave front, rotate CCW)
    TailwardDustdevilCCW = 37322, // Boss->self, 7.2+0.8s cast, single-target, visual (cleave back, rotate CCW)
    FangwardDustdevilAOE = 39817, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakDustdevilF = 39845, // Helper->location, 2.1s cast, range 13 circle
    TailwardDustdevilAOE = 39818, // Helper->location, 2.1s cast, range 60 90-degree cone
    DeadlyDustcloakDustdevilB = 39846, // Helper->location, 2.1s cast, range 13 circle
    RightwardSandspoutRotate = 37325, // Boss->self, no cast, single-target, visual (rotate right/CW)
    RightwardSandspoutRotateAOE = 39819, // Helper->location, 0.9s cast, range 60 90-degree cone
    DeadlyDustcloakDustdevilR = 39847, // Helper->location, 0.9s cast, range 13 circle
    LeftwardSandspoutRotate = 37326, // Boss->self, no cast, single-target, visual (rotate left/CW)
    LeftwardSandspoutRotateAOE = 39820, // Helper->location, 0.9s cast, range 60 90-degree cone
    DeadlyDustcloakDustdevilL = 39848, // Helper->location, 0.9s cast, range 13 circle

    DesertTempestStart = 36758, // Boss->location, no cast, single-target, visual (???)
    DesertTempestC = 37331, // Boss->self, 7.3+0.7s cast, single-target, visual (circle)
    DesertTempestCAOE = 37328, // Helper->location, 1.1s cast, range 19 circle
    DesertTempestD = 37332, // Boss->self, 7.3+0.7s cast, single-target, visual (donut)
    DesertTempestDAOE = 37329, // Helper->location, 1.1s cast, range 14-60 donut
    DesertTempestCD = 37333, // Boss->self, 7.3+0.7s cast, single-target, visual (left circle, right donut)
    DesertTempestCDLAOE = 37336, // Helper->location, 1.1s cast, range 19 180-degree cone
    DesertTempestCDRAOE = 37337, // Helper->location, 1.1s cast, range 14-60 180-degree cone
    DesertTempestDC = 37334, // Boss->self, 7.3+0.7s cast, single-target, visual (right circle, left donut)
    DesertTempestDCRAOE = 37335, // Helper->location, 1.1s cast, range 19 180-degree cone
    DesertTempestDCLAOE = 37338, // Helper->location, 1.1s cast, range 14-60 180-degree cone

    Landswallow = 38642, // Boss->location, 14.0s cast, range 38 width 27 rect (charge from center to border)
    LandswallowAOE1 = 38644, // Boss->location, no cast, range 68 width 27 rect (charge across arena)
    LandswallowAOE2 = 38645, // Boss->location, no cast, range 50 width 27 rect (charge diagonally with same cardinality)
    LandswallowAOE3 = 38646, // Boss->location, no cast, range 63 width 27 rect (charge from cardinal to intercardinal or vice versa)
    LandswallowVisual1 = 37578, // Helper->location, 13.8s cast, range 14 width 22 rect
    LandswallowVisual2 = 37579, // Helper->location, 14.8s cast, range 14 width 22 rect
    LandswallowVisual3 = 37580, // Helper->location, 16.2s cast, range 14 width 22 rect
    LandswallowVisual4 = 37581, // Helper->location, 17.6s cast, range 14 width 22 rect
    LandswallowVisual5 = 37582, // Helper->location, 19.0s cast, range 14 width 22 rect
    LandswallowVisual6 = 37583, // Helper->location, 20.4s cast, range 14 width 22 rect
}

public enum IconID : uint
{
    RotateCW = 540, // Boss
    RotateCCW = 541, // Boss
}

class Devour(BossModule module) : Components.StandardAOEs(module, AID.Devour, 8);
class Touchdown(BossModule module) : Components.RaidwideCast(module, AID.Touchdown);
class SummoningSands(BossModule module) : Components.StandardAOEs(module, AID.SummoningSands, new AOEShapeCircle(6));
class SandburstLong(BossModule module) : Components.StandardAOEs(module, AID.SandburstLong, 12);
class SandburstShort(BossModule module) : Components.StandardAOEs(module, AID.SandburstShort, 12);

class SandspoutDustdevil(BossModule module) : Components.GenericAOEs(module)
{
    private Angle _nextRotation;
    private Angle _increment;
    private DateTime _nextActivation;
    private int _remainingCasts;
    private int _numDustdevilCasts = 7; // at the beginning of the fight, boss does sequences of 7 casts, at some point it switches to doing 4-4 casts

    private static readonly AOEShapeCone _shapeCleave = new(60, 45.Degrees());
    private static readonly AOEShapeCircle _shapeOut = new(13);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_remainingCasts > 1)
        {
            yield return new(_shapeCleave, Module.PrimaryActor.Position, _nextRotation + _increment, _nextActivation.AddSeconds(2.6f));
        }
        if (_remainingCasts > 0)
        {
            yield return new(_shapeOut, Module.PrimaryActor.Position, default, _nextActivation);
            yield return new(_shapeCleave, Module.PrimaryActor.Position, _nextRotation, _nextActivation, ArenaColor.Danger);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // this is rotating quickly, we really want to stay as close to the imminent aoe as possible
        if (_remainingCasts > 2)
            hints.AddForbiddenZone(_shapeCleave.CheckFn(Module.PrimaryActor.Position, _nextRotation + 2 * _increment), _nextActivation.AddSeconds(5.2f));
        if (_remainingCasts > 3)
            hints.AddForbiddenZone(ShapeContains.Cone(Module.PrimaryActor.Position, _shapeCleave.Radius, _nextRotation + 2.9f * _increment, 0.9f * _shapeCleave.HalfAngle), _nextActivation.AddSeconds(5.2f));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (offset, increment, casts) = (AID)spell.Action.ID switch
        {
            AID.FangwardSandspout => (default, default, 1),
            AID.TailwardSandspout => (180.Degrees(), default, 1),
            AID.RightwardSandspout => (-90.Degrees(), default, 1),
            AID.LeftwardSandspout => (90.Degrees(), default, 1),
            AID.FangwardDustdevilCW => (default, -90.Degrees(), _numDustdevilCasts),
            AID.FangwardDustdevilCCW => (default, 90.Degrees(), _numDustdevilCasts),
            AID.TailwardDustdevilCW => (180.Degrees(), -90.Degrees(), _numDustdevilCasts),
            AID.TailwardDustdevilCCW => (180.Degrees(), 90.Degrees(), _numDustdevilCasts),
            _ => default
        };
        if (casts > 0)
        {
            if (casts == 7 && _remainingCasts > 0)
                casts = _numDustdevilCasts = 4; // TODO: later in the fight, we start getting 4 casts rather than 7 - this is a heuristic to try to guess this

            _nextRotation = spell.Rotation + offset;
            _increment = increment;
            _nextActivation = Module.CastFinishAt(spell, 0.8f);
            _remainingCasts = casts;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.FangwardSandspoutAOE or AID.TailwardSandspoutAOE or AID.RightwardSandspoutAOE or AID.LeftwardSandspoutAOE
            or AID.FangwardDustdevilAOE or AID.TailwardDustdevilAOE or AID.RightwardSandspoutRotateAOE or AID.LeftwardSandspoutRotateAOE)
        {
            --_remainingCasts;
            _nextRotation += _increment;
            _nextActivation = WorldState.FutureTime(2.6f);
        }
    }
}

class DesertTempest(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shapeFullCircle = new(19);
    private static readonly AOEShapeDonut _shapeFullDonut = new(14, 60);
    private static readonly AOEShapeCone _shapeHalfCircle = new(19, 90.Degrees());
    private static readonly AOEShapeDonutSector _shapeHalfDonut = new(14, 60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DesertTempestC:
                _aoes.Add(new(_shapeFullCircle, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.9f)));
                break;
            case AID.DesertTempestD:
                _aoes.Add(new(_shapeFullDonut, caster.Position, spell.Rotation, Module.CastFinishAt(spell, 0.9f)));
                break;
            case AID.DesertTempestCD:
                _aoes.Add(new(_shapeHalfCircle, caster.Position, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 0.9f)));
                _aoes.Add(new(_shapeHalfDonut, caster.Position, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 0.9f)));
                break;
            case AID.DesertTempestDC:
                _aoes.Add(new(_shapeHalfDonut, caster.Position, spell.Rotation + 90.Degrees(), Module.CastFinishAt(spell, 0.9f)));
                _aoes.Add(new(_shapeHalfCircle, caster.Position, spell.Rotation - 90.Degrees(), Module.CastFinishAt(spell, 0.9f)));
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.DesertTempestCAOE or AID.DesertTempestDAOE or AID.DesertTempestCDLAOE or AID.DesertTempestCDRAOE or AID.DesertTempestDCLAOE or AID.DesertTempestDCRAOE && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class Landswallow(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(68, 13.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Landswallow or AID.LandswallowVisual2 or AID.LandswallowVisual3 or AID.LandswallowVisual4 or AID.LandswallowVisual5 or AID.LandswallowVisual6)
        {
            _aoes.Add(new(_shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
            _aoes.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Landswallow or AID.LandswallowAOE1 or AID.LandswallowAOE2 or AID.LandswallowAOE3 && _aoes.Count > 0)
        {
            _aoes.RemoveAt(0);
        }
    }
}

class TtokrroneStates : StateMachineBuilder
{
    public TtokrroneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Devour>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<SummoningSands>()
            .ActivateOnEnter<SandburstLong>()
            .ActivateOnEnter<SandburstShort>()
            .ActivateOnEnter<SandspoutDustdevil>()
            .ActivateOnEnter<DesertTempest>()
            .ActivateOnEnter<Landswallow>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.Fate, GroupID = 1871, NameID = 12733)]
public class Ttokrrone(WorldState ws, Actor primary) : BossModule(ws, primary, new(53, -820), new ArenaBoundsCircle(29.5f))
{
    // if boss is pulled when player is really far away and helpers aren't loaded, some components might never see resolve casts and get stuck forever
    protected override bool CheckPull() => base.CheckPull() && Enemies(OID.Helper).Any();
}

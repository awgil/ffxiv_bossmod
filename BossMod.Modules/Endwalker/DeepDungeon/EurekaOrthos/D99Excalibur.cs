namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D99Excalibur;

public enum OID : uint
{
    Boss = 0x3CFC, // R5.100, x1
    Caliburnus = 0x3CFD, // R1.000, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 31355, // Boss->player, no cast, single-target
    Teleport = 31326, // Boss->location, no cast, single-target
    ParadoxumVisual = 31353, // Boss->self, 5.0s cast, single-target
    Paradoxum = 31354, // Helper->self, no cast, range 100 circle, raidwide
    Caliburni = 31333, // Boss->self, 5.0+0.5s cast, single-target, places swords
    Steelstrike1 = 31334, // Caliburnus->location, 2.0s cast, width 4 rect charge
    Steelstrike2 = 31335, // Caliburnus->location, 2.0s cast, width 4 rect charge
    ThermalDivideIceLeft = 31349, // Boss->self, 5.0+2.0s cast, single-target, ice left, fire right
    ThermalDivideFireLeft = 31350, // Boss->self, 5.0+2.0s cast, single-target, fire left, ice right
    ThermalDivide1 = 32701, // Helper->self, 6.7s cast, range 40 width 8 rect
    ThermalDivide2 = 31351, // Helper->self, no cast, range 40 width 40 rect
    CallDimensionBlade = 31338, // Boss->self, 4.0s cast, single-target
    SolidSoulsCaliber = 31327, // Boss->self, 6.0s cast, range 10 circle
    EmptySoulsCaliber = 31328, // Boss->self, 6.0s cast, range 5-40 donut
    VacuumSlash = 31342, // Helper->self, 7.0s cast, range 80 45-degree cone
    Flameforge = 31329, // Boss->self, 3.0s cast, single-target, next raidwide is fire
    Frostforge = 31330, // Boss->self, 3.0s cast, single-target, next raidwide is ice
    SteelFlame = 31336, // Caliburnus->location, 2.0s cast, width 4 rect charge
    SteelFrost = 31337, // Caliburnus->location, 2.0s cast, width 4 rect charge
    AbyssalSlash1 = 31339, // Helper->self, 7.0s cast, range 2-7 180-degree donut segment
    AbyssalSlash2 = 31340, // Helper->self, 7.0s cast, range 7-12 180-degree donut segment
    AbyssalSlash3 = 31341, // Helper->self, 7.0s cast, range 17-22 180-degree donut segment
    FlamesRevelation = 31331, // Helper->self, no cast, range 60 circle
    FrostRevelation = 31332, // Helper->self, no cast, range 60 circle
    ExflammeusVisual = 31343, // Boss->self, 4.0s cast, single-target
    Exflammeus = 31344, // Helper->self, 5.0s cast, range 8 circle
    Exglacialis = 31345, // Boss->self, 4.0s cast, single-target
    IceShoot = 31346, // Helper->self, 5.0s cast, range 6 circle
    IceBloomCircle = 31347, // Helper->self, 4.0s cast, range 6 circle
    IceBloomCross = 31348, // Helper->self, no cast, range 40 width 5 cross
}

public enum SID : uint
{
    SoulOfFire = 3410, // Helper->player, extra=0x0
    SoulOfIce = 3411, // Helper->player, extra=0x0
    CaliburnusElement = 2552, // none->Caliburnus, extra=0x219 (fire) / 0x21A (ice)
}

class Steelstrike(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect _rect = new(20, 2);
    private static readonly AOEShapeRect _rectBoth = new(20, 2, 20);
    private readonly List<AOEInstance> _aoes = [];
    private readonly List<Angle> _angles = [];
    private readonly List<AOEInstance> _swordsFire = [];
    private readonly List<AOEInstance> _swordsIce = [];
    private bool? _nextIce;
    private BitMask _soulOfFire;
    private BitMask _soulOfIce;
    private int _numCaliburni;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var safe = SafeSwords(slot);
        if (safe == null)
            return _aoes;
        var first = safe[0].Rotation;
        var last = safe[^1].Rotation;
        return _aoes.Where(a => a.Rotation != first && a.Rotation != last)
            .Prepend(safe[^1] with { Color = ArenaColor.SafeFromAOE, Risky = false })
            .Prepend(safe[0] with { Color = ArenaColor.SafeFromAOE, Risky = false });
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        var safe = SafeSwords(slot);
        if (safe != null && _aoes.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.Intersection([.. new[] { safe[0], safe[^1] }.Select(a => ShapeDistance.InvertedRect(a.Origin, a.Rotation, 20, 20, 2))]), _aoes[0].Activation);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_nextIce is bool ice)
            hints.Add(ice ? "Next raidwide: Ice" : "Next raidwide: Fire");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_nextIce == true && _soulOfIce[slot])
            hints.Add("Get hit by a fire blade!");
        else if (_nextIce == false && _soulOfFire[slot])
            hints.Add("Get hit by a single ice blade!");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Caliburni:
                var first = _numCaliburni++ == 0;
                var count = first ? 10 : 5;
                var start = (first ? -45 : -20).Degrees();
                for (var i = 0; i < count; ++i)
                {
                    var rot = spell.Rotation + start + i * 10.Degrees();
                    if (first)
                        AddSword(rot);
                    else
                        for (var j = 0; j < 3; ++j)
                            AddSword(rot + j * 120.Degrees());
                }
                break;
            case AID.ThermalDivide1:
            case AID.ParadoxumVisual:
                var activation = Module.CastFinishAt(spell, 3.4f);
                foreach (var a in _angles)
                    _aoes.Add(new(_rectBoth, Arena.Center, a, activation));
                break;
            case AID.Flameforge:
                _nextIce = false;
                break;
            case AID.Frostforge:
                _nextIce = true;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Steelstrike1:
                _aoes.Clear();
                break;
            case AID.Steelstrike2:
            case AID.SteelFlame:
            case AID.SteelFrost:
                _aoes.Clear();
                _angles.Clear();
                _swordsFire.Clear();
                _swordsIce.Clear();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlamesRevelation or AID.FrostRevelation)
            _nextIce = null;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.CaliburnusElement:
                var sword = _aoes.FindIndex(a => a.Rotation.AlmostEqual(actor.Rotation, Angle.DegToRad));
                if (sword >= 0)
                    (status.Extra == 0x219 ? _swordsFire : _swordsIce).Add(_aoes[sword]);
                if (_swordsFire.Count == 5 && _swordsIce.Count == 5)
                {
                    _swordsFire.SortBy(a => a.Rotation.Rad);
                    _swordsIce.SortBy(a => a.Rotation.Rad);
                }
                break;
            case SID.SoulOfFire:
                SetSoul(ref _soulOfFire, actor, true);
                break;
            case SID.SoulOfIce:
                SetSoul(ref _soulOfIce, actor, true);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SoulOfFire:
                SetSoul(ref _soulOfFire, actor, false);
                break;
            case SID.SoulOfIce:
                SetSoul(ref _soulOfIce, actor, false);
                break;
        }
    }

    private void AddSword(Angle rot)
    {
        _aoes.Add(new(_rect, Arena.Center, rot));
        _angles.Add(rot);
    }

    private List<AOEInstance>? SafeSwords(int slot)
    {
        if (_aoes.Count == 0 || _nextIce is not bool ice)
            return null;
        var swords = ice && _soulOfIce[slot] ? _swordsFire : !ice && _soulOfFire[slot] ? _swordsIce : null;
        return swords?.Count > 0 ? swords : null;
    }

    private void SetSoul(ref BitMask mask, Actor actor, bool value)
    {
        if (Raid.TryFindSlot(actor, out var slot))
            mask[slot] = value;
    }
}

class ThermalDivideSides(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeCone _cone = new(40, 90.Degrees());
    private bool? _fireLeft;
    private Angle _rotation;
    private WDir _offset;
    private DateTime _activation;
    private BitMask _soulOfFire;
    private BitMask _soulOfIce;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_fireLeft is not bool fireLeft)
            yield break;
        var side = fireLeft ? _soulOfFire[slot] : _soulOfIce[slot];
        var dir = side ? 1 : -1;
        yield return new(_cone, Arena.Center + dir * _offset, _rotation + dir * 90.Degrees(), _activation);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_fireLeft != null)
            hints.Add("Go to safe side!", ActiveAOEs(slot, actor).Any(c => c.Check(actor.Position)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ThermalDivideIceLeft or AID.ThermalDivideFireLeft)
        {
            _fireLeft = (AID)spell.Action.ID == AID.ThermalDivideFireLeft;
            _rotation = spell.Rotation;
            _offset = 4 * (spell.Rotation + 90.Degrees()).ToDirection();
            _activation = Module.CastFinishAt(spell, 1.9f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ThermalDivide2)
            _fireLeft = null;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (!Raid.TryFindSlot(actor, out var slot))
            return;
        if ((SID)status.ID == SID.SoulOfFire)
            _soulOfFire.Set(slot);
        else if ((SID)status.ID == SID.SoulOfIce)
            _soulOfIce.Set(slot);
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if (!Raid.TryFindSlot(actor, out var slot))
            return;
        if ((SID)status.ID == SID.SoulOfFire)
            _soulOfFire.Clear(slot);
        else if ((SID)status.ID == SID.SoulOfIce)
            _soulOfIce.Clear(slot);
    }
}

class IceBloomCross(BossModule module) : Components.GenericAOEs(module, AID.IceBloomCross)
{
    private readonly List<(WPos Center, DateTime Activation, int CrossesLeft)> _stars = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var s in _stars.Take(3))
        {
            if (s.CrossesLeft > 1)
                yield return new(new AOEShapeCross(40, 2.5f), s.Center, 45.Degrees(), s.Activation);
            yield return new(new AOEShapeCross(40, 2.5f), s.Center, default, s.Activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.IceShoot)
            _stars.Add((spell.LocXZ, Module.CastFinishAt(spell, 5.2f), 2));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action != WatchedAction || _stars.Count == 0)
            return;
        ++NumCasts;
        var (center, activation, left) = _stars[0];
        if (--left > 0)
            _stars[0] = (center, activation, left);
        else
            _stars.RemoveAt(0);
    }
}

class AbyssalSlash1(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSlash1, new AOEShapeDonutSector(2, 7, 90.Degrees()));
class AbyssalSlash2(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSlash2, new AOEShapeDonutSector(7, 12, 90.Degrees()));
class AbyssalSlash3(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSlash3, new AOEShapeDonutSector(17, 22, 90.Degrees()));
class VacuumSlash(BossModule module) : Components.StandardAOEs(module, AID.VacuumSlash, new AOEShapeCone(80, 22.5f.Degrees()));
class ThermalDivide(BossModule module) : Components.StandardAOEs(module, AID.ThermalDivide1, new AOEShapeRect(40, 4));
class Exflammeus(BossModule module) : Components.StandardAOEs(module, AID.Exflammeus, new AOEShapeCircle(8));
class IceShoot(BossModule module) : Components.GroupedAOEs(module, [AID.IceShoot, AID.IceBloomCircle], new AOEShapeCircle(6));
class EmptySoulsCaliber(BossModule module) : Components.StandardAOEs(module, AID.EmptySoulsCaliber, new AOEShapeDonut(5, 40));
class SolidSoulsCaliber(BossModule module) : Components.StandardAOEs(module, AID.SolidSoulsCaliber, new AOEShapeCircle(10));

class D99ExcaliburStates : StateMachineBuilder
{
    public D99ExcaliburStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<SolidSoulsCaliber>()
            .ActivateOnEnter<EmptySoulsCaliber>()
            .ActivateOnEnter<VacuumSlash>()
            .ActivateOnEnter<AbyssalSlash1>()
            .ActivateOnEnter<AbyssalSlash2>()
            .ActivateOnEnter<AbyssalSlash3>()
            .ActivateOnEnter<Exflammeus>()
            .ActivateOnEnter<IceShoot>()
            .ActivateOnEnter<IceBloomCross>()
            .ActivateOnEnter<ThermalDivide>()
            .ActivateOnEnter<ThermalDivideSides>()
            .ActivateOnEnter<Steelstrike>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 906, NameID = 12100)]
public class D99Excalibur(ModuleInit init) : BossModule(init, new(-600, -300), new ArenaBoundsCircle(19.5f));

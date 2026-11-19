namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.D99Excalibur;

public enum OID : uint
{
    Boss = 0x3CFC, // R5.1
    Caliburnus = 0x3CFD, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack = 31355, // Boss->player, no cast, single-target

    ParadoxBoss = 31353, // Boss->self, 5.0s cast, single-target // Raidwide
    ParadoxRoomwide = 31354, // Helper->self, no cast, range 100 circle // Assigns buffs w/ boss

    Caliburni = 31333, // Boss->self, 5.0+0.5s cast, single-target

    Steelstrike1 = 31334, // Caliburnus->location, 2.0s cast, width 4 rect charge
    Steelstrike2 = 31335, // Caliburnus->location, 2.0s cast, width 4 rect charge

    ThermalDivideILFR = 31349, // Boss->self, 5.0+2.0s cast, single-target // Ice Left | Fire Right
    ThermalDivideFLIR = 31350, // Boss->self, 5.0+2.0s cast, single-target // Fire Left | Ice Right
    ThermalDivide1 = 32701, // Helper->self, 6.7s cast, range 40 width 8 rect
    ThermalDivide2 = 31351, // Helper->self, no cast, range 40 width 40 rect
    CallDimensionBlade = 31338, // Boss->self, 4.0s cast, single-target
    SteelFlame = 31336, // Caliburnus->location, 2.0s cast, width 4 rect charge
    SteelFrost = 31337, // Caliburnus->location, 2.0s cast, width 4 rect charge
    Frostforge = 31330, // Boss->self, 3.0s cast, single-target
    Flameforge = 31329, // Boss->self, 3.0s cast, single-target

    ExflammeusBoss = 31343, // Boss->self, 4.0s cast, single-target
    ExflammeusHelpers = 31344, // Helper->self, 5.0s cast, range 8 circle

    SolidSoulsCaliber = 31327, // Boss->self, 6.0s cast, range 10 circle
    EmptySoulsCaliber = 31328, // Boss->self, 6.0s cast, range 5-40 donut
    AbyssalSlash1 = 31339, // Helper->self, 7.0s cast, range 2-7 donut
    AbyssalSlash2 = 31340, // Helper->self, 7.0s cast, range 7-12 donut
    AbyssalSlash3 = 31341, // Helper->self, 7.0s cast, range 17-22 donut
    VacuumSlash = 31342, // Helper->self, 7.0s cast, range 80 45-degree cone

    // Raidwide that cast after solid/empty souls. Need to be opposite elements
    FlamesRevelation = 31331, // Helper->self, no cast, range 60 circle
    FrostsRevelation = 31332, // Helper->self, no cast, range 60 circle

    Exglacialis = 31345, // Boss->self, 4.0s cast, single-target
    IceShoot = 31346, // Helper->self, 5.0s cast, range 6 circle
    IceBloomAoe = 31347, // Helper->self, 4.0s cast, range 6 circle
    IceBloomCross = 31348, // Helper->self, no cast, range 40 width 5 cross

    Teleport = 31326, // Boss->location, no cast, single-target
}

public enum SID : uint
{
    SoulOfIce = 3411, // Helper->player, extra=0x0
    SoulOfFire = 3410, // Helper->player, extra=0x0
    CaliburnusElement = 2552 // none->Caliburnus, extra=0x219/0x21A (0x21A = frost, 0x219 = fire)
}

class SolidSouls(BossModule module) : Components.StandardAOEs(module, AID.SolidSoulsCaliber, new AOEShapeCircle(10));
class EmptySouls(BossModule module) : Components.StandardAOEs(module, AID.EmptySoulsCaliber, new AOEShapeDonut(5, 40));
class AbyssalSlashSmall(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSlash1, new AOEShapeDonutSector(2, 7, 90.Degrees()));
class AbyssalSlashMedium(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSlash2, new AOEShapeDonutSector(7, 12, 90.Degrees()));
class AbyssalSlashLarge(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSlash3, new AOEShapeDonutSector(17, 22, 90.Degrees()));
class VacuumSlash(BossModule module) : Components.StandardAOEs(module, AID.VacuumSlash, new AOEShapeCone(80, 22.5f.Degrees()));
class Exflammeus(BossModule module) : Components.StandardAOEs(module, AID.ExflammeusHelpers, new AOEShapeCircle(8), 9, highlightImminent: true);
class IceShootAoe(BossModule module) : Components.StandardAOEs(module, AID.IceShoot, new AOEShapeCircle(6));
class IceBloom(BossModule module) : Components.GenericAOEs(module)
{
    static readonly AOEShapeCircle _circle = new(6);
    static readonly AOEShapeCross _cross = new(40, 2.5f);

    readonly List<Actor> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _casters)
        {
            var cast = c.CastInfo!;
            var activation = Module.CastFinishAt(cast);
            yield return new(_circle, cast.LocXZ, Activation: activation);
            yield return new(_cross, cast.LocXZ, cast.Rotation, activation);
            yield return new(_cross, cast.LocXZ, cast.Rotation + 45f.Degrees(), activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.IceBloomAoe)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.IceBloomAoe)
            _casters.Remove(caster);
    }
}
class ThermalDivide(BossModule module) : Components.GenericAOEs(module)
{
    enum Element
    {
        None,
        Ice,
        Fire
    }

    static readonly AOEShapeCone _cone = new(40, 90f.Degrees());
    static readonly AOEShapeRect _centerRect = new(40, 4);
    static readonly Angle _a90 = 90f.Degrees();

    bool? _fireLeft;
    Angle _rotation;
    WDir _offset;
    WPos _origin;
    DateTime _activation;

    readonly Element[] _playerElement = new Element[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (!Raid.TryFindSlot(actor, out var slot))
            return;

        switch ((SID)status.ID)
        {
            case SID.SoulOfIce:
                _playerElement[slot] = Element.Ice;
                break;
            case SID.SoulOfFire:
                _playerElement[slot] = Element.Fire;
                break;
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_fireLeft == null)
            yield break;

        var pos1 = _origin + _offset;
        var pos2 = _origin - _offset;
        var element = _playerElement[slot];

        // no debuff only happens after a raise at a bad time; still pick a side
        var matchingIsLeft = element == Element.None
            ? !_fireLeft.Value
            : element == Element.Fire ? _fireLeft.Value : !_fireLeft.Value;

        if (matchingIsLeft)
        {
            yield return new(_cone, pos1, _rotation + _a90, _activation);
            yield return new(_cone, pos2, _rotation - _a90, _activation, ArenaColor.SafeFromAOE, Inverted: true);
        }
        else
        {
            yield return new(_cone, pos2, _rotation - _a90, _activation);
            yield return new(_cone, pos1, _rotation + _a90, _activation, ArenaColor.SafeFromAOE, Inverted: true);
        }

        yield return new(_centerRect, _origin, _rotation, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ThermalDivideILFR or AID.ThermalDivideFLIR)
        {
            _fireLeft = (AID)spell.Action.ID == AID.ThermalDivideFLIR;
            _rotation = spell.Rotation;
            _origin = spell.LocXZ;
            _offset = 4 * (spell.Rotation + _a90).ToDirection();
            _activation = Module.CastFinishAt(spell, 1.9f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThermalDivide1 or AID.ThermalDivide2)
            _fireLeft = null;
    }
}
class Steelstrike(BossModule module) : Components.GenericAOEs(module)
{
    enum Element
    {
        None,
        Fire,
        Ice
    }

    static readonly AOEShapeRect _rect = new(40, 2);
    static readonly WPos _arenaCenter = new(-600, -300);
    const float _centerForbidRadius = 8f;
    const float _outerBias = 0.75f;
    const float _goalWeight = 10f;

    readonly Dictionary<Actor, AOEInstance> _aoes = [];
    readonly Dictionary<Element, List<AOEInstance>> _swords = new()
    {
        [Element.Fire] = [],
        [Element.Ice] = []
    };

    Element? _nextRaidwide;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var all = _aoes.Values;
        if (_nextRaidwide == null)
            return all;

        var element = PlayerElement(actor);
        if (element == Element.None || element != _nextRaidwide)
            return all;

        var safeSwords = _swords[SafeElement(_nextRaidwide.Value)];
        var result = all.Except(safeSwords).ToList();
        result.AddRange(safeSwords.Select(s => s with { Color = ArenaColor.SafeFromAOE, Risky = false }));
        return result;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_nextRaidwide == null)
            return;

        var element = PlayerElement(actor);
        if (element == Element.None || element != _nextRaidwide)
            return;

        var safeSwords = _swords[SafeElement(_nextRaidwide.Value)]
            .Where(s => s.Shape is AOEShapeRect)
            .Select(s =>
            {
                var rect = (AOEShapeRect)s.Shape;
                var outerPoint = s.Origin + s.Rotation.ToDirection() * (rect.LengthFront * _outerBias);
                return (rect, s.Origin, s.Rotation, outerPoint);
            })
            .ToList();

        if (safeSwords.Count == 0)
            return;

        var safe = ShapeDistance.Union([.. safeSwords.Select(s => ShapeDistance.Rect(s.Origin, s.Rotation, s.rect.LengthFront, s.rect.LengthBack, s.rect.HalfWidth))]);
        hints.AddForbiddenZone(ShapeDistance.Union([-safe, ShapeDistance.Circle(_arenaCenter, _centerForbidRadius)]));

        var nearest = safeSwords.MinBy(s => (s.outerPoint - actor.Position).LengthSq());
        hints.GoalZones.Add(AIHints.GoalProximity(nearest.outerPoint, nearest.rect.LengthFront * 0.25f, _goalWeight));
    }

    static Element PlayerElement(Actor actor)
    {
        if (actor.FindStatus((uint)SID.SoulOfFire) != null)
            return Element.Fire;
        if (actor.FindStatus((uint)SID.SoulOfIce) != null)
            return Element.Ice;
        return Element.None;
    }

    static Element SafeElement(Element raidwideElement) => raidwideElement == Element.Fire ? Element.Ice : Element.Fire;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Steelstrike1:
            case AID.Steelstrike2:
            case AID.SteelFlame:
            case AID.SteelFrost:
                var dir = spell.LocXZ - caster.Position;
                var aoe = new AOEInstance(_rect, caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell));
                _aoes[caster] = aoe;

                if ((AID)spell.Action.ID == AID.SteelFlame)
                    _swords[Element.Fire].Add(aoe);
                else if ((AID)spell.Action.ID == AID.SteelFrost)
                    _swords[Element.Ice].Add(aoe);
                break;
            case AID.Flameforge:
                _nextRaidwide = Element.Fire;
                break;
            case AID.Frostforge:
                _nextRaidwide = Element.Ice;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.Steelstrike1 or AID.Steelstrike2 or AID.SteelFlame or AID.SteelFrost)
        {
            if (_aoes.TryGetValue(caster, out var aoe))
            {
                _swords[Element.Fire].Remove(aoe);
                _swords[Element.Ice].Remove(aoe);
            }
            _aoes.Remove(caster);
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        // only the very first swords need this; later ones carry the element in the cast ID
        if ((SID)status.ID != SID.CaliburnusElement)
            return;

        if (!_aoes.TryGetValue(actor, out var aoe))
            return;

        var element = status.Extra == 0x219 ? Element.Fire : Element.Ice;
        if (!_swords[Element.Fire].Contains(aoe) && !_swords[Element.Ice].Contains(aoe))
            _swords[element].Add(aoe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FlamesRevelation or AID.FrostsRevelation)
            _nextRaidwide = null;
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_nextRaidwide != null)
            hints.Add($"Next raidwide: {_nextRaidwide}");
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (_nextRaidwide == null)
            return;

        var element = PlayerElement(actor);
        if (element != Element.None && element == _nextRaidwide)
            hints.Add($"Get hit by a {SafeElement(element)} blade!");
    }
}

class D99ExcaliburStates : StateMachineBuilder
{
    public D99ExcaliburStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ThermalDivide>()
            .ActivateOnEnter<SolidSouls>()
            .ActivateOnEnter<EmptySouls>()
            .ActivateOnEnter<AbyssalSlashSmall>()
            .ActivateOnEnter<AbyssalSlashMedium>()
            .ActivateOnEnter<AbyssalSlashLarge>()
            .ActivateOnEnter<Exflammeus>()
            .ActivateOnEnter<IceShootAoe>()
            .ActivateOnEnter<IceBloom>()
            .ActivateOnEnter<VacuumSlash>()
            .ActivateOnEnter<Steelstrike>();
    }
}

[ModuleInfo(Contributors = "LegendofIceman", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 906, NameID = 12100)]
public class D99Excalibur(ModuleInit init) : BossModule(init, new(-600, -300), new ArenaBoundsCircle(19.5f));

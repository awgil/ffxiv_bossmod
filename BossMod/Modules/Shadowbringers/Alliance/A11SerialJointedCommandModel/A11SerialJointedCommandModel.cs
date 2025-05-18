

namespace BossMod.Shadowbringers.Alliance.A11SerialJointedCommandModel;

public enum OID : uint
{
    Boss = 0x2C61,
    Helper = 0x233C,
    _Gen_SerialJointedServiceModel = 0x2C63, // R2.400, x12
    _Gen_SerialJointedServiceModel1 = 0x2C65, // R1.000, x0 (spawn during fight)
}

public enum IconID : uint
{
    _Gen_Icon_198 = 198, // player->self
    _Gen_Icon_164 = 164, // player->self
}

public enum AID : uint
{
    _AutoAttack_Attack = 872, // Boss->player, no cast, single-target
    _Weaponskill_SystematicSiege = 18610, // Boss->self, 2.5s cast, single-target
    _Weaponskill_ = 19249, // 2C63->self, no cast, single-target
    _Weaponskill_1 = 18611, // 2C63->self, 1.0s cast, single-target
    _Weaponskill_ClangingBlow = 18638, // Boss->player, 4.0s cast, single-target
    _Weaponskill_EnergyBomb = 18612, // 2C65->player/2C66, no cast, single-target
    _Weaponskill_EnergyBombardment = 18615, // Boss->self, 3.0s cast, single-target
    _Weaponskill_EnergyBombardment1 = 18616, // Helper->location, 3.0s cast, range 4 circle
    _Weaponskill_ForcefulImpact = 18639, // Boss->self, 4.0s cast, range 100 circle
    _Weaponskill_EnergyAssault = 18613, // Boss->self, 5.0s cast, single-target
    _Weaponskill_EnergyAssault1 = 18614, // Helper->self, no cast, range 30 ?-degree cone
    _Weaponskill_2 = 18960, // Boss->self, no cast, single-target
    _Weaponskill_SystematicTargeting = 18628, // Boss->self, 2.5s cast, single-target
    _Weaponskill_HighPoweredLaser = 18629, // 2C63->self, no cast, range 70 width 4 rect
    _Weaponskill_SidestrikingSpin = 18634, // Boss->self, 6.0s cast, single-target
    _Weaponskill_SidestrikingSpin1 = 18635, // Helper->self, 6.3s cast, range 30 width 12 rect
    _Weaponskill_SidestrikingSpin2 = 18636, // Helper->self, 6.3s cast, range 30 width 12 rect
    _Weaponskill_SystematicAirstrike = 18617, // Boss->self, 2.5s cast, single-target
    _Weaponskill_3 = 19250, // 2C63->self, no cast, single-target
    _Weaponskill_AirToSurfaceEnergy = 18618, // Helper->self, no cast, range 5 circle
    _Weaponskill_Shockwave = 18627, // Boss->self, 5.0s cast, range 100 circle
    _Weaponskill_EnergyRing = 18619, // Boss->self, 3.5s cast, single-target
    _Weaponskill_EnergyRing1 = 18620, // Helper->self, 4.7s cast, range 12 circle
    _Weaponskill_EnergyRing2 = 18621, // Boss->self, no cast, single-target
    _Weaponskill_EnergyRing3 = 18622, // Helper->self, 6.7s cast, range 12-24 donut
    _Weaponskill_EnergyRing4 = 18623, // Boss->self, no cast, single-target
    _Weaponskill_EnergyRing5 = 18624, // Helper->self, 8.7s cast, range 24-36 donut
    _Weaponskill_EnergyRing6 = 18625, // Boss->self, no cast, single-target
    _Weaponskill_EnergyRing7 = 18626, // Helper->self, 10.7s cast, range 36-48 donut
}

class ForcefulImpact(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_ForcefulImpact);
class ClangingBlow(BossModule module) : Components.SingleTargetCast(module, AID._Weaponskill_ClangingBlow);

class EnergyBomb(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_EnergyBomb)
{
    public readonly List<Actor> Bombs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Bombs.Select(b => new AOEInstance(new AOEShapeCircle(2), b.Position, b.Rotation));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x11D2 && actor.OID == 0x2C65)
            Bombs.Add(actor);
        if (id == 0x11E7 && actor.OID == 0x2C65)
            Bombs.Remove(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && WorldState.Actors.Find(spell.MainTargetID) is { } target)
        {
            Service.Log($"{target.Position} exploded by {caster.Position} ({(caster.Position - target.Position).Length()})");
            Bombs.Remove(caster);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var aoe in ActiveAOEs(slot, actor))
        {
            hints.AddForbiddenZone(aoe.Shape, aoe.Origin, aoe.Rotation, aoe.Activation);
            hints.AddForbiddenZone(ShapeContains.Capsule(aoe.Origin, aoe.Rotation, 10, 2), WorldState.FutureTime(4));
            hints.AddForbiddenZone(ShapeContains.Capsule(aoe.Origin, aoe.Rotation, 5, 2), WorldState.FutureTime(2));
        }
    }
}

// there's no good way to figure out which player is baiting which turret so we treat them like regular AOEs
class HighPoweredLaser(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_HighPoweredLaser)
{
    public readonly List<Actor> Turrets = [];
    public DateTime Activation { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Activation == default ? [] : Turrets.Select(t => new AOEInstance(new AOEShapeRect(40, 2), t.Position, t.Rotation, Activation, Risky: WorldState.FutureTime(1) > Activation));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == 0x2C63 && id == 0x1E43)
            Turrets.Add(actor);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID._Gen_Icon_164 && Activation == default)
            Activation = WorldState.FutureTime(6.6f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Turrets.Remove(caster);
        }
    }
}

class SidestrikingSpin(BossModule module) : Components.GroupedAOEs(module, [AID._Weaponskill_SidestrikingSpin1, AID._Weaponskill_SidestrikingSpin2], new AOEShapeRect(30, 6));
class EnergyBombardment(BossModule module) : Components.StandardAOEs(module, AID._Weaponskill_EnergyBombardment1, new AOEShapeCircle(4));

class EnergyAssault(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_EnergyAssault1)
{
    private readonly List<(Actor, DateTime)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCone(30, 45.Degrees()), c.Item1.Position, c.Item1.CastInfo?.Rotation ?? c.Item1.Rotation, c.Item2));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_EnergyAssault)
            Casters.Add((caster, Module.CastFinishAt(spell, 2.2f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts >= 5)
                Casters.Clear();
        }
    }
}

class AirToSurfaceEnergy(BossModule module) : Components.GenericAOEs(module, AID._Weaponskill_AirToSurfaceEnergy)
{
    private readonly List<WPos> _spawns = [];
    private readonly List<(WPos expected, WPos actual)> _unexpected = [];
    private static readonly List<WPos> _centers = [.. CurveApprox.Rect(new(12, 0), new(0, 12)).Select(c => new WPos(-500, 0) + c)];

    public int NumStarts;

    public static readonly List<WDir> PatternOut = [
        new(10, -4.6f),
        new(10, -7.8f),
        new(9.1f, -10),
        new(5.9f, -10),
        new(2.7f, -10),
        new(-0.5f, -10),
        new(-3.7f, -10),
        new(-7, -10),
        new(-10, -10),
        new(-10, -6.8f),
        new(-10, -3.6f),
        new(-10, -0.4f),
        new(-10, 2.8f),
        new(-10, 6),
        new(-10, 9.2f)
    ];
    public static readonly List<WDir> PatternIn = [
        new(2.4f, 4),
        new(4, 2.5f),
        new(4, -0.7f),
        new(4, -3.9f),
        new(1.2f, -4),
        new(-2, -4),
        new(-4, -2.9f),
        new(-4, 0.3f),
        new(-4, 3.5f),
        new(-1.4f, 4),
        new(1.7f, 4),
        new(4, 3.2f),
        new(4, 0),
        new(4, -3.2f),
        new(1.9f, -4)
    ];

    public const int AOEsToShow = 5;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select((p, i) => p with { Color = i < 8 ? ArenaColor.Danger : ArenaColor.AOE }).Take(AOEsToShow * 8).Reverse();

    private readonly List<AOEInstance> _predicted = [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_3)
        {
            NumStarts++;
            _spawns.Add(caster.Position);
            var pivot = _centers.MinBy(c => (c - caster.Position).Length());
            var close = caster.Position.AlmostEqual(pivot, 8);
            var patternBase = caster.Position.AlmostEqual(pivot, 8) ? PatternIn : PatternOut;
            var pattern = NumStarts < 9 ? patternBase.Take(10) : patternBase;

            var (closeRot, farRot) = Angle.FromDirection(caster.Position - pivot).Deg switch
            {
                > 90 => (-90.Degrees(), 90.Degrees()),
                > 0 => (180.Degrees(), default),
                > -90 => (90.Degrees(), -90.Degrees()),
                _ => (default, 180.Degrees())
            };

            var start = WorldState.FutureTime(12.2f);
            foreach (var p in pattern)
            {
                _predicted.Add(new AOEInstance(new AOEShapeCircle(5), pivot + p.Rotate(close ? closeRot : farRot), default, start));
                start = start.AddSeconds(1.1f);
            }
            _predicted.SortBy(p => p.Activation);
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_SidestrikingSpin)
        {
            _spawns.Clear();
        }

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var ix = _predicted.FindIndex(p => p.Origin.AlmostEqual(caster.Position, 0.5f));
            if (ix < 0)
            {
                ReportError($"missing predicted cast for {caster} at {caster.Position}");
                _unexpected.Add((default, caster.Position));
            }
            else
            {
                var dist = (caster.Position - _predicted[ix].Origin).Length();
                if (dist > 0.25f)
                {
                    ReportError($"distance: {dist}");
                    _unexpected.Add((_predicted[ix].Origin, caster.Position));
                }
                _predicted.RemoveAt(ix);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var s in _spawns)
        {
            var c = _centers.MinBy(c => (s - c).LengthSq());
            var off = s - c;
            Arena.TextWorld(s, Angle.FromDirection(off).ToString(), ArenaColor.Object);
        }

        foreach (var (exp, act) in _unexpected)
        {
            Arena.AddCircle(act, 5, ArenaColor.Enemy);
            Arena.AddCircle(exp, 5, ArenaColor.Object);
        }
    }
}

class ATSELog(BossModule module) : BossComponent(module)
{
    private readonly List<WPos> _casts = [];
    private List<WDir>[][] _offs = [[[], []], [[], []], [[], []], [[], []]];
    private static readonly List<WPos> Centers = CurveApprox.Rect(new(12, 0), new(0, 12)).Select(c => new WPos(-500, 0) + c).ToList();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID._Weaponskill_AirToSurfaceEnergy)
        {
            _casts.Add(caster.Position);
            var closest = Centers.MinBy(c => (c - caster.Position).Length());
            var ix = Centers.IndexOf(closest);
            var close = caster.Position.AlmostEqual(closest, 8) ? 0 : 1;
            _offs[ix][close].Add(caster.Position - closest);
        }

        if ((AID)spell.Action.ID == AID._Weaponskill_SidestrikingSpin)
        {
            _casts.Clear();
            _offs = [[[], []], [[], []], [[], []], [[], []]];
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        //foreach (var c in _casts)
        //    Arena.AddCircle(c, 5, ArenaColor.Danger);
    }
}

class A11SerialJointedCommandModelStates : StateMachineBuilder
{
    public A11SerialJointedCommandModelStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ForcefulImpact>()
            .ActivateOnEnter<ClangingBlow>()
            .ActivateOnEnter<EnergyBomb>()
            .ActivateOnEnter<HighPoweredLaser>()
            .ActivateOnEnter<EnergyBombardment>()
            .ActivateOnEnter<SidestrikingSpin>()
            .ActivateOnEnter<EnergyAssault>()
            .ActivateOnEnter<AirToSurfaceEnergy>()
            .ActivateOnEnter<ATSELog>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9141)]
public class A11SerialJointedCommandModel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-500, 0), new ArenaBoundsSquare(23.5f));


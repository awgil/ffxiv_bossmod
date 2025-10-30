namespace BossMod.Shadowbringers.Alliance.A11SerialJointedCommandModel;

public enum OID : uint
{
    Boss = 0x2C61,
    Helper = 0x233C,
    Turret1 = 0x2C63, // R2.400, x12
    Turret2 = 0x2C65, // R1.000, x0 (spawn during fight)
}

public enum IconID : uint
{
    Tankbuster = 198, // player->self
    LockOn = 164, // player->self
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    SystematicSiege = 18610, // Boss->self, 2.5s cast, single-target
    Unk0 = 19249, // Turret1->self, no cast, single-target
    Unk1 = 18611, // Turret1->self, 1.0s cast, single-target
    ClangingBlow = 18638, // Boss->player, 4.0s cast, single-target
    EnergyBomb = 18612, // Turret2->player/2P, no cast, single-target
    EnergyBombardment = 18615, // Boss->self, 3.0s cast, single-target
    EnergyBombardment1 = 18616, // Helper->location, 3.0s cast, range 4 circle
    ForcefulImpact = 18639, // Boss->self, 4.0s cast, range 100 circle
    EnergyAssault = 18613, // Boss->self, 5.0s cast, single-target
    EnergyAssault1 = 18614, // Helper->self, no cast, range 30 90-degree cone
    Unk2 = 18960, // Boss->self, no cast, single-target
    SystematicTargeting = 18628, // Boss->self, 2.5s cast, single-target
    HighPoweredLaser = 18629, // Turret1->self, no cast, range 70 width 4 rect
    SidestrikingSpin = 18634, // Boss->self, 6.0s cast, single-target
    SidestrikingSpin1 = 18635, // Helper->self, 6.3s cast, range 30 width 12 rect
    SidestrikingSpin2 = 18636, // Helper->self, 6.3s cast, range 30 width 12 rect
    CentrifugalSpin = 18632, // Boss->self, 6.0s cast, single-target
    CentrifugalSpin1 = 18633, // Helper->self, 6.3s cast, range 30 width 8 rect
    SystematicAirstrike = 18617, // Boss->self, 2.5s cast, single-target
    AirToSurfaceAppear = 19250, // Turret1->self, no cast, single-target
    AirToSurfaceEnergy = 18618, // Helper->self, no cast, range 5 circle
    Shockwave = 18627, // Boss->self, 5.0s cast, range 100 circle
    EnergyRingCast1 = 18619, // Boss->self, 3.5s cast, single-target
    EnergyRingCast2 = 18621, // Boss->self, no cast, single-target
    EnergyRingCast3 = 18623, // Boss->self, no cast, single-target
    EnergyRingCast4 = 18625, // Boss->self, no cast, single-target
    EnergyRing1 = 18620, // Helper->self, 4.7s cast, range 12 circle
    EnergyRing2 = 18622, // Helper->self, 6.7s cast, range 12-24 donut
    EnergyRing3 = 18624, // Helper->self, 8.7s cast, range 24-36 donut
    EnergyRing4 = 18626, // Helper->self, 10.7s cast, range 36-48 donut
    SystematicSuppression = 18630, // Boss->self, 2.5s cast, single-target
    HighCaliberLaserCast = 18631, // Turret1->self, 7.0s cast, single-target
    HighCaliberLaser = 18682, // Helper->self, 7.0s cast, range 70 width 24 rect
}

class ForcefulImpact(BossModule module) : Components.RaidwideCast(module, AID.ForcefulImpact);
class ClangingBlow(BossModule module) : Components.SingleTargetCast(module, AID.ClangingBlow);
class CentrifugalSpin(BossModule module) : Components.StandardAOEs(module, AID.CentrifugalSpin1, new AOEShapeRect(30, 4));
class Shockwave(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Shockwave, 15);
class HighCaliberLaser(BossModule module) : Components.StandardAOEs(module, AID.HighCaliberLaser, new AOEShapeRect(70, 12))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var group1 = DateTime.MinValue;
        foreach (var aoe in base.ActiveAOEs(slot, actor))
        {
            if (group1 == DateTime.MinValue)
                group1 = aoe.Activation.AddSeconds(0.5f);
            yield return aoe with { Color = aoe.Activation < group1 ? ArenaColor.Danger : ArenaColor.AOE, Risky = aoe.Activation < group1 };
        }
    }
}

class EnergyBomb : Components.PersistentVoidzone
{
    private readonly List<Actor> _balls = [];

    public EnergyBomb(BossModule module) : base(module, 2, _ => [], 8)
    {
        Sources = _ => _balls;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EnergyBomb)
            _balls.Remove(caster);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (actor.OID == (uint)OID.Turret2)
        {
            if (id == 0x11D2)
                _balls.Add(actor);
            if (id == 0x11E7)
                _balls.Remove(actor);
        }
    }
}

// there's no good way to figure out which player is baiting which turret so we treat them like regular AOEs
class HighPoweredLaser(BossModule module) : Components.GenericAOEs(module, AID.HighPoweredLaser)
{
    public readonly List<Actor> Turrets = [];
    public DateTime Activation { get; private set; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Activation == default ? [] : Turrets.Select(t => new AOEInstance(new AOEShapeRect(40, 2), t.Position, t.Rotation, Activation, Risky: WorldState.FutureTime(1) > Activation));

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.Turret1 && id == 0x1E43)
            Turrets.Add(actor);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.LockOn && Activation == default)
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

class SidestrikingSpin(BossModule module) : Components.GroupedAOEs(module, [AID.SidestrikingSpin1, AID.SidestrikingSpin2], new AOEShapeRect(30, 6));
class EnergyBombardment(BossModule module) : Components.StandardAOEs(module, AID.EnergyBombardment1, new AOEShapeCircle(4));

class EnergyAssault(BossModule module) : Components.GenericAOEs(module, AID.EnergyAssault1)
{
    private readonly List<(Actor, DateTime)> Casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Casters.Select(c => new AOEInstance(new AOEShapeCone(30, 45.Degrees()), c.Item1.Position, c.Item1.CastInfo?.Rotation ?? c.Item1.Rotation, c.Item2));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EnergyAssault)
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

class AirToSurfaceEnergy(BossModule module) : Components.GenericAOEs(module, AID.AirToSurfaceEnergy)
{
    private static readonly List<WPos> _centers = [.. CurveApprox.Rect(new(12, 0), new(0, 12)).Select(c => new WPos(-500, 0) + c)];
    private readonly List<AOEInstance> _predicted = [];

    public int NumStarts;
    public const int AOEsToShow = 5;

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

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Select((p, i) => p with { Color = i < 8 ? ArenaColor.Danger : ArenaColor.AOE }).Take(AOEsToShow * 8).Reverse();

    public override void Update()
    {
        // remove garbage from minimap if the component screws up
        _predicted.RemoveAll(p => p.Activation.AddSeconds(5) < WorldState.CurrentTime);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AirToSurfaceAppear)
        {
            NumStarts++;
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

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var ix = _predicted.FindIndex(p => p.Origin.AlmostEqual(caster.Position, 0.5f));
            if (ix < 0)
                ReportError($"missing predicted cast for {caster} at {caster.Position}");
            else
                _predicted.RemoveAt(ix);
        }
    }
}

class EnergyRing(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(12), new AOEShapeDonut(12, 24), new AOEShapeDonut(24, 36), new AOEShapeDonut(36, 48)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EnergyRing1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var order = (AID)spell.Action.ID switch
        {
            AID.EnergyRing1 => 0,
            AID.EnergyRing2 => 1,
            AID.EnergyRing3 => 2,
            AID.EnergyRing4 => 3,
            _ => -1
        };
        AdvanceSequence(order, caster.Position, WorldState.FutureTime(2));
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
            .ActivateOnEnter<CentrifugalSpin>()
            .ActivateOnEnter<EnergyAssault>()
            .ActivateOnEnter<AirToSurfaceEnergy>()
            .ActivateOnEnter<EnergyRing>()
            .ActivateOnEnter<Shockwave>()
            .ActivateOnEnter<HighCaliberLaser>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9141)]
public class A11SerialJointedCommandModel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-500, 0), new ArenaBoundsSquare(23.5f));

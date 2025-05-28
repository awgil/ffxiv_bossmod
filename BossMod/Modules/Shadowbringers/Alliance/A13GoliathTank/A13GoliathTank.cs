using Lumina.Extensions;

namespace BossMod.Shadowbringers.Alliance.A13GoliathTank;

public enum OID : uint
{
    Boss = 0x2C7E,
    FlightUnit = 0x2C80,
    Helper = 0x233C,
    MediumExploder = 0x2C7F, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    EnergyRingCast = 18738, // Boss->self, 3.0s cast, single-target
    EnergyRingVisual1 = 18740, // Boss->self, no cast, single-target
    EnergyRingVisual2 = 18741, // Boss->self, no cast, single-target
    EnergyRingVisual3 = 18742, // Boss->self, no cast, single-target
    EnergyRingVisual4 = 18739, // Boss->self, no cast, single-target
    EnergyRing1 = 18743, // Helper->self, 4.0s cast, range 12 circle
    EnergyRing2 = 18744, // Helper->self, 4.0s cast, range 12-24 donut
    EnergyRing3 = 18745, // Helper->self, 4.0s cast, range 24-36 donut
    EnergyRing4 = 18746, // Helper->self, 4.0s cast, range 36-48 donut
    ConvenientSelfDestruction = 18748, // 2C7F->self, no cast, range 10 circle
    LaserTurret = 18747, // Boss->self, 4.0s cast, range 85 width 10 rect
    AutoAttackUnit = 18189, // FlightUnit->player, no cast, single-target
    FlightDash1 = 18749, // FlightUnit->self, no cast, single-target
    FlightDash2 = 18751, // FlightUnit->location, no cast, single-target
    AreaBombingManeuver = 18754, // FlightUnit->self, 3.0s cast, single-target
    BallisticImpact = 18755, // Helper->location, 1.0s cast, range 4 circle
    A360DegreeBombingManeuver = 18753, // FlightUnit->self, 5.0s cast, range 100 circle
    LightfastBlade = 18752, // FlightUnit->self, 5.0s cast, range 48 180-degree cone
}

public enum IconID : uint
{
    Spread = 189, // MediumExploder->self
    Marker = 23, // player->self
}

public enum TetherID : uint
{
    Generic = 17, // MediumExploder/player->player/FlightUnit
}

class R360DegreeBombingManeuver(BossModule module) : Components.RaidwideCast(module, AID.A360DegreeBombingManeuver);
class LaserTurret(BossModule module) : Components.StandardAOEs(module, AID.LaserTurret, new AOEShapeRect(85, 5));
class LightfastBlade(BossModule module) : Components.StandardAOEs(module, AID.LightfastBlade, new AOEShapeCone(48, 90.Degrees()));
class EnergyRing(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(12), new AOEShapeDonut(12, 24), new AOEShapeDonut(24, 36), new AOEShapeDonut(36, 48)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.EnergyRing1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void Update()
    {
        if (Module.PrimaryActor.IsDeadOrDestroyed)
            Sequences.Clear();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var index = (AID)spell.Action.ID switch
        {
            AID.EnergyRing1 => 0,
            AID.EnergyRing2 => 1,
            AID.EnergyRing3 => 2,
            AID.EnergyRing4 => 3,
            _ => -1
        };
        AdvanceSequence(index, caster.Position, WorldState.FutureTime(2));
    }
}
class ConvenientSelfDestruction(BossModule module) : Components.GenericBaitAway(module, AID.ConvenientSelfDestruction, centerAtTarget: true)
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.MediumExploder && WorldState.Actors.Find(tether.Target) is { } target)
            CurrentBaits.Add(new(source, target, new AOEShapeCircle(10), WorldState.FutureTime(9.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.RemoveAll(b => b.Source == caster);
        }
    }
}

class AreaBombingBait(BossModule module) : Components.GenericBaitAway(module)
{
    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (WorldState.Actors.Find(tether.Target) is { } target && (OID)target.OID == OID.FlightUnit)
            CurrentBaits.Add(new(target, source, new AOEShapeRect(60, 4), WorldState.FutureTime(6)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BallisticImpact)
            CurrentBaits.Clear();
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (ActiveBaitsOn(actor).FirstOrNull() is { } bait)
            // if baiting, just stay far away enough from the boss that we can dodge out of exas in time
            hints.AddForbiddenZone(new AOEShapeCircle(8), bait.Source.Position, activation: bait.Activation);
        else
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class AreaBombingExa(BossModule module) : Components.Exaflare(module, new AOEShapeCircle(4), AID.BallisticImpact)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && !Lines.Any(l => l.Rotation == caster.Rotation))
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = caster.Rotation.ToDirection() * 4,
                Rotation = caster.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2,
                ExplosionsLeft = 7,
                MaxShownExplosions = 4
            });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            var l = Lines.FindIndex(l => l.Rotation == caster.Rotation);
            if (l >= 0)
                AdvanceLine(Lines[l], caster.Position);
        }
    }
}

class A13GoliathTankStates : StateMachineBuilder
{
    public A13GoliathTankStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<EnergyRing>()
            .ActivateOnEnter<ConvenientSelfDestruction>()
            .ActivateOnEnter<LaserTurret>()
            .ActivateOnEnter<AreaBombingBait>()
            .ActivateOnEnter<AreaBombingExa>()
            .ActivateOnEnter<R360DegreeBombingManeuver>()
            .ActivateOnEnter<LightfastBlade>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed && Module.Enemies(OID.FlightUnit).All(f => f.IsDeadOrDestroyed || f.HPMP.CurHP == 1);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9138)]
public class A13GoliathTank(WorldState ws, Actor primary) : BossModule(ws, primary, new(-780, 555), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.FlightUnit), ArenaColor.Enemy);
    }
}


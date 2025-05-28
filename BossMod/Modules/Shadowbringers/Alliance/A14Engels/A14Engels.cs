namespace BossMod.Shadowbringers.Alliance.A14Engels;

public enum OID : uint
{
    Boss = 0x2557,
    Helper = 0x233C,
    Helper2 = 0x2CCF, // R0.500, x1
    Marx1 = 0x2C0B, // R0.700, x1
    Marx2 = 0x2C0C, // R0.700, x1
    ReverseJointedGoliath = 0x2C07, // R3.600, x0 (spawn during fight)
    SmallBiped = 0x2C08, // R0.960, x0 (spawn during fight)
    MarxL = 0x2C09, // R18.500, x0 (spawn during fight)
    MarxR = 0x2C0A, // R18.500, x0 (spawn during fight)
    EnergyDispersalTower = 0x1EAEC7,
    IncendiaryBomb = 0x1EAEC8,
}

public enum AID : uint
{
    AutoBoss = 18257, // Boss->self, no cast, single-target
    AutoHelper = 18258, // Helper->player, no cast, single-target
    MarxSmashLeftBoss = 18214, // Boss->self, 6.0s cast, single-target, left side
    MarxSmashRightBoss = 18215, // Boss->self, 6.0s cast, single-target, right side
    MarxSmashLeft = 18216, // Helper->self, 1.6s cast, range 60 width 30 rect
    MarxSmashRight = 18217, // Helper->self, 1.6s cast, range 60 width 30 rect
    PrecisionGuidedMissileCast = 18259, // Boss->self, 4.0s cast, single-target
    PrecisionGuidedMissile = 18260, // Helper->player, 4.0s cast, range 6 circle
    IncendiaryBombingCast = 18232, // Boss->self, 5.0s cast, single-target
    IncendiaryBombing = 18233, // Helper->location, 4.0s cast, range 8 circle
    GuidedMissileCast = 18229, // Boss->self, 3.5s cast, single-target
    GuidedMissileFirst = 18230, // Helper->location, 5.0s cast, range 6 circle
    GuidedMissileRest = 18231, // Helper->location, no cast, range 6 circle
    DiffuseLaser = 18261, // Boss->self, 4.0s cast, range 60 width 60 rect
    MarxSmashOutsideFirst = 18222, // Boss->self, 6.0s cast, single-target
    MarxSmashOutsideFar = 18223, // Helper->self, 1.5s cast, range 35 width 60 rect
    MarxSmashOutsideSecond = 18224, // Boss->self, no cast, single-target
    MarxSmashOutsideLeft = 18226, // Helper->self, 0.5s cast, range 60 width 20 rect
    MarxSmashOutsideRight = 18225, // Helper->self, 0.5s cast, range 60 width 20 rect
    MarxSmashInsideFirst = 18218, // Boss->self, 6.0s cast, single-target
    MarxSmashInsideNear = 18219, // Helper->self, 1.5s cast, range 30 width 60 rect
    MarxSmashInsideSecond = 18220, // Boss->self, no cast, single-target
    MarxSmashInsideMiddle = 18221, // Helper->self, 0.5s cast, range 60 width 30 rect
    EnergyBarrage = 18236, // Boss->self, 3.0s cast, single-target
    LaserSight = 18234, // Boss->self, 4.1s cast, range 100 width 20 rect
    LaserSightRepeat = 18235, // Helper->self, no cast, range 100 width 20 rect
    EnergyDispersal = 18237, // Helper->self, no cast, range 4 circle
    SurfaceMissileCast = 18227, // Boss->self, 3.5s cast, single-target
    SurfaceMissile = 18228, // Helper->location, 4.0s cast, range 6 circle
    AutoLarge = 18264, // ReverseJointedGoliath->player, no cast, single-target
    AutoSmall = 872, // SmallBiped->player, no cast, single-target
    ArmLaser = 18263, // ReverseJointedGoliath->self, 3.0s cast, range 30 90-degree cone
    WideAngleDiffuseLaserVisual = 18240, // Boss->self, no cast, single-target
    WideAngleDiffuseLaser = 18241, // Helper->self, no cast, range 60 width 60 rect
    DemolishStructureVisual = 18244, // Boss->self, no cast, single-target
    DemolishStructure = 18245, // Helper->self, 9.3s cast, range 50 circle
    MarxActivation = 18600, // Boss->self, 3.0s cast, single-target
    MarxThrustVisual = 18262, // 2C0C/2C0B->self, 5.0s cast, single-target
    MarxThrust = 18684, // Helper->self, 5.5s cast, range 30 width 20 rect
    AreaBombardment = 18256, // Boss->self, 3.0s cast, single-target
    IncendiarySaturationBombingVisual = 18254, // Boss->self, 3.0s cast, single-target
    IncendiarySaturationBombing = 18255, // Helper->self, 6.0s cast, range 30 width 60 rect
    MarxCrushVisual = 18246, // Boss->self, 6.0s cast, single-target
    MarxCrush = 18247, // Helper->self, 6.0s cast, range 15 width 30 rect
    Frack = 18253, // Helper->self, no cast, range 15 circle
    RadiateHeat = 18252, // Helper->self, no cast, range 50 circle
    CrushingWheel = 18251, // 2C0A/2C09->self, 12.0s cast, range 20 width 30 rect

    Unk1 = 18239, // Boss->self, no cast, single-target
    Unk2 = 18243, // Boss->self, no cast, single-target
}

public enum IconID : uint
{
    Tankbuster = 198, // player->self
    IncendiaryBombing = 23, // player->self
    Chase = 197, // player->self
}

class PrecisionGuidedMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID.PrecisionGuidedMissile, 6);
class IncendiaryBombing(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.IncendiaryBombing, AID.IncendiaryBombing, 8, 9.1f)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == SpreadAction)
            Spreads.Clear();
    }
}
class IncendiaryBombingVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 8, AID.IncendiaryBombing, m => m.Enemies(OID.IncendiaryBomb).Where(e => e.EventState != 7), 0.1f);
class DiffuseLaser(BossModule module) : Components.RaidwideCast(module, AID.DiffuseLaser);

class LaserSight(BossModule module) : Components.GenericAOEs(module, AID.LaserSight)
{
    private readonly List<Actor> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(100, 10), c.Position, c.Rotation, Module.CastFinishAt(c.CastInfo)));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _casters.Add(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction || (AID)spell.Action.ID == AID.LaserSightRepeat)
        {
            NumCasts++;
            if (NumCasts >= 5)
            {
                _casters.Clear();
                NumCasts = 0;
            }
        }
    }
}

class EnergyDispersal(BossModule module) : Components.GenericTowers(module, AID.EnergyDispersal)
{
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.EnergyDispersalTower)
            Towers.Add(new(actor.Position, 4, maxSoakers: int.MaxValue, activation: WorldState.FutureTime(12.1f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
        }
    }
}

class SurfaceMissile(BossModule module) : Components.StandardAOEs(module, AID.SurfaceMissile, new AOEShapeCircle(6));

class Adds(BossModule module) : Components.AddsMulti(module, [OID.SmallBiped, OID.ReverseJointedGoliath]);
// TODO: add hints to point goliath away from party
class ArmLaser(BossModule module) : Components.StandardAOEs(module, AID.ArmLaser, new AOEShapeCone(30, 45.Degrees()));

// TODO figure out what the telegraph is for Wide-Angle Diffuse Laser going off, there is boss dialogue but it doesn't look like it lines up with any actions
// it happens a bit after the last reverse-jointed goliath dies but i don't really want to use that

class DemolishStructure(BossModule module) : Components.StandardAOEs(module, AID.DemolishStructure, new AOEShapeCircle(25));
class DemolishBounds(BossModule module) : BossComponent(module)
{
    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0B && state == 0x00020001)
            Arena.Center = new(900, 785);
    }
}

class MarxThrust(BossModule module) : Components.StandardAOEs(module, AID.MarxThrust, new AOEShapeRect(30, 10));

class IncendiarySaturationBombing(BossModule module) : Components.StandardAOEs(module, AID.IncendiarySaturationBombing, new AOEShapeRect(30, 30));
class IncendiarySaturationBombingVoidzone(BossModule module) : BossComponent(module)
{
    private WPos _oldCenter;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.IncendiarySaturationBombing)
        {
            _oldCenter = Arena.Center;
            Arena.Center += new WDir(0, -15);
            Arena.Bounds = new ArenaBoundsRect(30, 15);
        }
        if ((AID)spell.Action.ID == AID.MarxCrush)
            Arena.Bounds = new ArenaBoundsSquare(15);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x0C && state == 0x00080004)
        {
            Arena.Center = _oldCenter;
            Arena.Bounds = new ArenaBoundsSquare(30);
        }
    }
}
class AddsArms(BossModule module) : Components.AddsMulti(module, [OID.MarxR, OID.MarxL]);
// TODO wheel enrage
class CrushingWheel(BossModule module) : Components.StandardAOEs(module, AID.CrushingWheel, new AOEShapeRect(20, 15));

class A14EngelsStates : StateMachineBuilder
{
    public A14EngelsStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MarxSmashLR>()
            .ActivateOnEnter<PrecisionGuidedMissile>()
            .ActivateOnEnter<IncendiaryBombing>()
            .ActivateOnEnter<IncendiaryBombingVoidzone>()
            .ActivateOnEnter<GuidedMissileBait>()
            .ActivateOnEnter<GuidedMissile>()
            .ActivateOnEnter<DiffuseLaser>()
            .ActivateOnEnter<MarxSmashOutside>()
            .ActivateOnEnter<MarxSmashInside>()
            .ActivateOnEnter<LaserSight>()
            .ActivateOnEnter<EnergyDispersal>()
            .ActivateOnEnter<SurfaceMissile>()
            .ActivateOnEnter<Adds>()
            .ActivateOnEnter<ArmLaser>()
            .ActivateOnEnter<DemolishStructure>()
            .ActivateOnEnter<DemolishBounds>()
            .ActivateOnEnter<MarxThrust>()
            .ActivateOnEnter<IncendiarySaturationBombing>()
            .ActivateOnEnter<AddsArms>()
            .ActivateOnEnter<CrushingWheel>()
            .ActivateOnEnter<IncendiarySaturationBombingVoidzone>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9147)]
public class A14Engels(WorldState ws, Actor primary) : BossModule(ws, primary, new(900, 670), new ArenaBoundsSquare(30));


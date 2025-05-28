namespace BossMod.Shadowbringers.Alliance.A12Hobbes;

public enum OID : uint
{
    Boss = 0x2C2B,
    SmallExploder = 0x2C62, // R0.960, x0 (spawn during fight)
    Helper = 0x233C,
    AutoPart = 0x2C73, // R1.000, x3, Part type
    ImpactHelper = 0x2C2D, // R1.000, x1
    RobotArm = 0x2C2C, // R1.000, x1
    TetherHelper = 0x18D6, // R0.500, x6
}

public enum AID : uint
{
    AutoAttack = 18430, // AutoPart->player, no cast, single-target
    LaserResistanceTest = 18437, // Boss->self, 4.0s cast, range 50 circle
    LaserResistanceTestRepeat = 18438, // Boss->self, no cast, range 50 circle

    ShockingDischarge = 18443, // Helper->self, 2.0s cast, range 5 circle
    VariableCombatTestCast = 18446, // RobotArm->self, 5.0s cast, single-target

    ImpactRotation1 = 18709, // ImpactHelper->self, no cast, single-target
    ImpactRotation2 = 18710, // ImpactHelper->self, no cast, single-target
    ImpactRotation3 = 18711, // ImpactHelper->self, no cast, single-target
    ImpactRotation4 = 18712, // ImpactHelper->self, no cast, single-target
    Impact = 18450, // Helper->self, 7.7s cast, range 20 circle
    Towerfall = 18451, // Helper->self, no cast, range 20 width 8 rect

    FireResistanceTestInside = 18454, // Helper->self, no cast, range 70 20-degree cone
    FireResistanceTestOutside = 18455, // Helper->self, no cast, range 12 width 38 rect
    FireResistanceTestHalf = 18456, // Helper->self, no cast, range 22 width 42 rect

    VariableCombatConeSlow = 18885, // Helper->self, 5.7s cast, range 20 60-degree cone
    VariableCombatCircleSlow = 18886, // Helper->self, 5.7s cast, range 2 circle
    VariableCombatDonutSlow = 18887, // Helper->self, 5.7s cast, range 7-19 donut
    VariableCombatConeFast = 18447, // Helper->self, 2.0s cast, range 20 60-degree cone
    VariableCombatCircleFast = 18448, // Helper->self, 2.0s cast, range 2 circle
    VariableCombatDonutFast = 18449, // Helper->self, 2.0s cast, range 7-19 donut
    ConvenientSelfDestruction = 18460, // SmallExploder->self, no cast, range 5 circle

    RingLaser1Cast = 18431, // Boss->self, 3.4s cast, single-target
    RingLaser2Cast = 18432, // Boss->self, 1.0s cast, single-target
    RingLaser3Cast = 18433, // Boss->self, 1.0s cast, single-target
    RingLaser1 = 18434, // Helper->self, 4.0s cast, range 15-20 donut
    RingLaser2 = 18435, // Helper->self, 4.0s cast, range 10-15 donut
    RingLaser3 = 18436, // Helper->self, 4.0s cast, range 5-10 donut

    LaserSightCast = 18439, // Boss->self, 8.0s cast, single-target
    LaserSightTarget = 18440, // Helper->player, no cast, single-target
    LaserSightStack = 18441, // Helper->self, no cast, range 65 width 8 rect

    UnwillingCargo = 18458, // Helper->self, no cast, range 40 width 7 rect, 15y knockback
    ElectromagneticPulse = 18457, // Helper->self, no cast, range 40 width 5 rect
    OilWell = 18459, // Helper->self, no cast, ???, hits either the 4 circular platforms on SE platform or everything except the 4 circular platforms
    ShortRangeMissile = 18453, // Helper->players, 8.0s cast, range 8 circle

    ArmMotion1 = 18701, // RobotArm->self, no cast, single-target
    ArmMotion2 = 18702, // RobotArm->self, no cast, single-target
    ArmMotion3 = 18703, // RobotArm->self, no cast, single-target
    ArmMotion4 = 18704, // RobotArm->self, no cast, single-target
    ArmMotion5 = 18705, // RobotArm->self, no cast, single-target
    ArmMotion6 = 18706, // RobotArm->self, no cast, single-target

    ArmUnk1 = 18442, // RobotArm->self, no cast, single-target
    ArmUnk2 = 18444, // RobotArm->self, no cast, single-target
    Unk1 = 18707, // ImpactHelper->self, no cast, single-target
    UnkVariableCombatTest = 18445, // RobotArm->self, 5.0s cast, single-target
    UnkShortRangeMissile = 18452, // Helper->self, no cast, single-target
}

public enum IconID : uint
{
    RotateCCW = 168, // RobotArm->self
    RotateCW = 167, // RobotArm->self
    ShortRangeMissile = 196, // player->self
}

public enum TetherID : uint
{
    BossToWall = 99, // TetherHelper->TetherHelper
    ConvenientSelfDestruction = 84, // SmallExploder->player
}

class LaserResistanceTest(BossModule module) : Components.RaidwideCast(module, AID.LaserResistanceTest);
class ShockingDischarge(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.ShockingDischarge, m => m.Enemies(0x1EAEE6).Where(e => e.EventState != 7), 1.03f);
class VariableCombatTestCone(BossModule module) : Components.GroupedAOEs(module, [AID.VariableCombatConeSlow, AID.VariableCombatConeFast], new AOEShapeCone(20, 30.Degrees()));
class VariableCombatTestCircle(BossModule module) : Components.GroupedAOEs(module, [AID.VariableCombatCircleSlow, AID.VariableCombatCircleFast], new AOEShapeCircle(2));
class VariableCombatTestDonut(BossModule module) : Components.GroupedAOEs(module, [AID.VariableCombatDonutSlow, AID.VariableCombatDonutFast], new AOEShapeDonut(7, 19));
class RingLaser(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeDonut(15, 20), new AOEShapeDonut(10, 15), new AOEShapeDonut(5, 10)])
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.RingLaser1)
            AddSequence(caster.Position, Module.CastFinishAt(spell));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        var ix = (AID)spell.Action.ID switch
        {
            AID.RingLaser1 => 0,
            AID.RingLaser2 => 1,
            AID.RingLaser3 => 2,
            _ => -1
        };
        AdvanceSequence(ix, caster.Position, WorldState.FutureTime(2.1f));
    }
}

class LaserSight(BossModule module) : Components.GenericWildCharge(module, 4, fixedLength: 60)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.LaserSightTarget && Raid.TryFindSlot(spell.MainTargetID, out var slot))
        {
            Source = Module.PrimaryActor;
            PlayerRoles[slot] = PlayerRole.Target;
            for (var i = 0; i < PlayerRoles.Length; i++)
                if (PlayerRoles[i] == PlayerRole.Ignore)
                    PlayerRoles[i] = PlayerRole.Share;
        }

        if ((AID)spell.Action.ID == AID.LaserSightStack)
        {
            NumCasts++;
            Array.Fill(PlayerRoles, PlayerRole.Ignore);
        }
    }
}
class ShortRangeMissile(BossModule module) : Components.SpreadFromCastTargets(module, AID.ShortRangeMissile, 8)
{
    private readonly UnwillingCargo? _cargo = module.FindComponent<UnwillingCargo>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_cargo?.IsAffected(actor) == true)
            return;

        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_cargo?.IsAffected(actor) == true)
            return;

        base.AddHints(slot, actor, hints);
    }
}

class A12HobbesStates : StateMachineBuilder
{
    public A12HobbesStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<LaserResistanceTest>()
            .ActivateOnEnter<ShockingDischarge>()
            .ActivateOnEnter<VariableCombatTestCone>()
            .ActivateOnEnter<VariableCombatTestCircle>()
            .ActivateOnEnter<VariableCombatTestDonut>()
            .ActivateOnEnter<Impact>()
            .ActivateOnEnter<ConvenientSelfDestruction>()
            .ActivateOnEnter<Towerfall>()
            .ActivateOnEnter<FireResistanceTest>()
            .ActivateOnEnter<RingLaser>()
            .ActivateOnEnter<LaserSight>()
            .ActivateOnEnter<UnwillingCargo>()
            .ActivateOnEnter<UnwillingCargoRing>()
            .ActivateOnEnter<ShortRangeMissile>()
            .ActivateOnEnter<OilWell>()
            .ActivateOnEnter<ElectromagneticPulse>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9143)]
public class A12Hobbes(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, CircleBounds)
{
    public static readonly WPos ArenaCenter = new(-805, -240);
    public static readonly WDir[] PlatformOffsets = [new(-26, 15), new(0, -30), new(26, 15)];
    public static readonly WPos[] PlatformCenters = [.. PlatformOffsets.Select(d => ArenaCenter + d)];
    public static readonly ArenaBoundsCustom CircleBounds = MakeBounds();

    private static ArenaBoundsCustom MakeBounds() => new(50, new(PlatformOffsets.Select(p => new RelPolygonWithHoles([.. CurveApprox.Circle(20, 1 / 20f).Select(c => c + p)])).ToList()));
}

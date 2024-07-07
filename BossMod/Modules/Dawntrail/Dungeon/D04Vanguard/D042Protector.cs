namespace BossMod.Dawntrail.Dungeon.D04Vanguard.D042Protector;

public enum OID : uint
{
    Boss = 0x4237, // R5.830, x1
    Helper = 0x233C, // R0.500, x7, 523 type
    LaserTurret = 0x4238, // R0.960, x16
    FulminousFence = 0x4255, // R1.000, x4
    ExplosiveTurret = 0x4239, // R0.960, x8
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 878, // Boss->player, no cast, single-target

    Electrowave = 37161, // Boss->self, 5.0s cast, range 50 circle // Raidwide

    SearchAndDestroy = 37154, // Boss->self, 3.0s cast, single-target // Spawns Adds and starts victim lazers
    BlastCannon = 37151, // LaserTurret->self, 3.0s cast, range 26 width 4 rect
    Shock = 37156, // ExplosiveTurret->location, 2.5s cast, range 3 circle // Baided circle aoe
    HomingCannon = 37155, // LaserTurret->self, 2.5s cast, range 50 width 2 rect // Line aoes

    FulminousFence = 37149, // Boss->self, 3.0s cast, single-target // Spawns Fulminous Fence objects
    ElectrostaticContact = 37158, // FulminousFence->player, no cast, single-target

    BatteryCircuit1 = 37159, // Boss->self, 5.0s cast, single-target
    BatteryCircuit2 = 37344, // Helper->self, no cast, range 30 30.000-degree cone
    BatteryCircuit3 = 37351, // Helper->self, 5.0s cast, range 30 30.000-degree cone

    RapidThunder = 37162, // Boss->player, 5.0s cast, single-target
    MotionSensor = 37150, // Boss->self, 3.0s cast, single-target

    Bombardment = 39016, // Helper->location, 3.0s cast, range 5 circle

    Electrowhirl1 = 37160, // Helper->self, 3.0s cast, range 6 circle
    Electrowhirl2 = 37350, // Helper->self, 5.0s cast, range 6 circle

    TrackingBolt1 = 37348, // Boss->self, 8.0s cast, single-target
    TrackingBolt2 = 37349, // Helper->player, 8.0s cast, range 8 circle // Spread marker

    UnknownWeaponskill1 = 37153, // Boss->self, no cast, single-target
    UnknownWeaponskill2 = 37343, // Helper->player, no cast, single-target

    UnknownWeaponskill3 = 37347, // Helper->player, no cast, single-target
    HeavyBlastCannon = 37345, // Boss->self/players, 8.0s cast, range 36 width 8 rect // party stack line aoe
}

public enum SID : uint
{
    UnknownStatus = 2056, // Boss->Boss, extra=0x2CE
    Paralysis = 3463, // FulminousFence->player, extra=0x0
    AccelerationBomb = 3802, // Helper->player, extra=0x0
}

public enum IconID : uint
{
    Icon167 = 167, // Boss
    Icon218 = 218, // player
    Icon267 = 267, // player
    Icon196 = 196, // player
}

class Electrowave(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Electrowave));
class BlastCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlastCannon), new AOEShapeRect(26, 2));
class Shock(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Shock), 3);
class HomingCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HomingCannon), new AOEShapeRect(50, 1));
class BatteryCircuit3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BatteryCircuit3), new AOEShapeCone(30, 15.Degrees()));
class Bombardment(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Bombardment), 5);
class Electrowhirl1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Electrowhirl1), new AOEShapeCircle(6));
class Electrowhirl2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Electrowhirl2), new AOEShapeCircle(6));
class TrackingBolt2(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.TrackingBolt2), 8);

class D042ProtectorStates : StateMachineBuilder
{
    public D042ProtectorStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<BlastCannon>()
            .ActivateOnEnter<Shock>()
            .ActivateOnEnter<HomingCannon>()
            .ActivateOnEnter<BatteryCircuit3>()
            .ActivateOnEnter<Bombardment>()
            .ActivateOnEnter<Electrowhirl1>()
            .ActivateOnEnter<Electrowhirl2>()
            .ActivateOnEnter<TrackingBolt2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 831, NameID = 12757)]
public class D042Protector(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -100), new ArenaBoundsRect(12, 20));

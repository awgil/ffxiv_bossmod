namespace BossMod.Endwalker.DeepDungeon.EurekaOrthos.DD99Excalibur;

public enum OID : uint
{
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x1, EventObj type
    Boss = 0x3CFC, // R5.100, x1
    Caliburnus = 0x3CFD, // R1.000, x15
    Helper = 0x233C, // R0.500, x34, Helper type
}

public enum AID : uint
{
    AutoAttack = 31355, // Boss->player, no cast, single-target
    BossTeleport = 31326, // Boss->location, no cast, single-target // Teleporting to a location in the arena

    EmptySoulsCaliber = 31328, // Boss->self, 6.0s cast, range 5-40 donut
    SolidSoulsCaliber = 31327, // Boss->self, 6.0s cast, range 10 circle
    // these two attacks are shared with either Vacuum Slash or AbyssalSlash 1-3
    VacuumSlash = 31342, // Helper->self, 7.0s cast, range 80 45?-degree cone // visually looks like this, need to see if I can spawn the effect to really check
    AbyssalSlash1 = 31339, // Helper->self, 7.0s cast, range 2-7 donut
    AbyssalSlash2 = 31340, // Helper->self, 7.0s cast, range 7-12 donut
    AbyssalSlash3 = 31341, // Helper->self, 7.0s cast, range 17-22 donut

    Steelstrike1 = 31334, // Caliburnus->location, 2.0s cast, width 4 rect charge // cast with the first set

    Flameforge = 31329, // Boss->self, 3.0s cast, single-target // Need to have the ice debuff after the next attack
    Frostforge = 31330, // Boss->self, 3.0s cast, single-target // Need to have the fire debuff after the next attack
    FlamesRevelation = 31331, // Helper->self, no cast, range 60 circle // Need to have the ice debuff for this, or you die
    FrostsRevelation = 31332, // Helper->self, no cast, range 60 circle // need to have the fire debuff for this, or you die

    SteelFlame = 31336, // Caliburnus->location, 2.0s cast, width 4 rect charge
    SteelFrost = 31337, // Caliburnus->location, 2.0s cast, width 4 rect charge
    Steelstrike2 = 31335, // Caliburnus->location, 2.0s cast, width 4 rect charge // cast with the flame/frost steel

    ExflammeusSetup = 31343, // Boss->self, 4.0s cast, single-target // setup for the fire/flame aoe's
    ExflammeusAOE = 31344, // Helper->self, 5.0s cast, range 8 circle
    Exglacialis = 31345, // Boss->self, 4.0s cast, single-target // Setup cast for the ice aoe's
    IceShoot = 31346, // Helper->self, 5.0s cast, range 6 circle
    IceBloomCircle = 31347, // Helper->self, 4.0s cast, range 6 circle
    IceBloomRect = 31348, // Helper->self, no cast, range 40 width 5 cross

    Caliburni = 31333, // Boss->self, 5.0+0.5s cast, single-target // Swords being cast to head outside the arena here, will return back later // visual attack
    CallDimensionBlade = 31338, // Boss->self, 4.0s cast, single-target // Setup cast, does nothing
    ParadoxumCast = 31353, // Boss->self, 5.0s cast, single-target // Visual side of the cast
    ParadoxumHelper = 31354, // Helper->self, no cast, range 100 circle // Assigns Players/Caliburnus elements
    ThermalDivideILFR = 31349, // Boss->self, 5.0+2.0s cast, single-target // (Looking at boss) Ice left, Fire Right (Need to check)
    ThermalDivideFLIR = 31350, // Boss->self, 5.0+2.0s cast, single-target // Fire Left, Ice Right (Need to check)
    ThermalDivideA = 31351, // Helper->self, no cast, range 40 width 40 rect
    ThermalDivideB = 32701, // Helper->self, 6.7s cast, range 40 width 8 rect
}

class AbyssalSlash1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbyssalSlash1), new AOEShapeDonutSector(2, 7, 90.Degrees()));
class AbyssalSlash2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbyssalSlash2), new AOEShapeDonutSector(7, 12, 90.Degrees()));
class AbyssalSlash3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbyssalSlash3), new AOEShapeDonutSector(17, 22, 90.Degrees()));
class VacuumSlash(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VacuumSlash), new AOEShapeCone(80, 22.5f.Degrees()));
class ThermalDivide(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThermalDivideB), new AOEShapeRect(40, 4));
class Exflammeus(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExflammeusAOE), new AOEShapeCircle(8));
class IceShoot(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IceShoot), new AOEShapeCircle(6));
class IceBloomCircle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IceBloomCircle), new AOEShapeCircle(6));
class EmptySoulsCaliber(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.EmptySoulsCaliber), new AOEShapeDonut(5, 40));
class SolidSoulsCaliber(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SolidSoulsCaliber), new AOEShapeCircle(10));

class DD99ExcaliburStates : StateMachineBuilder
{
    public DD99ExcaliburStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 906, NameID = 12100)]
public class DD99Excalibur(WorldState ws, Actor primary) : BossModule(ws, primary, new(-600, -300), new ArenaBoundsCircle(20));

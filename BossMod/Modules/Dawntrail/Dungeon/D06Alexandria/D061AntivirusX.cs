namespace BossMod.Dawntrail.Dungeon.D06Alexandria.D061AntivirusX;

public enum OID : uint
{
    Boss = 0x4173, // R8.000, x1
    Helper = 0x233C, // R0.500, x20, 523 type
    ElectricCharge = 0x18D6, // R0.500, x6
    InterferonC = 0x4175, // R1.000, x0 (spawn during fight)
    InterferonR = 0x4174, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 36388, // Boss->player, no cast, single-target

    ImmuneResponse1 = 36378, // Boss->self, 5.0s cast, single-target
    ImmuneResponse2 = 36379, // Helper->self, 6.0s cast, range 40 ?-degree cone // Frontal AOE cone
    ImmuneResponse3 = 36380, // Boss->self, 5.0s cast, single-target
    ImmuneResponse4 = 36381, // Helper->self, 6.0s cast, range 40 ?-degree cone // Side and back AOE cone

    PathocrossPurge = 36383, // InterferonC->self, 1.0s cast, range 40 width 6 cross
    PathocircuitPurge = 36382, // InterferonR->self, 1.0s cast, range ?-40 donut

    Quarantine1 = 36384, // Boss->self, 3.0s cast, single-target
    Quarantine2 = 36386, // Helper->players, no cast, range 6 circle

    Disinfection = 36385, // Helper->player, no cast, range 6 circle
    Cytolysis = 36387, // Boss->self, 5.0s cast, range 40 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Helper/InterferonC/InterferonR->player, extra=0x1/0x2/0x3
    Electrocution1 = 3073, // none->player, extra=0x0
    Electrocution2 = 3074, // none->player, extra=0x0
}

public enum IconID : uint
{
    Spreadmarker = 344, // player
    Stackmarker = 62, // player
}
class ImmuneResponse2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ImmuneResponse2), new AOEShapeCone(40, 50.Degrees()));
class ImmuneResponse4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ImmuneResponse4), new AOEShapeCone(40, 135.Degrees()));
class PathocrossPurge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PathocrossPurge), new AOEShapeCross(40, 3));
class PathocircuitPurge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PathocircuitPurge), new AOEShapeDonut(6, 40));

class Quarantine2(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Stackmarker, ActionID.MakeSpell(AID.Quarantine2), 6, 5, 4);
class Disinfection(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Spreadmarker, ActionID.MakeSpell(AID.Disinfection), 5, 6);

class Cytolysis(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Cytolysis));

class D061AntivirusXStates : StateMachineBuilder
{
    public D061AntivirusXStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ImmuneResponse2>()
            .ActivateOnEnter<ImmuneResponse4>()
            .ActivateOnEnter<PathocrossPurge>()
            .ActivateOnEnter<PathocircuitPurge>()
            .ActivateOnEnter<Quarantine2>()
            .ActivateOnEnter<Disinfection>()
            .ActivateOnEnter<Cytolysis>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 827, NameID = 12844)]
public class D061AntivirusX(WorldState ws, Actor primary) : BossModule(ws, primary, new(852, 819.5f), new ArenaBoundsSquare(16));

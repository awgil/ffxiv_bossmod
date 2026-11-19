namespace BossMod.Endwalker.Dungeon.D07Smileton.D072Frameworker;

public enum OID : uint
{
    Boss = 0x34D1, // R4.000, x1
    SmileySupporter = 0x34D2, // R1.000, x4
    PrintedWorker = 0x34E9, // R1.000, x2
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast, single-target
    CircularSaw = 26437, // Boss->self, 5.0s cast, range 40 circle, raidwide
    LeapForwardBoss = 26438, // Boss->location, 7.0s cast, range 15 circle
    LeapForwardVisual = 26442, // Helper->self, no cast, single-target
    LeapForwardWorker = 26439, // PrintedWorker->location, 7.0s cast, range 15 circle
    PrintWorkers = 28092, // Boss->self, 3.0s cast, single-target, visual (tethers supporters)
    PrintWorkersVisual = 26443, // SmileySupporter->self, no cast, single-target, spawns printed workers
    OmnidimensionalOnslaught = 26440, // Boss->self, 5.0s cast, single-target, visual
    SteelBeam = 26436, // Boss->player, 5.0s cast, single-target, tankbuster
    OmnidimensionalOnslaughtAOE = 26441, // Helper->self, 5.0s cast, range 40 45-degree cone, cardinals then intercardinals 2s later
}

class CircularSaw(BossModule module) : Components.RaidwideCast(module, AID.CircularSaw);
class SteelBeam(BossModule module) : Components.SingleTargetCast(module, AID.SteelBeam);
class LeapForward(BossModule module) : Components.GroupedAOEs(module, [AID.LeapForwardBoss, AID.LeapForwardWorker], new AOEShapeCircle(15));
class OmnidimensionalOnslaught(BossModule module) : Components.StandardAOEs(module, AID.OmnidimensionalOnslaughtAOE, new AOEShapeCone(40, 22.5f.Degrees()), 4);

class D072FrameworkerStates : StateMachineBuilder
{
    public D072FrameworkerStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<CircularSaw>()
            .ActivateOnEnter<SteelBeam>()
            .ActivateOnEnter<LeapForward>()
            .ActivateOnEnter<OmnidimensionalOnslaught>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 794, NameID = 10333)]
public class D072Frameworker(ModuleInit init) : BossModule(init, new(64, -112), new ArenaBoundsCircle(20));

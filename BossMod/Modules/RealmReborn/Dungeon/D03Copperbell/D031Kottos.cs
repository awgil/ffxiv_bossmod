namespace BossMod.RealmReborn.Dungeon.D03Copperbell.D031Kottos;

public enum OID : uint
{
    Boss = 0x387C,
    Helper = 0x233C, // x3
}

public enum AID : uint
{
    AutoAttack = 870, // Boss->player, no cast
    GrandSlam = 28545, // Boss->player, 5.0s cast, tankbuster
    LumberingLeapJumpFirst = 28543, // Boss->location, 8.0s cast, visual & teleport
    LumberingLeapAOE = 28544, // Helper->self, 9.0s cast, range 12 aoe
    LumberingLeapJumpRest = 28549, // Boss->location, no cast, teleport
    ColossalSlam = 28546, // Boss->self, 4.0s cast, range 30 60-degree cone aoe
    Catapult = 28547, // Boss->player, 5.0s cast, single target damage at random target
}

class GrandSlam(BossModule module) : Components.SingleTargetCast(module, AID.GrandSlam);
class LumberingLeap(BossModule module) : Components.StandardAOEs(module, AID.LumberingLeapAOE, new AOEShapeCircle(12));
class ColossalSlam(BossModule module) : Components.StandardAOEs(module, AID.ColossalSlam, new AOEShapeCone(30, 30.Degrees()));
class Catapult(BossModule module) : Components.SingleTargetCast(module, AID.Catapult, "Single-target damage");

class D031KottosStates : StateMachineBuilder
{
    public D031KottosStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GrandSlam>()
            .ActivateOnEnter<LumberingLeap>()
            .ActivateOnEnter<ColossalSlam>()
            .ActivateOnEnter<Catapult>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 3, NameID = 548)]
public class D031Kottos(WorldState ws, Actor primary) : BossModule(ws, primary, new(43, -89.56f), new ArenaBoundsCircle(15));

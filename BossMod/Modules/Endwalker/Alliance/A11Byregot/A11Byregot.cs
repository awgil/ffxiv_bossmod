namespace BossMod.Endwalker.Alliance.A11Byregot;

class ByregotWard() : Components.BaitAwayCast(ActionID.MakeSpell(AID.ByregotWard), new AOEShapeCone(10, 45.Degrees()));


[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866, NameID = 11281, SortOrder = 1)]
public class A11Byregot(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsSquare(new(0, 700), 25));
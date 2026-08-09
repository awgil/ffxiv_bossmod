namespace BossMod.Endwalker.Alliance.A11Byregot;

sealed class ByregotWard(BossModule module) : Components.BaitAwayCast(module, (uint)AID.ByregotWard, new AOEShapeCone(10f, 45f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 866u, NameID = 11281u, SortOrder = 1, PlanLevel = 90)]
public class A11Byregot(WorldState ws, Actor primary) : BossModule(ws, primary, new(0f, 700f), new ArenaBoundsSquare(24.5f));

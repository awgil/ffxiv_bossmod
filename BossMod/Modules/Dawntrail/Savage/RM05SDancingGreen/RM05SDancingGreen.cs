namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1020, NameID = 13778, PlanLevel = 100, Contributors = "xan")]
public class DancingGreen(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));

namespace BossMod.Endwalker.Savage.P11SThemis;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 941, NameID = 12388, PlanLevel = 90)]
public class P11SThemis(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

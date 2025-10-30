namespace BossMod.Dawntrail.Extreme.Ex5Necron;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1062, NameID = 14093, PlanLevel = 100)]
public class Ex5Necron(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(18, 15));

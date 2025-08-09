namespace BossMod.Dawntrail.Extreme.Ex5Necron;

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1062, NameID = 14093, DevOnly = true)]
public class Ex5Necron(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));


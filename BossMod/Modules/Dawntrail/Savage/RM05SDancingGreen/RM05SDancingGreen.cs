namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

#if DEBUG
[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1020, NameID = 13778)]
public class DancingGreen(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));
#endif

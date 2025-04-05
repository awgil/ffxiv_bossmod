namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1022, NameID = 13822)]
public class SugarRiot(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));


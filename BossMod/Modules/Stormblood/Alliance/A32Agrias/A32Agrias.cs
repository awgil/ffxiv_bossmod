namespace BossMod.Stormblood.Alliance.A32Agrias;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7916)]
public class A32Agrias(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, -54), new ArenaBoundsCircle(30));

namespace BossMod.Stormblood.Alliance.A13Rofocale;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 281, NameID = 6941)]
public class A13Rofocale(WorldState ws, Actor primary) : BossModule(ws, primary, new(106, -190), new ArenaBoundsCircle(30));


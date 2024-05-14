namespace BossMod.Shadowbringers.Alliance.A21AegisUnit;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9642)]
public class A21AegisUnit(WorldState ws, Actor primary) : BossModule(ws, primary, new(-230, 190), new ArenaBoundsCircle(30));

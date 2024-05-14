namespace BossMod.Shadowbringers.Alliance.A23ArtilleryUnit;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9650)]
public class A23ArtilleryUnit(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -115), new ArenaBoundsCircle(30));

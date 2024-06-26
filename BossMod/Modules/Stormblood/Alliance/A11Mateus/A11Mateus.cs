namespace BossMod.Stormblood.Alliance.A11Mateus;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 281, NameID = 6929)]
public class A11Mateus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-320, 240), new ArenaBoundsCircle(30));

namespace BossMod.Endwalker.Savage.P11SThemis;

[ConfigDisplay(Order = 0x1B0, Parent = typeof(EndwalkerConfig))]
public class P11SThemisConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 941, NameID = 12388)]
public class P11SThemis(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

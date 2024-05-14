namespace BossMod.Shadowbringers.Alliance.A11CommandModel;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9141)] //Other service models 9142 and 9155, nonservice model 9923
public class A11CommandModel(WorldState ws, Actor primary) : BossModule(ws, primary, new(-500, -10), new ArenaBoundsCircle(20));

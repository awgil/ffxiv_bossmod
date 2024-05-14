namespace BossMod.Stormblood.Alliance.A21Famfrit;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 550, NameID = 7245)]
public class A21Famfrit(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200, 66), new ArenaBoundsCircle(35));

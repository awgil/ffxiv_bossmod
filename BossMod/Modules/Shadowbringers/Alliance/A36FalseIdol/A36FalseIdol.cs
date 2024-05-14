namespace BossMod.Shadowbringers.Alliance.A36FalseIdol;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9948)]
public class A36FalseIdol(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, -725), new ArenaBoundsCircle(30));

namespace BossMod.Shadowbringers.Alliance.A25Compound2P;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9644)]
public class A25Compound2P(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -700), new ArenaBoundsCircle(20));

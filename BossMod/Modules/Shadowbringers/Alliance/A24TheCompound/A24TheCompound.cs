namespace BossMod.Shadowbringers.Alliance.A24TheCompound;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9646)]
public class A24TheCompound(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -700), new ArenaBoundsCircle(30));

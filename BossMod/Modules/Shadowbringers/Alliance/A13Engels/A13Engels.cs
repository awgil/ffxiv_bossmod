namespace BossMod.Shadowbringers.Alliance.A13Engels;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 700, NameID = 9147)]
public class A13MarxEngels(WorldState ws, Actor primary) : BossModule(ws, primary, new(900, 670), new ArenaBoundsCircle(30));

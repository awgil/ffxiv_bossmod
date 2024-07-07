namespace BossMod.Shadowbringers.Alliance.A37HerInfloresence;

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 779, NameID = 9949)]
public class A37HerInfloresence(WorldState ws, Actor primary) : BossModule(ws, primary, new(-700, -725), new ArenaBoundsCircle(30));

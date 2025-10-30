namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058, NameID = 14043)]
public class A23Kamlanaut(WorldState ws, Actor primary) : BossModule(ws, primary, new(-200, 150), new ArenaBoundsCircle(29.5f));


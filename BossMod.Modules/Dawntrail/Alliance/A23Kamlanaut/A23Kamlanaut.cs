namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058, NameID = 14043)]
public class A23Kamlanaut(ModuleInit init) : BossModule(init, new(-200, 150), new ArenaBoundsCircle(29.5f));


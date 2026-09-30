namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058, NameID = 14086)]
public class A24Ealdnarche(ModuleInit init) : BossModule(init, new(800, -800), new ArenaBoundsSquare(24));

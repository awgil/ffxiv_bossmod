namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058, NameID = 14086)]
public class A24Ealdnarche(ModuleArgs init) : BossModule(init, new(800, -800), new ArenaBoundsSquare(24));

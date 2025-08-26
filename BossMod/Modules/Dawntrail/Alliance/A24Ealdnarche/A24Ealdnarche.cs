namespace BossMod.Dawntrail.Alliance.A24Ealdnarche;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058, NameID = 14086)]
public class A24Ealdnarche(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, -800), new ArenaBoundsSquare(24));

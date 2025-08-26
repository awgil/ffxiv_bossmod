namespace BossMod.Dawntrail.Alliance.A21FaithboundKirin;

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1058, NameID = 14053)]
public class A21FaithboundKirin(WorldState ws, Actor primary) : BossModule(ws, primary, new(-850, 780), new ArenaBoundsCircle(29.5f));

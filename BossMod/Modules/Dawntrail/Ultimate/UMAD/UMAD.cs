namespace BossMod.Dawntrail.Ultimate.UMAD;

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1094, NameID = 7131, PlanLevel = 100)]
public class UMAD(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public override bool ShouldPrioritizeAllEnemies => true;
}

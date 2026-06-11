namespace BossMod.Dawntrail.Ultimate.UMAD;

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1094, NameID = 7131, PlanLevel = 100)]
public class UMAD(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public override bool ShouldPrioritizeAllEnemies => true;

    Actor? _bossP2;

    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        _bossP2 ??= Enemies(OID.BossP2).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
    }
}

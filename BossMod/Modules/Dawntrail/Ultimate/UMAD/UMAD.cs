namespace BossMod.Dawntrail.Ultimate.UMAD;

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1094, NameID = 7131, PlanLevel = 100)]
public class UMAD(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public override bool ShouldPrioritizeAllEnemies => true;

    Actor? _bossP2;
    Actor? _chaosP3;
    Actor? _exdeathP3;
    Actor? _kefkaP3;

    public Actor? BossP2() => _bossP2;
    public Actor? ChaosP3() => _chaosP3;
    public Actor? ExdeathP3() => _exdeathP3;
    public Actor? KefkaP3() => _kefkaP3;

    protected override void UpdateModule()
    {
        _bossP2 ??= Enemies(OID.BossP2).FirstOrDefault();
        _chaosP3 ??= Enemies(OID.ChaosP3).FirstOrDefault();
        _exdeathP3 ??= Enemies(OID.ExdeathP3).FirstOrDefault();

        if (StateMachine.ActivePhaseIndex == 2)
            // same OID, different actor :)
            _kefkaP3 ??= Enemies(OID.BossP1).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
        Arena.Actor(_chaosP3, ArenaColor.Enemy);
        Arena.Actor(_exdeathP3, ArenaColor.Enemy);
        Arena.Actor(_kefkaP3, ArenaColor.Enemy);
    }
}

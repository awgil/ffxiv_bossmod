namespace BossMod.Dawntrail.Ultimate.UMAD;

[ModuleInfo(Incomplete = true, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1094, NameID = 7131, PlanLevel = 100)]
public class UMAD(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public override bool ShouldPrioritizeAllEnemies => true;

    Actor? _bossP2;
    Actor? _chaosP3;
    Actor? _exdeathP3;
    Actor? _kefkaP3;
    Actor? _kefkaP4;
    Actor? _neoExdeathP4;
    Actor? _chaosP4;
    Actor? _kefkaP5;

    public Actor? BossP2() => _bossP2;
    public Actor? ChaosP3() => _chaosP3;
    public Actor? ExdeathP3() => _exdeathP3;
    public Actor? KefkaP3() => _kefkaP3;
    public Actor? NeoExdeathP4() => _neoExdeathP4;
    public Actor? ChaosP4() => _chaosP4;
    public Actor? KefkaP4() => _kefkaP4;
    public Actor? KefkaP5() => _kefkaP5;

    protected override void UpdateModule()
    {
        _bossP2 ??= Enemies(OID.BossP2).FirstOrDefault();
        _chaosP3 ??= Enemies(OID.ChaosP3).FirstOrDefault();
        _exdeathP3 ??= Enemies(OID.ExdeathP3).FirstOrDefault();

        if (StateMachine.ActivePhaseIndex == 2)
            // same OID, different actor :)
            _kefkaP3 ??= Enemies(OID.BossP1).FirstOrDefault();

        if (StateMachine.ActivePhaseIndex == 3)
        {
            _kefkaP4 ??= Enemies(OID.KefkaP4).FirstOrDefault();
            _neoExdeathP4 ??= Enemies(OID.NeoExdeathP4).FirstOrDefault();
            _chaosP4 ??= Enemies(OID.ChaosP4).FirstOrDefault();
        }

        if (StateMachine.ActivePhaseIndex == 4)
            _kefkaP5 ??= Enemies(OID.KefkaP5).FirstOrDefault();
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
        Arena.Actor(_chaosP3, ArenaColor.Enemy);
        Arena.Actor(_exdeathP3, ArenaColor.Enemy);
        Arena.Actor(_kefkaP4, ArenaColor.Enemy);
        Arena.Actor(_kefkaP5, ArenaColor.Enemy);
    }
}

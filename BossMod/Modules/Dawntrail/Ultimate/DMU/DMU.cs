namespace BossMod.Dawntrail.Ultimate.DMU;

sealed class LightOfJudgment(BossModule module) : Components.RaidwideCast(module, (uint)AID.LightOfJudgment);

[ModuleInfo(BossModuleInfo.Maturity.WIP,
    StatesType = typeof(DMUStates),
    ConfigType = typeof(DMUConfig),
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID),
    StatusIDType = typeof(SID),
    TetherIDType = typeof(TetherID),
    IconIDType = typeof(IconID),
    PrimaryActorOID = (uint)OID.Kefka,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Ultimate,
    GroupType = BossModuleInfo.GroupType.CFC,
    GroupID = 1094u,
    NameID = 7131u,
    SortOrder = 1,
    PlanLevel = 100)]
[SkipLocalsInit]
public sealed class DMU(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(20f))
{
    public override bool ShouldPrioritizeAllEnemies => true;

    //private Actor? bossP1;
    public Actor? BossP1() => PrimaryActor;

    private Actor? bossP2;
    public Actor? BossP2() => bossP2;

    private Actor? bossP3;
    public Actor? BossP3() => bossP3;
    private Actor? chaosP3;
    public Actor? ChaosP3() => chaosP3;
    private Actor? exdeathP3;
    public Actor? ExdeathP3() => exdeathP3;

    private Actor? kefkaP4;
    public Actor? KefkaP4() => kefkaP4;
    private Actor? neoExdeath;
    public Actor? NeoExdeath() => neoExdeath;
    private Actor? chaosP4;
    public Actor? ChaosP4() => chaosP4;

    private Actor? kefkaP5;
    public Actor? KefkaP5() => kefkaP5;

    protected override void UpdateModule()
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case 1:
                bossP2 ??= GetActor((uint)OID.BossP2);
                break;
            case 2:
                bossP3 ??= GetActor((uint)OID.Kefka);
                chaosP3 ??= GetActor((uint)OID.Chaos);
                exdeathP3 ??= GetActor((uint)OID.Exdeath);
                break;
            case 3:
                kefkaP4 ??= GetActor((uint)OID.KefkaP4);
                chaosP4 ??= GetActor((uint)OID.ChaosP4);
                neoExdeath ??= GetActor((uint)OID.NeoExdeath);
                break;
            case 4:
                kefkaP5 ??= GetActor((uint)OID.KefkaP5);
                break;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case -1:
            case 0:
                Arena.Actor(PrimaryActor);
                break;
            case 1:
                Arena.Actor(bossP2);
                break;
            case 2:
                Arena.Actor(chaosP3);
                Arena.Actor(exdeathP3);
                Arena.Actor(bossP3);
                break;
            case 3:
                Arena.Actor(kefkaP4);
                break;
            case 4:
                Arena.Actor(kefkaP5);
                break;
        }
    }
}

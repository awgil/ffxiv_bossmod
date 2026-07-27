namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P2QuadrupleSlap(BossModule module) : Components.TankSwap(module, (uint)AID.QuadrupleSlapFirst, (uint)AID.QuadrupleSlapFirst, (uint)AID.QuadrupleSlapSecond, default, 4.1d);
sealed class P3Junction(BossModule module) : Components.CastCounter(module, (uint)AID.Junction);
sealed class P3BlackHalo(BossModule module) : Components.CastSharedTankbuster(module, (uint)AID.BlackHalo, new AOEShapeCone(60f, 45f.Degrees())); // TODO: verify angle

sealed class P4HallowedWings(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.HallowedWingsL, (uint)AID.HallowedWingsR], new AOEShapeRect(80f, 20f));

sealed class P5ParadiseLost(BossModule module) : Components.CastCounter(module, (uint)AID.ParadiseLostP5AOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1006, NameID = 9707, PlanLevel = 100)]
public sealed class FRU : BossModule
{
    public FRU(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private FRU(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom([new Polygon(new(100f, 100f), 20f, 64)]) { IsCircle = true };
        return (arena.Center, arena);
    }

    public static readonly ArenaBoundsSquare PathfindHugBorderBounds = new(20f); // this is a hack to allow precise positioning near border by some mechanics, TODO reconsider

    public override bool ShouldPrioritizeAllEnemies => true;

    private Actor? _bossP2;
    private Actor? _iceVeil;
    private Actor? _bossP3;
    private Actor? _bossP4Usurper;
    private Actor? _bossP4Oracle;
    private Actor? _bossP5;

    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;
    public Actor? IceVeil() => _iceVeil;
    public Actor? BossP3() => _bossP3;
    public Actor? BossP4Usurper() => _bossP4Usurper;
    public Actor? BossP4Oracle() => _bossP4Oracle;
    public Actor? BossP5() => _bossP5;

    protected override void UpdateModule()
    {
        switch (StateMachine.ActivePhaseIndex)
        {
            case 1:
                _bossP2 ??= GetActor((uint)OID.BossP2);
                _iceVeil ??= GetActor((uint)OID.IceVeil);
                break;
            case 2:
                _bossP3 ??= GetActor((uint)OID.BossP3);
                _bossP4Usurper ??= GetActor((uint)OID.UsurperOfFrostP4);
                _bossP4Oracle ??= GetActor((uint)OID.OracleOfDarknessP4);
                break;
            case 3:
                _bossP5 ??= GetActor((uint)OID.BossP5);
                break;
        }
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actor(_bossP2);
        Arena.Actor(_bossP3);
        Arena.Actor(_bossP4Usurper);
        Arena.Actor(_bossP4Oracle);
        Arena.Actor(_bossP5);
    }
}

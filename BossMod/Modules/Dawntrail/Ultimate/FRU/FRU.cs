namespace BossMod.Dawntrail.Ultimate.FRU;

class P2QuadrupleSlap(BossModule module) : Components.TankSwap(module, AID.QuadrupleSlapFirst, AID.QuadrupleSlapFirst, AID.QuadrupleSlapSecond, 4.1f, null, true);
class P3Junction(BossModule module) : Components.CastCounter(module, AID.Junction);
class P3BlackHalo(BossModule module) : Components.CastSharedTankbuster(module, AID.BlackHalo, new AOEShapeCone(60, 45.Degrees())); // TODO: verify angle
class P4HallowedWingsL(BossModule module) : Components.StandardAOEs(module, AID.HallowedWingsL, new AOEShapeRect(80, 20, 0, 90.Degrees()));
class P4HallowedWingsR(BossModule module) : Components.StandardAOEs(module, AID.HallowedWingsR, new AOEShapeRect(80, 20, 0, -90.Degrees()));
class P5ParadiseLost(BossModule module) : Components.CastCounter(module, AID.ParadiseLostP5AOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1006, NameID = 9707, PlanLevel = 100)]
public class FRU(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    public static readonly ArenaBoundsSquare PathfindHugBorderBounds = new(20); // this is a hack to allow precise positioning near border by some mechanics, TODO reconsider

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
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.BossP2).FirstOrDefault() : null;
        _iceVeil ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.IceVeil).FirstOrDefault() : null;
        _bossP3 ??= StateMachine.ActivePhaseIndex == 2 ? Enemies(OID.BossP3).FirstOrDefault() : null;
        _bossP4Usurper ??= StateMachine.ActivePhaseIndex == 2 ? Enemies(OID.UsurperOfFrostP4).FirstOrDefault() : null;
        _bossP4Oracle ??= StateMachine.ActivePhaseIndex == 2 ? Enemies(OID.OracleOfDarknessP4).FirstOrDefault() : null;
        _bossP5 ??= StateMachine.ActivePhaseIndex == 3 ? Enemies(OID.BossP5).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
        Arena.Actor(_bossP3, ArenaColor.Enemy);
        Arena.Actor(_bossP4Usurper, ArenaColor.Enemy);
        Arena.Actor(_bossP4Oracle, ArenaColor.Enemy);
        Arena.Actor(_bossP5, ArenaColor.Enemy);
    }
}

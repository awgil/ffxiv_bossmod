namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1SuddenDownpour(BossModule module) : Components.CastCounter(module, AID.SuddenDownpourAOE);
class P1TrekShotN(BossModule module) : Components.StandardAOEs(module, AID.TrekShotNAOE, new AOEShapeCone(65, 60.Degrees()));
class P1TrekShotS(BossModule module) : Components.StandardAOEs(module, AID.TrekShotSAOE, new AOEShapeCone(65, 60.Degrees()));
class P1SoaringMinuet1(BossModule module) : Components.StandardAOEs(module, AID.SoaringMinuet1, new AOEShapeCone(65, 135.Degrees()));
class P1SoaringMinuet2(BossModule module) : Components.StandardAOEs(module, AID.SoaringMinuet2, new AOEShapeCone(65, 135.Degrees()));
class P1Arrow(BossModule module) : Components.BaitAwayCast(module, AID.ArrowP1AOE, new AOEShapeCircle(6), true);
class P1Downhill(BossModule module) : Components.StandardAOEs(module, AID.DownhillP1AOE, 6);
class P2MovingMountains(BossModule module) : Components.CastCounter(module, AID.MovingMountains);
class P2PeakPeril(BossModule module) : Components.CastCounter(module, AID.PeakPeril);
class P2Shockwave(BossModule module) : Components.CastCounter(module, AID.Shockwave);
class P2PitonPull(BossModule module) : Components.StandardAOEs(module, AID.PitonPullAOE, 22);
class P2Altitude(BossModule module) : Components.StandardAOEs(module, AID.AltitudeAOE, 6);
class P2Arrow(BossModule module) : Components.BaitAwayCast(module, AID.ArrowP2AOE, new AOEShapeCircle(10), true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11300, SortOrder = 4)]
public class A33Oschon(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 750), new ArenaBoundsSquare(25))
{
    private Actor? _bossP2;

    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.BossP2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
    }
}

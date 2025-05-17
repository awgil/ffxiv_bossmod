namespace BossMod.Dawntrail.Savage.RM04SWickedThunder;

class BewitchingFlight(BossModule module) : Components.StandardAOEs(module, AID.BewitchingFlightAOE, new AOEShapeRect(40, 2.5f));
class WickedJolt(BossModule module) : Components.TankSwap(module, AID.WickedJolt, AID.WickedJolt, AID.WickedJoltSecond, 3.2f, new AOEShapeRect(60, 2.5f), false);
class Soulshock(BossModule module) : Components.CastCounter(module, AID.Soulshock);
class Impact(BossModule module) : Components.CastCounter(module, AID.Impact);
class Cannonbolt(BossModule module) : Components.CastCounter(module, AID.Cannonbolt);
class CrossTailSwitch(BossModule module) : Components.CastCounter(module, AID.CrossTailSwitchAOE);
class CrossTailSwitchLast(BossModule module) : Components.CastCounter(module, AID.CrossTailSwitchLast);
class WickedSpecialCenter(BossModule module) : Components.StandardAOEs(module, AID.WickedSpecialCenterAOE, new AOEShapeRect(40, 10));
class WickedSpecialSides(BossModule module) : Components.StandardAOEs(module, AID.WickedSpecialSidesAOE, new AOEShapeRect(40, 7.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 992, NameID = 13057, PlanLevel = 100)]
public class RM04SWickedThunder(WorldState ws, Actor primary) : BossModule(ws, primary, P1Center, P1DefaultBounds)
{
    public static readonly WPos P1Center = new(100, 100);
    public static readonly WPos P2Center = new(100, 165);
    public static readonly ArenaBoundsSquare P1DefaultBounds = new(20);
    public static readonly ArenaBoundsCustom P1IonClusterRBounds = new(20, new(CurveApprox.Rect(new WDir(+15, 0), new(0, 1), 5, 20)));
    public static readonly ArenaBoundsCustom P1IonClusterLBounds = new(20, new(CurveApprox.Rect(new WDir(-15, 0), new(0, 1), 5, 20)));
    public static readonly ArenaBoundsRect P2DefaultBounds = new(20, 15);
    public static readonly ArenaBoundsCustom P2CircleBounds = new(20, new(CurveApprox.Circle(15, 0.01f)));
    public static readonly ArenaBoundsCustom P2TowersBounds = new(20, P2DefaultBounds.Clipper.Union(new(CurveApprox.Rect(new WDir(+15, 0), new(0, 1), 5, 15)), new(CurveApprox.Rect(new WDir(-15, 0), new(0, 1), 5, 15))));

    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? BossP2() => _bossP2;

    private Actor? _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossP2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
    }
}

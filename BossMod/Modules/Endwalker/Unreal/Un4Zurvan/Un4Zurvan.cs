namespace BossMod.Endwalker.Unreal.Un4Zurvan;

class P1MetalCutter(BossModule module) : Components.Cleave(module, AID.MetalCutterP1, new AOEShapeCone(37.44f, 45.Degrees()), (uint)OID.BossP1);
class P1FlareStar(BossModule module) : Components.StandardAOEs(module, AID.FlareStarAOE, 6);
class P1Purge(BossModule module) : Components.CastCounter(module, AID.Purge);
class P2MetalCutter(BossModule module) : Components.Cleave(module, AID.MetalCutterP2, new AOEShapeCone(37.44f, 45.Degrees()), (uint)OID.BossP2);
class P2IcyVoidzone(BossModule module) : Components.PersistentVoidzone(module, 5, m => m.Enemies(OID.IcyVoidzone).Where(z => z.EventState != 7));
class P2BitingHalberd(BossModule module) : Components.StandardAOEs(module, AID.BitingHalberd, new AOEShapeCone(55.27f, 135.Degrees()));
class P2TailEnd(BossModule module) : Components.StandardAOEs(module, AID.TailEnd, new AOEShapeCircle(15));
class P2Ciclicle(BossModule module) : Components.StandardAOEs(module, AID.Ciclicle, new AOEShapeDonut(10, 20)); // TODO: verify inner radius
class P2SouthernCross(BossModule module) : Components.StandardAOEs(module, AID.SouthernCrossAOE, 6);
class P2SouthernCrossVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.SouthernCrossVoidzone).Where(z => z.EventState != 7));
class P2WaveCannon(BossModule module) : Components.BaitAwayCast(module, AID.WaveCannonSolo, new AOEShapeRect(55.27f, 5));
class P2TyrfingFire(BossModule module) : Components.Cleave(module, AID.TyrfingFire, new AOEShapeCircle(5), (uint)OID.BossP2, originAtTarget: true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 951, NameID = 5567, PlanLevel = 90)]
public class Un4Zurvan(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    private Actor? _bossP2;

    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;
    public Actor? BossP2() => _bossP2;

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

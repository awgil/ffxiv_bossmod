namespace BossMod.Dawntrail.Ultimate.FRU;

class P1BrightfireSmall(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BrightfireSmall), new AOEShapeCircle(5));
class P1BrightfireLarge(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BrightfireLarge), new AOEShapeCircle(10));
class P2QuadrupleSlap(BossModule module) : Components.TankSwap(module, ActionID.MakeSpell(AID.QuadrupleSlapFirst), ActionID.MakeSpell(AID.QuadrupleSlapFirst), ActionID.MakeSpell(AID.QuadrupleSlapSecond), 4.1f, null, true);
class P2CrystalOfLight(BossModule module) : Components.Adds(module, (uint)OID.CrystalOfLight);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1006, NameID = 9707, PlanLevel = 100)]
public class FRU(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20))
{
    private Actor? _bossP2;
    private Actor? _iceVeil;

    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;
    public Actor? IceVeil() => _iceVeil;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.BossP2).FirstOrDefault() : null;
        _iceVeil ??= StateMachine.ActivePhaseIndex == 1 ? Enemies(OID.IceVeil).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
        Arena.Actor(_iceVeil, ArenaColor.Enemy);
    }
}

namespace BossMod.Dawntrail.Alliance.A13ArkAngels;

class Cloudsplitter(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.CloudsplitterAOE), new AOEShapeCircle(6), true);
class TachiYukikaze(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TachiYukikaze), new AOEShapeRect(50, 2.5f));
class TachiGekko(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.TachiGekko));
class TachiKasha(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TachiKasha), new AOEShapeCircle(20));
class ConcertedDissolution(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ConcertedDissolution), new AOEShapeCone(40, 15.Degrees())); // TODO: verify angle
class LightsChain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightsChain), new AOEShapeDonut(3, 40)); // TODO: verify inner radius
class Meteor(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.Meteor));
class CrossReaver(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CrossReaverAOE), new AOEShapeCross(50, 6));
class ArkShield(BossModule module) : Components.Adds(module, (uint)OID.ArkShield);
class CriticalReaverRaidwide(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CriticalReaverRaidwide));
class CriticalReaverEnrage(BossModule module) : Components.CastInterruptHint(module, ActionID.MakeSpell(AID.CriticalReaverEnrage));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossGK, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13641)]
public class A13ArkAngels(WorldState ws, Actor primary) : BossModule(ws, primary, new(865, -820), new ArenaBoundsCircle(25))
{
    private Actor? _bossHM;
    private Actor? _bossEV;
    private Actor? _bossMR;
    private Actor? _bossTT;
    public Actor? BossHM() => _bossHM;
    public Actor? BossEV() => _bossEV;
    public Actor? BossMR() => _bossMR;
    public Actor? BossTT() => _bossTT;
    public Actor? BossGK() => PrimaryActor;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossHM ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossHM).FirstOrDefault() : null;
        _bossEV ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossEV).FirstOrDefault() : null;
        _bossMR ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossMR).FirstOrDefault() : null;
        _bossTT ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossTT).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossHM, ArenaColor.Enemy);
        Arena.Actor(_bossEV, ArenaColor.Enemy);
        Arena.Actor(_bossMR, ArenaColor.Enemy);
        Arena.Actor(_bossTT, ArenaColor.Enemy);
    }
}

namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class P1TripleTrial(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.TripleTrial), new AOEShapeCone(18.5f, 30.Degrees())); // TODO: verify angle
class P1Ein(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Ein), new AOEShapeRect(50, 22.5f));
class P2GenesisCochma(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.GenesisCochma));
class P2GenesisBinah(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.GenesisBinah));
class P3EinSofOhr(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.EinSofOhrAOE));
class P3Yesod(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Yesod), new AOEShapeCircle(4));
class P3PillarOfMercyAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PillarOfMercyAOE), new AOEShapeCircle(5));
class P3PillarOfMercyKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.PillarOfMercyAOE), 17);
class P3Malkuth(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Malkuth), 25);
class P3Ascension(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Ascension)); // TODO: show safe spot?..
class P3PillarOfSeverity(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.PillarOfSeverityAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 875, NameID = 4776)]
public class Un2Sephirot(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    public Actor? BossP1() => PrimaryActor.IsDestroyed ? null : PrimaryActor;

    private Actor? _bossP3;
    public Actor? BossP3() => _bossP3;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP3 ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.BossP3).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (StateMachine.ActivePhaseIndex <= 0)
            Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        else if (StateMachine.ActivePhaseIndex == 2)
            Arena.Actor(_bossP3, ArenaColor.Enemy);
    }
}

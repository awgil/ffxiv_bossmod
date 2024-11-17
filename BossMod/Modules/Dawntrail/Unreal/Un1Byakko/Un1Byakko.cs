namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class StormPulseRepeat(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.StormPulseRepeat));
class HeavenlyStrike(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.HeavenlyStrike), new AOEShapeCircle(3), true);
class FireAndLightningBoss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FireAndLightningBoss), new AOEShapeRect(54.3f, 10));
class FireAndLightningAdd(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FireAndLightningAdd), new AOEShapeRect(54.75f, 10));
class SteelClaw(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.SteelClaw), new AOEShapeCone(17.75f, 30.Degrees()), (uint)OID.Hakutei); // TODO: verify angle
class WhiteHerald(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.WhiteHerald, ActionID.MakeSpell(AID.WhiteHerald), 15, 5.1f); // TODO: verify falloff
class DistantClap(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.DistantClap), new AOEShapeDonut(4, 25));
class SweepTheLegBoss(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SweepTheLegBoss), new AOEShapeCone(28.3f, 135.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1007, NameID = 7092, PlanLevel = 100)]
public class Un1Byakko(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20))
{
    private Actor? _hakutei;
    public Actor? Boss() => PrimaryActor;
    public Actor? Hakutei() => _hakutei;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _hakutei ??= StateMachine.ActivePhaseIndex >= 0 ? Enemies(OID.Hakutei).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_hakutei, ArenaColor.Enemy);
    }
}

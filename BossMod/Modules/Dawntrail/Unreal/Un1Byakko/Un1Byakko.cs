namespace BossMod.Dawntrail.Unreal.Un1Byakko;

class StormPulseRepeat(BossModule module) : Components.CastCounter(module, AID.StormPulseRepeat);
class HeavenlyStrike(BossModule module) : Components.BaitAwayCast(module, AID.HeavenlyStrike, new AOEShapeCircle(3), true);
class FireAndLightningBoss(BossModule module) : Components.StandardAOEs(module, AID.FireAndLightningBoss, new AOEShapeRect(54.3f, 10));
class FireAndLightningAdd(BossModule module) : Components.StandardAOEs(module, AID.FireAndLightningAdd, new AOEShapeRect(54.75f, 10));
class SteelClaw(BossModule module) : Components.Cleave(module, AID.SteelClaw, new AOEShapeCone(17.75f, 30.Degrees()), (uint)OID.Hakutei); // TODO: verify angle
class WhiteHerald(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.WhiteHerald, AID.WhiteHerald, 15, 5.1f); // TODO: verify falloff
class DistantClap(BossModule module) : Components.StandardAOEs(module, AID.DistantClap, new AOEShapeDonut(4, 25));
class SweepTheLegBoss(BossModule module) : Components.StandardAOEs(module, AID.SweepTheLegBoss, new AOEShapeCone(28.3f, 135.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 1007, NameID = 7092, PlanLevel = 100)]
public class Un1Byakko(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), NormalBounds)
{
    public static readonly ArenaBoundsCircle NormalBounds = new(20);
    public static readonly ArenaBoundsCircle IntermissionBounds = new(15);

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

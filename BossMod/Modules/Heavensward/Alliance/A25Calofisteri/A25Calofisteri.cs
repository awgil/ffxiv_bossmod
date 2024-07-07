namespace BossMod.Heavensward.Alliance.A25Calofisteri;

class AuraBurst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AuraBurst));
class DepthCharge(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.DepthCharge), 5);
class Extension2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Extension2), 6);
class FeintParticleBeam1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.FeintParticleBeam1), 3);
class Penetration(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Penetration), 50, kind: Kind.TowardsOrigin);
class Graft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Graft), new AOEShapeCircle(5));
class Haircut1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Haircut1), new AOEShapeCone(25.5f, 90.Degrees()));
class Haircut2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Haircut2), new AOEShapeCone(25.5f, 90.Degrees()));
class SplitEnd1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SplitEnd1), new AOEShapeCone(25.5f, 22.5f.Degrees()));
class SplitEnd2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SplitEnd2), new AOEShapeCone(25.5f, 22.5f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4897)]
public class A25Calofisteri(WorldState ws, Actor primary) : BossModule(ws, primary, new(-300, -35), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Bijou1), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Bijou2), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GrandBijou), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LivingLock1), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LivingLock2), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LivingLock3), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.LurkingLock), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Entanglement), ArenaColor.Enemy);
    }
}

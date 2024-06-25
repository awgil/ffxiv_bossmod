namespace BossMod.Heavensward.Alliance.A21ArachneEve;

class DarkSpike(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.DarkSpike));
class SilkenSpray(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SilkenSpray), new AOEShapeCone(24.5f, 30.Degrees()));
class ShadowBurst(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ShadowBurst), 6, 8);
class SpiderThread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SpiderThread), 6);
class Tremblor1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Tremblor1), new AOEShapeCircle(10));
class Tremblor2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Tremblor2), new AOEShapeCircle(20));
class Tremblor3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Tremblor3), new AOEShapeCircle(30));
class FrondAffeared(BossModule module) : Components.CastGaze(module, ActionID.MakeSpell(AID.FrondAffeared));
class TheWidowsEmbrace(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.TheWidowsEmbrace), 18, kind: Kind.TowardsOrigin, stopAtWall: true);
class TheWidowsKiss(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.TheWidowsKiss), 4, kind: Kind.TowardsOrigin, stopAtWall: true);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 168, NameID = 4871)]
public class A21ArachneEve(WorldState ws, Actor primary) : BossModule(ws, primary, new(20, -60), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Keyknot), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Webmaiden), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SpittingSpider), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SkitteringSpider), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.EarthAether), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.DeepEarthAether), ArenaColor.Enemy);
    }
}

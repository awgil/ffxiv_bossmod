namespace BossMod.Heavensward.Alliance.A33ProtoUltima;

class AetherialPool(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.AetherialPool), 40, kind: Kind.TowardsOrigin, stopAtWall: true);
class AetherochemicalFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AetherochemicalFlare));
class AetherochemicalLaser1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaser1), new AOEShapeRect(70, 4));
class AetherochemicalLaser2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaser2), new AOEShapeRect(70, 4));
class AetherochemicalLaser3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AetherochemicalLaser3), new AOEShapeRect(70, 4));

class CitadelBuster2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.CitadelBuster2), new AOEShapeRect(60, 5));
class FlareStar(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlareStar), new AOEShapeCircle(31));

class Rotoswipe(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Rotoswipe), new AOEShapeCone(11, 60.Degrees()));

class WreckingBall(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.WreckingBall), 8);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 220, NameID = 3780)]
public class A33ProtoUltima(WorldState ws, Actor primary) : BossModule(ws, primary, new(-350, -50), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.AllaganDreadnaught), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.AetherCollector), ArenaColor.Enemy);
    }
}

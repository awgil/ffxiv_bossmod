namespace BossMod.RealmReborn.Alliance.A33Cerberus;

class TailBlow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TailBlow), new AOEShapeCone(19, 45.Degrees()));
class Slabber(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Slabber), 8);
class Mini(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Mini), new AOEShapeCircle(9));
class SulphurousBreath1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SulphurousBreath1), new AOEShapeRect(35, 3));
class SulphurousBreath2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SulphurousBreath2), new AOEShapeRect(45, 3));
class LightningBolt2(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.LightningBolt2), 2);
class HoundOutOfHell(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.HoundOutOfHell), 7);
class Ululation(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Ululation));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 111, NameID = 3224)]
public class A33Cerberus(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -197), new ArenaBoundsRect(20, 40))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GastricJuice), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.StomachWall), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Wolfsbane), ArenaColor.Enemy);
    }
}

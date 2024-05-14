namespace BossMod.RealmReborn.Alliance.A12Thanatos;

class BlackCloud(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BlackCloud), 6);
class Cloudscourge(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Cloudscourge), 6);
class VoidFireII(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.VoidFireII), 5);
class AstralLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AstralLight), new AOEShapeCircle(6.8f));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 710)]
public class A12Thanatos(WorldState ws, Actor primary) : BossModule(ws, primary, new(440, 280), new ArenaBoundsCircle(30))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.MagicPot), ArenaColor.Object);
        Arena.Actors(Enemies(OID.Nemesis), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Sandman), ArenaColor.Enemy);
    }
}
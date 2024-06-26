namespace BossMod.RealmReborn.Alliance.A21Scylla;

class Topple(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Topple), new AOEShapeCircle(6.75f));
class SearingChain(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SearingChain), new AOEShapeRect(61, 2));
class InfiniteAnguish(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.InfiniteAnguish), new AOEShapeDonut(6, 12));
class AncientFlare(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AncientFlare));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 102, NameID = 2809)]
public class A21Scylla(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -192), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.StaffOfEldering), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ShudderingSoul), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.ShiveringSoul), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.SmolderingSoul), ArenaColor.Enemy);
    }
}

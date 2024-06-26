namespace BossMod.RealmReborn.Alliance.A13KingBehemoth;

class EclipticMeteor(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.EclipticMeteor), 60, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.Comet).Where(a => !a.IsDead);
}
class SelfDestruct(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SelfDestruct), new AOEShapeCircle(8.4f));
class CharybdisAOE(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.CharybdisAOE), m => m.Enemies(OID.Charybdis).Where(v => v.EventState != 7), 0.1f);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 727)]
public class A13KingBehemoth(WorldState ws, Actor primary) : BossModule(ws, primary, new(-110, -380), new ArenaBoundsCircle(35))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.IronGiant), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.Puroboros), ArenaColor.Enemy);
    }
}

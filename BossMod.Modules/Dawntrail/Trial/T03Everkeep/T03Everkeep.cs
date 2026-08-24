namespace BossMod.Dawntrail.Trial.T03Everkeep;

[ModuleInfo(Contributors = "Gabriel Deleon", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 995, NameID = 12881)]
public class T03Everkeep(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), NormalBounds)
{
    public static readonly ArenaBoundsRect NormalBounds = new(20, 20, 45.Degrees());
    public static readonly ArenaBoundsRect SmallBounds = new(10, 10, 45.Degrees(), 20);

    public Actor? BossP1() => PrimaryActor;

    // The P2 boss actor is re-spawned mid-fight (the old instance is destroyed and a new one with
    // the same OID is spawned ~0.4s later during the big arena transition), so we query dynamically
    // and skip destroyed instances instead of caching the first one we see.
    public Actor? BossP2() => Enemies(OID.BossP2).FirstOrDefault(a => !a.IsDestroyed);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(BossP2(), ArenaColor.Enemy);
    }
}

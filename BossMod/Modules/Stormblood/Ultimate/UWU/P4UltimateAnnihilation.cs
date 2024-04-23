namespace BossMod.Stormblood.Ultimate.UWU;

class P4UltimateAnnihilation(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _orbs = module.Enemies(OID.Aetheroplasm);

    private const float _radius = 6;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in _orbs.Where(o => !o.IsDead))
        {
            Arena.Actor(orb, ArenaColor.Object, true);
            Arena.AddCircle(orb.Position, _radius, ArenaColor.Object);
        }
    }
}

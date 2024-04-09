namespace BossMod.Stormblood.Ultimate.UWU;

class P4UltimateAnnihilation : BossComponent
{
    private IReadOnlyList<Actor> _orbs = ActorEnumeration.EmptyList;

    private static readonly float _radius = 6;

    public override void Init(BossModule module)
    {
        _orbs = module.Enemies(OID.Aetheroplasm);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in _orbs.Where(o => !o.IsDead))
        {
            arena.Actor(orb, ArenaColor.Object, true);
            arena.AddCircle(orb.Position, _radius, ArenaColor.Object);
        }
    }
}

namespace BossMod.Stormblood.Ultimate.UWU;

// TODO: kill priorities
class P2Nails : BossComponent
{
    private IReadOnlyList<Actor> _nails = ActorEnumeration.EmptyList;

    public bool Active => _nails.Any(a => a.IsTargetable);

    public override void Init(BossModule module)
    {
        _nails = module.Enemies(OID.InfernalNail);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        arena.Actors(_nails, ArenaColor.Enemy);
    }
}

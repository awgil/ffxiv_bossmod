namespace BossMod.Stormblood.Ultimate.UWU;

class P4MagitekBits(BossModule module) : BossComponent(module)
{
    private readonly IReadOnlyList<Actor> _bits = module.Enemies(OID.MagitekBit);

    public bool Active => _bits.Any(b => b.IsTargetable);

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(_bits, ArenaColor.Enemy);
    }
}

namespace BossMod.Stormblood.Ultimate.UWU;

class P4UltimateAnnihilation(BossModule module) : BossComponent(module)
{
    private readonly List<Actor> _orbs = module.Enemies((uint)OID.Aetheroplasm);

    private const float _radius = 6f;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var orb in _orbs.Where(o => !o.IsDead))
        {
            Arena.Actor(orb, Colors.Object, true);
            Arena.ZoneCircleOutline(orb.Position, _radius, Colors.Object);
        }
    }
}

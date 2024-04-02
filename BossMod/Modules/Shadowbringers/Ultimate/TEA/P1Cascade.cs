namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1Cascade : Components.PersistentVoidzone
{
    public P1Cascade() : base(8, m => m.Enemies(OID.LiquidRage)) { }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.Actors(module.Enemies(OID.Embolus), ArenaColor.Object, true);
    }
}

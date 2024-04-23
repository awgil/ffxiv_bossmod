namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1Cascade(BossModule module) : Components.PersistentVoidzone(module, 8, m => m.Enemies(OID.LiquidRage))
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID.Embolus), ArenaColor.Object, true);
    }
}

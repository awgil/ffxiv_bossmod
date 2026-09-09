namespace BossMod.Shadowbringers.Ultimate.TEA;

class P1Cascade(BossModule module) : Components.Voidzone(module, 7.6f, OID.LiquidRage)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actors(Module.Enemies(OID.Embolus), ArenaColor.Object, true);
    }
}

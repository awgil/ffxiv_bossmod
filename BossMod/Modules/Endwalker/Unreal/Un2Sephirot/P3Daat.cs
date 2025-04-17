namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class P3Daat(BossModule module) : Components.CastCounter(module, AID.DaatRandom)
{
    private const float radius = 5;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Raid.WithoutSlot().InRadiusExcluding(actor, radius).Any())
            hints.Add("Spread!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddCircle(pc.Position, radius, ArenaColor.Danger);
    }
}

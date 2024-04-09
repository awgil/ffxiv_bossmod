namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class P3Daat : Components.CastCounter
{
    private static readonly float radius = 5;

    public P3Daat() : base(ActionID.MakeSpell(AID.DaatRandom)) { }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Raid.WithoutSlot().InRadiusExcluding(actor, radius).Any())
            hints.Add("Spread!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        arena.AddCircle(pc.Position, radius, ArenaColor.Danger);
    }
}

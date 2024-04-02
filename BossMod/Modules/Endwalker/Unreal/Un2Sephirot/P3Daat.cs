namespace BossMod.Endwalker.Unreal.Un2Sephirot;

class P3Daat : Components.CastCounter
{
    private static readonly float radius = 5;

    public P3Daat() : base(ActionID.MakeSpell(AID.DaatRandom)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (module.Raid.WithoutSlot().InRadiusExcluding(actor, radius).Any())
            hints.Add("Spread!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.AddCircle(pc.Position, radius, ArenaColor.Danger);
    }
}

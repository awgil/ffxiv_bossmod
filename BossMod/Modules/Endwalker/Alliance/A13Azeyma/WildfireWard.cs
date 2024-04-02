namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WildfireWard : Components.KnockbackFromCastTarget
{
    private static readonly WPos[] _tri = { new(-750, -762), new(-760.392f, -744), new(-739.608f, -744) };

    public WildfireWard() : base(ActionID.MakeSpell(AID.IlluminatingGlimpse), 15, false, 1, kind: Kind.DirLeft) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (!actor.Position.InTri(_tri[0], _tri[1], _tri[2]))
            hints.Add("Go to safe zone!");
        if (CalculateMovements(module, slot, actor).Any(e => !e.to.InTri(_tri[0], _tri[1], _tri[2])))
            hints.Add("About to be knocked into fire!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.ZoneTri(_tri[0], _tri[1], _tri[2], ArenaColor.SafeFromAOE);
    }
}

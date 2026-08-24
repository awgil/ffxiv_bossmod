namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WildfireWard(BossModule module) : Components.KnockbackFromCastTarget(module, AID.IlluminatingGlimpse, 15, false, 1, kind: Kind.DirLeft)
{
    private static readonly WPos[] _tri = [new(-750, -762), new(-760.392f, -744), new(-739.608f, -744)];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!actor.Position.InTri(_tri[0], _tri[1], _tri[2]))
            hints.Add("Go to safe zone!");
        if (CalculateMovements(slot, actor).Any(e => !e.to.InTri(_tri[0], _tri[1], _tri[2])))
            hints.Add("About to be knocked into fire!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        Arena.ZoneTri(_tri[0], _tri[1], _tri[2], ArenaColor.SafeFromAOE);
    }
}

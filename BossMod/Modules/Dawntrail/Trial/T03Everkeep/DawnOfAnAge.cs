namespace BossMod.Dawntrail.Trial.T03Everkeep;

// Dawn of an Age is the raidwide that also shrinks the arena: when the cast resolves the platform
// collapses to a 20x20 center square for the Chasm of Vollok + Forged Track sequence. Actualize
// restores the full 40x40 arena at the end of that sequence.
class DawnOfAnAge(BossModule module) : Components.RaidwideCast(module, AID.DawnOfAnAge)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action == WatchedAction)
            Module.Arena.Bounds = T03Everkeep.SmallBounds;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        // While the cast is up, forbid everything outside the post-shrink diamond so ranged jobs
        // pull in toward center before the platform collapses around them.
        foreach (var c in Casters)
            hints.AddForbiddenZone(ShapeContains.InvertedRect(Module.Center, 45.Degrees(), 20, 20, 20), Module.CastFinishAt(c.CastInfo));
    }

    // Cardinal extents of the rotated-45° diamonds: corners sit at half-extent × √2 from center.
    private static readonly float OuterCardinal = 20f * MathF.Sqrt(2);
    private static readonly float InnerCardinal = 10f * MathF.Sqrt(2);

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Casters.Count == 0)
            return;
        // Paint the four diamond wings that will collapse — the donut between SmallBounds and
        // NormalBounds, drawn as four quadrilaterals between adjacent diamond cardinals.
        var c = Module.Center;
        var nO = new WPos(c.X, c.Z - OuterCardinal);
        var eO = new WPos(c.X + OuterCardinal, c.Z);
        var sO = new WPos(c.X, c.Z + OuterCardinal);
        var wO = new WPos(c.X - OuterCardinal, c.Z);
        var nI = new WPos(c.X, c.Z - InnerCardinal);
        var eI = new WPos(c.X + InnerCardinal, c.Z);
        var sI = new WPos(c.X, c.Z + InnerCardinal);
        var wI = new WPos(c.X - InnerCardinal, c.Z);
        Arena.ZonePoly("doaa-ne", [nO, eO, eI, nI], ArenaColor.AOE);
        Arena.ZonePoly("doaa-se", [eO, sO, sI, eI], ArenaColor.AOE);
        Arena.ZonePoly("doaa-sw", [sO, wO, wI, sI], ArenaColor.AOE);
        Arena.ZonePoly("doaa-nw", [wO, nO, nI, wI], ArenaColor.AOE);
    }
}

namespace BossMod.Global.MaskedCarnivale;

public class Layout2Corners : BossComponent
{
    public static IEnumerable<WPos> Wall1A()
    {
        yield return new WPos(85, 95);
        yield return new WPos(95, 95);
        yield return new WPos(95, 94.5f);
        yield return new WPos(85, 94.5f);
    }
    public static IEnumerable<WPos> Wall1B()
    {
        yield return new WPos(95, 94.5f);
        yield return new WPos(95, 89);
        yield return new WPos(94.5f, 89);
        yield return new WPos(94.5f, 94.5f);
    }

    public static IEnumerable<WPos> Wall2A()
    {
        yield return new WPos(105, 95);
        yield return new WPos(115, 95);
        yield return new WPos(115, 94.5f);
        yield return new WPos(105, 94.5f);
    }
    public static IEnumerable<WPos> Wall2B()
    {
        yield return new WPos(105, 94.5f);
        yield return new WPos(105, 89);
        yield return new WPos(105.5f, 89);
        yield return new WPos(105.5f, 94.5f);
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.AddPolygon(Wall1A(), ArenaColor.Border);
        arena.AddPolygon(Wall1B(), ArenaColor.Border);
        arena.AddPolygon(Wall2A(), ArenaColor.Border);
        arena.AddPolygon(Wall2B(), ArenaColor.Border);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Wall1A(), false));
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Wall1B(), false));
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Wall2A(), false));
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Wall2B(), true));
    }
}

public class Layout4Quads : BossComponent
{
    public static IEnumerable<WPos> Quad1()
    {
        yield return new WPos(107, 110);
        yield return new WPos(110, 113);
        yield return new WPos(113, 110);
        yield return new WPos(110, 107);
    }

    public static IEnumerable<WPos> Quad2()
    {
        yield return new WPos(93, 110);
        yield return new WPos(90, 107);
        yield return new WPos(87, 110);
        yield return new WPos(90, 113);
    }
    public static IEnumerable<WPos> Quad3()
    {
        yield return new WPos(90, 93);
        yield return new WPos(93, 90);
        yield return new WPos(90, 87);
        yield return new WPos(87, 90);
    }

    public static IEnumerable<WPos> Quad4()
    {
        yield return new WPos(110, 93);
        yield return new WPos(113, 90);
        yield return new WPos(110, 87);
        yield return new WPos(107, 90);
    }
    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.AddPolygon(Quad1(), ArenaColor.Border);
        arena.AddPolygon(Quad2(), ArenaColor.Border);
        arena.AddPolygon(Quad3(), ArenaColor.Border);
        arena.AddPolygon(Quad4(), ArenaColor.Border);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Quad1(), false));
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Quad2(), false));
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Quad3(), false));
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Quad4(), false));
    }
}

public class LayoutBigQuad : BossComponent
{
    public static IEnumerable<WPos> Quad()
    {
        yield return new WPos(100, 107);
        yield return new WPos(107, 100);
        yield return new WPos(100, 93);
        yield return new WPos(93, 100);
    }
    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.AddPolygon(Quad(), ArenaColor.Border);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(module, slot, actor, assignment, hints);
        hints.AddForbiddenZone(ShapeDistance.ConvexPolygon(Quad(), false));
    }
}

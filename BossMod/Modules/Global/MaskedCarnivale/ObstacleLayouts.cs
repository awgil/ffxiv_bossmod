namespace BossMod.Global.MaskedCarnivale;

public class Layout2Corners(BossModule module) : BossComponent(module)
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

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddPolygon(Wall1A(), ArenaColor.Border);
        Arena.AddPolygon(Wall1B(), ArenaColor.Border);
        Arena.AddPolygon(Wall2A(), ArenaColor.Border);
        Arena.AddPolygon(Wall2B(), ArenaColor.Border);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Wall1A(), false));
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Wall1B(), false));
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Wall2A(), false));
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Wall2B(), true));
    }
}

public class Layout4Quads(BossModule module) : BossComponent(module)
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
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddPolygon(Quad1(), ArenaColor.Border);
        Arena.AddPolygon(Quad2(), ArenaColor.Border);
        Arena.AddPolygon(Quad3(), ArenaColor.Border);
        Arena.AddPolygon(Quad4(), ArenaColor.Border);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Quad1(), false));
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Quad2(), false));
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Quad3(), false));
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Quad4(), false));
    }
}

public class LayoutBigQuad(BossModule module) : BossComponent(module)
{
    public static IEnumerable<WPos> Quad()
    {
        yield return new WPos(100, 107);
        yield return new WPos(107, 100);
        yield return new WPos(100, 93);
        yield return new WPos(93, 100);
    }
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.AddPolygon(Quad(), ArenaColor.Border);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        hints.AddForbiddenZone(ShapeContains.ConvexPolygon(Quad(), false));
    }
}

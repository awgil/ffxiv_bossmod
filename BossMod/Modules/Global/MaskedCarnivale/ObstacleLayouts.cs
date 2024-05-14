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

    private static readonly List<Shape> union = [new Circle(new(100, 100), 25)];
    private static readonly List<Shape> difference = [new PolygonCustom(Wall1A()), new PolygonCustom(Wall1B()), new PolygonCustom(Wall2A()), new PolygonCustom(Wall2B())];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);
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

    private static readonly List<Shape> union = [new Circle(new(100, 100), 25)];
    private static readonly List<Shape> difference = [new PolygonCustom(Quad1()), new PolygonCustom(Quad2()), new PolygonCustom(Quad3()), new PolygonCustom(Quad4())];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);
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

    private static readonly List<Shape> union = [new Circle(new(100, 100), 25)];
    private static readonly List<Shape> difference = [new PolygonCustom(Quad())];
    public static readonly ArenaBounds arena = new ArenaBoundsComplex(union, difference);
}

namespace BossMod.Global.MaskedCarnivale;

public static class Layouts
{
    public static readonly WPos ArenaCenter = new(100f, 100f);
    private static readonly Angle a45 = 45f.Degrees();
    private static readonly Angle a90a = 89.98f.Degrees(), a90b = 90f.Degrees();
    private static readonly Polygon[] _circleBig = [new Polygon(ArenaCenter, 25f, 64)];
    private static readonly Polygon[] _circleBigAdj = [new Polygon(ArenaCenter, 24.5f, 64)];
    private static readonly Rectangle[] walls = [new(new(90f, 94.75f), 5f, 0.25f), new(new(94.75f, 92f), 3.01134109f, 0.25f, a90a),
    new(new(110f, 94.75f), 5f, 0.25f), new(new(105.25f, 92f), 3.01134109f, 0.25f, a90b)];
    private static readonly Square[] _bigQuad = [new Square(ArenaCenter, 4.9f, a45)];
    private const float sideLength = 1.9f;
    private static readonly Square[] squares = [new(new(110f, 110f), sideLength, a45), new(new(90f, 110f), sideLength, a45),
    new(new(110f, 90f), sideLength, a45), new(new(90f, 90f), sideLength, a45)];

    public static readonly ArenaBoundsCustom Layout4Quads = new(_circleBig, squares, AdjustForHitboxInwards: true);
    public static readonly ArenaBoundsCustom Layout2Corners = new(_circleBig, walls, AdjustForHitboxInwards: true);
    public static readonly RelSimplifiedComplexPolygon Layout2CornersBlockers = PolygonClipper.GetCombinedPolygon(ArenaCenter, _circleBigAdj, walls);
    public static readonly ArenaBoundsCustom LayoutBigQuad = new(_circleBig, _bigQuad, AdjustForHitboxInwards: true);
    public static readonly RelSimplifiedComplexPolygon LayoutBigQuadBlockers = PolygonClipper.GetCombinedPolygon(ArenaCenter, _circleBigAdj, _bigQuad);
    public static readonly ArenaBoundsCustom CircleSmall = new([new Polygon(ArenaCenter, 16.01379f, 32)]) { IsCircle = true };
    public static readonly ArenaBoundsCustom CircleBig = new(_circleBigAdj) { IsCircle = true };
}

namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
class RM06SSugarRiotConfig : ConfigNode
{
    [PropertyDisplay("Enable priority hints for adds phase (only used by VBM autorotation)")]
    public bool EnableAddsHints = false;

    [PropertyDisplay("Forbid DoTs on squirrels, mantas, rams, and jabberwock", depends: nameof(EnableAddsHints))]
    public bool SmartDots = true;

    public enum CatDotStrategy
    {
        [PropertyDisplay("From ranged only")]
        RangedOnly,
        [PropertyDisplay("From all roles")]
        All,
        [PropertyDisplay("None")]
        None
    }

    [PropertyDisplay("Allow DoT usage on cat", depends: nameof(EnableAddsHints))]
    public CatDotStrategy CatDotPolicy = CatDotStrategy.RangedOnly;

    [PropertyDisplay("Use single-target rotation on rays", depends: nameof(EnableAddsHints))]
    public bool MantaPrio = true;

    [PropertyDisplay("Prioritize jabberwock over other adds", depends: nameof(EnableAddsHints))]
    public bool JabberwockPrio = true;

    [PropertyDisplay("Prevent melees and tanks from hitting untethered rays", depends: nameof(EnableAddsHints))]
    public bool ForbiddenManta = true;
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1022, NameID = 13822, PlanLevel = 100, Contributors = "xan")]
public class RM06SSugarRiot(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsSquare(20))
{
    public static readonly WPos ArenaCenter = new(100, 100);

    public record struct Bridge(WPos Center, Angle Rotation);

    public static readonly Bridge[] Bridges = [
        new(new(91.578f, 103.488f), 22.5f.Degrees()),
        new(new(107.05f, 105.787f), 142.5f.Degrees()),
        new(new(101.19f, 90.962f), 82.5f.Degrees())
    ];

    public static readonly AOEShapeRect BridgeShape = new(3.6f, 2.475f, 3.6f);

    public static readonly RelSimplifiedComplexPolygon BridgesPoly = BuildBridgesPoly();

    // very low resolution non-overlapping cones with only the bridges removed; we don't need exact land mass precision because we already have it from arenabounds (and the removed bridges are only used for drawing on minimap)
    public static readonly (Angle, RelSimplifiedComplexPolygon)[] IslandCones = BuildIslandPolys([
        114.Degrees(),
        234.Degrees(),
        354.Degrees()
    ]);
    public static readonly RelSimplifiedComplexPolygon RiverPoly = BuildRiverPoly();
    public static readonly RelSimplifiedComplexPolygon LavaPoly = BuildLavaPoly();

    public static readonly ArenaBoundsCustom BoundsMinusRiver = BuildBoundsMinusRiver();

    public static int GetIsland(WPos p) => Array.FindIndex(IslandCones, i => p.InCone(ArenaCenter, i.Item1, 60.Degrees()));

    private static ArenaBoundsCustom BuildBoundsMinusRiver() => new(20, new PolygonClipper().Difference(new(CurveApprox.Rect(new WDir(1, 0), 20, 20)), new(RiverPoly)));

    private static RelSimplifiedComplexPolygon BuildRiverPoly()
    {
        RelSimplifiedComplexPolygon RiverPoly = new();

        var arcRiverN = new WPos(131.71599f, 92.44231f);
        var arcRiverSW = new WPos(100.90272f, 132.59357f);
        var arcRiverSE = new WPos(127.77293f, 82.919174f);

        RiverPoly.Parts.Add(ClipBridges(new(CurveApprox.DonutSector(28, 32.9f, 195.Degrees(), 264.Degrees(), 1 / 112.5f).Select(p => p + (arcRiverN - ArenaCenter)))));

        RiverPoly.Parts.Add(ClipBridges(new(CurveApprox.DonutSector(28, 32.9f, -40.Degrees(), 0.Degrees(), 1 / 112.5f).Select(p => p + (arcRiverSE - ArenaCenter)))));

        RiverPoly.Parts.Add(ClipBridges(new(CurveApprox.DonutSector(28, 32.9f, 199.Degrees(), 250f.Degrees(), 1 / 112.5f).Select(p => p + (arcRiverSW - ArenaCenter)))));

        var arcS = CurveApprox.CircleArc(new WPos(98.287796f, 113.00449f), 8.9f, 135.Degrees(), 210.Degrees(), 1 / 112.5f).Select(a => a - ArenaCenter);
        var arcE = CurveApprox.CircleArc(new WPos(112.11865f, 94.97823f), 8.9f, 258.Degrees(), 330.Degrees(), 1 / 112.5f).Select(a => a - ArenaCenter);
        var arcW = CurveApprox.CircleArc(new WPos(89.59171f, 92.015785f), 8.9f, 20.Degrees(), 86.Degrees(), 1 / 112.5f).Select(a => a - ArenaCenter);

        RiverPoly.Parts.Add(ClipBridges(new([.. arcS, .. arcW, .. arcE])));
        return RiverPoly;
    }

    private static RelSimplifiedComplexPolygon BuildBridgesPoly()
    {
        var poly = new RelSimplifiedComplexPolygon();
        foreach (var b in Bridges)
            poly.Parts.Add(new([.. CurveApprox.Rect(b.Center, b.Rotation.ToDirection(), BridgeShape.HalfWidth, BridgeShape.LengthFront).Select(r => r - ArenaCenter)]));
        return poly;
    }

    private static (Angle, RelSimplifiedComplexPolygon)[] BuildIslandPolys(Angle[] directions)
    {
        List<(Angle, RelSimplifiedComplexPolygon)> output = [];
        var clipper = new PolygonClipper();
        foreach (var dir in directions)
        {
            var isleCone = CurveApprox.CircleSector(40, dir - 60.Degrees(), dir + 60.Degrees(), 1);
            output.Add((dir, clipper.Difference(new(isleCone), new(BridgesPoly))));
        }
        return [.. output];
    }

    private static RelPolygonWithHoles ClipBridges(RelSimplifiedComplexPolygon input) => new PolygonClipper().Difference(new(input), new(BridgesPoly)).Parts.Single();

    private static RelSimplifiedComplexPolygon BuildLavaPoly()
    {
        // hack bs to connect the 4 separate river polygons together without going through all the trouble again
        var p0 = RiverPoly.Parts[0];
        var p1 = RiverPoly.Parts[1];
        var p3 = RiverPoly.Parts[3];
        List<WDir> allVerts = [p0.Vertices[0]];
        var p0i = 1;
        while (p0i < p0.Vertices.Count)
        {
            if (p0.Vertices[p0i].Z < p0.Vertices[p0i - 1].Z)
                break;
            allVerts.Add(p0.Vertices[p0i++]);
        }

        allVerts.Add(p3.Vertices[0]);
        var p3i = 1;
        while (p3i < p3.Vertices.Count)
        {
            if (p3.Vertices[p3i].Z > 0 && p3.Vertices[p3i].X < p3.Vertices[p3i - 1].X)
                break;
            allVerts.Add(p3.Vertices[p3i++]);
        }

        allVerts.Add(p1.Vertices[^1]);
        allVerts.AddRange(p1.Vertices[0..^1]);

        while (p3i < p3.Vertices.Count)
        {
            if (p3.Vertices[p3i].Z < 3)
                break;
            allVerts.Add(p3.Vertices[p3i++]);
        }

        allVerts.AddRange(RiverPoly.Parts[2].Vertices);
        allVerts.AddRange(p3.Vertices[p3i..]);
        allVerts.AddRange(p0.Vertices[p0i..]);

        return new(allVerts);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        // boss can walk "out of bounds" during storm/lava phase but we don't want her to be transparent
        Arena.ActorInsideBounds(PrimaryActor.Position, PrimaryActor.Rotation, ArenaColor.Enemy);
    }
}

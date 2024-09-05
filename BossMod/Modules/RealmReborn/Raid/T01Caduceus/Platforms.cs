namespace BossMod.RealmReborn.Raid.T01Caduceus;

// we have 12 hexagonal platforms and 1 octagonal; sorted S to N, then E to W - so entrance platform has index 0, octagonal (NW) platform has index 12
class Platforms(BossModule module) : BossComponent(module)
{
    public const float HexaPlatformSide = 9;
    public const float OctaPlatformLong = 13;
    public const float OctaPlatformShort = 7;
    public const float HexaCenterToSideCornerX = HexaPlatformSide * 0.8660254f; // sqrt(3) / 2
    public const float HexaCenterToSideCornerZ = HexaPlatformSide * 0.5f;
    public const float HexaNeighbourDistX = HexaCenterToSideCornerX * 2;
    public const float HexaNeighbourDistZ = HexaPlatformSide * 1.5f;

    public static readonly WPos ClosestPlatformCenter = new(0.6f, -374); // (0,0) on hexa grid
    public static readonly (int, int)[] HexaPlatforms = [(0, 0), (0, 1), (1, 1), (0, 2), (1, 2), (2, 2), (3, 2), (0, 3), (1, 3), (2, 3), (1, 4), (2, 4)];
    public static readonly (int, int) OctaPlatform = (3, 4);
    public static readonly WPos[] HexaPlatformCenters = HexaPlatforms.Select(HexaCenter).ToArray();
    public static readonly WDir OctaCenterOffset = 0.5f * new WDir(OctaPlatformShort, OctaPlatformLong - HexaPlatformSide);
    public static readonly WPos OctaPlatformCenter = HexaCenter(OctaPlatform) - OctaCenterOffset;

    // it is possible to move from platform if height difference is < 0.5, or jump if height difference is < 2
    public static readonly float[] PlatformHeights = [4.5f, 0.9f, 0.5f, -0.7f, -0.3f, 0.1f, 0.5f, 1.7f, 1.3f, 0.9f, 2.1f, 2.5f, 4.9f];
    public static readonly (int lower, int upper)[] HighEdges = [(1, 0), (3, 7), (4, 7), (9, 12), (11, 12)];
    public static readonly (int lower, int upper)[] JumpEdges = [(3, 1), (4, 1), (4, 2), (4, 8), (5, 8), (5, 9), (8, 10), (8, 11), (9, 11)];

    public static readonly Func<WPos, float>[] PlatformShapes = Enumerable.Range(0, HexaPlatformCenters.Length + 1).Select(i => ShapeDistance.ConvexPolygon(PlatformPoly(i), true, Pathfinding.NavigationDecision.ForbiddenZoneCushion)).ToArray();
    public static readonly Func<WPos, float>[] HighEdgeShapes = HighEdges.Select(e => HexaEdge(e.lower, e.upper)).Select(e => ShapeDistance.Rect(e.Item1, e.Item2, 0)).ToArray();
    public static readonly (WPos p, WDir d, float l)[] JumpEdgeSegments = JumpEdges.Select(e => HexaEdge(e.lower, e.upper)).Select(e => (e.Item1, (e.Item2 - e.Item1).Normalized(), (e.Item2 - e.Item1).Length())).ToArray();

    public static readonly BitMask AllPlatforms = new(0x1FFF);

    public static WPos HexaCenter((int x, int y) c) => ClosestPlatformCenter - new WDir(c.x * HexaNeighbourDistX + ((c.y & 1) != 0 ? HexaCenterToSideCornerX : 0), c.y * HexaNeighbourDistZ);
    public static readonly WDir[] HexaCornerOffsets = [
        new(HexaCenterToSideCornerX, -HexaCenterToSideCornerZ),
        new(HexaCenterToSideCornerX, HexaCenterToSideCornerZ),
        new(0, HexaPlatformSide),
        new(-HexaCenterToSideCornerX, HexaCenterToSideCornerZ),
        new(-HexaCenterToSideCornerX, -HexaCenterToSideCornerZ),
        new(0, -HexaPlatformSide)
    ];

    public static IEnumerable<WPos> HexaPoly(WPos center) => HexaCornerOffsets.Select(off => center + off);
    public static IEnumerable<WPos> OctaPoly()
    {
        yield return OctaPlatformCenter + new WDir(OctaCenterOffset.X, -OctaCenterOffset.Z - HexaPlatformSide);
        yield return OctaPlatformCenter + new WDir(OctaCenterOffset.X + HexaCenterToSideCornerX, -OctaCenterOffset.Z - HexaCenterToSideCornerZ);
        yield return OctaPlatformCenter + new WDir(OctaCenterOffset.X + HexaCenterToSideCornerX, +OctaCenterOffset.Z + HexaCenterToSideCornerZ);
        yield return OctaPlatformCenter + new WDir(OctaCenterOffset.X, +OctaCenterOffset.Z + HexaPlatformSide);
        yield return OctaPlatformCenter - new WDir(OctaCenterOffset.X, -OctaCenterOffset.Z - HexaPlatformSide);
        yield return OctaPlatformCenter - new WDir(OctaCenterOffset.X + HexaCenterToSideCornerX, -OctaCenterOffset.Z - HexaCenterToSideCornerZ);
        yield return OctaPlatformCenter - new WDir(OctaCenterOffset.X + HexaCenterToSideCornerX, +OctaCenterOffset.Z + HexaCenterToSideCornerZ);
        yield return OctaPlatformCenter - new WDir(OctaCenterOffset.X, +OctaCenterOffset.Z + HexaPlatformSide);
    }

    public static IEnumerable<WPos> PlatformPoly(int index) => index < HexaPlatformCenters.Length ? HexaPoly(HexaPlatformCenters[index]) : OctaPoly();

    public static (WPos, WPos) HexaEdge(int from, int to)
    {
        var (x1, y1) = from < HexaPlatforms.Length ? HexaPlatforms[from] : OctaPlatform;
        var (x2, y2) = to < HexaPlatforms.Length ? HexaPlatforms[to] : OctaPlatform;
        var (o1, o2) = (x2 - x1, y2 - y1, y1 & 1) switch
        {
            (-1, 0, _) => (0, 1),
            (-1, -1, 0) or (0, -1, 1) => (1, 2),
            (0, -1, 0) or (1, -1, 1) => (2, 3),
            (1, 0, _) => (3, 4),
            (0, 1, 0) or (1, 1, 1) => (4, 5),
            (-1, 1, 0) or (0, 1, 1) => (5, 0),
            _ => (0, 0)
        };
        var c = HexaCenter((x1, y1));
        return (c + HexaCornerOffsets[o1], c + HexaCornerOffsets[o2]);
    }

    public static bool IntersectJumpEdge(WPos p, WDir d, float l) => JumpEdgeSegments.Any(e =>
    {
        var n = e.d.OrthoL();
        var dirDot = d.Dot(n);
        if (dirDot < 0.05f)
            return false;
        var ts = n.Dot(e.p - p) / dirDot;
        if (ts < 0 || ts > l)
            return false;
        var te = d.OrthoL().Dot(p - e.p) / e.d.Dot(d.OrthoL());
        return te >= 0 && te <= e.l;
    });

    public BitMask ActivePlatforms;
    public DateTime ExplosionAt { get; private set; }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        float blockedArea(WPos p)
        {
            var res = -PlatformShapes.Min(f => f(p));
            foreach (var (e, f) in HighEdges.Zip(HighEdgeShapes))
                if (actor.PosRot.Y + 0.1f < PlatformHeights[e.upper])
                    res = Math.Min(res, f(p));
            return res;
        }
        hints.AddForbiddenZone(blockedArea);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (int i in (ActivePlatforms ^ AllPlatforms).SetBits())
            Arena.AddPolygon(PlatformPoly(i), ArenaColor.Border);
        foreach (int i in ActivePlatforms.SetBits())
            Arena.AddPolygon(PlatformPoly(i), ArenaColor.Enemy);
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if (actor.OID == (uint)OID.Platform)
        {
            int i = Array.FindIndex(HexaPlatformCenters, c => actor.Position.InCircle(c, 2));
            if (i == -1)
                i = HexaPlatformCenters.Length;
            bool active = state == 2;
            ActivePlatforms[i] = active;
            if (active)
                ExplosionAt = WorldState.FutureTime(6);
        }
    }
}

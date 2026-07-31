using Dalamud.Bindings.ImGui;
using Dalamud.Interface;

namespace BossMod;

// note on coordinate systems:
// - world coordinates - X points West to East, Z points North to South - so SE is corner with both maximal coords, NW is corner with both minimal coords
//                       rotation 0 corresponds to South, and increases counterclockwise (so East is +pi/2, North is pi, West is -pi/2)
// - camera azimuth 0 correpsonds to camera looking North and increases counterclockwise
// - screen coordinates - X points left to right, Y points top to bottom
[SkipLocalsInit]
public sealed class MiniArena(WPos center, ArenaBounds bounds)
{
    public static readonly BossModuleConfig Config = Service.Config.Get<BossModuleConfig>();
    private WPos _center = center;
    private readonly TriangulationCache _triCache = new();
    private readonly PolygonCache _polyCache = new();

    public WPos Center
    {
        get => _center;
        set
        {
            if (_center != value)
            {
                _center = value;
                _triCache.Invalidate();
                _polyCache.Invalidate();
            }
        }
    }

    private ArenaBounds _bounds = bounds;
    public ArenaBounds Bounds
    {
        get => _bounds;
        set
        {
            if (!ReferenceEquals(_bounds, value))
            {
                _bounds = value;
                _triCache.Invalidate();
                _polyCache.Invalidate();
            }
        }
    }

    public float ScreenHalfSize => 150f * Config.ArenaScale;
    public float ScreenMarginSize => 20f * Config.ArenaScale;

    // these are set at the beginning of each draw
    public Vector2 ScreenCenter;
    private Angle _cameraAzimuth;
    private float _cameraSinAzimuth;
    private float _cameraCosAzimuth = 1f;

    public bool InBounds(WPos position) => _bounds.Contains(position - _center);
    public WPos ClampToBounds(WPos position) => _center + _bounds.ClampToBounds(position - _center);
    public float IntersectRayBounds(WPos rayOrigin, WDir rayDir) => _bounds.IntersectRay(rayOrigin - _center, rayDir);

    // prepare for drawing - set up internal state, clip rect etc.
    public void Begin(Angle cameraAzimuth)
    {
        var centerOffset = new Vector2(ScreenMarginSize + Config.SlackForRotations * ScreenHalfSize);
        var fullSize = 2f * centerOffset;
        var currentWindowSize = ImGui.GetWindowSize();
        var requiredWindowSize = Vector2.Max(fullSize, currentWindowSize);
        ImGui.SetWindowSize(requiredWindowSize);
        var cursor = ImGui.GetCursorScreenPos();
        ImGui.Dummy(fullSize);

        if (_bounds.ScreenHalfSize != ScreenHalfSize)
        {
            _bounds.ScreenHalfSize = ScreenHalfSize;
            _triCache.Invalidate();
            _polyCache.Invalidate();
        }
        else
        {
            _triCache.NextFrame();
        }

        ScreenCenter = cursor + centerOffset;

        _cameraAzimuth = cameraAzimuth;
        (_cameraSinAzimuth, _cameraCosAzimuth) = MathF.SinCos(cameraAzimuth.Rad);
        var wmin = ImGui.GetWindowPos();
        var wmax = wmin + ImGui.GetWindowSize();
        ImGui.GetWindowDrawList().PushClipRect(Vector2.Max(cursor, wmin), Vector2.Min(cursor + fullSize, wmax));

        if (Config.OpaqueArenaBackground)
        {
            Zone(_bounds.ShapeTriangulation, Colors.Background);
        }
    }

    // if you are 100% sure your primitive does not need clipping, you can use drawlist api directly
    // this helper allows converting world-space coords to screen-space ones
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 WorldPositionToScreenPosition(WPos p) => ScreenCenter + WorldOffsetToScreenOffset(p - _center);

    // this is useful for drawing on margins (TODO better api)
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public Vector2 RotatedCoords(Vector2 coords)
    {
        var cx = coords.X;
        var cy = coords.Y;
        var x = cx * _cameraCosAzimuth - cy * _cameraSinAzimuth;
        var y = cy * _cameraCosAzimuth + cx * _cameraSinAzimuth;
        return new(x, y);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private Vector2 WorldOffsetToScreenOffset(WDir worldOffset) => ScreenHalfSize * RotatedCoords(new(worldOffset.X, worldOffset.Z)) * _bounds.InvRadius;

    // unclipped primitive rendering that accept world-space positions; thin convenience wrappers around drawlist api
    public void AddLine(WPos a, WPos b, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        if (Config.ShowOutlinesAndShadows)
        {
            ImGui.GetWindowDrawList().AddLine(WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), Colors.Shadows, thickness + 1f);
        }

        ImGui.GetWindowDrawList().AddLine(WorldPositionToScreenPosition(a), WorldPositionToScreenPosition(b), color != default ? color : Colors.Danger, thickness);
    }

    public void AddTriangle(WPos p1, WPos p2, WPos p3, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        ImGui.GetWindowDrawList().AddTriangle(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color != default ? color : Colors.Danger, thickness);
    }

    public void AddTriangleFilled(WPos p1, WPos p2, WPos p3, uint color = default) => ImGui.GetWindowDrawList().AddTriangleFilled(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), color != default ? color : Colors.Danger);

    public void AddQuad(WPos p1, WPos p2, WPos p3, WPos p4, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        ImGui.GetWindowDrawList().AddQuad(WorldPositionToScreenPosition(p1), WorldPositionToScreenPosition(p2), WorldPositionToScreenPosition(p3), WorldPositionToScreenPosition(p4), color != default ? color : Colors.Danger, thickness);
    }

    public void AddRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        var side = halfWidth * direction.OrthoR();
        var front = origin + lenFront * direction;
        var back = origin - lenBack * direction;
        AddQuad(front + side, front - side, back - side, back + side, color, thickness);
    }

    public void AddPolygon(ReadOnlySpan<WPos> vertices, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        var len = vertices.Length;
        for (var i = 0; i < len; ++i)
        {
            PathLineTo(vertices[i]);
        }

        PathStroke(true, color != default ? color : Colors.Danger, thickness);
    }

    public void AddComplexPolygon(RelSimplifiedComplexPolygon poly, uint color = default, float thickness = 1f, bool addShadows = true)
    {
        var colors = color != default ? color : Colors.Danger;

        var dl = ImGui.GetWindowDrawList();
        var parts = CollectionsMarshal.AsSpan(poly.Parts);
        var len = parts.Length;
        var showShadows = addShadows && Config.ShowOutlinesAndShadows;
        var scale = Config.ThicknessScale;
        var thickness_ = thickness;
        var screencenter = ScreenCenter;

        for (var i = 0; i < len; ++i)
        {
            var part = parts[i];

            DrawContour(part.Exterior);
            var countH = part.HoleStarts.Count;
            for (var h = 0; h < countH; ++h)
            {
                DrawContour(part.Interior(h));
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void DrawContour(ReadOnlySpan<WDir> contour)
        {
            var len = contour.Length;
            Span<Vector2> points = stackalloc Vector2[len];

            for (var i = 0; i < len; ++i)
            {
                points[i] = screencenter + WorldOffsetToScreenOffset(contour[i]);
            }

            DrawPolygon(points);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        unsafe void DrawPolygon(Span<Vector2> points)
        {
            fixed (Vector2* p = points)
            {
                var len = points.Length;
                if (showShadows)
                {
                    dl.AddPolyline(p, len, Colors.Shadows, ImDrawFlags.Closed, (thickness + 1f) * scale);
                }
                dl.AddPolyline(p, len, colors, ImDrawFlags.Closed, thickness * scale);
            }
        }
    }

    // path api: add new point to path; this adds new edge from last added point, or defines first vertex if path is empty
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void PathLineTo(WPos p) => ImGui.GetWindowDrawList().PathLineToMergeDuplicate(WorldPositionToScreenPosition(p));

    // adds a bunch of points corresponding to arc - if path is non empty, this adds an edge from last point to first arc point
    public void PathArcTo(WPos center, float radius, float amin, float amax) => ImGui.GetWindowDrawList().PathArcTo(WorldPositionToScreenPosition(center), radius * _bounds.InvRadius * ScreenHalfSize, Angle.HalfPi - amin + _cameraAzimuth.Rad, Angle.HalfPi - amax + _cameraAzimuth.Rad);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void PathStroke(bool closed, uint color = default, float thickness = 1f)
    {
        thickness *= Config.ThicknessScale;
        ImGui.GetWindowDrawList().PathStroke(color != default ? color : Colors.Danger, closed ? ImDrawFlags.Closed : ImDrawFlags.None, thickness);
    }

    // draw clipped & triangulated zone
    public void Zone(List<RelTriangle> triangulation, uint color = default)
    {
        var drawlist = ImGui.GetWindowDrawList();
        var restoreFlags = drawlist.Flags;
        drawlist.Flags &= ~ImDrawListFlags.AntiAliasedFill;
        var triangles = CollectionsMarshal.AsSpan(triangulation);
        var len = triangles.Length;
        var col = color != default ? color : Colors.AOE;
        var center = ScreenCenter;

        var cosAzimuth = _cameraCosAzimuth;
        var sinAzimuth = _cameraSinAzimuth;
        var screenHalfSize = ScreenHalfSize;
        var invRadius = _bounds.InvRadius;

        for (var i = 0; i < len; ++i)
        {
            ref readonly var tri = ref triangles[i];
            var a = TransformCoords(tri.A);
            var b = TransformCoords(tri.B);
            var c = TransformCoords(tri.C);
            drawlist.AddTriangleFilled(center + a, center + b, center + c, col);
        }

        drawlist.Flags = restoreFlags;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Vector2 TransformCoords(in WDir worldOffset)
        {
            var x0 = worldOffset.X;
            var z0 = worldOffset.Z;
            var x = x0 * cosAzimuth - z0 * sinAzimuth;
            var z = z0 * cosAzimuth + x0 * sinAzimuth;
            return screenHalfSize * new Vector2(x, z) * invRadius;
        }
    }

    // draw zones - these are filled primitives clipped to arena border; note that triangulation is cached
    public void ZoneCone(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color)
    {
        ref var tri = ref _triCache.Get(1, center, innerRadius, outerRadius, centerDirection, halfAngle);
        tri ??= _bounds.ClipAndTriangulateCone(center - _center, innerRadius, outerRadius, centerDirection, halfAngle);
        Zone(tri, color);
    }

    public void ZoneCircle(WPos center, float radius, uint color)
    {
        ref var tri = ref _triCache.Get(2, center, radius);
        if (tri == null)
        {
            var offset = center - _center;
            var bounds = _bounds.ShapeSimplified;
            if ((bounds.ClosestPointOnBoundary(offset) - offset).LengthSq() >= radius * radius) // circle is farther than it's radius away from boundary 
            {
                if (bounds.Contains(offset)) // no need for clipping if circle is fully inside polygon
                {
                    tri = _bounds.Triangulate(_bounds.CirclePolygon(center, _center, radius));
                }
                else // circle is fully outside of polygon, don't create at all
                {
                    tri = [];
                }
            }
            else
            {
                tri = _bounds.ClipAndTriangulateCircle(center - _center, radius);
            }
        }
        Zone(tri, color);
    }

    public void ZoneDonut(WPos center, float innerRadius, float outerRadius, uint color)
    {
        ref var tri = ref _triCache.Get(3, center, innerRadius, outerRadius);
        tri ??= _bounds.ClipAndTriangulateDonut(center - _center, innerRadius, outerRadius);
        Zone(tri, color);
    }

    public void ZoneTri(WPos a, WPos b, WPos c, uint color)
    {
        ref var tri = ref _triCache.Get(4, a, b, c);
        tri ??= _bounds.ClipAndTriangulateTri(a - _center, b - _center, c - _center);
        Zone(tri, color);
    }

    public void ZoneIsoscelesTri(WPos apex, WDir height, WDir halfBase, uint color)
    {
        ref var tri = ref _triCache.Get(5, apex, height, halfBase);
        tri ??= _bounds.ClipAndTriangulateIsoscelesTri(apex - _center, height, halfBase);
        Zone(tri, color);
    }

    public void ZoneIsoscelesTri(WPos apex, Angle direction, Angle halfAngle, float height, uint color)
    {
        ref var tri = ref _triCache.Get(6, apex, direction, halfAngle, height);
        tri ??= _bounds.ClipAndTriangulateIsoscelesTri(apex - _center, direction, halfAngle, height);
        Zone(tri, color);
    }

    public void ZoneRect(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color)
    {
        ref var tri = ref _triCache.Get(7, origin, direction, lenFront, lenBack, halfWidth);
        tri ??= _bounds.ClipAndTriangulateRect(origin - _center, direction, lenFront, lenBack, halfWidth);
        Zone(tri, color);
    }

    public void ZoneRect(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color)
    {
        ref var tri = ref _triCache.Get(8, origin, direction, lenFront, lenBack, halfWidth);
        tri ??= _bounds.ClipAndTriangulateRect(origin - _center, direction, lenFront, lenBack, halfWidth);
        Zone(tri, color);
    }

    public void ZoneRect(WPos start, WPos end, float halfWidth, uint color)
    {
        ref var tri = ref _triCache.Get(9, start, end, halfWidth);
        tri ??= _bounds.ClipAndTriangulateRect(start - _center, end - _center, halfWidth);
        Zone(tri, color);
    }

    public void ZoneCross(WPos origin, Angle rotation, float range, float halfWidth, WPos[] contour, uint color)
    {
        ref var tri = ref _triCache.Get(10, origin, rotation, range, halfWidth);
        if (tri == null)
        {
            var len = contour.Length;
            var adjusted = new WDir[len];
            for (var i = 0; i < len; i++)
            {
                adjusted[i] = contour[i] - _center;
            }
            tri = _bounds.ClipAndTriangulate(adjusted);
        }
        Zone(tri, color);
    }

    public void ZoneRelPoly(int key, RelSimplifiedComplexPolygon poly, uint color)
    {
        ref var tri = ref _triCache.GetByHash(key);
        tri ??= _bounds.ClipAndTriangulate(poly);
        Zone(tri, color);
    }

    public void ZoneCapsule(WPos start, WDir direction, float radius, float length, uint color)
    {
        ref var tri = ref _triCache.Get(11, start, direction, radius, length);
        tri ??= _bounds.ClipAndTriangulateCapsule(start - _center, direction, radius, length);
        Zone(tri, color);
    }

    public void ZoneArcCapsule(WPos start, WPos orbitCenter, Angle angularLength, float radius, uint color)
    {
        ref var tri = ref _triCache.Get(13, start, orbitCenter, angularLength, radius);
        // startOffset: local translation; toOrbitCenter: vector from start to orbit center
        var startOffset = start - _center;
        var toOrbitCenter = orbitCenter - start;
        tri ??= _bounds.ClipAndTriangulateArcCapsule(startOffset, toOrbitCenter, angularLength, radius);
        Zone(tri, color);
    }

    // draw zone outlines - these are filled primitives clipped to arena border; note that clipped polygons are cached
    public void ZoneConeOutline(WPos center, float innerRadius, float outerRadius, Angle centerDirection, Angle halfAngle, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(1, center, innerRadius, outerRadius, centerDirection, halfAngle);
        poly ??= _bounds.ClipCone(center - _center, innerRadius, outerRadius, centerDirection, halfAngle);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneCircleOutline(WPos center, float radius, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(2, center, radius);
        if (poly == null)
        {
            var offset = center - _center;
            var bounds = _bounds.ShapeSimplified;
            if ((bounds.ClosestPointOnBoundary(offset) - offset).LengthSq() >= radius * radius) // circle is farther than it's radius away from boundary 
            {
                if (bounds.Contains(offset)) // no need for clipping if circle is fully inside polygon
                {
                    poly = _bounds.CirclePolygon(center, _center, radius);
                }
                else // circle is fully outside of polygon, don't create at all
                {
                    poly = new();
                }
            }
            else
            {
                poly = _bounds.ClipCircle(center - _center, radius);
            }
        }
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneCircleOutlineUnclipped(WPos center, float radius, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(14, center, radius);
        poly ??= _bounds.CirclePolygon(center, _center, radius);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneDonutOutline(WPos center, float innerRadius, float outerRadius, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(3, center, innerRadius, outerRadius);
        poly ??= _bounds.ClipDonut(center - _center, innerRadius, outerRadius);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneTriOutline(WPos a, WPos b, WPos c, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(4, a, b, c);
        poly ??= _bounds.ClipTri(a - _center, b - _center, c - _center);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneIsoscelesTriOutline(WPos apex, WDir height, WDir halfBase, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(5, apex, height, halfBase);
        poly ??= _bounds.ClipIsoscelesTri(apex - _center, height, halfBase);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneRectOutline(WPos origin, WDir direction, float lenFront, float lenBack, float halfWidth, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(7, origin, direction, lenFront, lenBack, halfWidth);
        poly ??= _bounds.ClipRect(origin - _center, direction, lenFront, lenBack, halfWidth);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneRectOutline(WPos origin, Angle direction, float lenFront, float lenBack, float halfWidth, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(8, origin, direction, lenFront, lenBack, halfWidth);
        poly ??= _bounds.ClipRect(origin - _center, direction, lenFront, lenBack, halfWidth);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneRectOutline(WPos start, WPos end, float halfWidth, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(9, start, end, halfWidth);
        poly ??= _bounds.ClipRect(start - _center, end - _center, halfWidth);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneCrossOutline(WPos origin, Angle rotation, float range, float halfWidth, WPos[] contour, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(10, origin, rotation, range, halfWidth);
        if (poly == null)
        {
            var len = contour.Length;
            var adjusted = new WDir[len];
            for (var i = 0; i < len; i++)
            {
                adjusted[i] = contour[i] - _center;
            }
            poly = _bounds.Clip(adjusted);
        }
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneRelPolyOutline(int key, RelSimplifiedComplexPolygon poly, uint color = default, float thickness = 1f)
    {
        ref var polygon = ref _polyCache.GetByHash(key);
        polygon ??= _bounds.Clip(poly);
        AddComplexPolygon(polygon, color, thickness);
    }

    public void ZoneCapsuleOutline(WPos start, WDir direction, float radius, float length, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(11, start, direction, radius, length);
        poly ??= _bounds.ClipCapsule(start - _center, direction, radius, length);
        AddComplexPolygon(poly, color, thickness);
    }

    public void ZoneArcCapsuleOutline(WPos start, WPos orbitCenter, Angle angularLength, float radius, uint color = default, float thickness = 1f)
    {
        ref var poly = ref _polyCache.Get(13, start, orbitCenter, angularLength, radius);
        // startOffset: local translation; toOrbitCenter: vector from start to orbit center
        var startOffset = start - _center;
        var toOrbitCenter = orbitCenter - start;
        poly ??= _bounds.ClipArcCapsule(startOffset, toOrbitCenter, angularLength, radius);
        AddComplexPolygon(poly, color, thickness);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TextScreen(Vector2 center, string text, uint color, float fontSize = 17f)
    {
        var size = ImGui.CalcTextSize(text) * Config.ArenaScale;
        ImGui.GetWindowDrawList().AddText(ImGui.GetFont(), fontSize * Config.ArenaScale, center - size * 0.5f, color, text);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void TextWorld(WPos center, string text, uint color, float fontSize = 17f) => TextScreen(WorldPositionToScreenPosition(center), text, color, fontSize);

    public void IconScreen(Vector2 center, FontAwesomeIcon icon, uint color, float fontSize = 17)
    {
        var size = ImGui.CalcTextSizeA(Service.IconFont, fontSize, float.MaxValue, float.MaxValue, icon.ToIconString(), out var i);
        size.X -= i * 0.5f;
        ImGui.GetWindowDrawList().AddText(Service.IconFont, fontSize, center - size / 2, color, icon.ToIconString());
    }

    public void IconWorld(WPos center, FontAwesomeIcon icon, uint color, float fontSize = 17) => IconScreen(WorldPositionToScreenPosition(center), icon, color, fontSize);

    public void CardinalNames()
    {
        var center = ScreenCenter;
        var fontSetting = Config.CardinalsFontSize;
        var offCenterSizeOffset = (ScreenHalfSize + ScreenMarginSize * 0.5f) * _bounds.ScaleFactor + fontSetting - 17f;
        var offS = RotatedCoords(new(default, offCenterSizeOffset));
        var offE = RotatedCoords(new(offCenterSizeOffset, default));
        TextScreen(center - offS, "N", Colors.CardinalN, fontSetting);
        TextScreen(center + offS, "S", Colors.CardinalS, fontSetting);
        TextScreen(center + offE, "E", Colors.CardinalE, fontSetting);
        TextScreen(center - offE, "W", Colors.CardinalW, fontSetting);
    }

    public void ActorInsideBounds(WPos position, Angle rotation, uint color)
    {
        var scale = Config.ActorScale * Config.ThicknessScale;
        var dir = rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale07 = position + scale07;
        var positionscale035 = position - scale035;
        var positionscale035pscale0433 = positionscale035 + scale0433;
        var positionscale035mscale0433 = positionscale035 - scale0433;
        if (Config.ShowOutlinesAndShadows)
        {
            AddTriangle(positionscale07, positionscale035pscale0433, positionscale035mscale0433, Colors.Shadows, 2f);
        }

        AddTriangleFilled(positionscale07, positionscale035pscale0433, positionscale035mscale0433, color);
    }

    public void ActorOutsideBounds(WPos position, Angle rotation, uint color)
    {
        var scale = Config.ActorScale;
        var dir = rotation.ToDirection();
        var scale07 = scale * 0.7f * dir;
        var scale035 = scale * 0.35f * dir;
        var scale0433 = scale * 0.433f * dir.OrthoR();
        var positionscale035 = position - scale035;
        AddTriangle(position + scale07, positionscale035 + scale0433, positionscale035 - scale0433, color);
    }

    public void ActorProjected(WPos from, WPos to, Angle rotation, uint color)
    {
        if (InBounds(to))
        {
            // projected position is inside bounds
            ActorInsideBounds(to, rotation, color);
            return;
        }

        var dir = to - from;
        var l = dir.Length();

        if (l == default)
        {
            return; // can't determine projection direction
        }

        dir /= l;
        var t = IntersectRayBounds(from, dir);
        if (t <= l)
        {
            ActorOutsideBounds(from + t * dir, rotation, color);
        }
    }

    public void Actor(WPos position, Angle rotation, uint color)
    {
        if (InBounds(position))
        {
            ActorInsideBounds(position, rotation, color);
        }
        else
        {
            ActorOutsideBounds(ClampToBounds(position), rotation, color);
        }
    }

    public void Actor(Actor? actor, uint color = default, bool allowDeadAndUntargetable = false)
    {
        if (actor != null && !actor.IsDestroyed && (allowDeadAndUntargetable || actor.IsTargetable && !actor.IsDead))
        {
            Actor(actor.Position, actor.Rotation, color == default ? Colors.Enemy : color);
        }
    }

    public void Actors(IEnumerable<Actor> actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        foreach (var a in actors)
        {
            Actor(a, color == default ? Colors.Enemy : color, allowDeadAndUntargetable);
        }
    }

    public void Actors(List<Actor> actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        var count = actors.Count;
        for (var i = 0; i < count; ++i)
        {
            Actor(actors[i], color == default ? Colors.Enemy : color, allowDeadAndUntargetable);
        }
    }

    public void Actors(BossModule module, uint[] actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        var actors_ = actors;
        var len = actors_.Length;
        var color_ = color == default ? Colors.Enemy : color;
        for (var i = 0; i < len; ++i)
        {
            var enemies = module.Enemies(actors[i]);
            var count = enemies.Count;
            for (var j = 0; j < count; ++j)
            {
                var enemy = enemies[j];
                if (!enemy.IsDestroyed && (allowDeadAndUntargetable || enemy.IsTargetable && !enemy.IsDead))
                {
                    Actor(enemy.Position, enemy.Rotation, color_);
                }
            }
        }
    }

    public void ActorsInBounds(BossModule module, uint[] actors, uint color = default, bool allowDeadAndUntargetable = false)
    {
        var actors_ = actors;
        var len = actors_.Length;
        var center = _center;
        var radius = Bounds.Radius;
        var color_ = color == default ? Colors.Enemy : color;
        for (var i = 0; i < len; ++i)
        {
            var enemies = module.Enemies(actors[i]);
            var count = enemies.Count;
            for (var j = 0; j < count; ++j)
            {
                var enemy = enemies[j];
                if (!enemy.IsDestroyed && enemy.Position.AlmostEqual(center, radius) && (allowDeadAndUntargetable || enemy.IsTargetable && !enemy.IsDead))
                {
                    Actor(enemy.Position, enemy.Rotation, color_);
                }
            }
        }
    }

    public static void End() => ImGui.GetWindowDrawList().PopClipRect();
}

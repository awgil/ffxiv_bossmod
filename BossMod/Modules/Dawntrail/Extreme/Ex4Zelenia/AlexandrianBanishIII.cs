namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class AlexandrianBanishIII(BossModule module) : Components.GenericBaitStack(module, (uint)AID.AlexandrianBanishIII)
{
    private readonly FloorTiles _tiles = module.FindComponent<FloorTiles>()!;
    private static readonly AOEShapeCircle circle = new(4f);

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.AlexandrianBanishIII)
        {
            CurrentBaits.Add(new(Arena.Center, actor, circle, WorldState.FutureTime(4.1d)));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        if (CurrentBaits[0].Target != pc)
            base.DrawArenaBackground(pcSlot, pc);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        if (CurrentBaits[0].Target == pc)
            Arena.ZoneCircleOutline(pc.Position, 4f, Colors.Safe);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = CurrentBaits.Count;
        if (count == 0)
            return;
        if (CurrentBaits[0].Target == actor)
            hints.Add("Intersect marked tiles to share damage over distance!");
        else
            base.AddHints(slot, actor, hints);
    }

    private WPos lastPos;

    public override void Update()
    {
        var baits = CollectionsMarshal.AsSpan(CurrentBaits);
        if (baits.Length == 0)
        {
            return;
        }

        ref var bait = ref baits[0];
        var loc = bait.Target.Position;
        if (lastPos == loc)
        {
            return; // don't bother with all these calculations if nothing changed...
        }
        var shapes = new List<Shape>
        {
            new Polygon(loc, 4f, 25)
        };

        Span<bool> visited = stackalloc bool[16];
        Span<bool> activeTiles = stackalloc bool[16];
        Span<bool> intersects = stackalloc bool[16];
        Span<(int ring, int idx)> stack = stackalloc (int, int)[16];
        var stackPtr = 0;
        var angle = 22.5f.Degrees();
        var pos = Arena.Center;
        for (var i = 0; i < 8; ++i)
        {
            activeTiles[i] = _tiles.InnerActiveTiles[i];
            activeTiles[i + 8] = _tiles.OuterActiveTiles[i];

            var dir = FloorTiles.TileDirections[i];
            intersects[i] = Intersect.CircleDonutSector(loc, 4f, pos, 2f, 8f, dir, angle);
            intersects[i + 8] = Intersect.CircleDonutSector(loc, 4f, pos, 8f, 16f, dir, angle);
        }

        for (var i = 0; i < 8; ++i)
        {
            for (var ring = 0; ring <= 1; ++ring)
            {
                var tileIdx = ring == 0 ? i : i + 8;
                if (visited[tileIdx] || !activeTiles[tileIdx] || !intersects[tileIdx])
                {
                    continue;
                }

                stackPtr = 0;
                stack[stackPtr++] = (ring, i);

                while (stackPtr > 0)
                {
                    var (r, j) = stack[--stackPtr];
                    var id = r == 0 ? j : j + 8;
                    if (visited[id] || !activeTiles[id])
                    {
                        continue;
                    }

                    visited[id] = true;

                    var shape = r == 0
                        ? new DonutSegmentV(Arena.Center, 2f, 8f, FloorTiles.TileAngles[j], angle, 8)
                        : new DonutSegmentV(Arena.Center, 8f, 16f, FloorTiles.TileAngles[j], angle, 8);

                    shapes.Add(shape);
                    var leftright = new int[2] { (j + 7) & 7, (j + 1) & 7 };
                    for (var k = 0; k < 2; ++k)
                    {
                        var n = leftright[k];
                        var nid = r == 0 ? n : n + 8;
                        if (!visited[nid] && activeTiles[nid])
                        {
                            stack[stackPtr++] = (r, n);
                        }
                    }

                    var otherRing = 1 - r;
                    var otherId = otherRing == 0 ? j : j + 8;
                    if (!visited[otherId] && activeTiles[otherId])
                    {
                        stack[stackPtr++] = (otherRing, j);
                    }
                }
            }
        }
        lastPos = loc;
        bait.Shape = new AOEShapeCustom(pos, [.. shapes]);
    }
}

sealed class AlexandrianBanishIIITargetHint(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly FloorTiles _tiles = module.FindComponent<FloorTiles>()!;
    private int slotTarget;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => slotTarget == slot ? _aoe : [];

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.AlexandrianBanishIII)
        {
            slotTarget = Raid.FindSlot(targetID);
            FloorTiles.GetLongestActiveChain(_tiles.InnerActiveTiles, _tiles.OuterActiveTiles, out var longestInner, out var longestOuter);
            var lenInner = longestInner.Length;
            var lenOuter = longestOuter.Length;
            var shapes = new DonutSegmentV[lenInner + lenOuter];
            var angle = 22.5f.Degrees();
            var center = Arena.Center;
            for (var i = 0; i < lenInner; ++i)
            {
                shapes[i] = new DonutSegmentV(center, 2f, 8f, FloorTiles.TileAngles[longestInner[i]], angle, 8);
            }
            for (var i = 0; i < lenOuter; ++i)
            {
                shapes[lenInner + i] = new DonutSegmentV(center, 8f, 16f, FloorTiles.TileAngles[longestOuter[i]], angle, 8);
            }
            _aoe = [new(new AOEShapeCustom(center, shapes, invertForbiddenZone: true), center, default, WorldState.FutureTime(4.1d), Colors.SafeFromAOE)];
        }
    }
}

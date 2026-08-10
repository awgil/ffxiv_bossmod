namespace BossMod.Dawntrail.Extreme.Ex4Zelenia;

sealed class DonutSectorTowers(BossModule module) : Components.GenericTowers(module, (uint)AID.ExplosionDonutSectorTower)
{
    private readonly FloorTiles _tiles = module.FindComponent<FloorTiles>()!;
    private BitMask forbidden;
    private int emblazoncounter;
    private int envccounter;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.Emblazon)
        {
            forbidden.Set(Raid.FindSlot(targetID));
            if (++emblazoncounter == 4)
            {
                var towers = CollectionsMarshal.AsSpan(Towers);
                var len = towers.Length;
                for (var i = 0; i < len; ++i)
                {
                    ref var t = ref towers[i];
                    t.ForbiddenSoakers = forbidden;
                }
            }
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // tower indices:
        // outer ring:
        // 0x14: 180°  → index 0
        // 0x15: 135°  → index 1
        // 0x16: 90°   → index 2
        // 0x17: 45°   → index 3
        // 0x18: 0°    → index 4
        // 0x19: -45°  → index 6
        // 0x1A: -90°  → index 7
        // 0x1B: -135° → index 8
        // inner ring:
        // 0x2D: 180°  → index 0
        // 0x2E: 135°  → index 1
        // 0x2F: 90°   → index 2
        // 0x30: 45°   → index 3
        // 0x31: 0°    → index 4
        // 0x32: -45°  → index 6
        // 0x33: -90°  → index 7
        // 0x34: -135° → index 8
        if (state == 0x00020001u)
        {
            void AddTower(AOEShapeDonutSector shape, byte idx)
            {
                var pos = Arena.Center;
                Towers.Add(new(pos, shape, activation: WorldState.FutureTime(13d), rotation: FloorTiles.TileAngles[index - idx], shapeDistance: shape.Distance(pos, default),
                invertedShapeDistance: shape.InvertedDistance(pos, default)));
            }
            if (index is >= 0x14 and <= 0x1B)
            {
                AddTower(FloorTiles.DonutS, 0x14);
            }
            else if (index is >= 0x2D and <= 0x34)
            {
                AddTower(FloorTiles.DonutSIn, 0x2D);
            }
        }
        else if (state is 0x00800040u or 0x80000040u && ++envccounter == 4)
        {
            var towers = CollectionsMarshal.AsSpan(Towers);
            var len = towers.Length;
            Span<bool> activeTiles = stackalloc bool[16];
            Span<bool> visited = stackalloc bool[16];
            Span<(int ring, int idx)> stack = stackalloc (int, int)[16];
            var angle = 22.5001f.Degrees(); // small epsilon to prevent wedges not merging correctly due to floating point rounding in some cases
            var pos = Arena.Center;

            for (var i = 0; i < 8; ++i)
            {
                activeTiles[i] = _tiles.InnerActiveTiles[i];
                activeTiles[i + 8] = _tiles.OuterActiveTiles[i];
            }

            for (var t = 0; t < len; ++t)
            {
                var tower = towers[t];
                var tileIndex = (int)((157.5f - tower.Rotation.Deg + 360f) % 360f / 45f);
                List<DonutSegmentV> shapes = [];
                visited.Clear();
                stack.Clear();

                var stackPtr = 0;

                // Try both rings (inner and outer tile with that index)
                for (var ring = 0; ring <= 1; ++ring)
                {
                    var id = ring == 0 ? tileIndex : tileIndex + 8;
                    if (!activeTiles[id])
                    {
                        continue;
                    }

                    stackPtr = 0;
                    stack[stackPtr++] = (ring, tileIndex);

                    while (stackPtr > 0)
                    {
                        var (r, j) = stack[--stackPtr];
                        var tid = r == 0 ? j : j + 8;
                        if (visited[tid])
                        {
                            continue;
                        }
                        if (!activeTiles[tid])
                        {
                            continue;
                        }

                        visited[tid] = true;

                        var shape = r == 0
                            ? new DonutSegmentV(pos, 2f, 8f, FloorTiles.TileAngles[j], angle, 8)
                            : new DonutSegmentV(pos, 8f, 16f, FloorTiles.TileAngles[j], angle, 8);

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
                        var oid = otherRing == 0 ? j : j + 8;
                        if (!visited[oid] && activeTiles[oid])
                        {
                            stack[stackPtr++] = (otherRing, j);
                        }
                    }

                    break; // only flood from one valid ring
                }

                if (shapes.Count > 0)
                {
                    ref var tow = ref towers[t];
                    var shape = new AOEShapeCustom(pos, [.. shapes]);
                    tow.Shape = shape;
                    tow.ShapeDistance = shape.Distance(pos, default);
                    tow.InvertedShapeDistance = shape.InvertedDistance(pos, default);
                }
            }
        }
    }
}

namespace BossMod.Dawntrail.Criterion.C01AMT.C013PariOfPlenty;

class GridMap
{
    public WPos Center;
    public int GridDimension;
    public float CellSize;
    private readonly float TotalSize;
    public float HalfSize;

    public enum TileType { E = 0, B = 1, R = 2 }
    public TileType[] tiles;

    public GridMap(int gridDimension, float cellSize, WPos center)
    {
        Center = center;
        GridDimension = gridDimension;
        CellSize = cellSize;
        TotalSize = gridDimension * cellSize;
        HalfSize = TotalSize / 2f;
        tiles = [.. Enumerable.Repeat(TileType.E, gridDimension * gridDimension)];
    }

    public WPos GridToWorld(int gridCell)
    {
        var startX = Center.X - HalfSize + CellSize * 0.5f;
        var startZ = Center.Z - HalfSize + CellSize * 0.5f;

        var x = startX + gridCell % GridDimension * CellSize;
        var z = startZ + gridCell / GridDimension * CellSize;

        return new WPos(x, z);
    }

    public int WorldToGrid(WPos worldPos)
    {
        var startX = Center.X - HalfSize + CellSize * 0.5f;
        var startZ = Center.Z - HalfSize + CellSize * 0.5f;

        var relX = worldPos.X - startX;
        var relZ = worldPos.Z - startZ;

        var x = (int)MathF.Floor(relX / CellSize);
        var z = (int)MathF.Floor(relZ / CellSize);

        x = Math.Clamp(x, 0, GridDimension - 1);
        z = Math.Clamp(z, 0, GridDimension - 1);
        return z * GridDimension + x;
    }

    public bool GridCellIntersectsShape(AOEShape shape, WPos pos, Angle angle, int gridCell)
    {
        var center = GridToWorld(gridCell);

        if (shape.Check(pos, center, angle))
        {
            return true;
        }
        var halfcell = CellSize * 0.5f;
        var centerX = center.X;
        var centerZ = center.Z;
        var corners = new[] {
            new WPos(centerX - halfcell, centerZ - halfcell),
            new WPos(centerX - halfcell, centerZ + halfcell),
            new WPos(centerX + halfcell, centerZ - halfcell),
            new WPos(centerX + halfcell, centerZ + halfcell)
        };

        foreach (var corner in corners)
        {
            if (shape.Check(corner, pos, angle))
            {
                return true;
            }
        }

        return false;
    }

    public void MarkTiles(AOEShape shape, WPos origin, Angle angle, TileType t)
    {
        for (var i = 0; i < GridDimension * GridDimension; ++i)
        {
            if (GridCellIntersectsShape(shape, origin, angle, i))
            {
                if (t == TileType.R)
                {
                    tiles[i] = TileType.R; // Red overrides everything for easier detecting of L shape
                }
                else if (t == TileType.B && tiles[i] != TileType.R)
                {
                    tiles[i] = TileType.B;
                }
            }
        }
    }

    public List<int[]> FindSafeSpotsFromRedIndices()
    {
        var results = new List<int[]>();
        var seen = new HashSet<(int, int, int)>();

        for (var y = 0; y < GridDimension; ++y)
        {
            for (var x = 0; x < GridDimension; ++x)
            {
                int[][] orientations = [
                    [y * GridDimension + x, y * GridDimension + x + 1, y * GridDimension + x + 2, (y + 1) * GridDimension + x, (y + 2) * GridDimension + x], // right & down
                    [y * GridDimension + x, y * GridDimension + x + 1, y * GridDimension + x + 2, (y - 1) * GridDimension + x, (y - 2) * GridDimension + x], // right & up
                    [y * GridDimension + x, y * GridDimension + x - 1, y * GridDimension + x - 2, (y + 1) * GridDimension + x, (y + 2) * GridDimension + x], // left & down
                    [y * GridDimension + x, y * GridDimension + x - 1, y * GridDimension + x - 2, (y - 1) * GridDimension + x, (y - 2) * GridDimension + x]  // left & up
                ];

                foreach (var shapeL in orientations)
                {
                    if (shapeL.Any(id => id < 0 || id >= GridDimension * GridDimension))
                    {
                        continue;
                    }

                    int p0 = shapeL[0], p1 = shapeL[1], p2 = shapeL[2], p3 = shapeL[3], p4 = shapeL[4];

                    // Looking for L shape where there is a red tile between each other tile
                    if (tiles[p1] != TileType.R || tiles[p3] != TileType.R)
                    {
                        continue;
                    }

                    int blueTiles = 0, emptyTiles = 0;

                    // Other points must be either empty or blue resulting in 2 blue & 1 empty
                    if (tiles[p0] == TileType.B)
                    {
                        ++blueTiles;
                    }
                    if (tiles[p2] == TileType.B)
                    {
                        ++blueTiles;
                    }
                    if (tiles[p4] == TileType.B)
                    {
                        ++blueTiles;
                    }

                    if (tiles[p0] == TileType.E)
                    {
                        ++emptyTiles;
                    }
                    if (tiles[p2] == TileType.E)
                    {
                        ++emptyTiles;
                    }
                    if (tiles[p4] == TileType.E)
                    {
                        ++emptyTiles;
                    }

                    if (blueTiles != 2 || emptyTiles != 1)
                    {
                        continue;
                    }

                    int empty = -1, blue1 = -1, blue2 = -1;
                    foreach (var i in new[] { p0, p2, p4 })
                    {
                        if (tiles[i] == TileType.E)
                        {
                            empty = i;
                        }

                        if (tiles[i] == TileType.B)
                        {
                            (blue1 == -1 ? ref blue1 : ref blue2) = i;
                        }
                    }

                    // Sort the order to avoid dupes
                    var minB = Math.Min(blue1, blue2);
                    var maxB = Math.Max(blue1, blue2);

                    if (seen.Add((minB, maxB, empty)))
                    {
                        results.Add([blue1, blue2, empty]);
                    }
                }
            }
        }

        return results;
    }
}

class ParisCurse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];
    private Actor? blueCrystal;
    private Actor? blueCrystalTracker; // Carpet that covers the blueCrystal
    private float blueCrystalTrackerDistance = float.MaxValue;
    private readonly List<Actor> redCrystals = [];

    GridMap? gridMap = null;
    private enum SafeHalf { Unknown, North, South }
    private SafeHalf safeHalf = SafeHalf.Unknown;
    private List<int[]> safeTiles = [];

    private int? assignment;

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChillingGleam)
        {
            NumCasts++;
        }

        if ((AID)spell.Action.ID == AID.CharmingBaubles)
        {
            var actor = Module.WorldState.Actors.FirstOrDefault(a => a.OID == (uint)OID.IcyBauble);
            if (actor != null)
            {
                blueCrystal = actor;
            }
        }

        if ((AID)spell.Action.ID == AID.CarpetTeleport)
        {
            if (blueCrystal == null)
            {
                return;
            }

            var difference = caster.Position - blueCrystal.Position;
            var distance = difference.Length();

            if (distance < blueCrystalTrackerDistance)
            {
                blueCrystalTrackerDistance = distance;
                blueCrystalTracker = caster;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ParisCurse)
        {
            gridMap = new GridMap(4, 10, Module.Center);
        }

        if ((AID)spell.Action.ID == AID.Unravel)
        {
            safeTiles = GenerateSafeSpots();
            assignment = ResolvePlayerAssignment(safeTiles);
        }

        if ((AID)spell.Action.ID == AID.BurningGleam)
        {
            redCrystals.Add(caster);
        }

        if ((AID)spell.Action.ID == AID.CurseFableflightRight)
        {
            safeHalf = SafeHalf.North;
        }

        if ((AID)spell.Action.ID == AID.CurseFableflightLeft)
        {
            safeHalf = SafeHalf.South;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc); // TODO is this needed anymore?

        if (gridMap == null)
        {
            return;
        }

        var startX = gridMap.Center.X - gridMap.HalfSize + gridMap.CellSize / 2f;
        var startZ = gridMap.Center.Z - gridMap.HalfSize + gridMap.CellSize / 2f;

        for (var z = 0; z < gridMap.GridDimension; ++z)
        {
            for (var x = 0; x < gridMap.GridDimension; ++x)
            {
                WPos pos = new WPos(startX + x * gridMap.CellSize, startZ + z * gridMap.CellSize);
                Arena.AddRect(pos, new WDir(1, 0), gridMap.CellSize / 2f, gridMap.CellSize / 2f, gridMap.CellSize / 2f, 0xffffffff, 2);
            }
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        aoes.Clear();

        if (gridMap == null)
        {
            return CollectionsMarshal.AsSpan(aoes);
        }

        AOEShapeRect cellShape = new(gridMap.CellSize / 2f, gridMap.CellSize / 2f, gridMap.CellSize / 2f);

        if (assignment.HasValue)
        {
            aoes.Add(new AOEInstance(cellShape, gridMap.GridToWorld(assignment.Value), default, WorldState.CurrentTime, Colors.SafeFromAOE, false));
            return CollectionsMarshal.AsSpan(aoes);
        }

        if (safeTiles.Count > 0)
        {
            foreach (var tile in safeTiles[0])
            {
                var colour = gridMap.tiles[tile] == GridMap.TileType.B ? Color.FromRGBA(0x268BD280).ABGR : Color.FromRGBA(0xC0C0C080).ABGR;
                aoes.Add(new AOEInstance(cellShape, gridMap.GridToWorld(tile), default, WorldState.CurrentTime, colour, false));
            }
        }

        return CollectionsMarshal.AsSpan(aoes);
    }

    private List<int[]> GenerateSafeSpots()
    {
        if (gridMap == null || blueCrystalTracker == null)
        {
            return [];
        }

        gridMap.MarkTiles(new AOEShapeCross(40, 4), blueCrystalTracker.Position, default, GridMap.TileType.B);
        foreach (var actor in redCrystals)
        {
            gridMap.MarkTiles(new AOEShapeCross(40, 4), actor.Position, default, GridMap.TileType.R);
        }

        var safeSpots = gridMap.FindSafeSpotsFromRedIndices();
        var candidates = safeSpots;

        if (candidates.Count > 0)
        {
            var allowed = safeHalf == SafeHalf.North ? [0, 1, 2, 3, 4, 5, 6, 8, 9, 12] : new[] { 3, 6, 7, 9, 10, 11, 12, 13, 14, 15 };
            candidates = [.. safeSpots.Where(tile => allowed.Contains(tile[0]) && allowed.Contains(tile[1]) && allowed.Contains(tile[2]))];
        }

        if (safeSpots.Count == 0)
        {
            return [];
        }

        return candidates;
    }

    // To check if each player in the group has a unique role
    private bool CheckAssignmentRoles(PartyState partyState)
    {
        var roles = Service.Config.Get<PartyRolesConfig>().EffectiveRolePerSlot(partyState);
        var tankRole = false;
        var healerRole = false;
        var meleeRole = false;
        var rangedRole = false;

        for (var i = 0; i < PartyState.MaxPartySize; i++)
        {
            switch (roles[i])
            {
                case Role.Tank:
                    tankRole = true;
                    break;
                case Role.Healer:
                    healerRole = true;
                    break;
                case Role.Melee:
                    meleeRole = true;
                    break;
                case Role.Ranged:
                    rangedRole = true;
                    break;
            }
        }

        if (!tankRole || !healerRole || !meleeRole || !rangedRole)
        {
            Service.Log("Missing unique roles - each player must have a unique role!");
            return false;
        }

        return true;
    }

    // Group assignment
    static int RolePriority(Role r) => r switch
    {
        Role.Tank => 0,
        Role.Melee => 1,
        Role.Ranged => 2,
        Role.Healer => 3,
        _ => -1
    };

    // Sorts the safe spots into north relative order
    private int[] SortSafeSpotsOrder(List<int[]> safeSpots)
    {
        foreach (var s in safeSpots)
        {
            var tiles = new[] { s[0], s[1], s[2] };
            var northNumber = (safeHalf == SafeHalf.South) ? tiles.Min() : tiles.Max(); // Depending on the safe side where which tile we should treat as new north
            var otherTiles = tiles.Where(t => t != northNumber).ToArray();

            // Depending on the safe side depends on the second tile -> min is north safe, max is south safe
            var secondTile = (safeHalf == SafeHalf.North) ? Math.Min(otherTiles[0], otherTiles[1]) : Math.Max(otherTiles[0], otherTiles[1]);
            var thirdTile = (otherTiles[0] == secondTile) ? otherTiles[1] : otherTiles[0]; // Remaining tile
            var ordered = new[] { northNumber, secondTile, thirdTile };
            return ordered;
        }

        return [];
    }

    bool SpreadPlayer(Actor player) => player.FindStatus((uint)SID.CurseOfSolitude) != null;
    bool StackPlayer(Actor player) => player.FindStatus((uint)SID.CurseOfCompanionship) != null;
    bool FirePlayer(Actor player) => player.FindStatus((uint)SID.CurseOfImmolation) != null;

    private int? ResolvePlayerAssignment(List<int[]> safeSpots)
    {
        if (gridMap == null)
        {
            return null;
        }

        if (!CheckAssignmentRoles(Raid))
        {
            return null;
        }

        // Get safe spots
        var emptyTile = 0;
        var blueTiles = new List<int>();

        var safeSpotsOrder = SortSafeSpotsOrder(safeSpots);
        foreach (var safeSpot in safeSpotsOrder)
        {
            if (gridMap.tiles[safeSpot] == GridMap.TileType.E)
            {
                emptyTile = safeSpot;
            }

            if (gridMap.tiles[safeSpot] == GridMap.TileType.B)
            {
                blueTiles.Add(safeSpot);
            }
        }

        // Get party and their debuffs
        var party = WorldState.Party.WithoutSlot();
        var roles = Service.Config.Get<PartyRolesConfig>().EffectiveRolePerSlot(Raid);

        var players = party.Select(p => new
        {
            Actor = p,
            Fire = FirePlayer(p),
            Stack = StackPlayer(p),
            Spread = SpreadPlayer(p),
            Priority = RolePriority(roles[Raid.FindSlot(p.InstanceID)])
        }).ToList();

        var actor = WorldState.Party.Player();
        if (actor == null)
        {
            return null;
        }
        var player = players.First(p => p.Actor.InstanceID == actor.InstanceID);

        // No fire debuff -> assign to empty tile
        if (!player.Fire)
        {
            return emptyTile;
        }

        // Fire debuff -> assign to one of the blue tiles
        if (player.Fire)
        {
            var firePlayers = players.Where(p => p.Fire).OrderBy(p => p.Priority).ToList();

            // Case 1: There are only 2 fire debuff players -> means both of them are spreads as well -> normal TMRH priority
            if (firePlayers.Count == 2)
            {
                var tileNumber = firePlayers.FindIndex(p => p.Actor.InstanceID == player.Actor.InstanceID);
                return blueTiles[tileNumber];
            }

            // Case 2: There are 3 fire debuff players -> means on the pair is stack/nothing and the other is a spread
            // Stack + nothing takes the first blue tile & spread takes the second blue tile
            if (firePlayers.Count == 3)
            {
                var spread = firePlayers.First(p => p.Spread);
                return player.Actor.InstanceID == spread.Actor.InstanceID ? blueTiles[1] : blueTiles[0];
            }
        }

        return null;
    }
}

class Fableflight(BossModule module) : Fireflight(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is var id && id == (uint)AID.CurseFableflightLeft)
        {
            side = -1;
        }
        else if (id == (uint)AID.CurseFableflightRight)
        {
            side = 1;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether) { }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.CarpetRideTether)
        {
            PathAOE pathAoe = new PathAOE();
            var actor = WorldState.Actors.Find(tether.Target);

            if (actor == null)
            {
                return;
            }

            pathAoe.actor = actor;
            pathAoe.startPosition = source.Position;
            pathAoe.endPosition = actor.Position;
            pathAoe.aoePosition = pathAOEs.Count + 1;
            pathAOEs.Add(pathAoe);
        }
    }

    // This function is needed for this version as "OnUntethered" doesn't happen until the AOE resolves, which is too late
    // and OnTethered is normally too early before the actor moves giving the wrong position
    public override void Update()
    {
        foreach (var pathAOE in pathAOEs)
        {
            var target = pathAOE.actor;
            if (target == null)
            {
                continue;
            }

            var pos = target.Position;
            if (!pathAOE.endPosition.Equals(pos))
            {
                pathAOE.endPosition = pos;
            }
        }
    }
}
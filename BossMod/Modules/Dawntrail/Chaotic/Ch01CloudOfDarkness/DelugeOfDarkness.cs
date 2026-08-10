namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

// envcontrols:
// 00 = main bounds telegraph
// - 00200010 - phase 1
// - 00020001 - phase 2
// - 00040004 - remove telegraph (note that actual bounds are controlled by something else!)
sealed class Phase2InnerCells(BossModule module) : Components.GenericAOEs(module)
{
    private readonly Ch01CloudOfDarknessConfig _config = Service.Config.Get<Ch01CloudOfDarknessConfig>();
    private readonly DateTime[] _breakTime = new DateTime[28];
    private static readonly AOEShapeRect square = new(3f, 3f, 3f);
    private static readonly Dictionary<int, (int x, int y)> _cellIndexToCoordinates = GenerateCellIndexToCoordinates();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_config.ShowOccupiedTiles)
            return [];
        var cell = CellIndex(actor.Position - Arena.Center) - 3;
        var tiles = new AOEInstance[28];
        var index = 0;
        for (var i = 0; i < 28; ++i)
        {
            ref var breaktime = ref _breakTime[i];
            if (breaktime != default)
            {
                if (i == cell)
                {
                    if ((breaktime - WorldState.CurrentTime).TotalSeconds < 6d)
                    {
                        tiles[index++] = new(square, CellCenter(i));
                    }
                }
                else
                    tiles[index++] = new(square, CellCenter(i), color: Colors.FutureVulnerable);
            }
        }
        return tiles.AsSpan()[..index];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var cell = CellIndex(actor.Position - Arena.Center) - 3;
        var breakTime = cell >= 0 && cell < _breakTime.Length ? _breakTime[cell] : default;
        if (breakTime != default)
        {
            var remaining = Math.Max(0d, (breakTime - WorldState.CurrentTime).TotalSeconds);
            hints.Add($"Cell breaks in {remaining:f1}s", remaining < 10d);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        // 03-1E = mid squares
        // - 08000001 - init
        // - 00200010 - become occupied
        // - 02000001 - become free
        // - 00800040 - player is standing for too long (38s), will break soon (in 6s)
        // - 00080004 - break
        // - 00020001 - repair
        // - arrangement:
        //      04             0B
        //   03 05 06 07 0E 0D 0C 0A
        //      08             0F
        //      09             10
        //      17             1E
        //      16             1D
        //   11 13 14 15 1C 1B 1A 18
        //      12             19
        if (index is < 0x03 or > 0x1E)
            return;
        _breakTime[index - 3] = state switch
        {
            0x00200010u => WorldState.FutureTime(44d),
            0x00800040u => WorldState.FutureTime(6d),
            0x00080004u => WorldState.CurrentTime,
            _ => default,
        };
    }

    private static int CoordinateToCell(float c) => (int)Math.Floor(c / 6);
    private static int CellIndex(WDir offset) => CellIndex(CoordinateToCell(offset.X), CoordinateToCell(offset.Z));
    private static int CellIndex(int x, int y) => (x, y) switch
    {
        (-4, -3) => 0x03,
        (-3, -4) => 0x04,
        (-3, -3) => 0x05,
        (-2, -3) => 0x06,
        (-1, -3) => 0x07,
        (-3, -2) => 0x08,
        (-3, -1) => 0x09,
        (+3, -3) => 0x0A,
        (+2, -4) => 0x0B,
        (+2, -3) => 0x0C,
        (+1, -3) => 0x0D,
        (+0, -3) => 0x0E,
        (+2, -2) => 0x0F,
        (+2, -1) => 0x10,
        (-4, +2) => 0x11,
        (-3, +3) => 0x12,
        (-3, +2) => 0x13,
        (-2, +2) => 0x14,
        (-1, +2) => 0x15,
        (-3, +1) => 0x16,
        (-3, +0) => 0x17,
        (+3, +2) => 0x18,
        (+2, +3) => 0x19,
        (+2, +2) => 0x1A,
        (+1, +2) => 0x1B,
        (+0, +2) => 0x1C,
        (+2, +1) => 0x1D,
        (+2, +0) => 0x1E,
        _ => 0
    };

    private static Dictionary<int, (int x, int y)> GenerateCellIndexToCoordinates()
    {
        var map = new Dictionary<int, (int x, int y)>();
        for (var x = -4; x <= 3; ++x)
        {
            for (var y = -4; y <= 3; ++y)
            {
                var index = CellIndex(x, y);
                if (index >= 0)
                    map[index] = (x, y);
            }
        }
        return map;
    }

    public static WPos CellCenter(int breakTimeIndex)
    {
        var cellIndex = breakTimeIndex + 3;
        if (_cellIndexToCoordinates.TryGetValue(cellIndex, out var coordinates))
        {
            var worldX = (coordinates.x + 0.5f) * 6f;
            var worldZ = (coordinates.y + 0.5f) * 6f;
            return new WPos(100f, 100f) + new WDir(worldX, worldZ);
        }
        else
            return default;
    }
}

sealed class Phase2AIHints(BossModule module) : BossComponent(module)
{
    private BitMask isInside;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            var e = hints.PotentialTargets[i];
            switch (e.Actor.OID)
            {
                case (uint)OID.Atomos:
                    if (isInside[slot])
                        e.Priority = AIHints.Enemy.PriorityInvincible;
                    else if (actor.Class.GetRole() == Role.Ranged)
                        e.Priority = 5;
                    break;
                case (uint)OID.StygianShadow:
                    if (isInside[slot])
                        e.Priority = AIHints.Enemy.PriorityInvincible;
                    break;
                case (uint)OID.Boss:
                    if (!isInside[slot])
                        e.Priority = AIHints.Enemy.PriorityInvincible;
                    break;

            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.InnerDarkness:
                isInside.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case (uint)SID.OuterDarkness:
                isInside.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }
}

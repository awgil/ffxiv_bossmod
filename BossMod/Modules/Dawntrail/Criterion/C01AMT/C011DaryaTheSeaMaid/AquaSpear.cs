namespace BossMod.Dawntrail.Criterion.C01AMT.C011DaryaTheSeaMaid;

// TODO rewrite this class -> there is a better way to do tiles rather than making your own grid map
class AquaSpear(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> aoes = [];

    private const float cellSize = 8f;
    private const int gridSize = 5;
    private readonly AOEShapeRect cellShape = new(4f, 4f, 4f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(aoes);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.WaterZone1)
        {
            var idx = PositionToIndex(caster.Position);
            var pos = IndexToPosition(idx);
            aoes.Add(new AOEInstance(cellShape, pos, default, WorldState.CurrentTime, Colors.Danger));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.WaterZone3 or (uint)AID.WaterZone2)
        {
            var idx = PositionToIndex(caster.Position);
            var pos = IndexToPosition(idx);
            aoes.Add(new AOEInstance(cellShape, pos, default, WorldState.CurrentTime, Colors.Danger));
            NumCasts++;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);

        var center = Arena.Center;

        const float total = gridSize * cellSize;
        const float half = total * 0.5f;
        const float halfcell = cellSize * 0.5f;
        var startX = center.X - half + halfcell;
        var startZ = center.Z - half + halfcell;
        const uint white = 0xFFFFFFFF;

        var dir = new WDir(1, 0);

        for (var iz = 0; iz < gridSize; ++iz)
        {
            for (var ix = 0; ix < gridSize; ++ix)
            {
                var pos = new WPos(startX + ix * cellSize, startZ + iz * cellSize);
                Arena.AddRect(pos, dir, halfcell, halfcell, halfcell, white, 2f);
            }
        }
    }

    private WPos IndexToPosition(int index)
    {
        const float halftotal = gridSize * cellSize * 0.5f;
        const float halfcell = cellSize * 0.5f;
        var startX = Arena.Center.X - halftotal + halfcell;
        var startZ = Arena.Center.Z - halftotal + halfcell;
        var x = startX + index % gridSize * cellSize;
        var z = startZ + index / gridSize * cellSize;
        return new WPos(x, z);
    }

    private int PositionToIndex(WPos pos)
    {
        const float halftotal = gridSize * cellSize * 0.5f;
        const float halfcell = cellSize * 0.5f;
        const float invCellSize = 1f / cellSize;

        var startX = Arena.Center.X - halftotal + halfcell;
        var startZ = Arena.Center.Z - halftotal + halfcell;
        var relX = pos.X - startX;
        var relZ = pos.Z - startZ;

        var x = (int)MathF.Round(relX * invCellSize);
        var z = (int)MathF.Round(relZ * invCellSize);

        x = Math.Clamp(x, 0, gridSize - 1);
        z = Math.Clamp(z, 0, gridSize - 1);
        return z * gridSize + x;
    }
}
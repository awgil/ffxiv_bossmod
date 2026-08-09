namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private readonly AOEShapeDonut donut = new(34f, 40f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    private static Square[] GetDefaultShape() => [new(new(100f, 100f), 40f)];
    private static Shape[] GetP2ShapeNoDonut() => [new Rectangle(new(100f, 115f), 24f, 3f), new Rectangle(new(100f, 85f), 24f, 3f), new Rectangle(new(115f, 100f), 3f, 24f),
    new Rectangle(new(85f, 100f), 3f, 24f), new Square(new(126.5f, 100f), 7.5f), new Square(new(73.5f, 100f), 7.5f)];
    private static DonutV[] GetP2Donut() => [new(new(100f, 100f), 34f, 40f, 80)];
    private static PolygonCustom[] GetDiamondShape() => [new([new(115f, 63f), new(128.28427f, 76.28427f), new(100f, 104.56854f), new(71.71573f, 76.28427f), new(85f, 63f)])];

    private static Square[] GenerateIntersectionBlockers() // at intersections there are small blockers to prevent players from skipping tiles
    {
        var a45 = 45f.Degrees();
        var a135 = 135f.Degrees();
        WDir[] dirs = [a45.ToDirection(), a135.ToDirection(), (-a45).ToDirection(), (-a135).ToDirection()];
        WPos[] pos = [new(85f, 85f), new(115f, 85f), new(115f, 115f), new(85f, 115f)];
        var distance = 3f * MathF.Sqrt(2);

        var squares = new Square[16];
        var index = 0;
        for (var i = 0; i < 4; ++i)
        {
            for (var j = 0; j < 4; ++j)
            {
                squares[index++] = new(pos[i] + distance * dirs[j], 1f, a45);
            }
        }
        return squares;
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00)
        {
            switch (state)
            {
                case 0x00200010u:
                    SetAOE(new AOEShapeCustom(Arena.Center, GetDefaultShape(), GetDiamondShape()));
                    break;
                case 0x00020001u:
                    SetAOE(new AOEShapeCustom(Arena.Center, GetDefaultShape(), [.. GetP2ShapeNoDonut(), .. GetP2Donut()]));
                    break;
            }
        }
        else if (index == 0x02)
        {
            switch (state)
            {
                case 0x00020001u:
                    SetArena(new(GetP2ShapeNoDonut(), GenerateIntersectionBlockers()));
                    break;
                case 0x00080004u:
                    SetArena(new([.. GetP2Donut(), .. GetP2ShapeNoDonut()], GenerateIntersectionBlockers()));
                    break;
            }
        }
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID != 0x8000000D)
        {
            return;
        }
        switch (param1)
        {
            case 0x10000000u: // default arena
                Arena.Bounds = new ArenaBoundsCircle(40f);
                Arena.Center = new(100f, 100f);
                break;
            case 0x20000000u: // (phase 2)
                SetArena(new([.. GetP2ShapeNoDonut(), .. GetP2Donut()], GenerateIntersectionBlockers()));
                break;
            case 0x40000000u: // diamond arena (phase 1)
                SetArena(new ArenaBoundsCustom(GetDiamondShape(), ScaleFactor: 1.414f));
                break;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarkDominion)
        {
            SetAOE(donut);
        }
    }

    private void SetArena(ArenaBoundsCustom bounds)
    {
        Arena.Bounds = bounds;
        Arena.Center = bounds.Center;
        _aoe = [];
    }

    private void SetAOE(AOEShape shape)
    {
        var pos = Arena.Center;
        _aoe = [new(shape, pos, default, WorldState.FutureTime(9d), shapeDistance: shape.Distance(pos, default))];
    }
}

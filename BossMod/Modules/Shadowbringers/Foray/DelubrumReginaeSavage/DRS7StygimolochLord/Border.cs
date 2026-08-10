namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS7StygimolochLord;

sealed class Border(BossModule module) : Components.GenericAOEs(module)
{
    private static Shape[] GetLabyrinthDifference()
    {
        var center = new WPos(-416f, -184f);
        return [new DonutV(center, 30f, 34.5f, 48), new DonutV(center, 17f, 25f, 48), new Polygon(center, 12f, 48)];
    }
    private static Rectangle[] GetLabyrinthUnion()
    {
        return [.. GenerateAlcoves(new(-416f, -211.5f)), .. GenerateAlcoves(WPos.RotateAroundOrigin(22.5f, new(-416f, -184f), new(-416f, -198.5f)), -22.5f.Degrees())];
    }

    private AOEInstance[]? _aoe;
    private void InitAOE()
    {
        var center = Arena.Center;
        var shape = new AOEShapeCustom(center, GetLabyrinthDifference(), GetLabyrinthUnion());
        _aoe = [new(shape, center, default, Module.WorldState.FutureTime(5d), shapeDistance: shape.Distance(center, default))];
    }

    private static Rectangle[] GenerateAlcoves(WPos basePosition, Angle start = default)
    {
        var a45 = -45f.Degrees();
        var center = new WPos(-416f, -184f);
        var rects = new Rectangle[8];
        rects[0] = new(basePosition, 2f, 4f, start);

        for (var i = 1; i < 8; ++i)
        {
            rects[i] = new(WPos.RotateAroundOrigin(i * 45f, center, basePosition), 2f, 4f, start + a45 * i);
        }
        return rects;
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe == null)
        {
            InitAOE();
        }
        return _aoe;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.MemoryOfTheLabyrinth)
        {
            _aoe = [];
            var arena = new ArenaBoundsCustom([new Polygon(new(-416f, -184f), 34.5f, 48)], GetLabyrinthDifference(), GetLabyrinthUnion());
            Arena.Bounds = arena;
            Arena.Center = arena.Center;
        }
    }
}

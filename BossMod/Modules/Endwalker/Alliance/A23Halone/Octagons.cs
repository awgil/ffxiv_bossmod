namespace BossMod.Endwalker.Alliance.A23Halone;

// TODO: assign alliances members to a specific octagon. in duty finder it is usually:
// NW (Octagon3): Alliance A
// NE (Octagon1): Alliance C
// S (Octagon2): Alliance B
class Octagons(BossModule module) : Components.GenericAOEs(module)
{
    private const float InnerRadius = 11.125f; // radii adjusted for hitbox radius
    private const float OuterRadius = 13.45f;
    private const int Vertices = 8;

    private static Polygon[] GetPolygonsShapes()
    {
        WPos[] spears = [new(-686f, 592f), new(-700f, 616.2f), new(-714f, 592f)];
        Angle[] angle = [-37.5f.Degrees(), 22.5f.Degrees(), 37.5f.Degrees()];
        return [new(spears[0], InnerRadius, Vertices, angle[0]),
            new(spears[0], OuterRadius, Vertices, angle[0]), new(spears[1], InnerRadius, Vertices, angle[1]),
            new(spears[1], OuterRadius, Vertices, angle[1]), new(spears[2], InnerRadius, Vertices, angle[2]),
            new(spears[2], OuterRadius, Vertices, angle[2])];
    }

    private List<Polygon> octagonsInner = [], octagonsOuter = [];

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        //x07 = south, x06 = east, x05 = west x00020001 walls activate, x00200004 disappear
        // telegraph - 0x00100008
        var update = false;
        switch (state)
        {
            case 0x00100008u when index == 0x07:
                var center = Arena.Center;
                var shape = new AOEShapeCustom(center, [new Square(new(-700f, 600f), 29.5f)], octagonsInner); // using a square should be less cpu intensive, gets clipped with arena border anyway
                _aoe = [new(shape, center, default, WorldState.FutureTime(9d), shapeDistance: shape.Distance(center, default))];
                break;
            case 0x00020001u when index == 0x07:
                update = true;
                var polys = GetPolygonsShapes();
                octagonsInner = [polys[0], polys[2], polys[4]];
                octagonsOuter = [polys[1], polys[3], polys[5]];
                _aoe = [];
                break;
            case 0x00200004u:
                RemoveOctagons(index);
                update = true;
                break;
        }
        if (update)
        {
            Arena.Bounds = new ArenaBoundsCustom([new Circle(new(-700f, 600f), 29.5f)], [.. octagonsOuter], [.. octagonsInner]);
        }
    }

    private void RemoveOctagons(byte index)
    {
        switch (index)
        {
            case 0x06:
                RemoveAll(new(-686f, 592f));
                break;
            case 0x07:
                RemoveAll(new(-700f, 616.2f));
                break;
            case 0x05:
                RemoveAll(new(-714f, 592f));
                break;
        }
        void RemoveAll(WPos center)
        {
            var count = octagonsInner.Count;
            for (var i = 0; i < count; ++i)
            {
                if (octagonsInner[i].Center == center)
                {
                    octagonsInner.RemoveAt(i);
                    octagonsOuter.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

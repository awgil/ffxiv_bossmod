namespace BossMod.Endwalker.Alliance.A31Thaliak;

sealed class TetraktysBorder(BossModule module) : Components.GenericAOEs(module)
{
    private static Polygon[] GetTriangle() => [new(new(-945, 948.71267f), 27.71281f, 3, 180f.Degrees())];
    private AOEInstance[] _aoe = [];
    public bool Active;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x04)
        {
            switch (state)
            {
                case 0x00200010u:
                    var center = Arena.Center;
                    _aoe = [new(new AOEShapeCustom(center, [new Square(center, 24f)], GetTriangle()), center, default, WorldState.FutureTime(6.5d))];
                    break;
                case 0x00020001u:
                    _aoe = [];
                    var arena = new ArenaBoundsCustom(GetTriangle());
                    Arena.Bounds = arena;
                    Arena.Center = arena.Center;
                    Active = true;
                    break;
                case 0x00080004u:
                    Arena.Bounds = new ArenaBoundsSquare(24f);
                    Arena.Center = new(-945f, 945f);
                    Active = false;
                    break;
            }
        }
    }
}

sealed class Tetraktys(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeTriCone _triSmall = new(16f, 30f.Degrees());
    private static readonly AOEShapeTriCone _triLarge = new(32f, 30f.Degrees());
    private static readonly Angle _rot1 = -0.003f.Degrees();
    private static readonly Angle _rot2 = -180.Degrees();
    private static readonly Angle _rot3 = 179.995f.Degrees();

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnMapEffect(byte index, uint state)
    {
        // 0x00020001 - small telegraph, 0x00080004 - small telegraph disappears, right after cast with bigger telegraph starts
        // indices are then arranged in natural order; all 'tricone' directions are either 0 or 180 (for small triangles only):
        // small        large
        //   5            E
        //  678          F 10
        // 9ABCD
        void AddAOEs(WPos[] positions, Angle[] rotations)
        {
            for (var i = 0; i < 3; ++i)
            {
                AddAOE(_triSmall, positions[i], rotations[i]);
            }
        }
        void AddAOE(AOEShapeTriCone shape, WPos pos, Angle rot) => AOEs.Add(new(shape, pos.Quantized(), rot, WorldState.FutureTime(3.8d)));

        if (state == 0x00020001)
        {
            switch (index)
            {
                case 0x05: // 05, 08, 0B always activate together
                    AddAOEs([new(-945f, 948.5f), new(-937f, 934.644f), new(-945f, 921f)], [_rot1, _rot1, _rot1]);
                    break;
                case 0x06: // 06, 09, 0C always activate together
                    AddAOEs([new(-937f, 962.356f), new(-961f, 948.5f), new(-953f, 934.644f)], [_rot3, _rot1, _rot1]);
                    break;
                case 0x07: // 07, 0A, 0D always activate together
                    AddAOEs([new(-929f, 948.5f), new(-953f, 962.356f), new(-945f, 948.5f)], [_rot1, _rot2, _rot2]);
                    break;
                case 0x0E:
                    AddAOE(_triLarge, new(-945f, 921f), _rot1);
                    break;
                case 0x0F:
                    AddAOE(_triLarge, new(-953, 934.644f), _rot1);
                    break;
                case 0x10:
                    AddAOE(_triLarge, new(-937, 934.644f), _rot1);
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TetraktysAOESmall or (uint)AID.TetraktysAOELarge)
        {
            ++NumCasts;
            if (AOEs.Count != 0)
                AOEs.RemoveAt(0);
        }
    }
}

sealed class TetraktuosKosmosCounter(BossModule module) : Components.CastCounter(module, (uint)AID.TetraktuosKosmosAOETri); // to handle tutorial of TetraktuosKosmos

sealed class TetraktuosKosmos(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [with(6)];
    private static readonly AOEShapeTriCone _shapeTri = new(16f, 30f.Degrees());
    private static readonly AOEShapeRect _shapeRect = new(30f, 8f);
    private static readonly Angle[] Angles =
    [-0.003f.Degrees(), -180f.Degrees(), 179.995f.Degrees(), 59.995f.Degrees(), -60f.Degrees(),
    119.997f.Degrees(), -120.003f.Degrees(), 60f.Degrees()];

    private static readonly (AOEShape shape, WPos pos, int angle)[] combos =
    [
        // 0x12
        (_shapeTri, new(-945f, 948.5f), 1),
        (_shapeRect, new(-945f, 935), 1),
        (_shapeRect, new(-948.827f, 941.828f), 4),
        (_shapeRect, new(-941.173f, 941.828f), 7),        
        // 0x14
        (_shapeTri, new(-953f, 962.356f), 1),
        (_shapeRect, new(-949f, 955.428f), 3),
        (_shapeRect, new(-957f, 955.428f), 4),
        (_shapeRect, new(-953f, 948.5f), 1),
        // 0x15
        (_shapeTri, new(-937f, 962.356f), 2),
        (_shapeRect, new(-937f, 948.5f), 1),
        (_shapeRect, new(-933f, 955.428f), 3),
        (_shapeRect, new(-941f, 955.428f), 4),      

        // pair 0x13 + 0x15
        (_shapeTri, new(-961f, 948.7f), 0),
        (_shapeTri, new(-937f, 962.356f), 2),
        (_shapeRect, new(-933f, 955.428f), 3),
        (_shapeRect, new(-941f, 955.428f), 4),
        (_shapeRect, new(-937f, 948.5f), 1),
        (_shapeRect, new(-957f, 955.428f), 5),

        // pair 0x12 + 0x16
        (_shapeTri, new(-945f, 948.5f), 1),
        (_shapeTri, new(-929, 948.7f), 0),
        (_shapeRect, new(-933f, 955.428f), 6),
        (_shapeRect, new(-941.173f, 941.828f), 7),
        (_shapeRect, new(-948.827f, 941.828f), 4),
        (_shapeRect, new(-945f, 935f), 1),

        // //pair 0x11 + 0x14
        (_shapeTri, new(-945f, 921f), 0),
        (_shapeTri, new(-953f, 962.356f), 1),
        (_shapeRect, new(-945f, 934.8f), 0),
        (_shapeRect, new(-953f, 948.5f), 1),
        (_shapeRect, new(-957f, 955.428f), 4),
        (_shapeRect, new(-949f, 955.428f), 3)
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void OnMapEffect(byte index, uint state)
    {
        // 0x00020001 - small telegraph, 0x00080004 - small telegraph disappears, right after cast with bigger telegraph starts
        // indices are arranged similarly to small tetraktys, however some of the triangles are forbidden
        // small
        //       11
        //    XX 12 XX
        // 13 14 XX 15 16

        if (state != 0x00020001)
            return;
        var tutorialDone = Module.FindComponent<TetraktuosKosmosCounter>()?.NumCasts != 0;

        if (!tutorialDone)
            HandleTutorial(index);
        else
            HandleRest(index);
    }

    private void HandleTutorial(byte index)
    {
        switch (index)
        {
            case 0x12:
                AddAOEs([0, 1, 2, 3]);
                break;
            case 0x14:
                AddAOEs([4, 5, 6, 7]);
                break;
            case 0x15:
                AddAOEs([8, 9, 10, 11]);
                break;
        }
    }

    private void HandleRest(byte index)
    {
        switch (index)
        {
            case 0x13:
                AddAOEs([12, 13, 14, 15, 16, 17]);
                break;
            case 0x12:
                AddAOEs([18, 19, 20, 21, 22, 23]);
                break;
            case 0x11:
                AddAOEs([24, 25, 26, 27, 28, 29]);
                break;
        }
    }

    private void AddAOEs(int[] indices)
    {
        for (var i = 0; i < indices.Length; ++i)
        {
            var (shape, pos, angle) = combos[indices[i]];
            AOEs.Add(new(shape, pos.Quantized(), Angles[angle], WorldState.FutureTime(7.9d)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.TetraktuosKosmosAOETri)
        {
            AOEs.Clear();
            ++NumCasts;
        }
    }
}

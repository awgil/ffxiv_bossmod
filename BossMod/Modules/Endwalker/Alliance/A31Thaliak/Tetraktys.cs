namespace BossMod.Endwalker.Alliance.A31Thaliak;

class TetraktysBorder(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos NormalCenter = new(-945, 945);
    public static readonly ArenaBoundsSquare NormalBounds = new(24);
    private static readonly WPos TriangleCenter = new(-945, 941.5f);
    private static readonly TriangleE triangle = new(TriangleCenter, 48);
    private static readonly Square square = new(NormalCenter, 24);
    private static readonly ArenaBoundsComplex TriangleBounds = new([triangle]);
    private static readonly AOEShapeCustom transition = new([square], [triangle]);
    private AOEInstance? _aoe;
    public bool Active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 4)
        {
            switch (state)
            {
                case 0x00200010:
                    _aoe = new(transition, NormalCenter, default, WorldState.FutureTime(6.5f));
                    break;
                case 0x00020001:
                    _aoe = null;
                    Module.Arena.Bounds = TriangleBounds;
                    Module.Arena.Center = TriangleCenter;
                    Active = true;
                    break;
                case 0x00080004:
                    Module.Arena.Bounds = NormalBounds;
                    Module.Arena.Center = NormalCenter;
                    Active = false;
                    break;
            }
        }
    }
}

class Tetraktys(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeTriCone _triSmall = new(16, 30.Degrees());
    private static readonly AOEShapeTriCone _triLarge = new(32, 30.Degrees());
    private static readonly Angle _rot1 = -0.003f.Degrees();
    private static readonly Angle _rot2 = -180.Degrees();
    private static readonly Angle _rot3 = 179.995f.Degrees();

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnEventEnvControl(byte index, uint state)
    {
        // 0x00020001 - small telegraph, 0x00080004 - small telegraph disappears, right after cast with bigger telegraph starts
        // indices are then arranged in natural order; all 'tricone' directions are either 0 or 180 (for small triangles only):
        // small        large
        //   5            E
        //  678          F 10
        // 9ABCD

        var _activation = WorldState.FutureTime(3.8f);
        if (state == 0x00020001)
        {
            if (index == 0x07) //07, 0A, 0D always activate together
            {
                AOEs.Add(new(_triSmall, new WPos(-929, 948.5f), _rot1, _activation));
                AOEs.Add(new(_triSmall, new WPos(-953, 962.356f), _rot2, _activation));
                AOEs.Add(new(_triSmall, new WPos(-945, 948.5f), _rot2, _activation));
            }
            else if (index == 0x05) //05, 08, 0B always activate together
            {
                AOEs.Add(new(_triSmall, new WPos(-945, 948.5f), _rot1, _activation));
                AOEs.Add(new(_triSmall, new WPos(-937, 934.644f), _rot1, _activation));
                AOEs.Add(new(_triSmall, new WPos(-945, 921), _rot1, _activation));
            }
            else if (index == 0x06) //06, 09, 0C always activate together
            {
                AOEs.Add(new(_triSmall, new WPos(-937, 962.356f), _rot3, _activation));
                AOEs.Add(new(_triSmall, new WPos(-961, 948.5f), _rot1, _activation));
                AOEs.Add(new(_triSmall, new WPos(-953, 934.644f), _rot1, _activation));
            }
            else if (index == 0x0E)
                AOEs.Add(new(_triLarge, new WPos(-945, 921), _rot1, _activation));
            else if (index == 0x0F)
                AOEs.Add(new(_triLarge, new WPos(-953, 934.644f), _rot1, _activation));
            else if (index == 0x10)
                AOEs.Add(new(_triLarge, new WPos(-937, 934.644f), _rot1, _activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TetraktysAOESmall or AID.TetraktysAOELarge)
        {
            AOEs.RemoveAt(0);
            ++NumCasts;
        }
    }
}

class TetraktuosKosmosCounter(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TetraktuosKosmosAOETri)); // to handle tutorial of TetraktuosKosmos

class TetraktuosKosmos(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private static readonly AOEShapeTriCone _shapeTri = new(16, 30.Degrees());
    private static readonly AOEShapeRect _shapeRect = new(30, 8);
    private static readonly List<Angle> Angles =
    [-0.003f.Degrees(), -180.Degrees(), 179.995f.Degrees(), 59.995f.Degrees(), -60.Degrees(),
    119.997f.Degrees(), -120.003f.Degrees(), 60.Degrees()];

    private static readonly List<(AOEShape shape, WPos pos, int angle)> combos =
    [
        // 0x12
        (_shapeTri, new WPos(-945, 948.5f), 1),
        (_shapeRect, new WPos(-945, 935), 1),
        (_shapeRect, new WPos(-948.827f, 941.828f), 4),
        (_shapeRect, new WPos(-941.173f, 941.828f), 7),        
        // 0x14
        (_shapeTri, new WPos(-953, 962.356f), 1),
        (_shapeRect, new WPos(-949, 955.428f), 3),
        (_shapeRect, new WPos(-957, 955.428f), 4),
        (_shapeRect, new WPos(-953, 948.5f), 1),
        // 0x15
        (_shapeTri, new WPos(-937, 962.356f), 2),
        (_shapeRect, new WPos(-937, 948.5f), 1),
        (_shapeRect, new WPos(-933, 955.428f), 3),
        (_shapeRect, new WPos(-941, 955.428f), 4),      

        // pair 0x13 + 0x15
        (_shapeTri, new WPos(-961, 948.7f), 0),
        (_shapeTri, new WPos(-937, 962.356f), 2),
        (_shapeRect, new WPos(-933, 955.428f), 3),
        (_shapeRect, new WPos(-941, 955.428f), 4),
        (_shapeRect, new WPos(-937, 948.5f), 1),
        (_shapeRect, new WPos(-957, 955.428f), 5),

        // pair 0x12 + 0x16
        (_shapeTri, new WPos(-945, 948.5f), 1),
        (_shapeTri, new WPos(-929, 948.7f), 0),
        (_shapeRect, new WPos(-933, 955.428f), 6),
        (_shapeRect, new WPos(-941.173f, 941.828f), 7),
        (_shapeRect, new WPos(-948.827f, 941.828f), 4),
        (_shapeRect, new WPos(-945, 935), 1),

        // //pair 0x11 + 0x14
        (_shapeTri, new WPos(-945, 921), 0),
        (_shapeTri, new WPos(-953, 962.356f), 1),
        (_shapeRect, new WPos(-945, 934.8f), 0),
        (_shapeRect, new WPos(-953, 948.5f), 1),
        (_shapeRect, new WPos(-957, 955.428f), 4),
        (_shapeRect, new WPos(-949, 955.428f), 3)
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnEventEnvControl(byte index, uint state)
    {
        // 0x00020001 - small telegraph, 0x00080004 - small telegraph disappears, right after cast with bigger telegraph starts
        // indices are arranged similarly to small tetraktys, however some of the triangles are forbidden
        // small
        //       11
        //    XX 12 XX
        // 13 14 XX 15 16

        if (state != 0x00020001)
            return;
        var tutorialDone = Module.FindComponent<TetraktuosKosmosCounter>()?.NumCasts > 0;
        var activationTime = WorldState.FutureTime(7.9f);

        if (!tutorialDone)
            HandleTutorial(index, activationTime);
        else
            HandleRest(index, activationTime);
    }

    private void HandleTutorial(byte index, DateTime activationTime)
    {
        switch (index)
        {
            case 0x14:
                AddAOEs([0, 1, 2, 3], activationTime);
                break;
            case 0x15:
                AddAOEs([4, 5, 6, 7], activationTime);
                break;
            case 0x12:
                AddAOEs([8, 9, 10, 11], activationTime);
                break;
        }
    }

    private void HandleRest(byte index, DateTime activationTime)
    {
        switch (index)
        {
            case 0x13:
                AddAOEs([12, 13, 14, 15, 16, 17], activationTime);
                break;
            case 0x12:
                AddAOEs([18, 19, 20, 21, 22, 23], activationTime);
                break;
            case 0x11:
                AddAOEs([24, 25, 26, 27, 28, 29], activationTime);
                break;
        }
    }

    private void AddAOEs(int[] indices, DateTime activationTime)
    {
        foreach (var index in indices)
        {
            var (shape, pos, angle) = combos[index];
            AOEs.Add(new AOEInstance(shape, pos, Angles[angle], activationTime));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TetraktuosKosmosAOETri)
        {
            AOEs.Clear();
            ++NumCasts;
        }
    }
}

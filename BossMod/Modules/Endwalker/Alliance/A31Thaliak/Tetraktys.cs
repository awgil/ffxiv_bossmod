namespace BossMod.Endwalker.Alliance.A31Thaliak;

class TetraktysBorder(BossModule module) : Components.GenericAOEs(module)
{
    public bool Active;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(50, 24);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 4)
        {
            switch (state)
            {
                case 0x00200010: // telegraph - emulate by three rects from center of each side
                    var apex = Module.Center - new WDir(0, Module.Bounds.Radius);
                    var height = Module.Bounds.Radius * Helpers.sqrt3;
                    var activation = WorldState.FutureTime(6.5f);
                    _aoes.Add(new(_shape, apex + new WDir(0, height), default, activation));
                    _aoes.Add(new(_shape, apex + 0.5f * new WDir(+Module.Bounds.Radius, height), 120.Degrees(), activation));
                    _aoes.Add(new(_shape, apex + 0.5f * new WDir(-Module.Bounds.Radius, height), -120.Degrees(), activation));
                    break;
                case 0x00020001:
                    _aoes.Clear();
                    Active = true;
                    break;
                case 0x00080004:
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
        // restored pixel perfect solution

        var _activation = WorldState.FutureTime(3.8f);
        if (state == 0x00020001)
        {
            if (index == 0x07) //07, 0A, 0D always activate together
            {
                AOEs.Add(new(_triSmall, new WPos(-929, 948.5f), _rot1, _activation));
                AOEs.Add(new(_triSmall, new WPos(-953, 962.356f), _rot2, _activation));
                AOEs.Add(new(_triSmall, new WPos(-945, 948.5f), _rot2, _activation));
            }
            if (index == 0x05) //05, 08, 0B always activate together
            {
                AOEs.Add(new(_triSmall, new WPos(-945, 948.5f), _rot1, _activation));
                AOEs.Add(new(_triSmall, new WPos(-937, 934.644f), _rot1, _activation));
                AOEs.Add(new(_triSmall, new WPos(-945, 921), _rot1, _activation));
            }
            if (index == 0x06) //06, 09, 0C always activate together
            {
                AOEs.Add(new(_triSmall, new WPos(-937, 962.356f), _rot3, _activation));
                AOEs.Add(new(_triSmall, new WPos(-961, 948.5f), _rot1, _activation));
                AOEs.Add(new(_triSmall, new WPos(-953, 934.644f), _rot1, _activation));
            }
            if (index == 0x0E)
                AOEs.Add(new(_triLarge, new WPos(-945, 921), _rot1, _activation));
            if (index == 0x0F)
                AOEs.Add(new(_triLarge, new WPos(-953, 934.644f), _rot1, _activation));
            if (index == 0x10)
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
    private static readonly Angle _rot1 = -0.003f.Degrees();
    private static readonly Angle _rot2 = -180.Degrees();
    private static readonly Angle _rot3 = 179.995f.Degrees();
    private static readonly Angle _rot4 = 59.995f.Degrees();
    private static readonly Angle _rot5 = -60.Degrees();
    private static readonly Angle _rot6 = 119.997f.Degrees();
    private static readonly Angle _rot7 = -120.003f.Degrees();
    private static readonly Angle _rot8 = 60.Degrees();

    private static readonly AOEShapeTriCone _shapeTri = new(16, 30.Degrees());
    private static readonly AOEShapeRect _shapeRect = new(30, 8);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnEventEnvControl(byte index, uint state)
    {
        // 0x00020001 - small telegraph, 0x00080004 - small telegraph disappears, right after cast with bigger telegraph starts
        // indices are arranged similarly to small tetraktys, however some of the triangles are forbidden
        // small
        //       11
        //    XX 12 XX
        // 13 14 XX 15 16
        // restored pixel perfect solution since calculated coordinates did not match pixel perfect reality and could be wrong by at least 0.2 units

        var tutorialDone = Module.FindComponent<TetraktuosKosmosCounter>() == null || Module.FindComponent<TetraktuosKosmosCounter>()?.NumCasts > 0;
        var _activation = WorldState.FutureTime(7.9f);
        if (state == 0x00020001)
        {
            if (index == 0x13 && tutorialDone) //pair 13+15 always happen together after tutorial
            {
                AOEs.Add(new(_shapeTri, new WPos(-961, 948.7f), _rot1, _activation));
                AOEs.Add(new(_shapeTri, new WPos(-937, 962.356f), _rot3, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-933, 955.428f), _rot4, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-941, 955.428f), _rot5, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-937, 948.5f), _rot2, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-957, 955.428f), _rot6, _activation));
            }
            if (index == 0x12 && tutorialDone) //pair 12+16 always happen together after tutorial
            {
                AOEs.Add(new(_shapeTri, new WPos(-945, 948.5f), _rot2, _activation));
                AOEs.Add(new(_shapeTri, new WPos(-929, 948.7f), _rot1, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-933, 955.428f), _rot7, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-941.173f, 941.828f), _rot8, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-948.827f, 941.828f), _rot5, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-945, 935), _rot2, _activation));
            }
            if (index == 0x11 && tutorialDone) //pair 11+14 always happen together after tutorial
            {
                AOEs.Add(new(_shapeTri, new WPos(-945, 921), _rot1, _activation));
                AOEs.Add(new(_shapeTri, new WPos(-953, 962.356f), _rot2, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-945, 934.8f), _rot1, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-953, 948.5f), _rot2, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-957, 955.428f), _rot5, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-949, 955.428f), _rot4, _activation));
            }
            if (index == 0x14 && !tutorialDone)
            {
                AOEs.Add(new(_shapeTri, new WPos(-953, 962.356f), _rot2, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-949, 955.428f), _rot4, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-957, 955.428f), _rot5, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-953, 948.5f), _rot2, _activation));
            }
            if (index == 0x15 && !tutorialDone)
            {
                AOEs.Add(new(_shapeTri, new WPos(-937, 962.356f), _rot3, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-937, 948.5f), _rot2, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-933, 955.428f), _rot4, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-941, 955.428f), _rot5, _activation));
            }
            if (index == 0x12 && !tutorialDone)
            {
                AOEs.Add(new(_shapeTri, new WPos(-945, 948.5f), _rot2, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-945, 935), _rot2, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-948.827f, 941.828f), _rot5, _activation));
                AOEs.Add(new(_shapeRect, new WPos(-941.173f, 941.828f), _rot8, _activation));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TetraktuosKosmosAOETri)
        {
            AOEs.Clear(); // note: there are no overlaps between sets, and there are no rects that are fully outside border
            ++NumCasts;
        }
    }
}

namespace BossMod.Endwalker.Alliance.A31Thaliak;

class Tetraktys : BossComponent
{
    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        if (state == 0x00080004 && index == 0x04) // 02, 03, 04 always activate at the same time
            module.Arena.Bounds = new ArenaBoundsSquare(new(-945, 945), 24);
        if (state == 0x00200010 && index == 0x04) // 02, 03, 04 always deactivate at the same time
            module.Arena.Bounds = new ArenaBoundsTri(new(-945, 948.5f), 41);
    }
}

class TetraTriangles : Components.GenericAOEs
{
    private readonly List<AOEInstance> _aoes = [];
    private static readonly AOEShapeTriangle tri = new(16);
    private static readonly AOEShapeTriangle triBig = new(32);
    private static readonly AOEShapeRect rect = new(30, 8);
    private static readonly Angle _rot1 = -0.003f.Degrees();
    private static readonly Angle _rot2 = -180.Degrees();
    private static readonly Angle _rot3 = 179.995f.Degrees();
    private static readonly Angle _rot4 = 59.995f.Degrees();
    private static readonly Angle _rot5 = -60.Degrees();
    private static readonly Angle _rot6 = 179.995f.Degrees();
    private static readonly Angle _rot7 = 119.997f.Degrees();
    private static readonly Angle _rot8 = -120.003f.Degrees();
    private static readonly Angle _rot9 = 60.Degrees();
    private bool TutorialDone;

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor)
    {
        foreach (var c in _aoes)
            if (_aoes.Count > 0)
                yield return new(c.Shape, c.Origin, c.Rotation, c.Activation);
    }

    public override void OnEventEnvControl(BossModule module, byte index, uint state)
    {
        var _activation = module.WorldState.CurrentTime.AddSeconds(3.8f);
        var _activation2 = module.WorldState.CurrentTime.AddSeconds(7.9f);
        if (state == 0x00020001)
        {
            if (index == 0x07) //07, 0A, 0D always activate together
            {
                _aoes.Add(new(tri, new WPos(-929, 948.5f), _rot1, _activation));
                _aoes.Add(new(tri, new WPos(-953, 962.356f), _rot2, _activation));
                _aoes.Add(new(tri, new WPos(-945, 948.5f), _rot2, _activation));
            }
            if (index == 0x05) //05, 08, 0B always activate together
            {
                _aoes.Add(new(tri, new WPos(-945, 948.5f), _rot1, _activation));
                _aoes.Add(new(tri, new WPos(-937, 934.644f), _rot1, _activation));
                _aoes.Add(new(tri, new WPos(-945, 921), _rot1, _activation));
            }
            if (index == 0x06) //06, 09, 0C always activate together
            {
                _aoes.Add(new(tri, new WPos(-937, 962.356f), _rot3, _activation));
                _aoes.Add(new(tri, new WPos(-961, 948.5f), _rot1, _activation));
                _aoes.Add(new(tri, new WPos(-953, 934.644f), _rot1, _activation));
            }
            if (index == 0x0E)
                _aoes.Add(new(triBig, new WPos(-945, 921), _rot1, _activation));
            if (index == 0x0F)
                _aoes.Add(new(triBig, new WPos(-953, 934.644f), _rot1, _activation));
            if (index == 0x10)
                _aoes.Add(new(triBig, new WPos(-937, 934.644f), _rot1, _activation));
            if (index == 0x13 && TutorialDone) //pair 13+15 always happen together after tutorial
            {
                _aoes.Add(new(tri, new WPos(-961, 948.7f), _rot1, _activation2));
                _aoes.Add(new(tri, new WPos(-937, 962.356f), _rot6, _activation2));
                _aoes.Add(new(rect, new WPos(-933, 955.428f), _rot4, _activation2));
                _aoes.Add(new(rect, new WPos(-941, 955.428f), _rot5, _activation2));
                _aoes.Add(new(rect, new WPos(-937, 948.5f), _rot2, _activation2));
                _aoes.Add(new(rect, new WPos(-957, 955.428f), _rot7, _activation2));
            }
            if (index == 0x12 && TutorialDone) //pair 12+16 always happen together after tutorial
            {
                _aoes.Add(new(tri, new WPos(-945, 948.5f), _rot2, _activation2));
                _aoes.Add(new(tri, new WPos(-929, 948.7f), _rot1, _activation2));
                _aoes.Add(new(rect, new WPos(-933, 955.428f), _rot8, _activation2));
                _aoes.Add(new(rect, new WPos(-941.173f, 941.828f), _rot9, _activation2));
                _aoes.Add(new(rect, new WPos(-948.827f, 941.828f), _rot5, _activation2));
                _aoes.Add(new(rect, new WPos(-945, 935), _rot2, _activation2));
            }
            if (index == 0x11 && TutorialDone) //pair 11+14 always happen together after tutorial
            {
                _aoes.Add(new(tri, new WPos(-945, 921), _rot1, _activation2));
                _aoes.Add(new(tri, new WPos(-953, 962.356f), _rot2, _activation2));
                _aoes.Add(new(rect, new WPos(-945, 934.8f), _rot1, _activation2));
                _aoes.Add(new(rect, new WPos(-953, 948.5f), _rot2, _activation2));
                _aoes.Add(new(rect, new WPos(-957, 955.428f), _rot5, _activation2));
                _aoes.Add(new(rect, new WPos(-949, 955.428f), _rot4, _activation2));
            }
            if (index == 0x14 && !TutorialDone)
            {
                _aoes.Add(new(tri, new WPos(-953, 962.356f), _rot2, _activation));
                _aoes.Add(new(rect, new WPos(-949, 955.428f), _rot4, _activation));
                _aoes.Add(new(rect, new WPos(-957, 955.428f), _rot5, _activation));
                _aoes.Add(new(rect, new WPos(-953, 948.5f), _rot2, _activation));
                TutorialDone = true;
            }
            if (index == 0x15 && !TutorialDone)
            {
                _aoes.Add(new(tri, new WPos(-937, 962.356f), _rot6, _activation));
                _aoes.Add(new(rect, new WPos(-937, 948.5f), _rot2, _activation));
                _aoes.Add(new(rect, new WPos(-933, 955.428f), _rot4, _activation));
                _aoes.Add(new(rect, new WPos(-941, 955.428f), _rot5, _activation));
                TutorialDone = true;
            }
            if (index == 0x12 && !TutorialDone)
            {
                _aoes.Add(new(tri, new WPos(-945, 948.5f), _rot2, _activation));
                _aoes.Add(new(rect, new WPos(-945, 935), _rot2, _activation));
                _aoes.Add(new(rect, new WPos(-948.827f, 941.828f), _rot5, _activation));
                _aoes.Add(new(rect, new WPos(-941.173f, 941.828f), _rot9, _activation));
                TutorialDone = true;
            }
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.TetraBlueTriangles or AID.TetraGreenTriangles or AID.TetraktuosKosmosTri or AID.TetraktuosKosmosRect)
            _aoes.RemoveAt(0);
    }
}

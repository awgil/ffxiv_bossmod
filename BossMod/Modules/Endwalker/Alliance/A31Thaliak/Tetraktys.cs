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
                    var height = Module.Bounds.Radius * 1.732050808f; // side * sqrt(3) / 2
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

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnEventEnvControl(byte index, uint state)
    {
        // 0x00020001 - small telegraph, 0x00080004 - small telegraph disappears, right after cast with bigger telegraph starts
        // indices are then arranged in natural order; all 'tricone' directions are either 0 or 180 (for small triangles only):
        // small        large
        //   5            E
        //  678          F 10
        // 9ABCD
        if (state == 0x00020001 && index is >= 5 and <= 16)
        {
            var shape = index < 14 ? _triSmall : _triLarge;
            var zOffset = index switch
            {
                5 or 14 => 0,
                6 or 8 or 15 or 16 => 1,
                7 or 9 or 11 or 13 => 2,
                _ => 3
            };
            var xOffset = index switch
            {
                9 => -2,
                6 or 10 or 15 => -1,
                8 or 12 or 16 => +1,
                13 => +2,
                _ => 0
            };
            var dir = index is 7 or 10 or 12 ? 180.Degrees() : default;

            var halfSide = _triSmall.SideLength * 0.5f;
            var height = halfSide * 1.732050808f; // sqrt(3)
            var apex = new WPos(Module.Center.X + halfSide * xOffset, Module.Center.Z - Module.Bounds.Radius + height * zOffset);
            AOEs.Add(new(shape, apex, dir, WorldState.FutureTime(3.9f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TetraktysAOESmall or AID.TetraktysAOELarge)
        {
            var expectedShape = (AID)spell.Action.ID == AID.TetraktysAOESmall ? _triSmall : _triLarge;
            var cnt = AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1) && aoe.Rotation.AlmostEqual(caster.Rotation, 0.1f) && aoe.Shape == expectedShape);
            if (cnt != 1)
                ReportError($"{(AID)spell.Action.ID} removed {cnt} aoes");
            ++NumCasts;
        }
    }
}

class TetraktuosKosmos(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];

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
        if (state == 0x00020001 && index is >= 17 and <= 22)
        {
            var zOffset = index switch
            {
                17 => 0,
                18 or 19 or 22 => 2,
                _ => 3
            };
            var xOffset = index switch
            {
                19 => -2,
                20 => -1,
                21 => +1,
                22 => +2,
                _ => 0
            };
            var dir = index is 18 or 20 or 21 ? 180.Degrees() : default;

            var halfSide = _shapeTri.SideLength * 0.5f;
            var height = halfSide * 1.732050808f; // sqrt(3)
            var apex = new WPos(Module.Center.X + halfSide * xOffset, Module.Center.Z - Module.Bounds.Radius + height * zOffset);
            var resolve = WorldState.FutureTime(8);
            AOEs.Add(new(_shapeTri, apex, dir, resolve));
            AOEs.Add(new(_shapeRect, apex + height * dir.ToDirection(), dir, resolve));
            AOEs.Add(new(_shapeRect, apex + halfSide * (dir + 30.Degrees()).ToDirection(), dir + 120.Degrees(), resolve));
            AOEs.Add(new(_shapeRect, apex + halfSide * (dir - 30.Degrees()).ToDirection(), dir - 120.Degrees(), resolve));
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

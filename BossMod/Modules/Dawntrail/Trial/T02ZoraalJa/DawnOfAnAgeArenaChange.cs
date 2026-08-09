namespace BossMod.Dawntrail.Trial.T02ZoraalJaP2;

sealed class DawnOfAnAgeArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x20)
        {
            switch (state)
            {
                case 0x00020001u:
                    var center = Arena.Center;
                    var angle = 45f.Degrees();
                    var shape = new AOEShapeCustom(center, [new Square(center, 20f, angle)], [new Square(center, 10f, angle)]);
                    _aoe = [new(shape, center, default, WorldState.FutureTime(8d), shapeDistance: shape.Distance(center, default))];
                    break;
                case 0x00080004u:
                    _aoe = [];
                    Arena.Bounds = new ArenaBoundsSquare(10f, 45f.Degrees());
                    break;
            }
        }
        else if (index == 0x1B && state == 0x00080004u)
        {
            Arena.Bounds = T02ZoraalJa.ZoraalJa.GetDefaultBounds();
        }
    }
}

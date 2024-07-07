namespace BossMod.Dawntrail.Extreme.Ex2Zoraal;

class DawnofanAgeBorder(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos Center = new(100, 100);
    public static readonly ArenaBoundsSquare NormalBounds = new(20, 45.Degrees());
    public static readonly ArenaBoundsSquare SmallBounds = new(10, 45.Degrees());
    private static readonly Square square = new(Center, 20);
    private static readonly Square smallsquare = new(Center, 10);
    private static readonly AOEShapeCustom transition = new([square], [smallsquare]);
    private AOEInstance? _aoe;
    public bool Active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 3)
        {
            switch (state)
            {
                case 0x00200010:
                    _aoe = new(transition, Center, default, WorldState.FutureTime(6.5f));
                    break;
                case 0x00020001:
                    _aoe = null;
                    Module.Arena.Bounds = NormalBounds;
                    Active = true;
                    break;
                case 0x00080004:
                    Module.Arena.Bounds = SmallBounds;
                    Active = false;
                    break;
            }
        }
    }
}

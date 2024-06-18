namespace BossMod.Endwalker.Alliance.A13Azeyma;

class WildfireWard(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.IlluminatingGlimpse), 15, false, 1, kind: Kind.DirLeft);
class ArenaBounds(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly Shape triangle = new PolygonCustom([new(-761.5f, -743.38f), new(-750, -763.27f), new(-738.51f, -743.39f)]);
    private static readonly Shape circle = new Circle(A13Azeyma.NormalCenter, 29.5f);
    private static readonly AOEShapeCustom triangleCutOut = new([circle], [triangle]);
    private static readonly ArenaBoundsComplex TriangleBounds = new([triangle]);

    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x1C)
        {
            if (state == 0x00020001)
                _aoe = new(triangleCutOut, A13Azeyma.NormalCenter, default, Module.WorldState.FutureTime(5.7f));
            else if (state == 0x00200010)
            {
                _aoe = null;
                Arena.Bounds = TriangleBounds;
                Arena.Center = TriangleBounds.Center;
            }
            else if (state == 0x00080004)
                Arena.Bounds = A13Azeyma.NormalBounds;
        }
    }
}

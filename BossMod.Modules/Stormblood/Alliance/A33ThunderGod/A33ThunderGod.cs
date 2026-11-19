namespace BossMod.Stormblood.Alliance.A33ThunderGod;

public enum OID : uint
{
    Boss = 0x25D7,
    Helper = 0x233C,
}

class A33ThunderGodStates : StateMachineBuilder
{
    public A33ThunderGodStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7899)]
public class A33ThunderGod(ModuleInit init) : BossModule(init, new(-600, -600), DefaultBounds)
{
    public static readonly ArenaBoundsCustom DefaultBounds = MakeBounds();

    static ArenaBoundsCustom MakeBounds()
    {
        var platform = CurveApprox.Circle(9.6f, 0.05f);
        var starting = 90;
        var clipper = new PolygonClipper();
        var platforms = new RelSimplifiedComplexPolygon();
        for (var i = 0; i < 6; i++)
            platforms.Parts.Add(new([.. platform.Select(p => p + (starting + 60 * i).Degrees().ToDirection() * 25)]));

        // please look away
        List<WPos> bridgeAbs = [
            new(-600, -579.7298f),
            new(-598.51733f, -579.9248f), new(-597.07306f, -581.2062f),
            new(-596, -573.1124f), new(-597.7342f, -573.97614f),
            new(-600, -573.67786f),
            new(-602.2658f, -573.97614f), new(-604, -573.1124f),
            new(-602.92694f, -581.2062f), new(-601.48267f, -579.9248f)
        ];

        var b23 = bridgeAbs[2] - bridgeAbs[1];
        bridgeAbs[2] = bridgeAbs[1] + b23 * 2;
        var b43 = bridgeAbs[3] - bridgeAbs[4];
        bridgeAbs[3] = bridgeAbs[4] + b43 * 2;
        var b67 = bridgeAbs[7] - bridgeAbs[6];
        bridgeAbs[7] = bridgeAbs[6] + b67 * 2;
        var b98 = bridgeAbs[8] - bridgeAbs[9];
        bridgeAbs[8] = bridgeAbs[9] + b98 * 2;

        bridgeAbs.Reverse();

        var bridgeS = ShrinkPoly.Shrink(bridgeAbs.Select(s => s - new WPos(-600, -600)), 0.4f);

        for (var i = 0; i < 6; i++)
            platforms = clipper.Union(new(platforms), new(bridgeS.Select(p => p.Rotate((60 * i).Degrees()))));

        return new(34.5f, platforms);
    }
}

namespace BossMod.RealmReborn.Alliance.A12Atomos;

public enum OID : uint
{
    Boss = 0x963, // R4.000, x3
    Dira = 0x966, // R2.000, x0-2 (spawn during fight)
    Valefor = 0x964, // R5.000, x0-2 (spawn during fight)
    GreaterDemon = 0x965, // R2.000, x0-2 (spawn during fight)

    RingA = 0x1E8F7D,
    RingB = 0x1E8F7E,
    RingC = 0x1E8F7F,

    PlatformA = 0x1E8F80,
    PlatformB = 0X1E8F81,
    PlatformC = 0x1E8F82,
}

class A12AtomosStates : StateMachineBuilder
{
    public A12AtomosStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 92, NameID = 1872)]
public class A12Atomos(WorldState ws, Actor primary) : BossModule(ws, primary, new(232.5f, 280), CustomBounds)
{
    public static readonly ArenaBoundsCustom CustomBounds = AtomosBounds();
    private static ArenaBoundsCustom AtomosBounds()
    {
        var platform = CurveApprox.Rect(new WDir(37.65f, 0), new WDir(0, 12.3f));

        WDir[] cutoutShape = [new(-5.2f, 0), new(-2.5f, 2.6f), new(2.5f, 2.6f), new(5.2f, 0)];
        var cutout1 = cutoutShape.Select(d => d + new WDir(-18.8f, -12.8f));
        var cutout2 = cutoutShape.Select(d => d + new WDir(6.16f, -12.8f));

        var clipper = new PolygonClipper();

        var r01 = clipper.Difference(new(platform), new(cutout1));
        var r02 = clipper.Difference(new(r01), new(cutout2));
        var r03 = clipper.Difference(new(r02), new(cutout1.Select(d => d.MirrorZ())));
        var r1 = clipper.Difference(new(r03), new(cutout2.Select(d => d.MirrorZ())));

        var r2 = r1.Transform(new WDir(0, 35.2f), new(0, 1));
        var r3 = r1.Transform(new WDir(0, -35.2f), new(0, 1));

        var r4 = clipper.Union(new(r1), new(r2));
        var r5 = clipper.Union(new(r4), new(r3));

        return new(47.5f, r5, 1);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies(OID.Boss), ArenaColor.Enemy);
    }
}

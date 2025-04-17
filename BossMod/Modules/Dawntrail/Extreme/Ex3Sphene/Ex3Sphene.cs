namespace BossMod.Dawntrail.Extreme.Ex3Sphene;

class ProsecutionOfWar(BossModule module) : Components.TankSwap(module, AID.ProsecutionOfWar, AID.ProsecutionOfWar, AID.ProsecutionOfWarAOE, 3.1f, null, true);
class DyingMemory(BossModule module) : Components.CastCounter(module, AID.DyingMemory);
class DyingMemoryLast(BossModule module) : Components.CastCounter(module, AID.DyingMemoryLast);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.BossP1, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1017, NameID = 13029, PlanLevel = 100)]
public class Ex3Sphene(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), NormalBounds)
{
    public static readonly ArenaBoundsSquare NormalBounds = new(20);
    public static readonly ArenaBoundsCustom WindBounds = BuildWindBounds();
    public static readonly ArenaBoundsCustom EarthBounds = BuildEarthBounds();
    public static readonly ArenaBoundsCustom IceBounds = BuildIceBounds();
    public static readonly ArenaBoundsCustom IceBridgeBounds = BuildIceBridgeBounds();

    private Actor? _bossP2;
    public Actor? BossP1() => PrimaryActor;
    public Actor? BossP2() => _bossP2;

    protected override void UpdateModule()
    {
        // TODO: this is an ugly hack, think how multi-actor fights can be implemented without it...
        // the problem is that on wipe, any actor can be deleted and recreated in the same frame
        _bossP2 ??= StateMachine.ActivePhaseIndex > 0 ? Enemies(OID.BossP2).FirstOrDefault() : null;
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
        Arena.Actor(_bossP2, ArenaColor.Enemy);
    }

    private static ArenaBoundsCustom BuildWindBounds()
    {
        var platforms = new PolygonClipper.Operand();
        platforms.AddContour([new(-12.5f, -20), new(+12.5f, -20), new(+12.5f, -15), new(-12.5f, -15)]); // N platform
        platforms.AddContour([new(-12.5f, 0), new(+12.5f, 0), new(+12.5f, +5), new(-12.5f, +5)]); // S platform
        var diag1 = new PolygonClipper.Operand([new(-11, -15), new(-3.5f, -15), new(+11, 0), new(+3.5f, 0)]); // NW-SE diagonal
        var diag2 = new PolygonClipper.Operand([new(+3.5f, -15), new(+11, -15), new(-3.5f, 0), new(-11, 0)]); // NE-SW diagonal
        return new(20, NormalBounds.Clipper.Union(new(NormalBounds.Clipper.Union(diag1, diag2)), platforms));
    }

    private static ArenaBoundsCustom BuildEarthBounds()
    {
        var platforms = new PolygonClipper.Operand();
        platforms.AddContour(CurveApprox.Rect(new WDir(-8, -6), new WDir(0, 1), 4, 8)); // W platform
        platforms.AddContour(CurveApprox.Rect(new WDir(+8, -6), new WDir(0, 1), 4, 8)); // E platform
        return new(20, NormalBounds.Clipper.Simplify(platforms));
    }

    private static ArenaBoundsCustom BuildIceBounds()
    {
        var platforms = new PolygonClipper.Operand();
        platforms.AddContour(CurveApprox.Rect(new WDir(-12, -5), new WDir(0, 1), 4, 15)); // W platform
        platforms.AddContour(CurveApprox.Rect(new WDir(+12, -5), new WDir(0, 1), 4, 15)); // E platform
        platforms.AddContour(CurveApprox.Rect(new WDir(0, 1), 2, 10)); // central platform
        return new(20, NormalBounds.Clipper.Simplify(platforms));
    }

    private static ArenaBoundsCustom BuildIceBridgeBounds()
    {
        var bridges = new PolygonClipper.Operand();
        bridges.AddContour(CurveApprox.Rect(new WDir(0, -4), new WDir(0, 1), 8, 2)); // N bridges
        bridges.AddContour(CurveApprox.Rect(new WDir(0, +4), new WDir(0, 1), 8, 2)); // S bridges
        return new(20, NormalBounds.Clipper.Union(new(IceBounds.Poly), bridges));
    }
}

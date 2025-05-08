namespace BossMod.Dawntrail.Extreme.Ex2ZoraalJa;

class MultidirectionalDivide(BossModule module) : Components.StandardAOEs(module, AID.MultidirectionalDivide, new AOEShapeCross(30, 2));
class MultidirectionalDivideMain(BossModule module) : Components.StandardAOEs(module, AID.MultidirectionalDivideMain, new AOEShapeCross(30, 4));
class MultidirectionalDivideExtra(BossModule module) : Components.StandardAOEs(module, AID.MultidirectionalDivideExtra, new AOEShapeCross(40, 2));
class RegicidalRage(BossModule module) : Components.TankbusterTether(module, AID.RegicidalRageAOE, (uint)TetherID.RegicidalRage, 8);
class BitterWhirlwind(BossModule module) : Components.TankSwap(module, AID.BitterWhirlwind, AID.BitterWhirlwindAOEFirst, AID.BitterWhirlwindAOERest, 3.1f, new AOEShapeCircle(5), true);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, AID.BurningChainsAOE);
class HalfCircuitRect(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitAOERect, new AOEShapeRect(60, 60));
class HalfCircuitDonut(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitAOEDonut, new AOEShapeDonut(10, 30));
class HalfCircuitCircle(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitAOECircle, new AOEShapeCircle(10));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 996, NameID = 12882, PlanLevel = 100)]
public class Ex2ZoraalJa(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), NormalBounds)
{
    public static readonly WPos NormalCenter = new(100, 100);
    public static readonly ArenaBoundsRect NormalBounds = new(20, 20, 45.Degrees());
    public static readonly ArenaBoundsRect SmallBounds = new(10, 10, 45.Degrees(), 20);
    public static readonly ArenaBoundsCustom NWPlatformBounds = BuildTwoPlatformsBounds(135.Degrees());
    public static readonly ArenaBoundsCustom NEPlatformBounds = BuildTwoPlatformsBounds(-135.Degrees());

    private static ArenaBoundsCustom BuildTwoPlatformsBounds(Angle orientation)
    {
        var dir = orientation.ToDirection();
        var main = new PolygonClipper.Operand(CurveApprox.Rect(-15 * dir, dir, 10, 10));
        var side = new PolygonClipper.Operand(CurveApprox.Rect(+15 * dir, dir, 10, 10));
        return new(20, NormalBounds.Clipper.Union(main, side));
    }
}

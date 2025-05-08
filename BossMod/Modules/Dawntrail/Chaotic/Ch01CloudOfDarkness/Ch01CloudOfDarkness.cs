namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class Flare(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(25), (uint)IconID.Flare, AID.FlareAOE, 8.1f, true);
class StygianShadow(BossModule module) : Components.Adds(module, (uint)OID.StygianShadow);
class Atomos(BossModule module) : Components.Adds(module, (uint)OID.Atomos);
class GhastlyGloomCross(BossModule module) : Components.StandardAOEs(module, AID.GhastlyGloomCrossAOE, new AOEShapeCross(40, 15));
class GhastlyGloomDonut(BossModule module) : Components.StandardAOEs(module, AID.GhastlyGloomDonutAOE, new AOEShapeDonut(21, 40));
class FloodOfDarknessAdd(BossModule module) : Components.CastInterruptHint(module, AID.FloodOfDarknessAdd); // TODO: only if add is player's?..
class Excruciate(BossModule module) : Components.BaitAwayCast(module, AID.Excruciate, new AOEShapeCircle(4), true);
class LoomingChaos(BossModule module) : Components.CastCounter(module, AID.LoomingChaosAOE);

// TODO: tankswap hints component for phase1
// TODO: phase 2 teleport zones?
// TODO: grim embrace / curse of darkness prevent turning
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1010, NameID = 13624, PlanLevel = 100)]
public class Ch01CloudOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, DefaultCenter, InitialBounds)
{
    public static readonly WPos DefaultCenter = new(100, 100);
    public static readonly ArenaBoundsCircle InitialBounds = new(40);
    public static readonly ArenaBoundsCustom Phase1Bounds = new(InitialBounds.Radius, new(BuildPhase1BoundsContour()));
    public static readonly ArenaBoundsCustom Phase2Bounds = new(InitialBounds.Radius, BuildPhase2BoundsPoly());
    public static readonly WPos Phase1Midpoint = DefaultCenter + (Phase1Bounds.Poly.Parts[0].Vertices[1] + Phase1Bounds.Poly.Parts[0].Vertices[3]) * 0.5f;

    public static List<WDir> BuildPhase1BoundsContour()
    {
        // north 'diagonal' is at [+/-15, -37] (it almost intersects the initial circle - at x=15 z is ~37.08)
        // the main diagonal is 20, rotated by 45 degrees, which means that side corners are at x=+/- 40/sqrt(2), z = -37 + 40/sqrt(2) - 15
        var nz = -37;
        var nx = 15;
        var halfDiag = 40 / MathF.Sqrt(2);
        var cz = nz + halfDiag - nx;
        return [new(nx, nz), new(halfDiag, cz), new(0, cz + halfDiag), new(-halfDiag, cz), new(-nx, nz)];
    }

    public static RelSimplifiedComplexPolygon BuildPhase2BoundsPoly()
    {
        // mid is union of 4 rects
        var midHalfWidth = 3;
        var midHalfLength = 24;
        var midOffset = 15;
        var op1 = new PolygonClipper.Operand();
        var op2 = new PolygonClipper.Operand();
        op1.AddContour(CurveApprox.Rect(new WDir(0, +midOffset), new(1, 0), midHalfWidth, midHalfLength));
        op1.AddContour(CurveApprox.Rect(new WDir(0, -midOffset), new(1, 0), midHalfWidth, midHalfLength));
        op2.AddContour(CurveApprox.Rect(new WDir(+midOffset, 0), new(0, 1), midHalfWidth, midHalfLength));
        op2.AddContour(CurveApprox.Rect(new WDir(-midOffset, 0), new(0, 1), midHalfWidth, midHalfLength));
        var mid = InitialBounds.Clipper.Union(op1, op2);

        // sides is union of two platforms and the outside ring
        var sideHalfWidth = 7.5f;
        var sideHalfLength = 10;
        var sideOffset = 19 + sideHalfLength;
        var sideRingWidth = 6;
        op1.Clear();
        op2.Clear();
        op1.AddContour(CurveApprox.Rect(new WDir(+sideOffset, 0), new(1, 0), sideHalfWidth, sideHalfLength));
        op1.AddContour(CurveApprox.Rect(new WDir(-sideOffset, 0), new(1, 0), sideHalfWidth, sideHalfLength));
        op2.AddContour(CurveApprox.Circle(InitialBounds.Radius, 0.1f));
        op2.AddContour(CurveApprox.Circle(InitialBounds.Radius - sideRingWidth, 0.1f));
        var side = InitialBounds.Clipper.Union(op1, op2);

        op1.Clear();
        op2.Clear();
        op1.AddPolygon(mid);
        op2.AddPolygon(side);
        return InitialBounds.Clipper.Union(op1, op2);
    }
}

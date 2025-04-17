namespace BossMod.Dawntrail.Alliance.A14ShadowLord;

class Teleport(BossModule module) : Components.CastCounter(module, AID.Teleport);
class TeraSlash(BossModule module) : Components.CastCounter(module, AID.TeraSlash);
class UnbridledRage(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(100, 4), (uint)IconID.UnbridledRage, AID.UnbridledRageAOE, 5.9f);
class DarkNova(BossModule module) : Components.SpreadFromCastTargets(module, AID.DarkNova, 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13653)]
public class A14ShadowLord(WorldState ws, Actor primary) : BossModule(ws, primary, new(150, 800), NormalBounds)
{
    public static readonly ArenaBoundsCircle NormalBounds = new(30); // TODO: verify radius
    public static readonly ArenaBoundsCustom ChtonicBounds = BuildChtonicBounds();

    public static ArenaBoundsCustom BuildChtonicBounds()
    {
        var circles = new PolygonClipper.Operand();
        circles.AddContour(CurveApprox.Circle(8, 0.05f).Select(p => new WDir(p.X + 16, p.Z)));
        circles.AddContour(CurveApprox.Circle(8, 0.05f).Select(p => new WDir(p.X - 16, p.Z)));
        circles.AddContour(CurveApprox.Circle(8, 0.05f).Select(p => new WDir(p.X, p.Z + 16)));
        circles.AddContour(CurveApprox.Circle(8, 0.05f).Select(p => new WDir(p.X, p.Z - 16)));

        var intercard = 45.Degrees().ToDirection();
        var inner = (16 - 2) * 0.70710678f * intercard;
        var outer = (16 + 2) * 0.70710678f * intercard;
        var bridges = new PolygonClipper.Operand();
        bridges.AddContour(CurveApprox.Rect(inner, inner.OrthoL()));
        bridges.AddContour(CurveApprox.Rect(outer, outer.OrthoL()));

        return new(NormalBounds.Radius, NormalBounds.Clipper.Union(circles, bridges));
    }
}

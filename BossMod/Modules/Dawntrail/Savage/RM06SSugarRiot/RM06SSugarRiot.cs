namespace BossMod.Dawntrail.Savage.RM06SSugarRiot;

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1022, NameID = 13822, PlanLevel = 100)]
public class SugarRiot : BossModule
{
    public RelSimplifiedComplexPolygon RiverPoly { get; private set; }
    public RelSimplifiedComplexPolygon BridgePoly { get; private set; }

    public SugarRiot(WorldState ws, Actor primary) : base(ws, primary, new(100, 100), new ArenaBoundsSquare(20))
    {
        var arcRiverSW = new WPos(100.90272f, 132.59357f);
        var centerOffset = arcRiverSW - Center;

        RiverPoly = new(CurveApprox.DonutSector(28, 32.9f, 202.5f.Degrees(), 247.5f.Degrees(), 1 / 112.5f).Select(p => p + centerOffset));

        var arcRiverSE = new WPos(127.77293f, 82.919174f);
        centerOffset = arcRiverSE - Center;

        RiverPoly.Parts.Add(new([.. CurveApprox.DonutSector(28, 32.9f, (142.930197f - 180).Degrees(), (180 - 142.930197f).Degrees(), 1 / 112.5f).Select(p => p + centerOffset)]));

        var arcRiverN = new WPos(131.71599f, 92.44231f);
        centerOffset = arcRiverN - Center;

        RiverPoly.Parts.Add(new([.. CurveApprox.DonutSector(28, 32.9f, 201.59366f.Degrees(), 261.59366f.Degrees(), 1 / 112.5f).Select(p => p + centerOffset)]));

        var arcS = CurveApprox.CircleArc(new WPos(98.287796f, 113.00449f), 8.9f, 144.Degrees(), 201.Degrees(), 1 / 112.5f).Select(a => a - Center);
        var arcE = CurveApprox.CircleArc(new WPos(112.11865f, 94.97823f), 8.9f, 264.Degrees(), 321.Degrees(), 1 / 112.5f).Select(a => a - Center);
        var arcW = CurveApprox.CircleArc(new WPos(89.59171f, 92.015785f), 8.9f, 24.Degrees(), 81.Degrees(), 1 / 112.5f).Select(a => a - Center);

        RiverPoly.Parts.Add(new([.. arcS, .. arcW, .. arcE]));

        List<WPos> bridge1 = [new(111.61687f, 103.871864f), new(107.71751f, 100.8799f), new(102.84746f, 107.22682f), new(106.74679f, 110.21888f)];
        List<WPos> bridge2 = [new(90.798996f, 108.14075f), new(95.41878f, 106.22826f), new(92.35635f, 98.83513f), new(87.73737f, 100.74922f)];
        List<WPos> bridge3 = [new(105.48188f, 89.00543f), new(104.83112f, 93.96291f), new(96.89565f, 92.9182f), new(97.55017f, 87.96116f)];
        BridgePoly = new(bridge1.Select(b => b - Center));
        BridgePoly.Parts.Add(new([.. bridge2.Select(b => b - Center)]));
        BridgePoly.Parts.Add(new([.. bridge3.Select(b => b - Center)]));
    }
}


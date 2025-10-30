namespace BossMod.Dawntrail.Foray.ForkedTower.FT04Magitaur;

class UnsealedAura(BossModule module) : Components.RaidwideCastDelay(module, AID.UnsealedAuraCast, AID.UnsealedAura, 0.8f);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13947, PlanLevel = 100)]
public class FT04Magitaur(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaCenter, new ArenaBoundsCircle(31.5f))
{
    public override bool DrawAllPlayers => true;

    public static readonly AOEShapeCustom NotPlatforms = MakeNotPlatforms();

    public static readonly (WDir, Angle)[] Platforms = [
        (new WDir(0, 14.5f).Rotate(-120.Degrees()), -75.Degrees()),
        (new WDir(0, 14.5f), 45.Degrees()),
        (new WDir(0, 14.5f).Rotate(120.Degrees()), 165.Degrees())
    ];

    public static readonly WPos ArenaCenter = new(700, -674);

    public static int GetPlatform(WPos p) => Array.FindIndex(Platforms, pl => p.InRect(ArenaCenter + pl.Item1, pl.Item2.ToDirection(), 10, 10, 10));
    public static int GetPlatform(Actor a) => GetPlatform(a.Position);

    private static AOEShapeCustom MakeNotPlatforms()
    {
        IEnumerable<WDir> rect(Angle offset) => CurveApprox.Rect(new(10, 0), new(0, 10)).Select(d => d.Rotate(45.Degrees() + offset) + new WDir(0, 14.5f).Rotate(offset));

        var clipper = new PolygonClipper();
        RelSimplifiedComplexPolygon arena = new(CurveApprox.Circle(31.5f, 1 / 90f));

        arena = clipper.Difference(new(arena), new(rect(default)));
        arena = clipper.Difference(new(arena), new(rect(120.Degrees())));
        arena = clipper.Difference(new(arena), new(rect(-120.Degrees())));
        return new(arena);
    }

    protected override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var (off, angle) in Platforms)
            Arena.AddRect(Arena.Center + off, angle.ToDirection(), 10, 10, 10, ArenaColor.Border);
    }
}

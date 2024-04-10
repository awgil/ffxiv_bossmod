namespace BossMod.Endwalker.Alliance.A23Halone;

// TODO: assign alliances members to a specific octagon. in duty finder it is usually:
// NW (Octagon3): Alliance A
// NE (Octagon1): Alliance C
// S (Octagon2): Alliance B
class Octagons : BossComponent
{
    private static readonly WPos[] spears = [new(-686, 592), new(-700, 616.2f), new(-714, 592)];
    private const float RadianConversion = MathF.PI / 180;

    private static IEnumerable<WPos> Octagon1()
    {
        for (int i = 0; i < 9; ++i)
            yield return RotateAroundOrigin(37.5f + i * 45, spears[0], new(spears[0].X + 11.5f, spears[0].Z));
    }

    private static IEnumerable<WPos> Octagon2()
    {
        for (int i = 0; i < 9; ++i)
            yield return RotateAroundOrigin(22.5f + i * 45, spears[1], new(spears[1].X + 11.5f, spears[1].Z));
    }

    private static IEnumerable<WPos> Octagon3()
    {
        for (int i = 0; i < 9; ++i)
            yield return RotateAroundOrigin(-37.5f + i * 45, spears[2], new(spears[2].X + 11.5f, spears[2].Z));
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[0], 1) && !x.IsDead))
            foreach (var c in Octagon1())
                arena.PathLineTo(c);
        arena.PathStroke(false, ArenaColor.Border, 2);
        if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[1], 1) && !x.IsDead))
            foreach (var c in Octagon2())
                arena.PathLineTo(c);
        arena.PathStroke(false, ArenaColor.Border, 2);
        if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[2], 1) && !x.IsDead))
            foreach (var c in Octagon3())
                arena.PathLineTo(c);
        arena.PathStroke(false, ArenaColor.Border, 2);
    }

    private static WPos RotateAroundOrigin(float rotatebydegrees, WPos origin, WPos caster) //TODO: consider moving to utils for future use
    {
        float x = MathF.Cos(rotatebydegrees * RadianConversion) * (caster.X - origin.X) - MathF.Sin(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        float z = MathF.Sin(rotatebydegrees * RadianConversion) * (caster.X - origin.X) + MathF.Cos(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        return new WPos(origin.X + x, origin.Z + z);
    }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var octagons = new List<Func<WPos, float>>(); //inverted octagons
        var octagons2 = new List<Func<WPos, float>>(); //octagons
        
        base.AddAIHints(module, slot, actor, assignment, hints);

        if (module.Enemies(OID.GlacialSpearSmall).Count(x => !x.IsDead) == 3)
        {
            octagons.Add(ShapeDistance.InvertedConvexPolygon(Octagon1(), true));
            octagons.Add(ShapeDistance.InvertedConvexPolygon(Octagon2(), true));
            octagons.Add(ShapeDistance.InvertedConvexPolygon(Octagon3(), true));
        }
        else if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[0], 1) && !x.IsDead) && actor.Position.InConvexPolygon(Octagon1()))
            octagons.Add(ShapeDistance.InvertedConvexPolygon(Octagon1(), true));
        else if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[1], 1) && !x.IsDead) && actor.Position.InConvexPolygon(Octagon2()))
            octagons.Add(ShapeDistance.InvertedConvexPolygon(Octagon2(), true));
        else if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[2], 1) && !x.IsDead) && actor.Position.InConvexPolygon(Octagon3()))
            octagons.Add(ShapeDistance.InvertedConvexPolygon(Octagon3(), true));
        if (octagons.Count == 0)
        {
            if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[0], 1) && !x.IsDead) && !actor.Position.InConvexPolygon(Octagon1()))
                octagons2.Add(ShapeDistance.ConvexPolygon(Octagon1(), true));
            if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[1], 1) && !x.IsDead) && !actor.Position.InConvexPolygon(Octagon2()))
                octagons2.Add(ShapeDistance.ConvexPolygon(Octagon2(), true));
            if (module.Enemies(OID.GlacialSpearSmall).Any(x => x.Position.AlmostEqual(spears[2], 1) && !x.IsDead) && !actor.Position.InConvexPolygon(Octagon3()))
                octagons2.Add(ShapeDistance.ConvexPolygon(Octagon3(), true));
        }
        if (octagons.Count > 0)
            hints.AddForbiddenZone(p => octagons.Select(f => f(p)).Max());
        if (octagons2.Count > 0)
            hints.AddForbiddenZone(p => octagons2.Select(f => f(p)).Min());
    }
}
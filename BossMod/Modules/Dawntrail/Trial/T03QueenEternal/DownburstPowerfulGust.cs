namespace BossMod.Dawntrail.Trial.T03QueenEternal;

sealed class PowerfulGustKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.PowerfulGust, 20f, kind: Kind.DirForward, stopAfterWall: true)
{
    public RelSimplifiedComplexPolygon Polygon = T03QueenEternal.GetXArena().Polygon.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInComplexPolygonFixedDirection(Arena.Center, 20f * c.Direction.ToDirection(), Polygon), act);
            }
        }
    }
}

sealed class DownburstKB(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.Downburst, 10f, stopAfterWall: true)
{
    public RelSimplifiedComplexPolygon Polygon = T03QueenEternal.GetXArena().Polygon.Offset(-0.5f); // pretend polygon is 0.5y smaller than real for less suspect knockbacks

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOriginPlusIntersectionTest(Arena.Center, c.Origin, 10f, Polygon), act);
            }
        }
    }
}

sealed class PowerfulGustDownburstRW(BossModule module) : Components.RaidwideCasts(module, [(uint)AID.PowerfulGust, (uint)AID.Downburst]);

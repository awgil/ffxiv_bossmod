namespace BossMod.Dawntrail.Alliance.A23Kamlanaut;

sealed class ShieldBash(BossModule module) : Components.SimpleKnockbacks(module, (uint)AID.ShieldBash, 30f, stopAfterWall: true)
{
    public RelSimplifiedComplexPolygon Polygon;
    public bool PolygonInit;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count != 0)
        {
            ref readonly var c = ref Casters.Ref(0);
            var act = c.Activation;
            if (!IsImmune(slot, act))
            {
                if (!PolygonInit)
                {
                    Polygon = Arena.Bounds.Shape.Offset(-1f); // pretend polygon is 1y smaller than real for less suspect knockbacks
                    PolygonInit = true;
                }

                hints.AddForbiddenZone(new SDKnockbackInComplexPolygonAwayFromOrigin(Arena.Center, c.Origin, 30f, Polygon), act);
            }
        }
    }
}

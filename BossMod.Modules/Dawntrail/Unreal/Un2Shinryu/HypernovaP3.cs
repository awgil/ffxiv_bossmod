namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class HypernovaP3(BossModule module) : Components.CastHint(module, AID.HypernovaVisualP3, "Stack together")
{
    private WPos StackSpot() => Raid.WithoutSlot().Aggregate((sum: new WDir(), n: 0), (a, p) => (a.sum + (p.Position - Arena.Center), a.n + 1)) is var t && t.n > 0 ? Arena.Center + t.sum / t.n : Arena.Center;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(StackSpot(), 2), Module.CastFinishAt(Casters[0].CastInfo));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Casters.Count > 0)
            Arena.AddCircle(StackSpot(), 2, ArenaColor.Safe);
    }
}

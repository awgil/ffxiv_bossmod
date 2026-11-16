namespace BossMod.Dawntrail.Unreal.Un2Shinryu;

class TailSpitKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.TailSpit, 15)
{
    private static readonly WPos _spot = Un2Shinryu.P2Center + 6.5f * (-135f).Degrees().ToDirection();

    public override PointTest DestinationUnsafe(int slot, Actor actor) => PointTest.Never;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_spot, 1.5f), Module.CastFinishAt(Casters[0].CastInfo));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Casters.Count > 0 && !actor.Position.InCircle(_spot, 1.5f))
            hints.Add("Stand north-west of the center, the knockback throws you onto the tail!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (Casters.Count > 0)
            Arena.AddCircle(_spot, 1.5f, ArenaColor.Safe, 2);
    }
}

namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for infralateral arc mechanic (role stacks)
class InfralateralArc(BossModule module) : Components.CastCounter(module, AID.InfralateralArcAOE)
{
    private static readonly Angle _coneHalfAngle = 45.Degrees();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var pcRole = EffectiveRole(actor);
        var pcDir = Angle.FromDirection(actor.Position - Module.PrimaryActor.Position);
        if (Raid.WithoutSlot().Any(a => EffectiveRole(a) != pcRole && a.Position.InCone(Module.PrimaryActor.Position, pcDir, _coneHalfAngle)))
            hints.Add("Spread by roles!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pcRole = EffectiveRole(pc);
        var pcDir = Angle.FromDirection(pc.Position - Module.PrimaryActor.Position);
        foreach (var actor in Raid.WithoutSlot().Where(a => EffectiveRole(a) != pcRole))
            Arena.Actor(actor, actor.Position.InCone(Module.PrimaryActor.Position, pcDir, _coneHalfAngle) ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
    }

    private Role EffectiveRole(Actor a) => a.Role == Role.Ranged ? Role.Melee : a.Role;
}

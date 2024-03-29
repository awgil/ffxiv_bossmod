namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

// component for infralateral arc mechanic (role stacks)
class InfralateralArc : Components.CastCounter
{
    private static readonly Angle _coneHalfAngle = 45.Degrees();

    public InfralateralArc() : base(ActionID.MakeSpell(AID.InfralateralArcAOE)) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        var pcRole = EffectiveRole(actor);
        var pcDir = Angle.FromDirection(actor.Position - module.PrimaryActor.Position);
        if (module.Raid.WithoutSlot().Any(a => EffectiveRole(a) != pcRole && a.Position.InCone(module.PrimaryActor.Position, pcDir, _coneHalfAngle)))
            hints.Add("Spread by roles!");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        var pcRole = EffectiveRole(pc);
        var pcDir = Angle.FromDirection(pc.Position - module.PrimaryActor.Position);
        foreach (var actor in module.Raid.WithoutSlot().Where(a => EffectiveRole(a) != pcRole))
            arena.Actor(actor, actor.Position.InCone(module.PrimaryActor.Position, pcDir, _coneHalfAngle) ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
    }

    private Role EffectiveRole(Actor a) =>  a.Role == Role.Ranged ? Role.Melee : a.Role;
}

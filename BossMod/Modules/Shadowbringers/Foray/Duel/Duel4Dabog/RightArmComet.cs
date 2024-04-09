namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

class RightArmComet : Components.KnockbackFromCastTarget
{
    private static readonly float _radius = 5;

    public RightArmComet(AID aid, float distance) : base(ActionID.MakeSpell(aid), distance, shape: new AOEShapeCircle(_radius)) { }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (Casters.Any(c => !Shape!.Check(actor.Position, c)))
            hints.Add("Soak the tower!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var c in Casters)
            arena.AddCircle(c.Position, _radius, pc.Position.InCircle(c.Position, _radius) ? ArenaColor.Safe : ArenaColor.Danger, 2);
    }
}

class RightArmCometShort(BossModule module) : RightArmComet(module, AID.RightArmCometKnockbackShort, 12);

class RightArmCometLong(BossModule module) : RightArmComet(module, AID.RightArmCometKnockbackLong, 25);

namespace BossMod.Shadowbringers.Foray.Duel.Duel4Dabog;

class RightArmComet(BossModule module, AID aid, float distance) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(aid), distance, shape: new AOEShapeCircle(_radius))
{
    private const float _radius = 5;

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
            Arena.AddCircle(c.Position, _radius, pc.Position.InCircle(c.Position, _radius) ? ArenaColor.Safe : ArenaColor.Danger, 2);
    }
}
class RightArmCometShort(BossModule module) : RightArmComet(module, AID.RightArmCometKnockbackShort, 12);
class RightArmCometLong(BossModule module) : RightArmComet(module, AID.RightArmCometKnockbackLong, 25);

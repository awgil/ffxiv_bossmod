namespace BossMod.Endwalker.Savage.P6SHegemone;

class PteraIxou : Components.CastCounter
{
    private BitMask _vulnSnake;
    private BitMask _vulnWing;

    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public PteraIxou() : base(ActionID.MakeSpell(AID.PteraIxouAOESnake)) { } // doesn't matter which spell to track

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (ForbiddenCenters(module, slot).Any(dir => _shape.Check(actor.Position, module.Bounds.Center, dir)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        foreach (var dir in ForbiddenCenters(module, pcSlot))
            _shape.Draw(arena, module.Bounds.Center, dir);
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.GlossalResistanceDown:
                _vulnSnake.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.ChelicResistanceDown:
                _vulnWing.Set(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(BossModule module, Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.GlossalResistanceDown:
                _vulnSnake.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
            case SID.ChelicResistanceDown:
                _vulnWing.Clear(module.Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    private IEnumerable<Angle> ForbiddenCenters(BossModule module, int slot)
    {
        if (_vulnSnake[slot])
            yield return 90.Degrees();
        if (_vulnWing[slot])
            yield return -90.Degrees();
    }
}

class PteraIxouSpreadStack : Components.CastStackSpread
{
    public PteraIxouSpreadStack() : base(ActionID.MakeSpell(AID.PteraIxouUnholyDarkness), ActionID.MakeSpell(AID.PteraIxouDarkSphere), 6, 10, 3) { }
}

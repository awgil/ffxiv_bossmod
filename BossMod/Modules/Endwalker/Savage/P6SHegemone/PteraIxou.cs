namespace BossMod.Endwalker.Savage.P6SHegemone;

class PteraIxou(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.PteraIxouAOESnake)) // doesn't matter which spell to track
{
    private BitMask _vulnSnake;
    private BitMask _vulnWing;

    private static readonly AOEShapeCone _shape = new(30, 90.Degrees());

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (ForbiddenCenters(slot).Any(dir => _shape.Check(actor.Position, Module.Center, dir)))
            hints.Add("GTFO from aoe!");
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var dir in ForbiddenCenters(pcSlot))
            _shape.Draw(Arena, Module.Center, dir);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.GlossalResistanceDown:
                _vulnSnake.Set(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.ChelicResistanceDown:
                _vulnWing.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.GlossalResistanceDown:
                _vulnSnake.Clear(Raid.FindSlot(actor.InstanceID));
                break;
            case SID.ChelicResistanceDown:
                _vulnWing.Clear(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    private IEnumerable<Angle> ForbiddenCenters(int slot)
    {
        if (_vulnSnake[slot])
            yield return 90.Degrees();
        if (_vulnWing[slot])
            yield return -90.Degrees();
    }
}

class PteraIxouSpreadStack(BossModule module) : Components.CastStackSpread(module, ActionID.MakeSpell(AID.PteraIxouUnholyDarkness), ActionID.MakeSpell(AID.PteraIxouDarkSphere), 6, 10, 3);

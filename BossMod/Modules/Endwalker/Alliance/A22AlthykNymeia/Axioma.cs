namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

// TODO: show zone shapes
class Axioma(BossModule module) : BossComponent(module)
{
    public bool ShouldBeInZone { get; private set; }
    private BitMask _inZone;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_inZone[slot] != ShouldBeInZone)
            hints.Add(ShouldBeInZone ? "Go to dark zone!" : "GTFO from dark zone!");
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Heavy)
            _inZone.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Heavy)
            _inZone.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InexorablePullAOE)
            ShouldBeInZone = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InexorablePullAOE)
            ShouldBeInZone = false;
    }
}

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class BoldBoulderTrample : Components.UniformStackSpread
{
    public BoldBoulderTrample() : base(6, 20, 6) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BoldBoulder && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddSpread(target);
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BoldBoulder)
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.Trample)
            AddStack(actor);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Trample)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

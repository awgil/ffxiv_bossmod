namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class BoldBoulderTrample(BossModule module) : Components.UniformStackSpread(module, 6, 20, 6)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BoldBoulder && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddSpread(target);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.BoldBoulder)
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.TargetID);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID == IconID.Trample)
            AddStack(actor);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Trample)
            Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

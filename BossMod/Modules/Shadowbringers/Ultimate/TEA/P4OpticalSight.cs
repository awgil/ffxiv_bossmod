namespace BossMod.Shadowbringers.Ultimate.TEA;

class P4OpticalSight(BossModule module) : Components.UniformStackSpread(module, 6, 6, 4)
{
    public override void OnEventIcon(Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.OpticalSightSpread:
                AddSpread(actor, WorldState.FutureTime(5.1f));
                break;
            case IconID.OpticalSightStack:
                AddStack(actor, WorldState.FutureTime(5.1f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IndividualReprobation:
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
            case AID.CollectiveReprobation:
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                break;
        }
    }
}

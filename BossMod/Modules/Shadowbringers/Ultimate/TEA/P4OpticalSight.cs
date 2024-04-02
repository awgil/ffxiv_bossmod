namespace BossMod.Shadowbringers.Ultimate.TEA;

class P4OpticalSight : Components.UniformStackSpread
{
    public P4OpticalSight() : base(6, 6, 4) { }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.OpticalSightSpread:
                AddSpread(actor, module.WorldState.CurrentTime.AddSeconds(5.1f));
                break;
            case IconID.OpticalSightStack:
                AddStack(actor, module.WorldState.CurrentTime.AddSeconds(5.1f));
                break;
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

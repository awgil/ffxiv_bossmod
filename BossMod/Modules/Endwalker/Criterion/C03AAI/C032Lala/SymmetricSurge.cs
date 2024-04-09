namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class SymmetricSurge(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SurgeVector)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NSymmetricSurgeAOE or AID.SSymmetricSurgeAOE)
            Stacks.Clear();
    }
}

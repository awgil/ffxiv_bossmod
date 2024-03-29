namespace BossMod.Endwalker.Criterion.C03AAI.C032Lala;

class SymmetricSurge : Components.UniformStackSpread
{
    public SymmetricSurge() : base(6, 0) { }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SurgeVector)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NSymmetricSurgeAOE or AID.SSymmetricSurgeAOE)
            Stacks.Clear();
    }
}

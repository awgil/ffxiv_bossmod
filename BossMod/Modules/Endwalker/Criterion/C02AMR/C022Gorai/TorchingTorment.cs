namespace BossMod.Endwalker.VariantCriterion.C02AMR.C022Gorai;

sealed class TorchingTorment(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(6);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TorchingTorment && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NTorchingTormentAOE or (uint)AID.STorchingTormentAOE)
        {
            CurrentBaits.Clear();
            ++NumCasts;
        }
    }
}

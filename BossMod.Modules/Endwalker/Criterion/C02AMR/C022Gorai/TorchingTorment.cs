namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class TorchingTorment(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(6);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TorchingTorment && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NTorchingTormentAOE or AID.STorchingTormentAOE)
        {
            CurrentBaits.Clear();
            ++NumCasts;
        }
    }
}

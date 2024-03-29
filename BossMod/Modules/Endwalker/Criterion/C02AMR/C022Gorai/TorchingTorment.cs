namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class TorchingTorment : Components.GenericBaitAway
{
    private static readonly AOEShapeCircle _shape = new(6);

    public TorchingTorment() : base(centerAtTarget: true) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TorchingTorment && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            CurrentBaits.Add(new(caster, target, _shape));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NTorchingTormentAOE or AID.STorchingTormentAOE)
        {
            CurrentBaits.Clear();
            ++NumCasts;
        }
    }
}

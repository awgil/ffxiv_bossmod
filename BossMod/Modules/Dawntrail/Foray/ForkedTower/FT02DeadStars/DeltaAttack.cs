namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class DeltaAttack(BossModule module) : Components.RaidwideCastDelay(module, AID.DeltaAttackCast, AID.DeltaAttack, 0.5f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction || (AID)spell.Action.ID == AID.DeltaAttackUnk2)
        {
            NumCasts++;
            if (NumCasts >= 6)
                Activation = default;
        }
    }
}

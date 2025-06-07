namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class DeltaAttack(BossModule module) : Components.RaidwideCastDelay(module, AID._Spell_DeltaAttack1, AID._Spell_DeltaAttack2, 0.5f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction || (AID)spell.Action.ID == AID._Spell_DeltaAttack5)
        {
            NumCasts++;
            if (NumCasts >= 6)
                Activation = default;
        }
    }
}

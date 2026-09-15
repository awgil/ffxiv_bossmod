namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class RapidSequenceParticleBeam(BossModule module) : Components.GenericWildCharge(module, 3, AID.RapidSequenceParticleBeamAOE, 50)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        Source = null; // just in case, if mechanic was not finished properly, reset on next cast start
        if ((AID)spell.Action.ID == AID.RapidSequenceParticleBeam)
        {
            NumCasts = 0;
            Source = caster;
            Activation = Module.CastFinishAt(spell, 0.8f);
            // TODO: not sure how targets are selected, assume it's first healer of each alliance
            BitMask selectedTargetsInAlliance = default;
            foreach (var (i, p) in Raid.WithSlot())
            {
                if (p.Role == Role.Healer && !selectedTargetsInAlliance[i >> 3])
                {
                    PlayerRoles[i] = PlayerRole.TargetNotFirst;
                    selectedTargetsInAlliance.Set(i >> 3);
                }
                else
                {
                    PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Share : PlayerRole.ShareNotFirst;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RapidSequenceParticleBeamAOE && ++NumCasts >= 12)
            Source = null;
    }
}

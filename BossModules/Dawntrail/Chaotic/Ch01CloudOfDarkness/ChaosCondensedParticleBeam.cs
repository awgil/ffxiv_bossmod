namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class ChaosCondensedParticleBeam(BossModule module) : Components.GenericWildCharge(module, 3, default, 50)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ChaosCondensedParticleBeam)
        {
            Source = caster;
            Activation = Module.CastFinishAt(spell, 0.7f);
            foreach (var (i, p) in Raid.WithSlot(true))
                PlayerRoles[i] = p.Role == Role.Tank ? PlayerRole.Target : PlayerRole.ShareNotFirst;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ChaosCondensedParticleBeamAOE1 or AID.ChaosCondensedParticleBeamAOE2)
        {
            ++NumCasts;
            Source = null;
        }
    }
}

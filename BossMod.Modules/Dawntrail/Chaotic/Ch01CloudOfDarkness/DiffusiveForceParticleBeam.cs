namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

// note: it seems that normally first wave (radius 7) hits people inside, and second wave (radius 5) hits people outside
// however, if some of the players in the mid are dead, some players on the outside will be hit by first wave (up to a total of 12 hits)
// if there are <= 12 players alive, everyone will be hit by the first wave, and the second wave will never happen
// so for safety we just show larger radius around everyone
// TODO: show second wave for players not hit by first wave
class DiffusiveForceParticleBeam(BossModule module) : Components.UniformStackSpread(module, 0, 7)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DiffusiveForceParticleBeam)
            AddSpreads(Raid.WithoutSlot(true), Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DiffusiveForceParticleBeamAOE1 or AID.DiffusiveForceParticleBeamAOE2)
        {
            ++NumCasts;
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
        }
    }
}

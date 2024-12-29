namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

// TODO: who gets radius 7 and who gets radius 5?
// TODO: show for second wave too...
class DiffusiveForceParticleBeam(BossModule module) : Components.UniformStackSpread(module, 0, 7)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DiffusiveForceParticleBeam)
            AddSpreads(Raid.WithoutSlot(), Module.CastFinishAt(spell, 0.7f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.DiffusiveForceParticleBeamAOE1 or AID.DiffusiveForceParticleBeamAOE2)
            Spreads.Clear();
    }
}

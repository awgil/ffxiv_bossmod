namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

class ActivePivotParticleBeam(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeRect _shape = new(40, 9, 40);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var rotation = (AID)spell.Action.ID switch
        {
            AID.ActivePivotParticleBeamCW => -22.5f.Degrees(),
            AID.ActivePivotParticleBeamCCW => 22.5f.Degrees(),
            _ => default
        };
        if (rotation != default)
            Sequences.Add(new(_shape, caster.Position, spell.Rotation, rotation, Module.CastFinishAt(spell, 0.6f), 1.6f, 5));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ActivePivotParticleBeamAOE && Sequences.Count > 0)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

namespace BossMod.Dawntrail.Ultimate.FRU;

// TODO: can target change if boss is provoked mid cast?
class P4AkhMorn(BossModule module) : Components.UniformStackSpread(module, 4, 0, 4)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AkhMornOracle or AID.AkhMornUsurper && WorldState.Actors.Find(caster.TargetID) is var target && target != null)
            AddStack(target, Module.CastFinishAt(spell, 0.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornAOEOracle)
            ++NumCasts;
    }
}

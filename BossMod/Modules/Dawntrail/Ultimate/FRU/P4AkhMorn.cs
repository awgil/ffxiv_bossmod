namespace BossMod.Dawntrail.Ultimate.FRU;

class P4AkhMorn(BossModule module) : Components.UniformStackSpread(module, 4, 0, 4)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornOracle)
            AddStacks(Raid.WithoutSlot(true).Where(p => p.Role == Role.Tank), Module.CastFinishAt(spell, 0.9f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AkhMornAOEOracle)
            ++NumCasts;
    }
}

namespace BossMod.Stormblood.Ultimate.UCOB;

class P5Enrage(BossModule module) : Components.UniformStackSpread(module, 0, 4)
{
    public int NumCasts;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Enrage)
            AddSpreads(Raid.WithoutSlot(true), spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Enrage or AID.EnrageAOE)
            ++NumCasts;
    }
}

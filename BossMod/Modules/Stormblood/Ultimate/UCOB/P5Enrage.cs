namespace BossMod.Stormblood.Ultimate.UCOB;

class P5Enrage : Components.UniformStackSpread
{
    public int NumCasts;

    public P5Enrage() : base(0, 4) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Enrage)
            AddSpreads(module.Raid.WithoutSlot(true), spell.NPCFinishAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Enrage or AID.EnrageAOE)
            ++NumCasts;
    }
}

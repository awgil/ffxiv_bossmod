namespace BossMod.Endwalker.Savage.P11SThemis;

class Styx : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public Styx() : base(6, 0, 8) { }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Styx && module.WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddStack(target, spell.NPCFinishAt);
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Styx or AID.StyxAOE)
            ++NumCasts;
    }
}

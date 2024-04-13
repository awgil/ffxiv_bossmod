namespace BossMod.Endwalker.Savage.P11SThemis;

class Styx(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8)
{
    public int NumCasts { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Styx && WorldState.Actors.Find(spell.TargetID) is var target && target != null)
            AddStack(target, spell.NPCFinishAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.Styx or AID.StyxAOE)
            ++NumCasts;
    }
}

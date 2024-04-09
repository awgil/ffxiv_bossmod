namespace BossMod.Endwalker.Extreme.Ex1Zodiark;

class Styx(BossModule module) : Components.UniformStackSpread(module, 5, 0, 8)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.StyxAOE)
            ++NumCasts;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Styx)
            AddStack(actor);
    }
}

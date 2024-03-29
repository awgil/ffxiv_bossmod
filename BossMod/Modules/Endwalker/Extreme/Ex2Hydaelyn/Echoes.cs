namespace BossMod.Endwalker.Extreme.Ex2Hydaelyn;

class Echoes : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public Echoes() : base(6, 0, 8) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.EchoesAOE)
            ++NumCasts;
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Echoes)
            AddStack(actor);
    }
}

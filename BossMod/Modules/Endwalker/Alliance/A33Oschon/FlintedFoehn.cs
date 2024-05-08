namespace BossMod.Endwalker.Alliance.A33Oschon;

class P1FlintedFoehn(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlintedFoehnP1AOE)
            ++NumCasts;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlintedFoehn)
            AddStack(actor, WorldState.FutureTime(5.1f));
    }
}

class P2FlintedFoehn(BossModule module) : Components.UniformStackSpread(module, 8, 0, 8)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlintedFoehnP2AOE)
            ++NumCasts;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlintedFoehn)
            AddStack(actor, WorldState.FutureTime(5.1f));
    }
}

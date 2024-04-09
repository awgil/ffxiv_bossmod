namespace BossMod.Endwalker.Alliance.A33Oschon;

class FlintedFoehnP1(BossModule module) : Components.UniformStackSpread(module, 6, 0)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlintedFoehnStack)
        {
            ++NumCasts;
            if (NumCasts == 6)
            {
                Stacks.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlintedFoehnStack)
            AddStack(actor, WorldState.FutureTime(4.5f));
    }
}

class FlintedFoehnP2(BossModule module) : Components.UniformStackSpread(module, 8, 0)
{
    public int NumCasts { get; private set; }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FlintedFoehnStackP2)
        {
            ++NumCasts;
            if (NumCasts == 6)
            {
                Stacks.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlintedFoehnStack)
            AddStack(actor, WorldState.FutureTime(4.5f));
    }
}

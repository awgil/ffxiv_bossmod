namespace BossMod.Endwalker.Alliance.A33Oschon;

class FlintedFoehnP1 : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public FlintedFoehnP1() : base(6, 0) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlintedFoehnStack)
            AddStack(actor, module.WorldState.CurrentTime.AddSeconds(4.5f));
    }
}

class FlintedFoehnP2 : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public FlintedFoehnP2() : base(8, 0) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
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

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.FlintedFoehnStack)
            AddStack(actor, module.WorldState.CurrentTime.AddSeconds(4.5f));
    }
}

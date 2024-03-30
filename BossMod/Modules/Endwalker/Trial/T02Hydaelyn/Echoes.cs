namespace BossMod.Endwalker.Trial.T02Hydaelyn;

class Echoes : Components.UniformStackSpread
{
    public int NumCasts { get; private set; }

    public Echoes() : base(6, 0, 8) { }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Echoes)
        {
            ++NumCasts;
            if (NumCasts == 5)
            {
                Stacks.Clear();
                NumCasts = 0;
            }
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Echoes)
            AddStack(actor);
    }
}

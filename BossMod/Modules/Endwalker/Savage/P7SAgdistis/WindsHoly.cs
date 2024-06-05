namespace BossMod.Endwalker.Savage.P7SAgdistis;

class WindsHoly(BossModule module) : Components.UniformStackSpread(module, 6, 7, 4)
{
    public int NumCasts { get; private set; }
    private readonly List<Actor>[] _futureStacks = [[], [], [], []];
    private readonly List<Actor>[] _futureSpreads = [[], [], [], []];

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.InviolateWinds1:
            case SID.PurgatoryWinds1:
                AddSpread(actor);
                break;
            case SID.InviolateWinds2:
            case SID.PurgatoryWinds2:
                _futureSpreads[0].Add(actor);
                break;
            case SID.PurgatoryWinds3:
                _futureSpreads[1].Add(actor);
                break;
            case SID.PurgatoryWinds4:
                _futureSpreads[2].Add(actor);
                break;
            case SID.HolyBonds1:
            case SID.HolyPurgation1:
                AddStack(actor);
                break;
            case SID.HolyBonds2:
            case SID.HolyPurgation2:
                _futureStacks[0].Add(actor);
                break;
            case SID.HolyPurgation3:
                _futureStacks[1].Add(actor);
                break;
            case SID.HolyPurgation4:
                _futureStacks[2].Add(actor);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.HemitheosHolyExpire)
        {
            Stacks.Clear();
            Spreads.Clear();
            AddStacks(_futureStacks[NumCasts]);
            AddSpreads(_futureSpreads[NumCasts]);
            ++NumCasts;
        }
    }
}

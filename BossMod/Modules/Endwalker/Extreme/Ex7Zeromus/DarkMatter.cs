namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class DarkMatter(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly List<int> _remainingCasts = [];

    private static readonly AOEShapeCircle _shape = new(8);

    public int RemainingCasts => _remainingCasts.Count > 0 ? _remainingCasts.Min() : 0;

    public override void Update()
    {
        for (int i = CurrentBaits.Count - 1; i >= 0; i--)
        {
            if (CurrentBaits[i].Target.IsDestroyed || CurrentBaits[i].Target.IsDead)
            {
                CurrentBaits.RemoveAt(i);
                _remainingCasts.RemoveAt(i);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.DarkMatter)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
            _remainingCasts.Add(3);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkMatterAOE)
        {
            ++NumCasts;
            var index = CurrentBaits.FindIndex(b => b.Target.InstanceID == spell.MainTargetID);
            if (index >= 0)
            {
                --_remainingCasts[index];
            }
        }
    }
}

class ForkedLightningDarkBeckons(BossModule module) : Components.UniformStackSpread(module, 6, 5, 4, alwaysShowSpreads: true)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ForkedLightning)
            AddSpread(actor, status.ExpireAt);
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        switch ((IconID)iconID)
        {
            case IconID.DarkBeckonsUmbralRays:
                AddStack(actor, WorldState.FutureTime(5.1f));
                break;
            case IconID.DarkMatter:
                foreach (ref var s in Stacks.AsSpan())
                    s.ForbiddenPlayers.Set(Raid.FindSlot(actor.InstanceID));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ForkedLightning or AID.DarkBeckons)
        {
            Spreads.Clear();
            Stacks.Clear();
        }
    }
}

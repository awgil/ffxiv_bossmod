namespace BossMod.Dawntrail.Foray.ForkedTower.FT02DeadStars;

class PrimordialChaos(BossModule module) : Components.RaidwideCastDelay(module, AID.PrimordialChaosCast, AID.PrimordialChaos, 1.3f);

class Ooze(BossModule module) : Components.GenericAOEs(module)
{
    private readonly int[] _states = new int[PartyState.MaxPartySize];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var ele = Math.Sign(_states.BoundSafeAt(slot, 0));

        foreach (var aoe in _predicted.Take(2))
        {
            var safe = ele == -aoe.Element;
            yield return new AOEInstance(new AOEShapeCircle(22), aoe.Center, default, aoe.Predicted, safe ? ArenaColor.SafeFromAOE : ArenaColor.AOE, Risky: !safe);
        }
    }

    record struct Cast(WPos Center, int Element, DateTime Predicted);

    private readonly List<Cast> _predicted = [];

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var resolve = WorldState.FutureTime(9.1f + _predicted.Count / 2 * 3);
        switch ((AID)spell.Action.ID)
        {
            case AID.FrozenFalloutFireCast:
                _predicted.Add(new(caster.Position, 1, resolve));
                break;
            case AID.FrozenFalloutIceCast:
                _predicted.Add(new(caster.Position, -1, resolve));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FrozenFalloutIndicator)
        {
            NumCasts++;
            if (_predicted[0].Center.AlmostEqual(caster.Position, 1))
                _predicted.RemoveAt(0);
            else if (_predicted[1].Center.AlmostEqual(caster.Position, 1))
                _predicted.RemoveAt(1);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.NovaOoze or SID.IceOoze && Raid.TryFindSlot(actor, out var slot))
        {
            var extra = (int)status.Extra;
            if ((SID)status.ID == SID.IceOoze)
                extra = -extra;
            _states[slot] = extra;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.NovaOoze or SID.IceOoze && Raid.TryFindSlot(actor, out var slot))
            _states[slot] = 0;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var st = _states.BoundSafeAt(slot);
        if (st != 0)
        {
            var stacks = Math.Abs(st);
            var ele = st < 0 ? "Ice" : "Fire";
            hints.Add($"Debuff: {ele} x{stacks}", false);
        }
    }
}

class NoxiousNova(BossModule module) : Components.RaidwideCastDelay(module, AID.NoxiousNovaCast, AID.NoxiousNova, 0.8f);

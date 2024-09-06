namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class FireScourgeOfFire(BossModule module) : Components.UniformStackSpread(module, 5, 0, 4)
{
    private readonly List<int> _remainingCasts = [];

    // unfortunately, targets can die...
    public int RemainingCasts()
    {
        var max = 0;
        for (int i = 0; i < Stacks.Count; ++i)
        {
            if (Stacks[i].Target.IsDead)
                continue;
            if (_remainingCasts[i] > max)
                max = _remainingCasts[i];
        }
        return max;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.CalamitysInferno)
        {
            AddStack(actor, WorldState.FutureTime(7.1f));
            _remainingCasts.Add(3);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FireScourgeOfFire)
        {
            var index = Stacks.FindIndex(s => s.Target.InstanceID == spell.MainTargetID);
            if (index >= 0)
                --_remainingCasts[index];
        }
    }
}

class FireScourgeOfFireVoidzone(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, ActionID.MakeSpell(AID.FireScourgeOfFire), module => module.Enemies(OID.ScourgeOfFireVoidzone).Where(z => z.EventState != 7), 0.9f);

class FireScourgeOfIce(BossModule module) : Components.StayMove(module)
{
    public int NumImminent;
    public int NumActiveFreezes;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FreezingUp)
            ++NumActiveFreezes;
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.FreezingUp)
            --NumActiveFreezes;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.CalamitysChill && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            PlayerStates[slot] = new(Requirement.Move, default);
            ++NumImminent;
        }
    }
}

class IceScourgeOfFireIce(BossModule module) : Components.IconStackSpread(module, (uint)IconID.CalamitysInferno, (uint)IconID.CalamitysChill, ActionID.MakeSpell(AID.IceScourgeOfFire), ActionID.MakeSpell(AID.IceScourgeOfIce), 5, 16, 7.1f, 3, 3, true);
class FireIceScourgeOfThunder(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.CalamitysBolt, ActionID.MakeSpell(AID.FireIceScourgeOfThunder), 5, 7.1f);

// TODO: add hint if player and stack target has different levitate states
class ThunderScourgeOfFire(BossModule module) : Components.StackWithIcon(module, (uint)IconID.CalamitysInferno, ActionID.MakeSpell(AID.ThunderScourgeOfFire), 5, 7.1f, 4, 4);

// TODO: verify spread radius for ice boulders...
class ThunderScourgeOfIceThunder(BossModule module) : Components.UniformStackSpread(module, 0, 8, alwaysShowSpreads: true)
{
    public int NumCasts;
    private readonly ThunderPlatform? _platform = module.FindComponent<ThunderPlatform>();

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if ((IconID)iconID is IconID.CalamitysBolt or IconID.CalamitysChill)
        {
            AddSpread(actor, WorldState.FutureTime(7.1f));
            var slot = Raid.FindSlot(actor.InstanceID);
            if (slot >= 0 && _platform != null)
            {
                _platform.RequireHint[slot] = true;
                _platform.RequireLevitating[slot] = iconID == (uint)IconID.CalamitysChill;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThunderScourgeOfThunderFail or AID.ThunderScourgeOfThunder)
        {
            ++NumCasts;
            Spreads.Clear();
        }
    }
}

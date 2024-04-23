namespace BossMod.Shadowbringers.Ultimate.TEA;

// TODO: consider drawing tethers & shared sentence?..
class P4FateCalibrationBetaDebuffs(BossModule module) : P4ForcedMarchDebuffs(module)
{
    private enum Color { Unknown, Light, Dark }

    private readonly P4FateProjection? _proj = module.FindComponent<P4FateProjection>();
    private readonly Color[] _colors = new Color[PartyState.MaxPartySize];
    private readonly int[] _farTethers = [-1, -1];
    private readonly int[] _nearTethers = [-1, -1];
    private int _sharedSentence = -1;

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.FateCalibrationBeta:
                var fcbSlot = _proj?.ProjectionOwner(actor.InstanceID) ?? -1;
                if (fcbSlot >= 0)
                {
                    _colors[fcbSlot] = status.Extra switch
                    {
                        0x84 => Color.Light,
                        0x85 => Color.Dark,
                        _ => Color.Unknown
                    };
                }
                break;
            case SID.ContactProhibitionOrdained:
            case SID.ContactRegulationOrdained:
            case SID.EscapeProhibitionOrdained:
            case SID.EscapeDetectionOrdained:
                Done = true;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        switch ((TetherID)tether.ID)
        {
            case TetherID.RestrainingOrder:
                _farTethers[0] = _proj?.ProjectionOwner(source.InstanceID) ?? -1;
                _farTethers[1] = _proj?.ProjectionOwner(tether.Target) ?? -1;
                break;
            case TetherID.HouseArrest:
                _nearTethers[0] = _proj?.ProjectionOwner(source.InstanceID) ?? -1;
                _nearTethers[1] = _proj?.ProjectionOwner(tether.Target) ?? -1;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FateCalibrationAlphaSharedSentence:
                _sharedSentence = _proj?.ProjectionOwner(spell.MainTargetID) ?? -1;
                // note: at this point, we can distinguish light beacon vs light untethered (shared sentence hits untethered), but not darks
                InitColor(Color.Light, GuessLightBeacon());
                break;
            case AID.FateCalibrationBetaKillBeaconSpread:
            case AID.FateCalibrationBetaKillBeaconStack:
                // these are always cast at two beacons
                var slot = _proj?.ProjectionOwner(spell.MainTargetID) ?? -1;
                if (slot >= 0)
                    InitColor(_colors[slot], slot);
                break;
        }
    }

    protected override WDir SafeSpotDirection(int slot) => Debuffs[slot] switch
    {
        Debuff.LightBeacon => new(-8, -16), // between N and NW
        Debuff.LightFollow => new(11, -2), // slightly N from E
        Debuff.DarkBeacon => new(14, 0), // E
        _ => new(11, _nearTethers.Contains(slot) ? -2 : _farTethers.Contains(slot) ? +2 : 0), // around E, depending on tether
    };

    private int GuessLightBeacon()
    {
        for (int i = 0; i < PartyState.MaxPartySize; ++i)
            if (_colors[i] == Color.Light && i != _sharedSentence && !_farTethers.Contains(i) && !_nearTethers.Contains(i))
                return i;
        return -1;
    }

    private void InitColor(Color color, int beacon)
    {
        for (int i = 0; i < PartyState.MaxPartySize; ++i)
        {
            if (_colors[i] != color)
                continue;

            if (i == beacon)
            {
                Debuffs[i] = color == Color.Light ? Debuff.LightBeacon : Debuff.DarkBeacon;
                if (color == Color.Light)
                    LightBeacon = Raid[i];
                else
                    DarkBeacon = Raid[i];
            }
            else
            {
                Debuffs[i] = color == Color.Light ? Debuff.LightFollow : Debuff.DarkFollow;
            }
        }
    }
}

class P4FateCalibrationBetaJJump(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private bool _enabled;
    private readonly List<Actor> _jumpers = [];

    private static readonly AOEShapeCircle _shape = new(10);

    public void Show() => _enabled = true;

    public override void Update()
    {
        CurrentBaits.Clear();
        if (!_enabled)
            return;

        foreach (var source in _jumpers)
        {
            var target = Raid.WithoutSlot().Farthest(source.Position);
            if (target != null)
                CurrentBaits.Add(new(source, target, _shape));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FateCalibrationBetaJump:
                _jumpers.Add(caster);
                break;
            case AID.FateCalibrationBetaJJump:
                ++NumCasts;
                _jumpers.Remove(caster);
                break;
        }
    }
}

class P4FateCalibrationBetaOpticalSight(BossModule module) : Components.UniformStackSpread(module, 6, 6, 4)
{
    private enum Mechanic { Unknown, Stack, Spread }

    public bool Done { get; private set; }
    private Mechanic _mechanic;
    private readonly List<Actor> _stackTargets = [];

    public void Show()
    {
        switch (_mechanic)
        {
            case Mechanic.Stack:
                AddStacks(_stackTargets, WorldState.FutureTime(6.1f));
                break;
            case Mechanic.Spread:
                AddSpreads(Raid.WithoutSlot(true), WorldState.FutureTime(6.1f));
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_mechanic != Mechanic.Unknown)
            hints.Add($"{_mechanic} after jumps");
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FateCalibrationBetaKillBeaconSpread:
                _mechanic = Mechanic.Spread;
                break;
            case AID.FateCalibrationBetaKillBeaconStack:
                _mechanic = Mechanic.Stack;
                var target = Raid[Module.FindComponent<P4FateProjection>()?.ProjectionOwner(spell.MainTargetID) ?? -1];
                if (target != null)
                    _stackTargets.Add(target);
                break;
            case AID.IndividualReprobation:
                Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                Done = true;
                break;
            case AID.CollectiveReprobation:
                Stacks.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
                Done = true;
                break;
        }
    }
}

class P4FateCalibrationBetaRadiantSacrament(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _caster;
    private bool _enabled;

    private static readonly AOEShapeDonut _shape = new(10, 60); // TODO: verify inner radius

    public void Show() => _enabled = true;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_enabled && _caster != null)
            yield return new(_shape, _caster.Position);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FateCalibrationBetaDonut:
                _caster = caster;
                break;
            case AID.FateCalibrationBetaRadiantSacrament:
                ++NumCasts;
                _caster = null;
                break;
        }
    }
}

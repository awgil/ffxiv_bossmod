namespace BossMod.Endwalker.Ultimate.DSW2;

class P6MortalVow : Components.UniformStackSpread
{
    public int Progress { get; private set; } // 0 before application, N before Nth pass
    private readonly DSW2Config _config = Service.Config.Get<DSW2Config>();
    private Actor? _vow;
    private Actor? _target;
    private DateTime _vowExpiration;
    private readonly DateTime[] _atonementExpiration = new DateTime[PartyState.MaxPartySize];

    public P6MortalVow(BossModule module) : base(module, 5, 5, 2, 2, true, false)
    {
        // prepare for initial application on random DD
        AddSpreads(Raid.WithoutSlot(true).Where(p => p.Class.IsDD())); // TODO: activation
    }

    public void ShowNextPass()
    {
        if (_vow == null)
            return;
        _target = DetermineNextPassTarget();
        var forbidden = _target != null ? Raid.WithSlot(true).Exclude(_target).Mask() : Raid.WithSlot(true).WhereSlot(i => _atonementExpiration[i] < _vowExpiration).Mask();
        AddStack(_vow, _vowExpiration, forbidden);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_vow != null && _target != null && (actor == _vow || actor == _target))
            movementHints.Add(actor.Position, (actor == _vow ? _target : _vow).Position, ArenaColor.Safe);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_vow != null && _target != null && (pc == _vow || pc == _target))
            Arena.AddCircle(Module.Center, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MortalVow:
                _vow = actor;
                _vowExpiration = status.ExpireAt;
                break;
            case SID.MortalAtonement:
                if (Raid.TryFindSlot(actor, out var slot))
                    _atonementExpiration[slot] = status.ExpireAt;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MortalVow:
                if (_vow == actor)
                    _vow = null;
                break;
            case SID.MortalAtonement:
                if (Raid.TryFindSlot(actor, out var slot))
                    _atonementExpiration[slot] = default;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.MortalVowApply or AID.MortalVowPass)
        {
            ++Progress;
            Spreads.Clear();
            Stacks.Clear();
            _target = null;
        }
    }

    private Actor? DetermineNextPassTarget()
    {
        if (_config.P6MortalVowOrder == DSW2Config.P6MortalVow.None)
            return null;

        var assignments = Service.Config.Get<PartyRolesConfig>().SlotsPerAssignment(Raid);
        if (assignments.Length == 0)
            return null; // if assignments are unset, we can't define pass priority

        var role = Progress switch
        {
            1 => PartyRolesConfig.Assignment.MT,
            2 => PartyRolesConfig.Assignment.OT,
            3 => CanPassVowTo(assignments, PartyRolesConfig.Assignment.M1) ? PartyRolesConfig.Assignment.M1 : PartyRolesConfig.Assignment.M2,
            4 => _config.P6MortalVowOrder == DSW2Config.P6MortalVow.TanksMeleeR1 ? PartyRolesConfig.Assignment.R1 : PartyRolesConfig.Assignment.R2,
            _ => PartyRolesConfig.Assignment.Unassigned
        };
        return role != PartyRolesConfig.Assignment.Unassigned && CanPassVowTo(assignments, role) ? Raid[assignments[(int)role]] : null;
    }

    private bool CanPassVowTo(int[] assignments, PartyRolesConfig.Assignment role) => _atonementExpiration[assignments[(int)role]] < _vowExpiration;
}

namespace BossMod.Dawntrail.Ultimate.FRU;

class P3Apocalypse(BossModule module) : Components.GenericAOEs(module)
{
    private Angle? _starting;
    private Angle _rotation;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(9);

    public void Show(float delay)
    {
        void addAOE(WPos pos, DateTime activation) => _aoes.Add(new(_shape, pos, default, activation));
        void addPair(WDir offset, DateTime activation)
        {
            addAOE(Module.Center + offset, activation);
            addAOE(Module.Center - offset, activation);
        }
        void addAt(int position, DateTime activation)
        {
            if (position >= 0 && _starting != null)
                addPair(14 * (_starting.Value + _rotation * position).ToDirection(), activation);
            else if (position == -1)
                addPair(default, activation);
        }

        var activation = WorldState.FutureTime(delay);
        for (int i = -1; i < 6; ++i)
        {
            addAt(i + 1, activation);
            addAt(i, activation);
            addAt(i - 1, activation);
            activation = activation.AddSeconds(2);
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(6);

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.ApocalypseLight)
        {
            if (actor.Position.AlmostEqual(Module.Center, 1))
            {
                if (_starting == null)
                    _starting = actor.Rotation;
                else if (!_starting.Value.AlmostEqual(actor.Rotation + 180.Degrees(), 0.1f))
                    ReportError($"Inconsistent starting dir");
            }
            else
            {
                var rot = 0.5f * (actor.Rotation - Angle.FromDirection(actor.Position - Module.Center)).Normalized();
                if (_rotation == default)
                    _rotation = rot;
                else if (!_rotation.AlmostEqual(rot, 0.1f))
                    ReportError($"Inconsistent rotation dir");
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ApocalypseAOE)
        {
            ++NumCasts;
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            if (index >= 0)
                _aoes.RemoveAt(index);
            else
                ReportError($"Failed to find aoe @ {caster.Position}");
        }
    }
}

class P3ApocalypseDarkWater(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4, includeDeadTargets: true)
{
    public struct State
    {
        public int Order;
        public int AssignedGroup;
        public DateTime Expiration;
    }

    public int NumStatuses;
    public readonly State[] States = new State[PartyState.MaxPartySize];
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private string _swaps = "";

    // for uptime swaps, there are 6 possible swaps within each 'subgroup': no swaps, p1 with p1/p2, p2 with p1/p2 and both
    private static readonly BitMask[] _uptimeSwaps = [default, BitMask.Build(0, 4), BitMask.Build(0, 5), BitMask.Build(1, 4), BitMask.Build(1, 5), BitMask.Build(0, 1, 4, 5)];

    public void ShowOrder(int order)
    {
        for (int i = 0; i < States.Length; ++i)
            if (States[i].Order == order && Raid[i] is var player && player != null)
                AddStack(player, States[i].Expiration);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref var state = ref States[slot];
        if (state.AssignedGroup > 0)
            hints.Add($"Group: {state.AssignedGroup}", false);
        if (state.Order > 0)
            hints.Add($"Order: {state.Order}", false);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_swaps.Length > 0)
            hints.Add(_swaps);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkWater && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            States[slot].Expiration = status.ExpireAt;
            States[slot].Order = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
            {
                < 15 => 1,
                < 34 => 2,
                _ => 3,
            };
            if (++NumStatuses == 6)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkWater)
            Stacks.Clear();
    }

    private void InitAssignments()
    {
        Span<int> assignmentPerSlot = [-1, -1, -1, -1, -1, -1, -1, -1];
        Span<int> slotPerAssignment = [-1, -1, -1, -1, -1, -1, -1, -1];
        foreach (var (slot, group) in _config.P3ApocalypseAssignments.Resolve(Raid))
        {
            States[slot].AssignedGroup = group < 4 ? 1 : 2;
            assignmentPerSlot[slot] = group;
            slotPerAssignment[group] = slot;
        }

        if (slotPerAssignment[0] < 0)
            return; // no valid assignments

        var swap = _config.P3ApocalypseUptime ? FindUptimeSwap(slotPerAssignment) : FindStandardSwap(slotPerAssignment);
        for (int role = 0; role < slotPerAssignment.Length; ++role)
        {
            if (swap[role])
            {
                var slot = slotPerAssignment[role];
                ref var state = ref States[slot];
                state.AssignedGroup = 3 - state.AssignedGroup;
                if (_swaps.Length > 0)
                    _swaps += ", ";
                _swaps += Raid[slot]?.Name ?? "";
            }
        }
        _swaps = $"Swaps: {(_swaps.Length > 0 ? _swaps : "none")}";
    }

    private bool IsSwapValid(BitMask assignmentSwaps, ReadOnlySpan<int> slotPerAssignment)
    {
        BitMask result = default; // bits 0-3 are set if order N is in G1, 4-7 for G2
        for (int role = 0; role < slotPerAssignment.Length; ++role)
        {
            ref var state = ref States[slotPerAssignment[role]];
            var isGroup2 = state.AssignedGroup == (assignmentSwaps[role] ? 1 : 2);
            result.Set(state.Order + (isGroup2 ? 4 : 0));
        }
        return result.Raw == 0xFF;
    }

    private BitMask FindUptimeSwap(ReadOnlySpan<int> slotPerAssignment)
    {
        // search for first valid swap, starting with swaps that don't touch higher prios
        foreach (var highSwap in _uptimeSwaps)
        {
            foreach (var lowSwap in _uptimeSwaps)
            {
                var swap = lowSwap ^ new BitMask(highSwap.Raw << 2);
                if (IsSwapValid(swap, slotPerAssignment))
                    return swap;
            }
        }
        ReportError("Failed to find uptime swap");
        return FindStandardSwap(slotPerAssignment);
    }

    private BitMask FindStandardSwap(ReadOnlySpan<int> slotPerAssignment)
    {
        BitMask swap = default;
        Span<int> assignmentPerOrder = [-1, -1, -1, -1];
        for (int role = 0; role < slotPerAssignment.Length; ++role)
        {
            var slot = slotPerAssignment[role];
            var order = States[slot].Order;
            ref var partner = ref assignmentPerOrder[order];
            if (partner < 0)
                partner = role;
            else if ((role < 4) == (partner < 4))
                swap.Set(partner);
            // else: partner is naturally in other group
        }
        return swap;
    }
}

class P3SpiritTaker(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SpiritTaker)
            AddSpreads(Raid.WithoutSlot(true), Module.CastFinishAt(spell, 0.3f));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SpiritTakerAOE)
            Spreads.Clear();
    }
}

class P3ApocalypseDarkEruption(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.DarkEruption, ActionID.MakeSpell(AID.DarkEruption), 6, 5.1f);

class P3DarkestDanceBait(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.DarkestDanceBait), centerAtTarget: true)
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(8);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null && Raid.WithoutSlot().Farthest(_source.Position) is var target && target != null)
        {
            CurrentBaits.Add(new(_source, target, _shape, _activation));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DarkestDance)
        {
            ForbiddenPlayers = Raid.WithSlot(true).WhereActor(p => p.Role != Role.Tank).Mask();
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.4f);
        }
    }
}

namespace BossMod.Dawntrail.Ultimate.FRU;

// TODO: hints etc...
class P3UltimateRelativity(BossModule module) : Components.CastCounter(module, default)
{
    public struct PlayerState
    {
        public int FireOrder;
        public int RewindOrder;
        public int LaserOrder;
        public bool HaveDark;
        public WDir AssignedDir;
    }

    public readonly PlayerState[] States = new PlayerState[PartyState.MaxPartySize];
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private WDir _relNorth;
    private int _numYellowTethers;
    private DateTime _nextProgress;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (States[slot].RewindOrder == 0)
            return;

        var isSupport = actor.Class.IsSupport();
        var hint = string.Join(" > ", Enumerable.Range(NumCasts, 7 - NumCasts).Select(i => Hint(States[slot], isSupport, i)));
        hints.Add(hint, false);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var assignedDir = States[pcSlot].AssignedDir;
        if (assignedDir != default)
            Arena.AddCircle(Module.Center + RangeHint(States[pcSlot], pc.Class.IsSupport(), NumCasts) * assignedDir, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SpellInWaitingDarkFire:
                var slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    ref var state = ref States[slot];
                    state.FireOrder = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
                    {
                        < 15 => 1,
                        < 25 => 2,
                        _ => 3
                    };
                    state.LaserOrder = state.FireOrder switch
                    {
                        3 => 1,
                        1 => 2,
                        _ => 3,
                    };
                }
                break;
            case SID.SpellInWaitingDarkBlizzard:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].LaserOrder = actor.Class.IsSupport() ? 2 : 1;
                break;
            case SID.SpellInWaitingDarkEruption:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].HaveDark = true;
                break;
            case SID.SpellInWaitingReturn:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].RewindOrder = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 20 ? 1 : 2;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.DelightsHourglass && tether.ID == (uint)TetherID.UltimateRelativityQuicken)
        {
            _relNorth += source.Position - Module.Center;
            if (++_numYellowTethers == 3)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.UltimateRelativityUnholyDarkness or AID.UltimateRelativitySinboundMeltdownAOEFirst && WorldState.CurrentTime > _nextProgress)
        {
            ++NumCasts;
            _nextProgress = WorldState.FutureTime(1);
        }
    }

    private void InitAssignments()
    {
        // determine who flexes
        Span<(int slot, int prio)> eastFlex = [(-1, -1), (-1, -1)]; // [support, dd]
        foreach (var (slot, group) in _config.P3UltimateRelativityAssignment.Resolve(Raid))
        {
            var player = Raid[slot];
            if (player == null)
                continue;

            var isSupport = player.Class.IsSupport();
            if (States[slot].FireOrder != (isSupport ? 3 : 1))
                continue; // fixed assignment

            ref var flex = ref eastFlex[isSupport ? 0 : 1];
            if (group > flex.prio)
                flex = (slot, group);
        }

        // calculate positions
        var northDir = Angle.FromDirection(_relNorth);
        for (int i = 0; i < States.Length; ++i)
        {
            var player = Raid[i];
            if (player == null)
                continue;

            ref var state = ref States[i];
            var isSupport = player.Class.IsSupport();
            var eastSlot = eastFlex[isSupport ? 0 : 1].slot;
            bool? preferEast = eastSlot >= 0 ? eastSlot == i : null;
            var dir = player.Class.IsSupport() ? SupportDir(state.FireOrder, preferEast) : DDDir(state.FireOrder, preferEast);
            if (dir != null)
                state.AssignedDir = (northDir + dir.Value).ToDirection();
        }
    }

    private Angle? SupportDir(int fireOrder, bool? preferEast) => fireOrder switch
    {
        2 => 90.Degrees(),
        3 => preferEast == null ? null : preferEast.Value ? -45.Degrees() : 45.Degrees(),
        _ => 0.Degrees()
    };

    private Angle? DDDir(int fireOrder, bool? preferEast) => fireOrder switch
    {
        1 => preferEast == null ? null : preferEast.Value ? -135.Degrees() : 135.Degrees(),
        2 => -90.Degrees(),
        _ => 180.Degrees()
    };

    private float RangeHint(in PlayerState state, bool isSupport, int order) => order switch
    {
        0 => state.FireOrder == 1 ? 12 : 5,
        1 => state.LaserOrder == 1 || state.HaveDark ? 9.5f : 1,
        2 => state.FireOrder == 2 ? 12 : 1,
        3 => state.LaserOrder == 2 ? 9.5f : 2,
        4 => state.FireOrder == 3 ? 12 : 1,
        5 => state.LaserOrder == 3 ? 9.5f : 5,
        _ => 9.5f
    };

    // TODO: rethink this...
    private string Hint(in PlayerState state, bool isSupport, int order) => order switch
    {
        0 => state.FireOrder == 1 ? "Out" : "Stack", // 10s
        1 => state.LaserOrder == 1 ? "Laser" : state.HaveDark ? "Hourglass" : "Mid", // 15s
        2 => state.FireOrder == 2 ? "Out" : "Stack", // 20s
        3 => state.LaserOrder == 2 ? "Laser" : state.HaveDark ? "Hourglass" : "Mid", // 25s
        4 => state.FireOrder == 3 ? "Out" : "Stack", // 30s
        5 => state.LaserOrder == 3 ? "Laser" : "Mid", // 35s
        _ => "Look out"
    };
}

class P3UltimateRelativityDarkFireUnholyDarkness(BossModule module) : Components.UniformStackSpread(module, 6, 8, 5, alwaysShowSpreads: true)
{
    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SpellInWaitingUnholyDarkness:
                if ((status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 10)
                    AddStack(actor, status.ExpireAt);
                break;
            case SID.SpellInWaitingDarkFire:
                if ((status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 10)
                    AddSpread(actor, status.ExpireAt);
                break;
        }
    }
}

class P3UltimateRelativitySinboundMeltdownBait(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.UltimateRelativitySinboundMeltdownAOEFirst))
{
    private readonly P3UltimateRelativity? _rel = module.FindComponent<P3UltimateRelativity>();
    private readonly List<(Actor origin, Angle rotation, DateTime activation)> _rotations = [];

    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public Angle RotationAt(WPos pos) => _rotations.FirstOrDefault(r => r.origin.Position.AlmostEqual(pos, 1)).rotation;

    public override void Update()
    {
        CurrentBaits.Clear();
        for (int i = NumCasts; i < _rotations.Count; ++i)
        {
            var closest = Raid.WithoutSlot().Closest(_rotations[i].origin.Position);
            if (closest != null)
            {
                CurrentBaits.Add(new(_rotations[i].origin, closest, _shape, _rotations[i].activation));
            }
        }
    }

    // TODO: hints for proper baiting?..
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var shouldBait = (_rel?.States[slot].LaserOrder ?? 0) == CurrentOrder;
        var baitIndex = CurrentBaits.FindIndex(b => b.Target == actor);
        var isBaiting = baitIndex >= 0;
        if (isBaiting != shouldBait)
        {
            hints.Add(shouldBait ? "Bait laser!" : "GTFO from hourglass!");
        }

        if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from baited aoe!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);

        foreach (ref var b in CurrentBaits.AsSpan())
        {
            if (b.Target == pc && b.Source.Position.AlmostEqual(AssignedHourglass(pcSlot), 1))
            {
                // draw extra rotation hints for correctly baited hourglass
                var rot = RotationAt(b.Source.Position);
                for (int i = 1; i < 10; ++i)
                    _shape.Outline(Arena, b.Source.Position, b.Rotation + i * rot);
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((OID)actor.OID == OID.DelightsHourglass && (SID)status.ID == SID.DelightsHourglassRotation)
        {
            var rot = status.Extra switch
            {
                0x10D => 15.Degrees(),
                0x15C => -15.Degrees(),
                _ => default
            };
            if (rot != default)
                _rotations.Add((actor, rot, status.ExpireAt));
            else
                ReportError($"Unexpected rotation status param: {status.Extra:X}");
        }
    }

    private int CurrentOrder => NumCasts switch
    {
        < 3 => 1,
        < 6 => 2,
        < 8 => 3,
        _ => 0
    };

    private WPos AssignedHourglass(int slot) => Module.Center + 9.5f * (_rel?.States[slot].AssignedDir ?? default);
}

class P3UltimateRelativitySinboundMeltdownAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly P3UltimateRelativitySinboundMeltdownBait? _baits = module.FindComponent<P3UltimateRelativitySinboundMeltdownBait>();
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(50, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UltimateRelativitySinboundMeltdownAOEFirst:
                var rot = _baits?.RotationAt(caster.Position) ?? default;
                for (int i = 1; i < 10; ++i)
                    _aoes.Add(new(_shape, caster.Position, spell.Rotation + i * rot, WorldState.FutureTime(i + 1)));
                break;
            case AID.UltimateRelativitySinboundMeltdownAOERest:
                ++NumCasts;
                var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1) && aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
                if (count != 1)
                    ReportError($"Unexpected aoe at {caster.Position} -> {spell.Rotation}");
                break;
        }
    }
}

class P3UltimateRelativityDarkBlizzard(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.UltimateRelativityDarkBlizzard))
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeDonut _shape = new(2, 12); // TODO: verify inner radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_shape, _source.Position, default, _activation);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkBlizzard)
        {
            _source = actor;
            _activation = status.ExpireAt;
        }
    }
}

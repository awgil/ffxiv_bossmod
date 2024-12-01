namespace BossMod.Dawntrail.Ultimate.FRU;

// TODO: more positioning options?..
class P1FallOfFaith(BossModule module) : Components.CastCounter(module, default)
{
    private struct PlayerState
    {
        public int TetherOrder; // 0 if no tether, otherwise 1-4
        public bool? OddGroup;
        public WPos Spot1;
        public WPos Spot2;
    }

    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly PlayerState[] _states = new PlayerState[PartyState.MaxPartySize];
    private readonly List<Actor> _tetherTargets = [];
    private readonly List<Actor> _currentBaiters = [];
    private BitMask _fireTethers; // bit i is set if i'th tether is fire

    private static readonly AOEShapeCone _shapeFire = new(60, 45.Degrees());
    private static readonly AOEShapeCone _shapeLightning = new(60, 60.Degrees());

    public override void Update()
    {
        _currentBaiters.Clear();
        if (_tetherTargets.Count == 4 && NumCasts < _tetherTargets.Count)
        {
            var nextSource = _tetherTargets[NumCasts];
            _currentBaiters.AddRange(Raid.WithoutSlot().Exclude(nextSource).SortedByRange(nextSource.Position).Take(_fireTethers[NumCasts] ? 1 : 3));
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        ref var state = ref _states[slot];
        if (state.TetherOrder != 0)
            hints.Add($"Order: {state.TetherOrder}", false);
        else if (state.OddGroup != null)
            hints.Add($"Help group {(state.OddGroup.Value ? 1 : 2)}", false);

        if (ActiveBaits(slot, actor, true).Any(bait => bait.shape.Check(actor.Position, bait.origin, bait.dir)))
            hints.Add("GTFO from baited aoe!");
        // TODO: hint if actor is baiter while it's not his turn?
        // TODO: hint if actor is clipping others?
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_tetherTargets.Count > NumCasts)
            hints.Add(string.Join(" -> ", Enumerable.Range(NumCasts, _tetherTargets.Count - NumCasts).Select(i => _fireTethers[i] ? "Fire" : "Lightning")));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PlayerPriority.Normal;

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaits(pcSlot, pc, true))
            bait.shape.Draw(Arena, bait.origin, bait.dir, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaits(pcSlot, pc, false))
            bait.shape.Outline(Arena, bait.origin, bait.dir, _fireTethers[NumCasts] ? ArenaColor.Safe : ArenaColor.Danger);

        ref var state = ref _states[pcSlot];
        var firstBait = state.OddGroup == true ? 0 : 1;
        var safespot = NumCasts <= firstBait ? state.Spot1 : NumCasts <= firstBait + 2 ? state.Spot2 : default;
        if (safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.Fire or TetherID.Lightning)
        {
            _fireTethers[_tetherTargets.Count] = tether.ID == (uint)TetherID.Fire;

            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
                _tetherTargets.Add(target);

            var slot = Raid.FindSlot(tether.Target);
            if (slot >= 0)
            {
                var order = _states[slot].TetherOrder = _tetherTargets.Count;
                var odd = (order & 1) != 0;
                var firstBait = order <= 2;
                _states[slot].OddGroup = odd;
                _states[slot].Spot1 = TetherSpot(odd, !firstBait);
                _states[slot].Spot2 = TetherSpot(odd, firstBait);
            }

            if (_tetherTargets.Count == 4)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FallOfFaithSinsmite or AID.FallOfFaithSinblaze)
        {
            ++NumCasts;
        }
    }

    private void InitAssignments()
    {
        List<(int slot, int prio)> conga = [];
        foreach (var (slot, group) in _config.P1FallOfFaithAssignment.Resolve(Raid))
            if (_states[slot].TetherOrder == 0)
                conga.Add((slot, group));
        if (conga.Count != 4)
            return; // no assignments

        conga.SortBy(c => c.prio);
        InitNormalSpots(conga[0].slot, true, true);
        InitNormalSpots(conga[1].slot, true, false);
        InitNormalSpots(conga[2].slot, false, false);
        InitNormalSpots(conga[3].slot, false, true);
    }

    private WPos TetherSpot(bool odd, bool far)
    {
        var dir = _config.P1FallOfFaithEW ? 90.Degrees() : 0.Degrees();
        if (odd)
            dir -= 180.Degrees();
        return Module.Center + (far ? 7 : 4) * dir.ToDirection();
    }

    private WPos ProteanSpot(bool odd, bool close)
    {
        var baiter = TetherSpot(odd, false);
        var offset = close ? 3 : -3;
        WDir dir = _config.P1FallOfFaithEW ? new(0, -1) : new(1, 0);
        return baiter + offset * dir;
    }

    private WPos NormalSpot(bool odd, bool close, int order) => _fireTethers[order] ? TetherSpot(odd, true) : ProteanSpot(odd, close);

    private void InitNormalSpots(int slot, bool odd, bool close)
    {
        ref var state = ref _states[slot];
        state.OddGroup = odd;
        state.Spot1 = NormalSpot(odd, close, odd ? 0 : 1);
        state.Spot2 = NormalSpot(odd, close, odd ? 2 : 3);
    }

    private bool ShouldBeBaiting(int slot)
    {
        var nextBaitsOdd = (NumCasts & 1) == 0;
        ref var state = ref _states[slot];
        return state.OddGroup == null || state.OddGroup == nextBaitsOdd; // if there are no assignments, we don't actually know whether player should be baiting...
    }

    private IEnumerable<(AOEShapeCone shape, WPos origin, Angle dir)> ActiveBaits(int slot, Actor actor, bool wantDangerous)
    {
        if (_tetherTargets.Count < 4 || NumCasts >= _tetherTargets.Count)
            yield break;

        var isFire = _fireTethers[NumCasts];
        var shouldBait = ShouldBeBaiting(slot);
        var source = _tetherTargets[NumCasts];
        foreach (var target in _currentBaiters)
        {
            // if we shouldn't be baiting - all baits are dangerous, otherwise only other proteans are dangerous
            var dangerous = !shouldBait || !isFire && target != actor;
            if (dangerous == wantDangerous)
                yield return (isFire ? _shapeFire : _shapeLightning, source.Position, Angle.FromDirection(target.Position - source.Position));
        }
    }
}

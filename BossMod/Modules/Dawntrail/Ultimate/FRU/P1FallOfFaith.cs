namespace BossMod.Dawntrail.Ultimate.FRU;

// TODO: more positioning options?..
class P1FallOfFaith(BossModule module) : Components.CastCounter(module, default)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private readonly int[] _playerOrder = new int[PartyState.MaxPartySize]; // 0 if not assigned, 1-4 if tethered, 5-8 for conga help order (5/6 help group 1, 7/8 help group 2)
    private readonly List<Actor> _tetherTargets = [];
    private readonly List<Actor> _currentBaiters = [];
    private BitMask _fireTethers; // bit i is set if i'th tether is fire
    private int _numFetters;
    private DateTime _minHelpMove; // we want conga members to start moving with a slight delay

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
        var order = _playerOrder[slot];
        if (order > 4)
            hints.Add($"Help group {(order < 7 ? 1 : 2)}", false);
        else if (order > 0)
            hints.Add($"Order: {order}", false);

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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var baitOrder = NextAssignedBaitOrder(slot);
        if (baitOrder == 0 || _playerOrder[slot] >= 5 && WorldState.CurrentTime < _minHelpMove)
            return;
        var dest = TetherSpot(baitOrder);
        if (_playerOrder[slot] != baitOrder)
            dest += BaitOffset(_playerOrder[slot], _fireTethers[baitOrder - 1]);
        hints.AddForbiddenZone(ShapeContains.PrecisePosition(dest, new(0, 1), Module.Bounds.MapResolution, actor.Position, 0.1f));
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        // if player should bait, highlight source and other assistants
        var source = AssignedBaitSource(pcSlot);
        return source == player ? PlayerPriority.Danger : source != null && source == AssignedBaitSource(playerSlot) ? PlayerPriority.Interesting : PlayerPriority.Normal;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaits(pcSlot, pc, true))
            bait.shape.Draw(Arena, bait.origin, bait.dir, ArenaColor.AOE);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaits(pcSlot, pc, false))
            bait.shape.Outline(Arena, bait.origin, bait.dir, _fireTethers[NumCasts] ? ArenaColor.Safe : ArenaColor.Danger);

        var baitOrder = NextAssignedBaitOrder(pcSlot);
        if (baitOrder > 0)
        {
            var tetherSpot = TetherSpot(baitOrder);
            var isBaiter = _playerOrder[pcSlot] == baitOrder;
            Arena.AddCircle(tetherSpot, 1, isBaiter ? ArenaColor.Safe : ArenaColor.Danger);
            if (!isBaiter)
            {
                var offset = BaitOffset(_playerOrder[pcSlot], _fireTethers[baitOrder - 1]);
                if (offset != default)
                    Arena.AddCircle(tetherSpot + offset, 1, ArenaColor.Safe);
            }
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.Fire or TetherID.Lightning)
        {
            _fireTethers[_tetherTargets.Count] = tether.ID == (uint)TetherID.Fire;

            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
                _tetherTargets.Add(target);

            if (Raid.TryFindSlot(tether.Target, out var slot))
                _playerOrder[slot] = _tetherTargets.Count;

            if (_tetherTargets.Count == 4)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FloatingFetters:
                ++_numFetters;
                break;
            case AID.FallOfFaithSinsmite:
            case AID.FallOfFaithSinblaze:
                ++NumCasts;
                break;
        }
    }

    private void InitAssignments()
    {
        List<(int slot, int prio)> conga = [];
        foreach (var (slot, group) in _config.P1FallOfFaithAssignment.Resolve(Raid))
            if (_playerOrder[slot] == 0)
                conga.Add((slot, group));
        if (conga.Count != 4)
            return; // no assignments
        conga.SortBy(c => c.prio);
        for (int i = 0; i < conga.Count; ++i)
            _playerOrder[conga[i].slot] = i + 5;
        _minHelpMove = WorldState.FutureTime(1);
    }

    private bool IsGroupEven(int order) => order is 2 or 4 or 7 or 8;

    private int NextAssignedBaitOrder(int slot)
    {
        var order = _playerOrder[slot];
        if (order == 0)
            return 0;
        var nextCast = IsGroupEven(order) ? 1 : 0;
        if (NumCasts > nextCast)
            nextCast += 2; // first bait is done
        return NumCasts <= nextCast && nextCast < _tetherTargets.Count ? nextCast + 1 : 0;
    }

    private Actor? AssignedBaitSource(int slot)
    {
        var order = NextAssignedBaitOrder(slot);
        return order > 0 ? _tetherTargets[order - 1] : null;
    }

    private WDir GroupDirection(int order)
    {
        if (order == 0)
            return default;
        var dir = _config.P1FallOfFaithEW ? 90.Degrees() : 0.Degrees();
        if (!IsGroupEven(order))
            dir -= 180.Degrees();
        return dir.ToDirection();
    }

    // note: if target is fettered, it can no longer move
    private WPos TetherSpot(int order) => order <= _numFetters ? _tetherTargets[order - 1].Position : Module.Center + 5.5f * GroupDirection(order);

    private WDir CenterBaitOffset(int order) => GroupDirection(order) * 2;

    private WDir ProteanBaitOffset(int order)
    {
        if (order < 5)
            return default;
        WDir dir = _config.P1FallOfFaithEW ? new(0, -1) : new(1, 0);
        return (order is 5 or 8 ? 2 : -2) * dir;
    }

    private WDir BaitOffset(int order, bool fire) => order == 0 ? default : order < 5 || fire ? CenterBaitOffset(order) : ProteanBaitOffset(order);

    private bool ShouldBeBaiting(int slot)
    {
        var order = _playerOrder[slot];
        if (order == 0)
            return true; // if there are no assignments, we don't actually know whether player should be baiting...
        var nextBaitsEven = (NumCasts & 1) != 0;
        return IsGroupEven(order) == nextBaitsEven;
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

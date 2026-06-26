namespace BossMod.Dawntrail.Ultimate.UMAD;

class P2UltimateEmbrace(BossModule module) : Components.CastSharedTankbuster(module, AID.UltimateEmbrace, 5);

class P2ForsakenRaidwide(BossModule module) : Components.RaidwideCast(module, AID.Forsaken);

// notes: raidplan has clone baiters positioning relative to kefka's inner hitbox ring
// hitbox vfx isn't a specific integer size, it's scaled to the enemy, so the inner hitbox is roughly 5.15 units in radius (and the outer hitbox is 6.02)

// mapeffect: XX.00020001
// index is 1 (N) clockwise through 8 (NW)
// tower triggers 10.2s later
class P2PathOfLight(BossModule module) : Components.GenericTowers(module, AID.ThePathOfLight)
{
    // even towers appear about 0.5s before odd towers trigger, and i DO NOT want to have to write code accounting for 4 towers, so instead we save the even towers until the odd towers despawn
    readonly List<Tower> _stored = [];
    private (BitMask Left, BitMask Right) towerAssignments;

    public void SetTowerAssignments((BitMask Left, BitMask Right) value) => towerAssignments = value;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index is >= 1 and <= 8 && state == 0x00020001)
        {
            var angle = (180 - (index - 1) * 45).Degrees();

            var tower = new Tower(Arena.Center + angle.ToDirection() * 8, 4, minSoakers: 2, maxSoakers: 2, new(0xff), WorldState.FutureTime(10.2f));

            (Towers.Count >= 2 ? _stored : Towers).Add(tower);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            Towers.RemoveAll(t => t.Position.AlmostEqual(caster.Position, 1));
            if (Towers.Count == 0 && _stored.Count > 0)
            {
                Towers.AddRange(_stored);
                _stored.Clear();
            }
        }
    }

    public override void Update()
    {
        if (Towers.Count != 2)
            return;

        var (lMask, rMask) = towerAssignments;
        towerAssignments = (default, default);
        if (lMask == default && rMask == default)
            return;

        var (l, r) = (Towers[0], Towers[1]);
        Towers.Clear();
        if ((r.Position - Arena.Center).Dot((l.Position - Arena.Center).OrthoR()) > 0)
            (l, r) = (r, l);

        l.ForbiddenSoakers = ~lMask;
        r.ForbiddenSoakers = ~rMask;
        Towers.AddRange([l, r]);
    }

    public bool InAnyTower(Actor a) => Towers.Any(t => a.Position.InCircle(t.Position, t.Radius));

    public WDir DirToTowers() => Towers.Aggregate(new WDir(), (wd, t) => wd + (t.Position - Arena.Center)).Normalized();
}

class P2Shapes : Components.CastCounterMulti
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    P2PathOfLight? TowerComponent
    {
        get
        {
            field ??= Module.FindComponent<P2PathOfLight>();
            return field;
        }
    }

    public enum Shape
    {
        None,
        Stack,
        Spread,
        Cone
    }

    // actual current debuff on player
    public readonly Shape[] Shapes = new Shape[8];
    // what we are treating as the current debuff on the player, since we need to hold positions until the tower-triggered aoes go off
    public readonly Shape[] ResolvingShapes = new Shape[8];
    // used for determining group
    public readonly Shape[] InitialShapes = new Shape[8];
    public readonly int[] PartnerSlot = Utils.MakeArray(8, -1);
    public readonly int[] Tiebreaker = Utils.MakeArray(8, -1);

    public bool Baiting;

    int _numResolves;

    readonly WDir[] _prevTowers = [default, default];
    DateTime _nextDeadline;
    int _pendingCasts;
    int _pendingIcons;

    int _numAssignments;

    public enum TowerGroup { Unknown, A, B }

    public TowerGroup Group(int pcSlot) => InitialShapes[pcSlot] == default || PartnerSlot[pcSlot] < 0 ? TowerGroup.Unknown : InitialShapes[pcSlot] == InitialShapes[PartnerSlot[pcSlot]] ? TowerGroup.B : TowerGroup.A;

    public P2Shapes(BossModule module) : base(module, [AID.Spelldriver, AID.Spellscatter, AID.Spellwave])
    {
        var partnerIds = Utils.GenArray<int[]>(4, () => [-1, -1]);
        foreach (var (slot, grp) in _config.P2ForsakenPairs.Resolve(Raid))
        {
            if (partnerIds[grp][0] == -1)
                partnerIds[grp][0] = slot;
            else
                partnerIds[grp][1] = slot;
        }

        if (partnerIds.All(p => p.All(p1 => p1 >= 0)))
        {
            for (var i = 0; i < partnerIds.Length; i++)
            {
                PartnerSlot[partnerIds[i][0]] = partnerIds[i][1];
                PartnerSlot[partnerIds[i][1]] = partnerIds[i][0];
            }
        }
        foreach (var (slot, grp) in _config.P2ForsakenTiebreaker.Resolve(Raid))
            Tiebreaker[slot] = grp;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Shapes[slot] != default)
            hints.Add($"Current shape: {Shapes[slot]}", false);
        if (PartnerSlot[slot] >= 0 && Shapes[PartnerSlot[slot]] != default)
            hints.Add($"Partner shape: {Shapes[PartnerSlot[slot]]}", false);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var w in GetMoveHints(pcSlot))
            Arena.AddCircle(w, 0.5f, Baiting ? ArenaColor.Danger : ArenaColor.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Baiting)
            return;

        foreach (var spot in GetMoveHints(slot))
        {
            hints.ForbiddenZones.Clear();

            hints.AddForbiddenZone(ShapeContains.PrecisePosition(spot, new(0, 1), 0.5f, actor.Position, 0.1f), _nextDeadline);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var w in GetMoveHints(slot))
        {
            var origin = actor.Position;
            if (Baiting)
                foreach (var sp in Module.FindComponent<P2AllThingsEndingBait>()?.GetSafeSpot() ?? [])
                    origin = sp;
            movementHints.Add(origin, w, Baiting ? ArenaColor.Danger : ArenaColor.Safe);
        }
    }

    // there is an invisible status associated with each debuff, but note that at least one player per round gets the same shape they already had, which does NOT trigger statusgain again
    // icon -> cast event order is not consistent
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var shape = (IconID)iconID switch
        {
            IconID.ForsakenStack => Shape.Stack,
            IconID.ForsakenSpread => Shape.Spread,
            IconID.ForsakenCone => Shape.Cone,
            _ => default
        };

        // TODO: what happens if someone is dead when the first shapes are assigned?
        if (shape != default && Raid.TryFindSlot(actor, out var slot))
        {
            _numAssignments++;
            if (InitialShapes[slot] == default)
                InitialShapes[slot] = shape;
            else
                _pendingIcons--;
            Shapes[slot] = shape;

            // first tower set
            if (_numAssignments == 8)
            {
                _nextDeadline = WorldState.FutureTime(11.6f);
                AssignTowers();
            }

            // casts usually happen after icons. usually
            if (_numAssignments > 8 && _pendingIcons == 0 && _pendingCasts == 0)
                AssignTowers();
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellsTrouble && Raid.TryFindSlot(actor, out var slot))
        {
            Shapes[slot] = default;
            if (--_pendingIcons == 0 && _pendingCasts == 0)
                AssignTowers();
        }
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PartnerSlot[pcSlot] == playerSlot ? PlayerPriority.Critical : PlayerPriority.Irrelevant;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ThePathOfLight)
        {
            Shapes.CopyTo(ResolvingShapes);
            _prevTowers[1] = _prevTowers[0];
            _prevTowers[0] = caster.Position - Arena.Center;
            _pendingCasts += spell.Targets.Count;
            _pendingIcons += spell.Targets.Count;
            _nextDeadline = WorldState.FutureTime(0.6f);
        }

        if ((AID)spell.Action.ID is AID.Spellwave or AID.Spelldriver or AID.Spellscatter)
        {
            NumCasts++;
            if (--_pendingCasts == 0)
            {
                _numResolves++;
                // casts usually happen after icons
                if (_pendingIcons == 0)
                    AssignTowers();

                if (TowerComponent?.Towers.FirstOrNull() is { } nt)
                    _nextDeadline = nt.Activation;
            }
        }
    }

    public void Reset()
    {
        NumCasts = 0;
    }

    IEnumerable<WPos> GetMoveHints(int pcSlot)
    {
        if (Shapes.All(s => s == default))
            return [];

        if (_config.P2ForsakenStrategy != UMADConfig.P2ForsakenStrategyType.KroxyRinon)
            return [];

        if (TowerComponent is not { } towers || towers.NumCasts == 0 && towers.Towers.Count < 2)
            return [];

        WDir towersDir;

        if (_pendingCasts > 0 || _pendingIcons > 0)
            towersDir = ((_prevTowers[0] + _prevTowers[1]) * 0.5f).Normalized();
        else if (towers.Towers.Count == 2)
            towersDir = towers.DirToTowers();
        else
            // only happens during pre-even gap before new towers spawn; realistically we should just predict the tower rotation but that's slightly more work and thus annoying
            return [];

        var oddSet = _numResolves % 2 == 0;

        var (tLeft, tRight) = CurrentAssignments;

        if (tLeft[pcSlot] || tRight[pcSlot])
            return GetInsideTowerHints(pcSlot, oddSet, towersDir);
        else
            return GetOutsideTowerHints(pcSlot, oddSet, towersDir);
    }

    // the positions for these hints are copied verbatim from the raidplan, as opposed to being mathematically "optimal" or close to it, as we need to ensure that we don't mess with honest players' learned positioning
    IEnumerable<WPos> GetInsideTowerHints(int pcSlot, bool oddSet, WDir dirToTowers)
    {
        var (tLeft, tRight) = CurrentAssignments;

        if (oddSet)
        {
            if (tLeft[pcSlot])
            {
                switch (ResolvingShapes[pcSlot])
                {
                    case Shape.Cone:
                        // raidplan recommends that the support side stack stands exactly on the boss hitbox edge, which is at +6.02 units, meaning the maximum distance we can safely move away is <11.02
                        yield return Arena.Center + dirToTowers.Rotate(-45.Degrees()) * (8 + 2.5f);
                        break;
                    case Shape.Stack:
                        // use optimal solution (which covers entire tower) as opposed to the raidplan solution (which is easier to eyeball but can miss the cone player)
                        yield return Arena.Center + dirToTowers.Rotate(-45.Degrees()) * 7;
                        break;
                }
            }

            if (tRight[pcSlot])
            {
                switch (ResolvingShapes[pcSlot])
                {
                    case Shape.Stack:
                        // stand slightly further away from tower edge; we need to account for
                        // 1. always hitting both stack players, who sometimes like to position very deep, i.e. close to the midpoint between the two towers, they shouldn't really be doing this
                        // 2. not getting kicked; depending on how bad the baits were, this might be impossible
                        // 3. not clipping support side non-tower stack player
                        // this position is almost exactly what the kroxy raidplan recommends for this baiter
                        yield return Arena.Center + dirToTowers.Rotate(45.Degrees()) * 8 + dirToTowers.Rotate(35.Degrees()) * -3.25f;
                        break;
                    case Shape.Spread:
                        // dps side spread positioning barely matters as long as they're in tower
                        yield return Arena.Center + dirToTowers.Rotate(45.Degrees()) * 8 + dirToTowers * 3.5f;
                        break;
                }
            }
        }
        else
        {
            if (tLeft[pcSlot])
            {
                switch (ResolvingShapes[pcSlot])
                {
                    case Shape.Cone:
                        yield return Arena.Center + dirToTowers.Rotate(-45.Degrees()) * 8 + dirToTowers.Rotate(-16.Degrees()) * -3.6f;
                        break;
                    case Shape.Spread:
                        yield return Arena.Center + dirToTowers.Rotate(-45.Degrees()) * 8 + dirToTowers.Rotate(-10.Degrees()) * 3.5f;
                        break;
                }
            }
            if (tRight[pcSlot])
            {
                switch (ResolvingShapes[pcSlot])
                {
                    // raidplan wants us exactly on the intersection between kefka's inner hitbox ring and the tower edge, and cone baiters are positioned based on that
                    case Shape.Cone:
                        yield return Arena.Center + dirToTowers.Rotate(45.Degrees()) * 8 + dirToTowers.Rotate(16.Degrees()) * -3.6f;
                        break;
                    // again positioning is not super important, we just need to not accidentally bait cone
                    case Shape.Spread:
                        yield return Arena.Center + dirToTowers.Rotate(45.Degrees()) * 8 + dirToTowers.Rotate(10.Degrees()) * 3.5f;
                        break;
                }
            }
        }
    }

    static readonly float ConeBaitDistance = MathF.Sqrt(7.25f * 7.25f + 7.25f * 7.25f);

    IEnumerable<WPos> GetOutsideTowerHints(int pcSlot, bool oddSet, WDir dirToTowers)
    {
        var myOrder = (new BitMask(0xff) & ~(CurrentAssignments.Item1 | CurrentAssignments.Item2)).SetBits().OrderBy(i => Tiebreaker[i]).TakeWhile(i => i != pcSlot).Count();

        if (oddSet)
        {
            switch (myOrder)
            {
                case 0:
                    // behind supp tower
                    yield return Arena.Center + dirToTowers.Rotate(-45.Degrees()) * 12.5f;
                    break;
                case 1:
                    // in front of supp tower, try to stay away from dps stack
                    yield return Arena.Center + dirToTowers.Rotate(-45.Degrees()) * 3.5f;
                    break;
                case >= 2:
                    // in front of dps tower; make sure we are behind the line tangent to both tower edges on the kefka side, since that's where clones are hitting
                    yield return Arena.Center + dirToTowers.Rotate(30.Degrees()) * 3.5f;
                    break;
            }
        }
        else
        {
            switch (myOrder)
            {
                case 0:
                case 3:
                    // cone bait
                    yield return Arena.Center + dirToTowers.Rotate(myOrder == 0 ? -90.Degrees() : 90.Degrees()) * ConeBaitDistance;
                    break;
                case 1:
                case 2:
                    // melee range clone bait
                    yield return Arena.Center + dirToTowers.Rotate(myOrder == 1 ? 35.Degrees() : -35.Degrees()) * -6.02f;
                    break;
            }
        }
    }

    void AssignTowers()
    {
        Shapes.CopyTo(ResolvingShapes);
        SetAssignments(new(0xff), new(0xff));

        if (_config.P2ForsakenStrategy != UMADConfig.P2ForsakenStrategyType.KroxyRinon)
            return;

        BitMask leftTower = new();
        BitMask rightTower = new();

        var oddSet = _numResolves % 2 == 0;

        foreach (var (slot, _) in Raid.WithSlot())
        {
            var group = Group(slot);
            if (group == TowerGroup.Unknown)
                return;

            void tiebreak(Shape s)
            {
                // 0 if no corresponding player is found, e.g. if they died (mechanic is bricked anyway at that point), avoid breaking hints entirely
                var order = Tiebreaker[slot] - Tiebreaker[Enumerable.Range(0, 8).Where(s0 => Group(s0) == group && Shapes[s0] == s && s0 != slot).DefaultIfEmpty(slot).First()];
                if (order <= 0)
                    leftTower.Set(slot);
                if (order >= 0)
                    rightTower.Set(slot);
            }

#pragma warning disable IDE0047 // Remove unnecessary parentheses, you serious bro?
            var isInTower = (group == TowerGroup.A) == (_numResolves is 0 or 1 or 2 or 7);
#pragma warning restore IDE0047 // Remove unnecessary parentheses
            if (!isInTower)
                continue;

            if (oddSet)
            {
                if (Shapes[slot] == Shape.Cone)
                    leftTower.Set(slot);
                if (Shapes[slot] == Shape.Spread)
                    rightTower.Set(slot);
                if (Shapes[slot] == Shape.Stack)
                    tiebreak(Shapes[slot]);
            }
            else
            {
                if (Shapes[slot] is Shape.Cone or Shape.Spread)
                    tiebreak(Shapes[slot]);
            }
        }

        SetAssignments(leftTower, rightTower);
    }

    (BitMask, BitMask) CurrentAssignments;

    void SetAssignments(BitMask left, BitMask right)
    {
        CurrentAssignments = (left, right);
        Module.FindComponent<P2PathOfLight>()!.SetTowerAssignments(CurrentAssignments);
    }
}

class P2StackSpread : Components.UniformStackSpread
{
    readonly P2PathOfLight _towers;

    P2Shapes? Shapes
    {
        get
        {
            field ??= Module.FindComponent<P2Shapes>();
            return field;
        }
    }

    bool _castPending;

    public P2StackSpread(BossModule module) : base(module, 5, 5, 3, 3, alwaysShowSpreads: true)
    {
        ExtraAISpreadThreshold = 0;
        _towers = module.FindComponent<P2PathOfLight>()!;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ThePathOfLight:
                _castPending = true;
                break;
            case AID.Spellscatter:
            case AID.Spellwave:
            case AID.Spelldriver:
                _castPending = false;
                break;
        }
    }

    public override void Update()
    {
        if (_castPending || Shapes == null)
            return;

        Stacks.Clear();
        Spreads.Clear();

        foreach (var (i, player) in Raid.WithSlot().WhereActor(_towers.InAnyTower))
        {
            if (Shapes.Shapes[i] == P2Shapes.Shape.Stack)
                AddStack(player, _towers.Towers[0].Activation);
            else if (Shapes.Shapes[i] == P2Shapes.Shape.Spread)
                AddSpread(player, _towers.Towers[0].Activation);
        }
    }
}

class P2Spellwave(BossModule module) : Components.GenericBaitAway(module, AID.Spellwave)
{
    readonly P2PathOfLight _towers = module.FindComponent<P2PathOfLight>()!;

    P2Shapes? Shapes
    {
        get
        {
            field ??= Module.FindComponent<P2Shapes>();
            return field;
        }
    }

    bool _castPending;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ThePathOfLight:
                _castPending = true;
                break;
            case AID.Spellscatter:
            case AID.Spellwave:
            case AID.Spelldriver:
                _castPending = false;
                break;
        }
    }

    // damage only
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var b in CurrentBaits)
        {
            // forbid target/source from dashing while bait is active
            // (this zone will be removed by the other component if role-specific forsaken hints are configured properly)
            if (b.Target == actor || b.Source == actor)
                hints.AddForbiddenZone(_ => true, DateTime.MaxValue);
        }
    }

    public override void Update()
    {
        if (_castPending || Shapes == null)
            return;

        CurrentBaits.Clear();

        foreach (var (i, player) in Raid.WithSlot().WhereSlot(s => Shapes.Shapes[s] == P2Shapes.Shape.Cone).WhereActor(_towers.InAnyTower))
        {
            var closest = Raid.WithoutSlot().Exclude(player).Closest(player.Position);
            if (closest != null)
                CurrentBaits.Add(new(player, closest, new AOEShapeCone(40, 45.Degrees()), _towers.Towers[0].Activation));
        }
    }
}

// boss/clone jumps on nearest player
class P2PastsEndFuturesEnd : Components.UniformStackSpread
{
    WPos _source;
    DateTime _activation;
    int _numJumps;

    public P2PastsEndFuturesEnd(BossModule module) : base(module, 0, 5)
    {
        ExtraAISpreadThreshold = 0;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.PastsEndCast or AID.FuturesEndCast)
        {
            _source = spell.LocXZ;
            _activation = Module.CastFinishAt(spell, 0.2f);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FuturesEndBossAOE or AID.PastsEndBossAOE or AID.FuturesEndCloneAOE or AID.PastsEndCloneAOE)
        {
            _numJumps++;
            if (_numJumps == 4)
            {
                _source = default;
                _numJumps = 0;
            }
        }
    }

    public override void Update()
    {
        Spreads.Clear();

        if (_source == default)
            return;

        AddSpreads(Raid.WithoutSlot().SortedByRange(_source).Take(4), _activation);
    }
}

class P2AllThingsEnding(BossModule module) : Components.GroupedAOEs(module, [AID.AllThingsEnding1, AID.AllThingsEnding2], new AOEShapeCone(100, 90.Degrees()));

// cast starts 6s after boss castevent
// based on replay analysis, it seems like these are each individually baited on a random player
class P2AllThingsEndingBait(BossModule module) : BossComponent(module)
{
    enum Bait
    {
        None,
        Front,
        Behind
    }

    Bait _next;
#pragma warning disable IDE0052 // Remove unread private members
    DateTime _activation;
#pragma warning restore IDE0052 // Remove unread private members

    readonly List<Actor> _sources = [];

    public bool Draw;
    public bool Casting { get; private set; }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FuturesEndCast:
                _next = Bait.Front;
                _activation = Module.CastFinishAt(spell, 6);
                break;
            case AID.PastsEndCast:
                _next = Bait.Behind;
                _activation = Module.CastFinishAt(spell, 6);
                break;
            case AID.AllThingsEnding1:
            case AID.AllThingsEnding2:
                _sources.Remove(caster);
                if (_sources.Count == 0)
                {
                    Casting = true;
                    _next = default;
                    _activation = default;
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.FuturesEndBossAOE or AID.PastsEndBossAOE or AID.FuturesEndCloneAOE or AID.PastsEndCloneAOE) // or FuturesEnd1/FuturesEnd2
            _sources.Add(caster);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Draw)
        {
            foreach (var src in _sources)
            {
                var dir = src.AngleTo(pc);
                if (_next == Bait.Behind)
                    dir += 180.Degrees();
                Arena.AddCone(src.Position, 30, dir, 90.Degrees(), ArenaColor.Danger);
            }

            foreach (var sp in GetSafeSpot())
                Arena.AddCircle(sp, 0.5f, ArenaColor.Safe);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (Draw)
            foreach (var p in GetSafeSpot())
                movementHints.Add(actor.Position, p, ArenaColor.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Draw)
            foreach (var p in GetSafeSpot())
                hints.AddForbiddenZone(ShapeContains.PrecisePosition(p, new(0, 1), 0.5f, actor.Position, 0.1f), _activation);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_next != default)
            hints.Add($"Next bait: {_next}");
    }

    public IEnumerable<WPos> GetSafeSpot()
    {
        if (Module.FindComponent<P2PathOfLight>() is not { } towers || towers.Towers.Count != 2)
            yield break;

        var dt = towers.DirToTowers();
        // max melee is 6.02 + 3 + 0.5
        yield return Arena.Center + dt * 9.25f * (_next == Bait.Behind ? 1 : -1);
    }
}

class P2LightOfJudgment(BossModule module) : Components.RaidwideCast(module, AID.LightOfJudgmentP2);

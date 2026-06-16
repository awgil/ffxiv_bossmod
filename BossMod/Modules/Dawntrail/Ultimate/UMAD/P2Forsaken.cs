namespace BossMod.Dawntrail.Ultimate.UMAD;

class P2UltimateEmbrace(BossModule module) : Components.CastSharedTankbuster(module, AID.UltimateEmbrace, 5);

class P2ForsakenRaidwide(BossModule module) : Components.RaidwideCast(module, AID.Forsaken);

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

    public readonly Shape[] Shapes = new Shape[8];
    public readonly Shape[] InitialShapes = new Shape[8];
    public readonly int[] PartnerSlot = Utils.MakeArray(8, -1);
    public readonly int[] Tiebreaker = Utils.MakeArray(8, -1);

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
        foreach (var w in _destinations[pcSlot])
            Arena.AddCircle(w, 0.5f, ArenaColor.Safe);
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var w in _destinations[slot])
            movementHints.Add((actor.Position, w, ArenaColor.Safe));
    }

    // there is an invisible status associated with each debuff, but note that at least one player per round gets the same shape they already had, which does NOT trigger statusgain again
    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var shape = (IconID)iconID switch
        {
            IconID.ForsakenStack => Shape.Stack,
            IconID.ForsakenSpread => Shape.Spread,
            IconID.ForsakenCone => Shape.Cone,
            _ => default
        };

        if (shape != default && Raid.TryFindSlot(actor, out var slot))
        {
            _numAssignments++;
            if (InitialShapes[slot] == default)
                InitialShapes[slot] = shape;
            Shapes[slot] = shape;
            if (_numAssignments >= 8 && _numAssignments % 4 == 0)
                AssignTowers();
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellsTrouble && Raid.TryFindSlot(actor, out var slot))
            Shapes[slot] = default;
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor) => PartnerSlot[pcSlot] == playerSlot ? PlayerPriority.Critical : PlayerPriority.Irrelevant;

    int _numTowersDone;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.ThePathOfLight)
        {
            _numTowersDone++;
            if (_numTowersDone == 14)
            {
                _numAssignments += 4; // hack
                AssignTowers();
            }
        }
    }

    public void Reset()
    {
        NumCasts = 0;
    }

    readonly List<WPos>[] _destinations = Utils.GenArray<List<WPos>>(8, () => []);

    public override void Update()
    {
        foreach (var w in _destinations)
            w.Clear();

        foreach (var (pcSlot, _) in Raid.WithSlot())
            UpdateMoveHints(pcSlot);
    }

    void UpdateMoveHints(int pcSlot)
    {
        if (Shapes[pcSlot] == default)
            return;

        if (_config.P2ForsakenStrategy != UMADConfig.P2ForsakenStrategyType.KroxyRinon)
            return;

        var (tLeft, tRight) = CurrentAssignments;
        if (tLeft[pcSlot] && tRight[pcSlot])
            return;

        if (TowerComponent is not { } towers || towers.Towers.Count != 2 || !towers.EnableHints)
            return;

        var dirToTowers = towers.Towers.Aggregate(new WDir(), (wd, t) => wd + (t.Position - Arena.Center)).Normalized();

        var oddSet = _numAssignments % 8 == 0;

        void addDestination(WPos p) => _destinations[pcSlot].Add(p);

        if (oddSet)
        {
            if (tLeft[pcSlot])
            {
                switch (Shapes[pcSlot])
                {
                    case Shape.Cone:
                        addDestination(Arena.Center + dirToTowers.Rotate(-45.Degrees()) * (8 + 3));
                        break;
                    case Shape.Stack:
                        addDestination(Arena.Center + dirToTowers.Rotate(-45.Degrees()) * 7);
                        break;
                }
            }

            if (tRight[pcSlot])
            {
                switch (Shapes[pcSlot])
                {
                    case Shape.Stack:
                        addDestination(Arena.Center + dirToTowers.Rotate(45.Degrees()) * 8 + dirToTowers.Rotate(20.Degrees()) * -3.5f);
                        break;
                    case Shape.Spread:
                        addDestination(Arena.Center + dirToTowers.Rotate(45.Degrees()) * 8 + dirToTowers * 3.5f);
                        break;
                }
            }
        }
        else
        {
            if (tLeft[pcSlot])
            {
                switch (Shapes[pcSlot])
                {
                    case Shape.Cone:
                        addDestination(Arena.Center + dirToTowers.Rotate(-45.Degrees()) * 8 + dirToTowers.Rotate(-10.Degrees()) * -3.5f);
                        break;
                    case Shape.Spread:
                        addDestination(Arena.Center + dirToTowers.Rotate(-45.Degrees()) * 8 + dirToTowers.Rotate(-10.Degrees()) * 3.5f);
                        break;
                }
            }
            if (tRight[pcSlot])
            {
                switch (Shapes[pcSlot])
                {
                    case Shape.Cone:
                        addDestination(Arena.Center + dirToTowers.Rotate(45.Degrees()) * 8 + dirToTowers.Rotate(10.Degrees()) * -3.5f);
                        break;
                    case Shape.Spread:
                        addDestination(Arena.Center + dirToTowers.Rotate(45.Degrees()) * 8 + dirToTowers.Rotate(10.Degrees()) * 3.5f);
                        break;
                }
            }
        }
    }

    void AssignTowers()
    {
        SetAssignments(new(0xff), new(0xff));

        if (_config.P2ForsakenStrategy != UMADConfig.P2ForsakenStrategyType.KroxyRinon)
            return;

        BitMask leftTower = new();
        BitMask rightTower = new();

        var oddSet = _numAssignments % 8 == 0;

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
            var isInTower = (group == TowerGroup.A) == (_numAssignments is 8 or 12 or 16 or 36);
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

class P2StackSpread(BossModule module) : Components.UniformStackSpread(module, 5, 5, 3, alwaysShowSpreads: true)
{
    readonly P2Shapes _shapes = module.FindComponent<P2Shapes>()!;
    readonly P2PathOfLight _towers = module.FindComponent<P2PathOfLight>()!;

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

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        // disallow autorot from dashing during shape resolution, sick fuck
        if (_castPending)
            hints.AddForbiddenZone(_ => true, WorldState.FutureTime(100));
    }

    public override void Update()
    {
        if (_castPending)
            return;

        Stacks.Clear();
        Spreads.Clear();

        foreach (var (i, player) in Raid.WithSlot().WhereActor(_towers.InAnyTower))
        {
            if (_shapes.Shapes[i] == P2Shapes.Shape.Stack)
                AddStack(player, _towers.Towers[0].Activation);
            else if (_shapes.Shapes[i] == P2Shapes.Shape.Spread)
                AddSpread(player, _towers.Towers[0].Activation);
        }
    }
}

class P2Spellwave(BossModule module) : Components.GenericBaitAway(module, AID.Spellwave)
{
    readonly P2Shapes _shapes = module.FindComponent<P2Shapes>()!;
    readonly P2PathOfLight _towers = module.FindComponent<P2PathOfLight>()!;

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

    public override void Update()
    {
        if (_castPending)
            return;

        CurrentBaits.Clear();

        foreach (var (i, player) in Raid.WithSlot().WhereSlot(s => _shapes.Shapes[s] == P2Shapes.Shape.Cone).WhereActor(_towers.InAnyTower))
        {
            var closest = Raid.WithoutSlot().Exclude(player).Closest(player.Position);
            if (closest != null)
                CurrentBaits.Add(new(player, closest, new AOEShapeCone(40, 45.Degrees()), _towers.Towers[0].Activation));
        }
    }
}

// boss/clone jumps on nearest player
class P2PastsEndFuturesEnd(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    WPos _source;
    DateTime _activation;
    int _numJumps;

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
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_next != default)
            hints.Add($"Next bait: {_next}");
    }
}

class P2LightOfJudgment(BossModule module) : Components.RaidwideCast(module, AID.LightOfJudgmentP2);

namespace BossMod.Endwalker.Ultimate.DSW2;

class P2SanctityOfTheWard2HeavensStakeCircles : Components.LocationTargetedAOEs
{
    public P2SanctityOfTheWard2HeavensStakeCircles() : base(ActionID.MakeSpell(AID.HeavensStakeAOE), 7) { }
}

class P2SanctityOfTheWard2HeavensStakeDonut : Components.SelfTargetedAOEs
{
    public P2SanctityOfTheWard2HeavensStakeDonut() : base(ActionID.MakeSpell(AID.HeavensStakeDonut), new AOEShapeDonut(15, 30)) { }
}

class P2SanctityOfTheWard2VoidzoneFire : Components.PersistentVoidzone
{
    public P2SanctityOfTheWard2VoidzoneFire() : base(7, m => m.Enemies(OID.VoidzoneFire).Where(z => z.EventState != 7)) { }
}

class P2SanctityOfTheWard2VoidzoneIce : Components.PersistentVoidzone
{
    public P2SanctityOfTheWard2VoidzoneIce() : base(7, m => m.Enemies(OID.VoidzoneIce).Where(z => z.EventState != 7)) { }
}

class P2SanctityOfTheWard2Knockback : Components.KnockbackFromCastTarget
{
    public P2SanctityOfTheWard2Knockback() : base(ActionID.MakeSpell(AID.FaithUnmoving), 16) { }

    public override void AddAIHints(BossModule module, int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Casters.Count > 0 && !actor.Position.InCircle(module.Bounds.Center, 12))
        {
            var action = actor.Class.GetClassCategory() is ClassCategory.Healer or ClassCategory.Caster ? ActionID.MakeSpell(WHM.AID.Surecast) : ActionID.MakeSpell(WAR.AID.ArmsLength);
            hints.PlannedActions.Add((action, actor, (float)((Casters.FirstOrDefault()?.CastInfo?.NPCFinishAt ?? module.WorldState.CurrentTime) - module.WorldState.CurrentTime).TotalSeconds, false));
        }
    }
}

// note: technically it's a 2-man stack, but that is not really helpful here...
class P2SanctityOfTheWard2HiemalStorm : Components.CastCounter
{
    public P2SanctityOfTheWard2HiemalStorm() : base(ActionID.MakeSpell(AID.HiemalStormAOE)) { }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        arena.AddCircle(pc.Position, 7, ArenaColor.Danger);
    }
}

// identifiers used by this component:
// - quadrant: N=0, E=1, S=2, W=3
// - towers 1: [0,11] are outer towers in CW order, starting from '11 o'clock' (CCW tower of N quadrant); [12,15] are inner towers in CCW order, starting from NE (NE-SE-SW-NW)
//   so, inner towers for quadrant k are [3*k, 3*k+2]; neighbouring inner are 12+k & 12+(k+3)%4
// TODO: move hints for prey (position for new meteor is snapshotted approximately when previous meteors do their aoes; actual actor appears ~0.5s later)
class P2SanctityOfTheWard2Towers1 : Components.CastTowers
{
    struct PlayerData
    {
        public int AssignedQuadrant;
        public BitMask AssignedTowers; // note: typically we have only 1 assigned tower, but in some cases two players can have two towers assigned to them, since we can't determine reliable priority
        public int PreyDistance; // 0 for non-prey targets
    }

    struct QuadrantData
    {
        public int PreySlot;
        public int NonPreySlot;
    }

    [Flags]
    enum AssignmentDebug
    {
        PreySwapLazy = 0x01,
        PreySwapCursed = 0x02,
        OuterSync = 0x04,
        OuterCenter = 0x08,
    }

    private AssignmentDebug _assignmentDebug;
    private bool _stormsDone;
    private bool _preyOnTH;
    private BitMask _preyTargets;
    private int[] _towerIndices = Utils.MakeArray(16, -1);
    private PlayerData[] _players = Utils.MakeArray(PartyState.MaxPartySize, new PlayerData() { AssignedQuadrant = -1 });
    private QuadrantData[] _quadrants = Utils.MakeArray(4, new QuadrantData() { PreySlot = -1, NonPreySlot = -1 });
    private BitMask _activeTowers;
    private string _preySwap = "";
    private string _preyHint = "";

    private static readonly float _stormPlacementOffset = 10;
    private static readonly float _cometLinkRange = 5;

    public bool Active => Towers.Count == 8;

    public P2SanctityOfTheWard2Towers1() : base(ActionID.MakeSpell(AID.Conviction2AOE), 3) { }

    // TODO: use some sort of a config update hook to simplify debugging...
    //public override void Update(BossModule module)
    //{
    //    if (Active)
    //    {
    //        for (int i = 0; i < _players.Length; ++i)
    //            _players[i].AssignedTowers.Reset();
    //        for (int i = 0; i < Towers.Count; ++i)
    //            Towers.AsSpan()[i].ForbiddenSoakers.Reset();
    //        InitAssignments(module);
    //    }
    //}

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        // note: we're not showing standard 'soak/gtfo' hints, they are useless - player should know where to go based on other hints...
        if (_players[slot].PreyDistance is var dist && dist > 0)
        {
            hints.Add($"Prey distance: {dist}deg", false);
        }

        if (Active && movementHints != null && _players[slot].AssignedQuadrant >= 0)
        {
            var from = actor.Position;
            var color = ArenaColor.Safe;
            if (!_stormsDone)
            {
                var stormPos = StormPlacementPosition(module, _players[slot].AssignedQuadrant);
                movementHints.Add(from, stormPos, color);
                from = stormPos;
                color = ArenaColor.Danger;
            }

            foreach (var tower in Towers.Where(t => !t.ForbiddenSoakers[slot]))
            {
                movementHints.Add(from, tower.Position, color);
            }
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (Active)
        {
            hints.Add($"Prey: {(_preyOnTH ? "T/H" : "DD")}, swap {_preySwap}, prey towers {_preyHint}");
        }
    }

    public override PlayerPriority CalcPriority(BossModule module, int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        return _preyTargets[playerSlot] ? PlayerPriority.Danger : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        base.DrawArenaForeground(module, pcSlot, pc, arena);

        if (Active)
        {
            float diag = module.Bounds.HalfSize / 1.414214f;
            arena.AddLine(module.Bounds.Center + new WDir(diag, diag), module.Bounds.Center - new WDir(diag, diag), ArenaColor.Border);
            arena.AddLine(module.Bounds.Center + new WDir(diag, -diag), module.Bounds.Center - new WDir(diag, -diag), ArenaColor.Border);
        }

        // TODO: move to separate comet component...
        if (_preyTargets[pcSlot])
        {
            foreach (var comet in module.Enemies(OID.HolyComet))
            {
                arena.Actor(comet, ArenaColor.Object, true);
                arena.AddCircle(comet.Position, _cometLinkRange, ArenaColor.Object);
            }
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(module, caster, spell);
        if (spell.Action == WatchedAction)
        {
            // mark tower as active
            int index = ClassifyTower(module, spell.LocXZ);
            _towerIndices[index] = Towers.Count - 1;
            _activeTowers.Set(index);

            if (Active)
                InitAssignments(module);
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if ((AID)spell.Action.ID is AID.HiemalStormAOE)
            _stormsDone = true;
    }

    // note: might as well use statuses...
    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.Prey)
        {
            _preyOnTH = actor.Class.IsSupport();
            _preyTargets.Set(module.Raid.FindSlot(actor.InstanceID));
        }
    }

    public static int ClassifyTower(BossModule module, WPos tower)
    {
        var offset = tower - module.Bounds.Center;
        var dir = Angle.FromDirection(offset);
        if (offset.LengthSq() < 7 * 7)
        {
            // inner tower: intercardinal, ~6m from center
            return 12 + (dir.Rad > 0 ? (dir.Rad > MathF.PI / 2 ? 0 : 1) : (dir.Rad < -MathF.PI / 2 ? 3 : 2));
        }
        else
        {
            // outer tower: ~18m from center, at cardinal or +- 30 degrees
            return (7 - (int)MathF.Round(dir.Rad / MathF.PI * 6)) % 12;
        }
    }

    // bit 0 = CCW, bit 1 = center, bit 2 = CW
    private ulong OuterTowersMask(int quadrant) => (_activeTowers.Raw >> (quadrant * 3)) & 0b111;

    private int SelectOuterTower(ulong mask, bool cw)
    {
        var cmask = new BitMask(mask);
        return cw ? cmask.HighestSetBit() : cmask.LowestSetBit();
    }

    // 0 = 180 degrees, 1 = 150 degrees, 2 = 120 degrees
    private int OuterTowerPenalty(int t1, int t2) => Math.Abs(t1 - t2);

    private bool TowerUnassigned(int t)
    {
        var index = _towerIndices[t];
        return index >= 0 && Towers[index].ForbiddenSoakers.None();
    }

    private void InitAssignments(BossModule module)
    {
        _preySwap = "unconfigured";
        _preyHint = "unknown";
        var config = Service.Config.Get<DSW2Config>();
        if (InitQuadrantAssignments(module, config))
        {
            InitQuadrantSwaps(module, config);
            if (InitOuterTowers(module, config))
            {
                InitInnerTowers(module, config);
            }
        }
    }

    // initial assignments, before swaps
    private bool InitQuadrantAssignments(BossModule module, DSW2Config config)
    {
        bool validAssignments = false;
        foreach (var (slot, quadrant) in config.P2Sanctity2Pairs.Resolve(module.Raid))
        {
            validAssignments = true;
            _players[slot].AssignedQuadrant = quadrant;

            bool isTH = module.Raid[slot]?.Role is Role.Tank or Role.Healer;
            if (isTH == _preyOnTH)
                _quadrants[quadrant].PreySlot = slot;
            else
                _quadrants[quadrant].NonPreySlot = slot;
        }
        return validAssignments;
    }

    // swap quadrants for prey roles according to our strategy
    private void InitQuadrantSwaps(BossModule module, DSW2Config config)
    {
        // preferred prey cardinals
        var q1 = config.P2Sanctity2PreyCardinals is DSW2Config.P2PreyCardinals.AlwaysEW or DSW2Config.P2PreyCardinals.PreferEW ? 1 : 0;
        var q2 = q1 + 2;

        // lazy preferences: if both preys are at wrong cardinal, prefer not moving
        if (config.P2Sanctity2PreyCardinals is DSW2Config.P2PreyCardinals.PreferNS or DSW2Config.P2PreyCardinals.PreferEW && !_preyTargets[_quadrants[q1].PreySlot] && !_preyTargets[_quadrants[q2].PreySlot])
        {
            _assignmentDebug |= AssignmentDebug.PreySwapLazy;
            q1 ^= 1;
            q2 ^= 1;
        }

        // check whether we don't have cursed pattern
        if (!config.P2Sanctity2ForcePreferredPrey)
        {
            // cursed patterns are CW+CCW or CCW+CW
            var q1towers = OuterTowersMask(q1);
            var q2towers = OuterTowersMask(q2);
            bool cursed = (q1towers & q2towers) == 0 && (q1towers | q2towers) == 0b101; // 100+001 or 001+100
            if (cursed)
            {
                _assignmentDebug |= AssignmentDebug.PreySwapCursed;
                q1 ^= 1;
                q2 ^= 1;
            }
        }

        // now that we've selected cardinals for preys, see whether any prey roles need to swap
        var q1swap = !_preyTargets[_quadrants[q1].PreySlot];
        var q2swap = !_preyTargets[_quadrants[q2].PreySlot];
        if (q1swap && q2swap)
        {
            // both preys at wrong cardinal, swap according to our strategy
            switch (config.P2Sanctity2SwapDirection)
            {
                case DSW2Config.P2PreySwapDirection.RotateCW:
                    // 0123->1230; this is equivalent to 3 swaps: 0123->1023->1032->1230
                    SwapPreyQuadrants(0, 1);
                    SwapPreyQuadrants(2, 3);
                    SwapPreyQuadrants(1, 3);
                    break;
                case DSW2Config.P2PreySwapDirection.RotateCCW:
                    // 0123->3012 => 0123->1023->1032->3012
                    SwapPreyQuadrants(0, 1);
                    SwapPreyQuadrants(2, 3);
                    SwapPreyQuadrants(0, 2);
                    break;
                case DSW2Config.P2PreySwapDirection.PairsNE:
                    // 0123->1032
                    SwapPreyQuadrants(0, 1);
                    SwapPreyQuadrants(2, 3);
                    break;
                case DSW2Config.P2PreySwapDirection.PairsNW:
                    // 0123->3210
                    SwapPreyQuadrants(0, 3);
                    SwapPreyQuadrants(1, 2);
                    break;
            }
            _preySwap = "both";
        }
        else if (q1swap || q2swap)
        {
            int swapQ1 = q1swap ? q1 : q2; // prey-role assigned to this quadrant is not a prey target
            int swapQ2 = q1 ^ 1; // arbitrary
            if (!_preyTargets[_quadrants[swapQ2].PreySlot])
                swapQ2 ^= 2; // we guessed wrong - our prey target to swap with is in remaining quadrant
            SwapPreyQuadrants(swapQ1, swapQ2);
            _preySwap = $"{QuadrantSwapHint(module, swapQ1)}/{QuadrantSwapHint(module, swapQ2)}";
        }
        else
        {
            _preySwap = "none";
        }
    }

    private void SwapPreyQuadrants(int q1, int q2)
    {
        int s1 = _quadrants[q1].PreySlot;
        int s2 = _quadrants[q2].PreySlot;
        _quadrants[q1].PreySlot = s2;
        _quadrants[q2].PreySlot = s1;
        _players[s1].AssignedQuadrant = q2;
        _players[s2].AssignedQuadrant = q1;
    }

    // outer tower assignments
    private bool InitOuterTowers(BossModule module, DSW2Config config)
    {
        if (config.P2Sanctity2OuterTowers == DSW2Config.P2OuterTowers.None)
            return false;

        // start by assigning towers in preferred direction
        var q1 = _preyTargets[_quadrants[0].PreySlot] ? 0 : 1;
        var q2 = q1 + 2;
        var q1towers = OuterTowersMask(q1);
        var q2towers = OuterTowersMask(q2);
        var q12cw = config.P2Sanctity2PreferCWTowerAsPrey;
        var q1selected = SelectOuterTower(q1towers, q12cw);
        var q2selected = SelectOuterTower(q2towers, q12cw);

        // try synchronized swap, if allowed
        // note that in 1-1 pattern it won't make any difference, in 1-2 pattern it can find better solution
        if (config.P2Sanctity2OuterTowers != DSW2Config.P2OuterTowers.AlwaysPreferred)
        {
            // try synchronized swap
            var q1alt = SelectOuterTower(q1towers, !q12cw);
            var q2alt = SelectOuterTower(q2towers, !q12cw);
            if (OuterTowerPenalty(q1selected, q2selected) > OuterTowerPenalty(q1alt, q2alt))
            {
                q12cw = !q12cw;
                q1selected = q1alt;
                q2selected = q2alt;
                _assignmentDebug |= AssignmentDebug.OuterSync;
            }
        }
        _preyHint = q12cw ? "cw" : "ccw";

        // try individual mixed swap, if allowed
        // note that for 1-1 or 1-2 patterns, this can never be better than sync swap (since there are only 2 options anyway)
        // for 2-2 pattern, the worst case is 150-degree pattern even without any swaps, sync swap could have improved that to optimal - in such case individual swaps won't matter
        // the only way individual swaps could be better if both synchronized options are 150, but there exists a 180 option 'both to central' (CW+center vs CCW+center)
        if (config.P2Sanctity2OuterTowers == DSW2Config.P2OuterTowers.Individual && q1selected != q2selected && (q1towers & q2towers & 0b010) != 0)
        {
            q1selected = 1;
            q2selected = 1;
            _preyHint = "center";
            // note: q12cw is now meaningless, but it doesn't matter
            _assignmentDebug |= AssignmentDebug.OuterCenter;
        }

        // ok, assign outer towers for prey targets
        AssignTower(_quadrants[q1].PreySlot, q1 * 3 + q1selected);
        AssignTower(_quadrants[q2].PreySlot, q2 * 3 + q2selected);
        _players[_quadrants[q1].PreySlot].PreyDistance = 180 + 30 * (q2selected - q1selected);
        _players[_quadrants[q2].PreySlot].PreyDistance = 180 + 30 * (q1selected - q2selected);

        // assign outer targets for remaining prey roles
        var q3 = q1 ^ 1;
        var q4 = q3 + 2;
        var q3towers = OuterTowersMask(q3);
        var q4towers = OuterTowersMask(q4);
        var q34cw = config.P2Sanctity2OuterTowers == DSW2Config.P2OuterTowers.SynchronizedRole ? q12cw : config.P2Sanctity2PreferCWTowerAsPrey;
        var q3selected = SelectOuterTower(q3towers, q34cw);
        var q4selected = SelectOuterTower(q4towers, q34cw);
        AssignTower(_quadrants[q3].PreySlot, q3 * 3 + q3selected);
        AssignTower(_quadrants[q4].PreySlot, q4 * 3 + q4selected);

        // and finally assign towers for non-prey roles
        Action<int, ulong, int> assignNonPrey = (q, towers, taken) =>
        {
            var remaining = new BitMask(towers ^ (1u << taken));
            if (remaining.Any())
                AssignTower(_quadrants[q].NonPreySlot, remaining.LowestSetBit() + 3 * q);
        };
        assignNonPrey(q1, q1towers, q1selected);
        assignNonPrey(q2, q2towers, q2selected);
        assignNonPrey(q3, q3towers, q3selected);
        assignNonPrey(q4, q4towers, q4selected);

        return true;
    }

    private void InitInnerTowers(BossModule module, DSW2Config config)
    {
        // now assign inner towers, as long as it can be done non-ambiguously
        switch (config.P2Sanctity2InnerTowers)
        {
            case DSW2Config.P2InnerTowers.Closest:
                AssignInnerTowersClosest();
                break;
            case DSW2Config.P2InnerTowers.CW:
                AssignInnerTowersCW();
                break;
        }

        // if we still have unassigned towers, assign each of them to each remaining player
        var ambiguousSlots = _quadrants.Select(q => q.NonPreySlot).Where(slot => _players[slot].AssignedTowers.None()).ToArray();
        for (int t = 12; t < _towerIndices.Length; ++t)
        {
            if (TowerUnassigned(t))
            {
                foreach (var slot in ambiguousSlots)
                {
                    AssignTower(slot, t);
                }
            }
        }
    }

    private void AssignTower(int slot, int tower)
    {
        _players[slot].AssignedTowers.Set(tower);
        var index = _towerIndices[tower];
        if (index >= 0)
        {
            ref var t = ref Towers.AsSpan()[index];
            if (t.ForbiddenSoakers.None())
                t.ForbiddenSoakers = new(ulong.MaxValue);
            t.ForbiddenSoakers.Clear(slot);
        }
    }

    private void AssignInnerTowersClosest()
    {
        while (true)
        {
            int unambiguousInnerTower = -1;
            int unambiguousQuadrant = -1;
            for (int q = 0; q < _quadrants.Length; ++q)
            {
                if (_players[_quadrants[q].NonPreySlot].AssignedTowers.Any())
                    continue;

                int potential = FindUnassignedUnambiguousInnerTower(q);
                if (potential == -1)
                    continue; // this quadrant has 2 or 0 unassigned inner towers

                if (unambiguousInnerTower == -1)
                {
                    // new potential assignment
                    unambiguousInnerTower = potential;
                    unambiguousQuadrant = q;
                }
                else if (unambiguousInnerTower == potential)
                {
                    // we have two quadrants that have 1 common inner tower, this is a bad pattern...
                    unambiguousInnerTower = -1;
                    break;
                }
                // else: ignore this tower on this iteration...
            }

            if (unambiguousInnerTower == -1)
                return;

            AssignTower(_quadrants[unambiguousQuadrant].NonPreySlot, unambiguousInnerTower);
        }
    }

    private void AssignInnerTowersCW()
    {
        for (int distance = 0; distance < _quadrants.Length; ++distance)
        {
            for (int q = 0; q < _quadrants.Length; ++q)
            {
                if (_players[_quadrants[q].NonPreySlot].AssignedTowers.None())
                {
                    var tower = 12 + ((q + distance) & 3);
                    if (TowerUnassigned(tower))
                    {
                        AssignTower(_quadrants[q].NonPreySlot, tower);
                    }
                }
            }
        }
    }

    private int FindUnassignedUnambiguousInnerTower(int quadrant)
    {
        int candidate1 = 12 + quadrant;
        int candidate2 = 12 + ((quadrant + 3) & 3);
        bool available1 = TowerUnassigned(candidate1);
        bool available2 = TowerUnassigned(candidate2);
        if (available1 == available2)
            return -1;
        else
            return available1 ? candidate1 : candidate2;
    }

    private WPos StormPlacementPosition(BossModule module, int quadrant)
    {
        var dir = (180 - quadrant * 90).Degrees();
        return module.Bounds.Center + _stormPlacementOffset * dir.ToDirection();
    }

    private string QuadrantSwapHint(BossModule module, int quadrant)
    {
        return quadrant switch
        {
            0 => "N",
            1 => "E",
            2 => "S",
            3 => "W",
            _ => "?"
        };

        //var pos = StormPlacementPosition(module, quadrant);

        //Waymark closest = Waymark.Count;
        //float closestD = float.MaxValue;

        //for (int i = 0; i < (int)Waymark.Count; ++i)
        //{
        //    var w = (Waymark)i;
        //    var p = module.WorldState.Waymarks[w];
        //    float d = p != null ? (pos - new WPos(p.Value.XZ())).LengthSq() : float.MaxValue;
        //    if (d < closestD)
        //    {
        //        closest = w;
        //        closestD = d;
        //    }
        //}
        //return closest < Waymark.Count ? closest.ToString() : "-";
    }
}

// identifiers used by this component:
// - towers 2: [0,7] - CW order, starting from N
class P2SanctityOfTheWard2Towers2 : Components.CastTowers
{
    private bool _preyOnTH;
    private BitMask _preyTargets;
    private int[] _playerTowers = Utils.MakeArray(PartyState.MaxPartySize, -1);

    public P2SanctityOfTheWard2Towers2() : base(ActionID.MakeSpell(AID.Conviction3AOE), 3) { }

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        // note: not drawing any default hints here...
    }

    public override void OnStatusGain(BossModule module, Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Prey)
        {
            bool first = _preyTargets.None();
            _preyOnTH = actor.Class.IsSupport();
            _preyTargets.Set(module.Raid.FindSlot(actor.InstanceID));

            // assign non-prey-role positions here
            if (first)
                InitNonPreyAssignments(module);
        }
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var index = ClassifyTower(module, spell.LocXZ);
            var forbidden = module.Raid.WithSlot(true).WhereSlot(s => _playerTowers[s] >= 0 && _playerTowers[s] != index).Mask();
            Towers.Add(new(spell.LocXZ, Radius, forbiddenSoakers: forbidden));
        }
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(module, caster, spell);
        if ((AID)spell.Action.ID == AID.Conviction2AOE && !spell.TargetXZ.InCircle(module.Bounds.Center, 7))
        {
            // we assign towers to prey role players according to the quadrant they were soaking their tower - this handles unexpected swaps on first towers gracefully
            foreach (var t in spell.Targets)
            {
                var slot = module.Raid.FindSlot(t.ID);
                if (module.Raid[slot]?.Class.IsSupport() == _preyOnTH)
                {
                    var towerOffset = spell.TargetXZ - module.Bounds.Center;
                    var towerIndex = towerOffset.Z switch
                    {
                        < -10 => 0, // N tower
                        > +10 => 4, // S tower
                        _ => towerOffset.X > 0 ? 2 : 6
                    };
                    if (_preyTargets[slot])
                        towerIndex = (towerIndex + 4) & 7; // preys will rotate 180 degrees
                    _playerTowers[slot] = towerIndex;
                }
            }
        }
    }

    private int ClassifyTower(BossModule module, WPos tower)
    {
        var offset = tower - module.Bounds.Center;
        var dir = Angle.FromDirection(offset);
        return (4 - (int)MathF.Round(dir.Rad / MathF.PI * 4)) % 8;
    }

    private void InitNonPreyAssignments(BossModule module)
    {
        var config = Service.Config.Get<DSW2Config>();
        foreach (var (slot, quadrant) in config.P2Sanctity2Pairs.Resolve(module.Raid))
        {
            if (module.Raid[slot]?.Class.IsSupport() != _preyOnTH)
            {
                var tower = 2 * quadrant;
                if (config.P2Sanctity2NonPreyTowerCW)
                    tower += 1;
                else
                    tower = tower == 0 ? 7 : tower - 1;
                _playerTowers[slot] = tower;
            }
        }
    }
}

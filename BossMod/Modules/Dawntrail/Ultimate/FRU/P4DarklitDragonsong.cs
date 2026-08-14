namespace BossMod.Dawntrail.Ultimate.FRU;

// tethers & general assignments
sealed class P4DarklitDragonsong(BossModule module) : BossComponent(module)
{
    public BitMask Stacks;
    public BitMask TowerSoakers;
    public BitMask AssignS;
    public BitMask AssignE;
    private readonly List<(Actor from, Actor to)> _tethers = [];

    public override void AddGlobalHints(GlobalHints hints)
    {
        var southTower = TowerSoakers & AssignS;
        if (southTower.Any())
            hints.Add($"Water in {((southTower & Stacks).Any() ? "S" : "N")} tower");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _tethers)
            Arena.AddLine(t.from.Position, t.to.Position, Colors.Safe); // TODO: min/max break distance
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingDarkWater)
            Stacks[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.LightRampantChains)
        {
            TowerSoakers[Raid.FindSlot(source.InstanceID)] = true;
            var target = WorldState.Actors.Find(tether.Target);
            if (target != null)
                _tethers.Add((source, target));
            if (_tethers.Count == 4)
                InitAssignments();
        }
    }

    private void InitAssignments()
    {
        Span<int> ccwOrderSlots = [-1, -1, -1, -1, -1, -1, -1, -1];
        int[] playerPrios = [-1, -1, -1, -1, -1, -1, -1, -1];
        foreach (var (slot, group) in Service.Config.Get<FRUConfig>().P4DarklitDragonsongAssignments.Resolve(Raid))
        {
            ccwOrderSlots[group] = slot;
            playerPrios[slot] = group;
        }
        if (ccwOrderSlots.Contains(-1))
            return; // assignments are not valid, bail

        // find the anchor (tethered player with lowest prio), players tethered to him on both sides will take S tower
        var anchorSlot = TowerSoakers.SetBits().MinBy(i => playerPrios[i]);
        var anchorPlayer = Raid[anchorSlot];
        foreach (var t in _tethers)
        {
            if (t.from == anchorPlayer)
                AssignS[Raid.FindSlot(t.to.InstanceID)] = true;
            else if (t.to == anchorPlayer)
                AssignS[Raid.FindSlot(t.from.InstanceID)] = true;
        }

        // E/W assignments for tower soakers
        AssignE.Set(SelectEastTowerSoaker(TowerSoakers & AssignS, playerPrios));
        AssignE.Set(SelectEastTowerSoaker(TowerSoakers & ~AssignS, playerPrios));

        // assignments for baiters in prio order
        var numAssigned = 0;
        foreach (var slot in ccwOrderSlots)
        {
            if (!TowerSoakers[slot])
            {
                // first and last go N, last two go E
                AssignS[slot] = numAssigned is 1 or 2;
                AssignE[slot] = numAssigned >= 2;
                ++numAssigned;
            }
        }

        // finally, if both stacks are on the same N/S side, bait needs to swap with other bait on same E/W side
        if ((AssignS & Stacks).NumSetBits() != 1)
        {
            var flexStack = Stacks & ~TowerSoakers; // mask containing one set bit, corresponding to non-soaker stack - he will need to flex
            var flexE = AssignE[flexStack.LowestSetBit()];
            var flexMask = (AssignE ^ new BitMask(flexE ? 0u : 0xFF)) & ~TowerSoakers; // mask containing both flexers
            if (flexStack.NumSetBits() != 1 || flexMask.NumSetBits() != 2 || (flexStack & flexMask) != flexStack)
                ReportError("Some error with swap logic, investigate");
            AssignS ^= flexMask;
        }
    }

    private int SelectEastTowerSoaker(BitMask soakers, ReadOnlySpan<int> prios)
    {
        if (soakers.NumSetBits() != 2)
            return -1;
        var s1 = soakers.LowestSetBit();
        var s2 = soakers.HighestSetBit();
        var p1 = prios[s1];
        var p2 = prios[s2];
        return p1 < 4 && p2 < 4 ? (p1 < p2 ? s1 : s2) // both 'western' (one is anchor) - lower (more cw) is east
            : p1 >= 4 && p2 >= 4 ? (p1 > p2 ? s1 : s2) // both 'eastern' (neither is anchor) - higher (more cw) is east
            : p1 < 4 ? s2 : s1; // whoever is more eastern
    }
}

sealed class P4DarklitDragonsongBrightHunger(BossModule module) : Components.GenericTowers(module, (uint)AID.BrightHunger)
{
    private readonly P4DarklitDragonsong? _darklit = module.FindComponent<P4DarklitDragonsong>();
    private int _numTethers;

    private const float TowerOffset = 8f;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_darklit == null || Towers.Count == 0 || _darklit.AssignS.None() || !_darklit.TowerSoakers[slot])
            return; // we only provide hints for soakers

        // stay on the far side of assigned tower, on the correct E/W side
        var towerPos = Arena.Center + new WDir(0, _darklit.AssignS[slot] ? +TowerOffset : -TowerOffset);
        hints.AddForbiddenZone(new SDInvertedCircle(towerPos, 3f), Towers[0].Activation);
        hints.AddForbiddenZone(new SDCircle(Arena.Center, TowerOffset), Towers[0].Activation);
        hints.AddForbiddenZone(new SDHalfPlane(Arena.Center, new(_darklit.AssignE[slot] ? +1 : -1, default)), Towers[0].Activation);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.LightRampantChains && ++_numTethers == 4 && _darklit != null)
        {
            var allowedN = _darklit.TowerSoakers & ~_darklit.AssignS;
            var allowedS = _darklit.TowerSoakers & _darklit.AssignS;
            if (_darklit.AssignS.None())
                allowedN = allowedS = _darklit.TowerSoakers; // no assignments, just mark both towers as good

            var towerOffset = new WDir(default, TowerOffset);
            var act = WorldState.FutureTime(10.4d);
            Towers.Add(new(Arena.Center - towerOffset, 4f, 2, 2, new BitMask(0xFF) ^ allowedN, act));
            Towers.Add(new(Arena.Center + towerOffset, 4f, 2, 2, new BitMask(0xFF) ^ allowedS, act));
        }
    }
}

sealed class P4DarklitDragonsongPathOfLight(BossModule module) : Components.GenericBaitAway(module, (uint)AID.PathOfLightAOE)
{
    private readonly P4DarklitDragonsong? _darklit = module.FindComponent<P4DarklitDragonsong>();
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(60f, 30f.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null && ForbiddenPlayers.Any())
            foreach (var p in Raid.WithoutSlot(false, true, true).SortedByRange(_source.Position).Take(4))
                CurrentBaits.Add(new(_source, p, _shape, _activation));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;

        var baitIndex = CurrentBaits.FindIndex(b => b.Target == actor);
        if (ForbiddenPlayers[slot])
        {
            if (baitIndex >= 0)
                hints.Add("Stay farther away!");
        }
        else
        {
            if (baitIndex < 0)
                hints.Add("Stay closer to bait!");
            else if (PlayersClippedBy(ref CurrentBaits.Ref(baitIndex)).Count != 0)
                hints.Add("Bait cone away from raid!");
        }

        if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, ref b)))
            hints.Add("GTFO from baited cone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_darklit == null || _source == null || _darklit.AssignS.None() || ForbiddenPlayers[slot])
            return; // we only provide hints for baiters

        // do not clip either tower (it's visible at half-angle = asin(4/8) = 30) or each other
        var dir = (_darklit.AssignE[slot] ? +1 : -1) * (_darklit.AssignS[slot] ? 60f : 120f).Degrees();
        hints.AddForbiddenZone(new SDPrecisePosition(Arena.Center + 4f * dir.ToDirection(), new(default, 1f), Arena.Bounds.MapResolution, actor.Position, 0.1f), _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DarklitDragonsongUsurper)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell, 12f);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.LightRampantChains)
            ForbiddenPlayers[Raid.FindSlot(source.InstanceID)] = true;
    }
}

sealed class P4DarklitDragonsongSpiritTaker(BossModule module) : SpiritTaker(module)
{
    //private readonly P4DarklitDragonsong? _darklit = module.FindComponent<P4DarklitDragonsong>();

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        //if (_darklit != null && _darklit.AssignS.Any() && Spreads.Count > 0 && Spreads[0].Activation > WorldState.FutureTime(1))
        //{
        //    // preposition
        //    hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + PrepositionOffset(_darklit, slot, actor), 1)), Spreads[0].Activation);
        //}
        //else
        {
            // just adjust and dodge...
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    // TODO: this is very arbitrary...
    //private WDir PrepositionOffset(P4DarklitDragonsong darklit, int slot, Actor actor)
    //{
    //    if (darklit.TowerSoakers[slot])
    //    {
    //        // tower soakers go to fixed spots, skewed not to hit the fragment
    //        var dir = (darklit.AssignE[slot] ? 1 : -1) * (darklit.AssignS[slot] ? 25 : 135).Degrees();
    //        return 9 * dir.ToDirection();
    //    }
    //    else
    //    {
    //        // baiting healer goes far west, tank goes near west, northern dd goes center, southern dd goes near east
    //        var off = actor.Role switch
    //        {
    //            Role.Healer => -12,
    //            Role.Tank => -6,
    //            _ => darklit.AssignS[slot] ? 6 : 0
    //        };
    //        return new(off, 0);
    //    }
    //}
}

sealed class P4DarklitDragonsongDarkWater(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, includeDeadTargets: true)
{
    private readonly P4DarklitDragonsong? _assignments = module.FindComponent<P4DarklitDragonsong>();
    private bool _resolveImminent;

    public void Show()
    {
        _resolveImminent = true;
        if (_assignments != null && _assignments.AssignS.Any())
        {
            foreach (ref var s in Stacks.AsSpan())
            {
                var isSouth = _assignments.AssignS[Raid.FindSlot(s.Target.InstanceID)];
                s.ForbiddenPlayers = _assignments.AssignS ^ new BitMask(isSouth ? 0xFF : 0u);
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_resolveImminent)
            base.AddHints(slot, actor, hints);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_resolveImminent || _assignments == null || _assignments.AssignS.None())
            return;

        var stacks = CollectionsMarshal.AsSpan(Stacks);
        var len = stacks.Length;
        for (var i = 0; i < len; ++i)
        {
            ref var s = ref stacks[i];
            if (!s.ForbiddenPlayers[slot])
            {
                if (s.Target == actor || s.Activation > WorldState.FutureTime(1.5d))
                {
                    // preposition
                    var off = 9 * (_assignments.AssignS[slot] ? 20f : 130f).Degrees().ToDirection();
                    var center = Arena.Center;
                    hints.AddForbiddenZone(new SDInvertedUnion([new SDCircle(center + off, 1f), new SDCircle(center + new WDir(-off.X, off.Z), 1f)]), s.Activation);
                }
                else
                {
                    // otherwise just stack tightly with our target, and avoid other
                    for (var j = 0; j < len; ++j)
                    {
                        ref var st = ref stacks[i];
                        hints.AddForbiddenZone(st.ForbiddenPlayers[slot] ? new SDCircle(st.Target.Position, st.Radius) : new SDInvertedCircle(st.Target.Position, 2f), st.Activation);
                    }
                }
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingDarkWater)
            AddStack(actor, status.ExpireAt);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DarkWater)
            Stacks.Clear();
    }
}

sealed class P4SomberDance(BossModule module) : Components.GenericBaitAway(module, centerAtTarget: true)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(8);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source == null)
            return;
        var targets = Raid.WithoutSlot(false, true, true);
        var target = NumCasts == 0 ? targets.Farthest(_source.Position) : targets.Closest(_source.Position);
        if (target != null)
            CurrentBaits.Add(new(_source, target, _shape, _activation));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (assignment == (_config.P4SomberDanceOTBait ? PartyRolesConfig.Assignment.OT : PartyRolesConfig.Assignment.MT))
        {
            // go far east/west
            var pos = Arena.Center + new WDir(actor.PosRot.X > Arena.Center.X ? 19f : -19f, default);
            hints.AddForbiddenZone(new SDInvertedCircle(pos, 1f), _activation);
        }
        else
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SomberDance)
        {
            ForbiddenPlayers = Raid.WithSlot(true, true, true).WhereActor(p => p.Role != Role.Tank).Mask();
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.4d);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SomberDanceAOE1 or (uint)AID.SomberDanceAOE2)
        {
            ++NumCasts;
            _activation = WorldState.FutureTime(3.2d);
            foreach (var t in spell.Targets)
                ForbiddenPlayers.Set(Raid.FindSlot(t.ID));
        }
    }
}

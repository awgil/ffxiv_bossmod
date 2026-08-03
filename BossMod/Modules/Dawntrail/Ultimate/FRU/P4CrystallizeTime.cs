namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P4CrystallizeTime(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, FangEruption, FangWater, FangDarkness, FangBlizzard, ClawAir, ClawBlizzard }

    public readonly Mechanic[] PlayerMechanics = new Mechanic[PartyState.MaxPartySize];
    public readonly int[] ClawSides = new int[PartyState.MaxPartySize]; // 0 if not assigned (bad config or no claw), +/-1 otherwise for E/W
    public WDir NorthSlowHourglass;
    public BitMask Cleansed;
    private int _numClaws;

    public Actor? FindPlayerByAssignment(Mechanic mechanic, int side)
    {
        var len = PlayerMechanics.Length;
        for (var i = 0; i < len; ++i)
            if (PlayerMechanics[i] == mechanic && ClawSides[i] == side)
                return Raid[i];
        return null;
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SpellInWaitingDarkEruption:
                AssignMechanic(actor, Mechanic.FangEruption); // always paired with fang
                break;
            case (uint)SID.SpellInWaitingDarkWater:
                AssignMechanic(actor, Mechanic.FangWater); // always paired with fang
                break;
            case (uint)SID.SpellInWaitingUnholyDarkness:
                AssignMechanic(actor, Mechanic.FangDarkness); // always paired with fang
                break;
            case (uint)SID.SpellInWaitingDarkBlizzard:
                AssignMechanic(actor, Mechanic.FangBlizzard, higherPrio: Mechanic.ClawBlizzard); // paired with either, we'll reassign to claw when reacting to claw buff
                break;
            case (uint)SID.SpellInWaitingDarkAero:
                AssignMechanic(actor, Mechanic.ClawAir); // always paired with claw
                break;
            case (uint)SID.Wyrmfang:
                break; // don't react
            case (uint)SID.Wyrmclaw:
                var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds; // 40s for aero, 17s for claw
                if (duration > 25d)
                    AssignMechanic(actor, Mechanic.ClawAir);
                else
                    AssignMechanic(actor, Mechanic.ClawBlizzard, Mechanic.FangBlizzard);
                if (++_numClaws == 4)
                    AssignClawSides();
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID is (uint)SID.Wyrmclaw or (uint)SID.Wyrmfang)
            Cleansed.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.UltimateRelativitySlow && source.PosRot.Z < Arena.Center.Z)
            NorthSlowHourglass = source.Position - Arena.Center;
    }

    private void AssignMechanic(Actor player, Mechanic mechanic, Mechanic lowerPrio = Mechanic.None, Mechanic higherPrio = Mechanic.None)
    {
        var slot = Raid.FindSlot(player.InstanceID);
        if (slot < 0)
            return;
        ref var mech = ref PlayerMechanics[slot];
        if (mech == Mechanic.None || mech == lowerPrio)
            mech = mechanic;
        else if (mech != higherPrio && mech != mechanic)
            ReportError($"Trying to assing {mechanic} to {player} who already has {mech}");
    }

    private void AssignClawSides()
    {
        void assign(int slot, int prio, ref (int slot, int prio) prev)
        {
            if (prev.prio < 0)
            {
                prev = (slot, prio);
            }
            else
            {
                var prevWest = prev.prio < prio;
                ClawSides[prev.slot] = prevWest ? -1 : 1;
                ClawSides[slot] = prevWest ? 1 : -1;
            }
        }
        Span<(int slot, int prio)> prios = [(-1, -1), (-1, -1), (-1, -1), (-1, -1), (-1, -1), (-1, -1), (-1, -1)];
        foreach (var (slot, group) in Service.Config.Get<FRUConfig>().P4CrystallizeTimeAssignments.Resolve(Raid))
            assign(slot, group, ref prios[(int)PlayerMechanics[slot]]);
    }
}

sealed class P4CrystallizeTimeDragonHead(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor head, int side)> Heads = [];
    private readonly P4CrystallizeTime? _ct = module.FindComponent<P4CrystallizeTime>();
    private readonly List<(Actor puddle, P4CrystallizeTime.Mechanic soaker)> _puddles = [];
    private int _numMaelstroms;

    public Actor? FindHead(int side)
    {
        var count = Heads.Count;
        for (var i = 0; i < count; ++i)
        {
            var h = Heads[i];
            if (h.side == side)
                return h.head;
        }
        return null;
    }

    public static int NumHeadHits(Actor? head) => head == null ? 2 : head.HitboxRadius < 2 ? 1 : 0;
    public Actor? FindInterceptor(Actor head, int side) => _ct?.FindPlayerByAssignment(NumHeadHits(head) > 0 ? P4CrystallizeTime.Mechanic.ClawAir : P4CrystallizeTime.Mechanic.ClawBlizzard, side);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // avoid wrong puddles and try to grab desired one
        if (_ct != null)
        {
            var pcAssignment = _ct.PlayerMechanics[slot];
            var count = _puddles.Count;
            for (var i = 0; i < count; ++i)
            {
                var p = _puddles[i];
                if (p.puddle.EventState != 7)
                {
                    if (p.soaker != pcAssignment)
                        hints.AddForbiddenZone(new SDCircle(p.puddle.Position, 2f));
                    else if (_numMaelstroms >= 6)
                        hints.GoalZones.Add(AIHints.GoalProximity(p.puddle.Position, 15f, 0.25f));
                }
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var count = Heads.Count;
        if (count == 0)
            return;
        for (var i = 0; i < count; ++i)
        {
            var h = Heads[i];
            Arena.Actor(h.head, Colors.Object, true);
            var interceptor = FindInterceptor(h.head, h.side);
            if (interceptor != null)
                Arena.ZoneCircleOutline(interceptor.Position, 12f);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_ct != null /*&& ShowPuddles && !_ct.Cleansed[pcSlot]*/)
        {
            var pcAssignment = _ct.PlayerMechanics[pcSlot];
            var count = _puddles.Count;
            for (var i = 0; i < count; ++i)
            {
                var p = _puddles[i];
                if (p.puddle.EventState != 7)
                    Arena.ZoneCircle(p.puddle.Position, 1f, p.soaker == pcAssignment ? Colors.SafeFromAOE : 0);
            }
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        switch (actor.OID)
        {
            case (uint)OID.DrachenWanderer:
                Heads.Add((actor, actor.PosRot.X > Arena.Center.X ? 1 : -1));
                break;
            case (uint)OID.DragonPuddle:
                // TODO: this is very arbitrary
                var mechanic = actor.PosRot.X < Arena.Center.X
                    ? AssignPuddle(P4CrystallizeTime.Mechanic.FangEruption, P4CrystallizeTime.Mechanic.FangBlizzard)
                    : AssignPuddle(P4CrystallizeTime.Mechanic.FangDarkness, P4CrystallizeTime.Mechanic.FangWater);
                _puddles.Add((actor, mechanic));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DrachenWandererDisappear:
                var count = Heads.Count;
                for (var i = 0; i < count; ++i)
                {
                    if (Heads[i].head == caster)
                    {
                        Heads.RemoveAt(i);
                        return;
                    }
                }
                break;
            case (uint)AID.CrystallizeTimeMaelstrom:
                ++_numMaelstroms;
                break;
        }
    }

    private P4CrystallizeTime.Mechanic AssignPuddle(P4CrystallizeTime.Mechanic first, P4CrystallizeTime.Mechanic second)
    {
        var count = _puddles.Count;
        for (var i = 0; i < count; ++i)
        {
            if (_puddles[i].soaker == first)
            {
                return second;
            }
        }
        return first;
    }
}

sealed class P4CrystallizeTimeMaelstrom(BossModule module) : Components.GenericAOEs(module, (uint)AID.CrystallizeTimeMaelstrom)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(12f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = AOEs.Count;
        if (count == 0)
            return [];
        var max = count > 2 ? 2 : count;
        return CollectionsMarshal.AsSpan(AOEs)[..max];
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by other components

    // assuming that this component is activated when speed cast starts - all hourglasses should be already created, and tethers should have appeared few frames ago
    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.SorrowsHourglass)
        {
            AOEs.Add(new(_shape, actor.Position.Quantized(), actor.Rotation, WorldState.FutureTime(13.2d)));
            SortHelpers.SortAOEByActivation(AOEs);
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var delay = tether.ID switch
        {
            (uint)TetherID.UltimateRelativitySlow => 18.3d,
            (uint)TetherID.UltimateRelativityQuicken => 7.7d,
            _ => 0
        };
        if (delay != 0)
        {
            var count = AOEs.Count;
            var pos = source.Position;
            for (var i = 0; i < count; ++i)
            {
                var aoe = AOEs[i];
                if (aoe.Origin.AlmostEqual(pos, 1f))
                {
                    AOEs.Ref(i).Activation = WorldState.FutureTime(delay);
                    SortHelpers.SortAOEByActivation(AOEs);
                    return;
                }
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == WatchedAction)
        {
            ++NumCasts;
            var count = AOEs.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var aoe = AOEs[i];
                if (aoe.Origin.AlmostEqual(pos, 1f))
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }
}

sealed class P4CrystallizeTimeDarkWater(BossModule module) : Components.UniformStackSpread(module, 6f, default, 4, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by other components

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingDarkWater)
        {
            BitMask forbidden = default;
            if (Module.FindComponent<P4CrystallizeTime>() is var ct && ct != null)
            {
                var len = ct.PlayerMechanics.Length;
                for (var i = 0; i < len; ++i)
                {
                    // should not be shared by eruption and all claws except air on slow side
                    forbidden[i] = ct.PlayerMechanics[i] switch
                    {
                        P4CrystallizeTime.Mechanic.FangEruption => true,
                        P4CrystallizeTime.Mechanic.ClawBlizzard => true,
                        P4CrystallizeTime.Mechanic.ClawAir => ct.ClawSides[i] * ct.NorthSlowHourglass.X > 0f,
                        _ => false
                    };
                }
            }
            AddStack(actor, status.ExpireAt, forbidden);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.DarkWater)
            Stacks.Clear();
    }
}

sealed class P4CrystallizeTimeDarkEruption(BossModule module) : Components.GenericBaitAway(module, (uint)AID.DarkEruption, centerAtTarget: true)
{
    private static readonly AOEShapeCircle _shape = new(6f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by other components

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingDarkEruption)
        {
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape, status.ExpireAt));
        }
    }
}

sealed class P4CrystallizeTimeDarkAero(BossModule module) : Components.GenericKnockback(module, (uint)AID.CrystallizeTimeDarkAero) // TODO: not sure whether it actually ignores immunes, if so need to warn about immunity
{
    private readonly List<Actor> _sources = [];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(15f);

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        var count = _sources.Count;
        if (count == 0)
            return [];
        List<Knockback> sources = [];
        for (var i = 0; i < count; ++i)
        {
            var s = _sources[i];
            if (s != actor)
                sources.Add(new(s.Position, 30f, _activation, _shape));
        }
        return CollectionsMarshal.AsSpan(sources);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingDarkAero)
        {
            _sources.Add(actor);
            _activation = status.ExpireAt;
        }
    }
}

sealed class P4CrystallizeTimeUnholyDarkness(BossModule module) : Components.UniformStackSpread(module, 6f, default, 5, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by other components

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingUnholyDarkness)
        {
            BitMask forbidden = default;
            if (Module.FindComponent<P4CrystallizeTime>() is var ct && ct != null)
            {
                var len = ct.PlayerMechanics.Length;
                for (var i = 0; i < len; ++i)
                {
                    // should not be shared by all claws except blizzard on slow side
                    forbidden[i] = ct.PlayerMechanics[i] switch
                    {
                        P4CrystallizeTime.Mechanic.ClawBlizzard => ct.ClawSides[i] * ct.NorthSlowHourglass.X < 0f,
                        P4CrystallizeTime.Mechanic.ClawAir => true,
                        _ => false
                    };
                }
            }
            AddStack(actor, status.ExpireAt, forbidden);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UltimateRelativityUnholyDarkness)
            Stacks.Clear();
    }
}

sealed class P4CrystallizeTimeTidalLight : Components.Exaflare
{
    public List<(WPos pos, Angle dir)> StartingPositions = [];
    public WDir StartingOffsetSum;

    public P4CrystallizeTimeTidalLight(BossModule module) : base(module, new AOEShapeRect(10f, 20f))
    {
        ImminentColor = Colors.AOE;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.TidalLightAOEFirst)
        {
            Lines.Add(new(caster.Position, 10f * spell.Rotation.ToDirection(), Module.CastFinishAt(spell), 2.1d, 4, 1, spell.Rotation));
            StartingPositions.Add((caster.Position, spell.Rotation));
            StartingOffsetSum += caster.Position - Arena.Center;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.TidalLightAOEFirst or (uint)AID.TidalLightAOERest)
        {
            ++NumCasts;
            var count = Lines.Count;
            var pos = caster.Position;
            for (var i = 0; i < count; ++i)
            {
                var line = Lines[i];
                if (line.Next.AlmostEqual(pos, 1f))
                {
                    AdvanceLine(line, pos);
                    if (line.ExplosionsLeft == 0)
                        Lines.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Failed to find entry for {caster.InstanceID:X}");
        }
    }
}

sealed class P4CrystallizeTimeQuietus(BossModule module) : Components.CastCounter(module, (uint)AID.Quietus);

sealed class P4CrystallizeTimeHints(BossModule module) : BossComponent(module)
{
    [Flags]
    public enum Hint
    {
        None = 0,
        SafespotRough = 1 << 0, // position roughly around safespot
        SafespotPrecise = 1 << 1, // position exactly at safespot
        Maelstrom = 1 << 2, // avoid maelstroms
        Heads = 1 << 3, // avoid head interceptors
        Knockback = 1 << 4, // position to knock back across
        KnockbackFrom = 1 << 5, // position to be a knockback source
        Mid = 1 << 6, // position closer to center if possible
    }

    private readonly P4CrystallizeTime? _ct = module.FindComponent<P4CrystallizeTime>();
    private readonly P4CrystallizeTimeDragonHead? _heads = module.FindComponent<P4CrystallizeTimeDragonHead>();
    private readonly P4CrystallizeTimeMaelstrom? _hourglass = module.FindComponent<P4CrystallizeTimeMaelstrom>();
    private DateTime KnockbacksResolve; // default before knockbacks are done, set to estimated resolve time after they are done
    private bool DarknessDone;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (actor.PendingKnockbacks.Count > 0)
            return; // don't move while waiting for kb to resolve...

        var hint = CalculateHint(slot);
        if (hint.offset != default)
        {
            // we want to stay really close to border
            if (hint.offset.LengthSq() > 324f)
                hint.offset *= 1.02632f;

            if (hint.hint.HasFlag(Hint.KnockbackFrom))
            {
                var party = Raid.WithoutSlot(false, true, true);
                var len = party.Length;
                for (var i = 0; i < len; ++i)
                {
                    if (party[i].PendingKnockbacks.Count != 0)
                    {
                        return; // don't even try moving until all knockbacks are resolved, that can fuck up others...
                    }
                }
            }
            if (hint.hint.HasFlag(Hint.SafespotRough))
            {
                hints.AddForbiddenZone(new SDInvertedCircle(Arena.Center + hint.offset, 1f), DateTime.MaxValue);
            }
            if (hint.hint.HasFlag(Hint.SafespotPrecise))
            {
                hints.PathfindMapBounds = FRU.PathfindHugBorderBounds;
                hints.AddForbiddenZone(new SDPrecisePosition(Arena.Center + hint.offset, new(default, 1f), Arena.Bounds.MapResolution, actor.Position, 0.1f));
            }
            if (hint.hint.HasFlag(Hint.Maelstrom) && _hourglass != null)
            {
                var count = _hourglass.AOEs.Count;
                var max = count > 2 ? 2 : count;
                for (var i = 0; i < max; ++i)
                {
                    var aoe = _hourglass.AOEs[i];
                    hints.AddForbiddenZone(aoe.Shape.Distance(aoe.Origin, aoe.Rotation), aoe.Activation);
                }
            }
            if (hint.hint.HasFlag(Hint.Heads) && _heads != null)
            {
                var count = _heads.Heads.Count;
                for (var i = 0; i < count; ++i)
                {
                    var h = _heads.Heads[i];
                    if (_heads.FindInterceptor(h.head, h.side) is var interceptor && interceptor != null && interceptor != actor)
                        hints.AddForbiddenZone(new SDCircle(interceptor.Position, 12f));
                }
            }
            if (hint.hint.HasFlag(Hint.Knockback) && _ct != null)
            {
                var source = _ct.FindPlayerByAssignment(P4CrystallizeTime.Mechanic.ClawAir, _ct.NorthSlowHourglass.X > 0f ? -1 : 1);
                var dest = Arena.Center + SafeOffsetDarknessStack(_ct.NorthSlowHourglass.X > 0 ? 1 : -1);
                var pos = source != null ? source.Position + 2 * (dest - source.Position).Normalized() : Arena.Center + hint.offset;
                hints.AddForbiddenZone(new SDPrecisePosition(pos, new(default, 1f), Arena.Bounds.MapResolution, actor.Position, 0.1f));
            }
            if (hint.hint.HasFlag(Hint.Mid) && _hourglass != null)
            {
                var count = _hourglass.AOEs.Count;
                var max = count > 2 ? 2 : count;
                for (var i = 0; i < max; ++i)
                {
                    if (_hourglass.AOEs[i].Check(actor.Position))
                        return;
                }
                // stay on correct side
                var dest = Arena.Center + new WDir(default, hint.offset.Z > 0f ? 18f : -18f);
                hints.GoalZones.Add(AIHints.GoalSingleTarget(dest, 2f, 0.5f));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var safeOffset = CalculateHint(pcSlot).offset;
        if (safeOffset != default)
            Arena.ZoneCircleOutline(Arena.Center + safeOffset, 1f, Colors.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.CrystallizeTimeDarkAero:
                KnockbacksResolve = WorldState.FutureTime(1d); // it takes ~0.8s to resolve knockbacks
                break;
            case (uint)AID.UltimateRelativityUnholyDarkness:
                DarknessDone = true;
                break;
        }
    }

    // these are all possible 'raw' safespot offsets; they expect valid arguments
    private static readonly Angle IdealSecondHeadBaitAngle = 33f.Degrees();
    private static WDir SafeOffsetDodgeFirstHourglassSouth(int side) => 19f * (side * 40f).Degrees().ToDirection();
    private static WDir SafeOffsetPreKnockbackSouth(int side, float radius) => radius * (side * 30f).Degrees().ToDirection();
    private static WDir SafeOffsetDarknessStack(int side) => 19f * (side * 140f).Degrees().ToDirection();
    private static WDir SafeOffsetDodgeSecondHourglassSouth(int side) => 19f * (side * 20f).Degrees().ToDirection();
    private static WDir SafeOffsetDodgeSecondHourglassEW(int side) => 19f * (side * 80f).Degrees().ToDirection(); // for ice that doesn't share unholy darkness
    private static WDir SafeOffsetFirstHeadBait(int side) => 13f * (side * 90f).Degrees().ToDirection();
    private static WDir SafeOffsetSecondHeadBait(int side) => 13f * (side * IdealSecondHeadBaitAngle).ToDirection();
    private static WDir SafeOffsetChillNorth(int side) => 6f * (side * 150f).Degrees().ToDirection(); // final for non-airs
    private static WDir SafeOffsetChillSouth(int side) => 6f * (side * 30f).Degrees().ToDirection(); // final for 2 airs

    // these determine rough safespot offset (depending on player state and mechanic progress) for drawing on arena or adding ai hints
    private (WDir offset, Hint hint) CalculateHint(int slot)
    {
        if (_ct == null || _heads == null || _hourglass == null || _ct.NorthSlowHourglass.X == 0)
            return default;
        var clawSide = _ct.ClawSides[slot];
        var northSlowSide = _ct.NorthSlowHourglass.X > 0f ? 1 : -1;
        return _ct.PlayerMechanics[slot] switch
        {
            P4CrystallizeTime.Mechanic.ClawAir => clawSide != 0 ? HintClawAir(clawSide, _hourglass.NumCasts, northSlowSide) : default,
            P4CrystallizeTime.Mechanic.ClawBlizzard => clawSide != 0 ? HintClawBlizzard(clawSide, _hourglass.NumCasts, northSlowSide) : default,
            P4CrystallizeTime.Mechanic.FangEruption => HintFangEruption(northSlowSide, _hourglass.NumCasts),
            _ => HintFangOther(_hourglass.NumCasts, northSlowSide)
        };
    }

    private (WDir offset, Hint hint) HintClawAir(int clawSide, int numHourglassesDone, int northSlowSide)
    {
        if (numHourglassesDone < 2)
            return (SafeOffsetDodgeFirstHourglassSouth(clawSide), Hint.SafespotRough | Hint.Maelstrom); // dodge first hourglass by the south side
        if (KnockbacksResolve == default)
            return (SafeOffsetPreKnockbackSouth(clawSide, 19f), Hint.SafespotPrecise); // preposition to knock party across
        if (numHourglassesDone < 4 && clawSide == northSlowSide)
            return (SafeOffsetDodgeSecondHourglassSouth(clawSide), Hint.SafespotRough | Hint.Maelstrom); // dodge second hourglass; note that player on the slow side can already go intercept the head
        // by now, blizzards have booped their heads, so now it's our turn
        var head = _heads?.FindHead(clawSide);
        if (head != null)
        {
            var headOff = head.Position - Arena.Center;
            var headDir = Angle.FromDirection(headOff) * clawSide; // always decreases as head moves
            var hint = clawSide != northSlowSide && WorldState.CurrentTime < KnockbacksResolve ? Hint.None : Hint.SafespotPrecise; // Hint.KnockbackFrom?.. depends on how new pending knockbacks work for others
            return (headDir.Rad > IdealSecondHeadBaitAngle.Rad ? SafeOffsetSecondHeadBait(clawSide) : headOff, hint);
        }
        // head is done, so dodge between last two hourglasses
        return (SafeOffsetChillSouth(northSlowSide), Hint.Maelstrom | Hint.Heads | Hint.Mid);
    }

    private (WDir offset, Hint hint) HintClawBlizzard(int clawSide, int numHourglassesDone, int northSlowSide)
    {
        if (P4CrystallizeTimeDragonHead.NumHeadHits(_heads?.FindHead(clawSide)) == 0)
            return (SafeOffsetFirstHeadBait(clawSide), Hint.SafespotPrecise); // intercept first head at E/W cardinal
        if (clawSide == northSlowSide)
            return HintFangEruption(northSlowSide, numHourglassesDone); // go stack with eruption after intercepting head
        // non-eruption side - dodge second hourglass, but then immediately dodge head boop
        if (numHourglassesDone < 4)
            return (SafeOffsetDodgeSecondHourglassEW(clawSide), Hint.SafespotRough | Hint.Maelstrom); // dodge second hourglass
        // dodge last hourglass and head
        return (SafeOffsetChillNorth(-northSlowSide), Hint.Maelstrom | Hint.Heads | Hint.Mid);
    }

    private (WDir offset, Hint hint) HintFangEruption(int northSlowSide, int numHourglassesDone)
    {
        if (!DarknessDone)
            return (SafeOffsetDarknessStack(northSlowSide), Hint.SafespotRough | Hint.Heads | (numHourglassesDone < 4 ? Hint.Maelstrom : Hint.None));
        return (SafeOffsetChillNorth(-northSlowSide), Hint.Maelstrom | Hint.Mid);
    }

    private (WDir offset, Hint hint) HintFangOther(int numHourglassesDone, int northSlowSide)
    {
        if (numHourglassesDone < 2)
            return (SafeOffsetDodgeFirstHourglassSouth(-northSlowSide), Hint.SafespotRough | Hint.Maelstrom); // dodge first hourglass by the south side
        if (KnockbacksResolve == default)
            return (SafeOffsetPreKnockbackSouth(-northSlowSide, 17f), Hint.Knockback); // preposition to knockback across arena
        // from now on move together with eruption
        return HintFangEruption(northSlowSide, numHourglassesDone);
    }
}

sealed class P4CrystallizeTimeRewind(BossModule module) : Components.GenericKnockback(module)
{
    public bool RewindDone;
    public bool ReturnDone;
    private readonly P4CrystallizeTime? _ct = module.FindComponent<P4CrystallizeTime>();
    private readonly P4CrystallizeTimeTidalLight? _exalines = module.FindComponent<P4CrystallizeTimeTidalLight>();

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor)
    {
        if (!RewindDone && _ct != null && _exalines != null && _ct.Cleansed[slot])
        {
            var exas = _exalines.StartingPositions;
            var count = exas.Count;
            var sources = new List<Knockback>();
            for (var i = 0; i < count; ++i)
            {
                var s = exas[i];
                sources.Add(new(s.pos, 20f, direction: s.dir, kind: Kind.DirForward));
            }
            return CollectionsMarshal.AsSpan(sources);
        }
        return [];
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (!RewindDone && _ct != null && _exalines != null && _ct.Cleansed[slot])
        {
            var players = Raid.WithoutSlot(false, true, true);
            players.Sort(static (a, b) => a.PosRot.X.CompareTo(b.PosRot.X));
            var xOrder = Array.IndexOf(players, actor);
            players.Sort(static (a, b) => a.PosRot.Z.CompareTo(b.PosRot.Z));
            var zOrder = Array.IndexOf(players, actor);
            if (xOrder >= 0 && zOrder >= 0)
            {
                if (_exalines.StartingOffsetSum.X > 0f)
                    xOrder = players.Length - 1 - xOrder;
                if (_exalines.StartingOffsetSum.Z > 0f)
                    zOrder = players.Length - 1 - zOrder;

                var isFirst = xOrder == 0 || zOrder == 0;
                var isTank = actor.Role == Role.Tank;
                if (isFirst != isTank)
                    hints.Add(isTank ? "Stay in front of the group!" : "Hide behind tank!");
                var isFirstX = xOrder < 4;
                var isFirstZ = zOrder < 4;
                if (isFirstX == isFirstZ)
                    hints.Add("Position in group properly!");
            }
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!RewindDone && _ct != null && _exalines != null && _ct.Cleansed[slot])
        {
            var midpoint = SafeCorner();
            hints.GoalZones.Add(AIHints.GoalProximity(midpoint, 15f, 0.5f));
            var destPoint = midpoint + AssignedPositionOffset(actor, assignment);
            hints.GoalZones.Add(AIHints.GoalProximity(destPoint, 1f, 1f));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!RewindDone && _exalines != null)
        {
            var midpoint = SafeCorner();
            Arena.ZoneCircleOutline(midpoint, 1f);
            var offset = AssignedPositionOffset(pc, Service.Config.Get<PartyRolesConfig>()[Raid.Members[pcSlot].ContentId]);
            if (offset != default)
                Arena.ZoneCircleOutline(midpoint + offset, 1, Colors.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.Return:
                RewindDone = true;
                break;
            case (uint)SID.Stun:
                ReturnDone = true;
                break;
        }
    }

    private WPos SafeCorner() => _exalines != null ? Arena.Center + 0.5f * _exalines.StartingOffsetSum : default;

    private WDir AssignedPositionOffset(Actor actor, PartyRolesConfig.Assignment assignment)
    {
        if (_exalines == null || assignment == PartyRolesConfig.Assignment.Unassigned)
            return default;
        // TODO: make configurable?..
        var isLeft = assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.R1;
        var offDir = (Angle.FromDirection(_exalines.StartingOffsetSum) + (isLeft ? 45 : -45).Degrees()).ToDirection();
        var normDir = isLeft ? offDir.OrthoL() : offDir.OrthoR();
        var (offX, offY) = actor.Role == Role.Tank ? (4, 1) : (1, 2);
        return offX * offDir + offY * normDir;
    }
}

// TODO: custom preposition ai hints
sealed class P4CrystallizeTimeSpiritTaker(BossModule module) : SpiritTaker(module);

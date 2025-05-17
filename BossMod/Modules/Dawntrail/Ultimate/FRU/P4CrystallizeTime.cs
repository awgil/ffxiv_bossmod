namespace BossMod.Dawntrail.Ultimate.FRU;

class P4CrystallizeTime(BossModule module) : BossComponent(module)
{
    public enum Mechanic { None, FangEruption, FangWater, FangDarkness, FangBlizzard, ClawAir, ClawBlizzard }

    public readonly Mechanic[] PlayerMechanics = new Mechanic[PartyState.MaxPartySize];
    public readonly int[] ClawSides = new int[PartyState.MaxPartySize]; // 0 if not assigned (bad config or no claw), +/-1 otherwise for E/W
    public WDir NorthSlowHourglass;
    public BitMask Cleansed;
    private int _numClaws;

    public Actor? FindPlayerByAssignment(Mechanic mechanic, int side)
    {
        for (int i = 0; i < PlayerMechanics.Length; ++i)
            if (PlayerMechanics[i] == mechanic && ClawSides[i] == side)
                return Raid[i];
        return null;
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SpellInWaitingDarkEruption:
                AssignMechanic(actor, Mechanic.FangEruption); // always paired with fang
                break;
            case SID.SpellInWaitingDarkWater:
                AssignMechanic(actor, Mechanic.FangWater); // always paired with fang
                break;
            case SID.SpellInWaitingUnholyDarkness:
                AssignMechanic(actor, Mechanic.FangDarkness); // always paired with fang
                break;
            case SID.SpellInWaitingDarkBlizzard:
                AssignMechanic(actor, Mechanic.FangBlizzard, higherPrio: Mechanic.ClawBlizzard); // paired with either, we'll reassign to claw when reacting to claw buff
                break;
            case SID.SpellInWaitingDarkAero:
                AssignMechanic(actor, Mechanic.ClawAir); // always paired with claw
                break;
            case SID.Wyrmfang:
                break; // don't react
            case SID.Wyrmclaw:
                var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds; // 40s for aero, 17s for claw
                if (duration > 25)
                    AssignMechanic(actor, Mechanic.ClawAir);
                else
                    AssignMechanic(actor, Mechanic.ClawBlizzard, Mechanic.FangBlizzard);
                if (++_numClaws == 4)
                    AssignClawSides();
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID is SID.Wyrmclaw or SID.Wyrmfang)
            Cleansed.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.UltimateRelativitySlow && source.Position.Z < Module.Center.Z)
            NorthSlowHourglass = source.Position - Module.Center;
    }

    private void AssignMechanic(Actor player, Mechanic mechanic, Mechanic lowerPrio = Mechanic.None, Mechanic higherPrio = Mechanic.None)
    {
        if (!Raid.TryFindSlot(player, out var slot))
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

class P4CrystallizeTimeDragonHead(BossModule module) : BossComponent(module)
{
    public readonly List<(Actor head, int side)> Heads = [];
    private readonly P4CrystallizeTime? _ct = module.FindComponent<P4CrystallizeTime>();
    private readonly List<(Actor puddle, P4CrystallizeTime.Mechanic soaker)> _puddles = [];
    private int _numMaelstroms;

    public Actor? FindHead(int side) => Heads.FirstOrDefault(v => v.side == side).head;
    public static int NumHeadHits(Actor? head) => head == null ? 2 : head.HitboxRadius < 2 ? 1 : 0;
    public Actor? FindInterceptor(Actor head, int side) => _ct?.FindPlayerByAssignment(NumHeadHits(head) > 0 ? P4CrystallizeTime.Mechanic.ClawAir : P4CrystallizeTime.Mechanic.ClawBlizzard, side);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        // avoid wrong puddles and try to grab desired one
        if (_ct != null)
        {
            var pcAssignment = _ct.PlayerMechanics[slot];
            foreach (var p in _puddles.Where(p => p.puddle.EventState != 7))
            {
                if (p.soaker != pcAssignment)
                    hints.AddForbiddenZone(ShapeContains.Circle(p.puddle.Position, 2));
                else if (_numMaelstroms >= 6)
                    hints.GoalZones.Add(hints.GoalProximity(p.puddle.Position, 15, 0.25f));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var h in Heads)
        {
            Arena.Actor(h.head, ArenaColor.Object, true);
            var interceptor = FindInterceptor(h.head, h.side);
            if (interceptor != null)
                Arena.AddCircle(interceptor.Position, 12, ArenaColor.Danger);
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_ct != null /*&& ShowPuddles && !_ct.Cleansed[pcSlot]*/)
        {
            var pcAssignment = _ct.PlayerMechanics[pcSlot];
            foreach (var p in _puddles)
                if (p.puddle.EventState != 7)
                    Arena.ZoneCircle(p.puddle.Position, 1, p.soaker == pcAssignment ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.DrachenWanderer:
                Heads.Add((actor, actor.Position.X > Module.Center.X ? 1 : -1));
                break;
            case OID.DragonPuddle:
                // TODO: this is very arbitrary
                var mechanic = actor.Position.X < Module.Center.X
                    ? AssignPuddle(P4CrystallizeTime.Mechanic.FangEruption, P4CrystallizeTime.Mechanic.FangBlizzard)
                    : AssignPuddle(P4CrystallizeTime.Mechanic.FangDarkness, P4CrystallizeTime.Mechanic.FangWater);
                _puddles.Add((actor, mechanic));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DrachenWandererDisappear:
                Heads.RemoveAll(h => h.head == caster);
                break;
            case AID.CrystallizeTimeMaelstrom:
                ++_numMaelstroms;
                break;
        }
    }

    private P4CrystallizeTime.Mechanic AssignPuddle(P4CrystallizeTime.Mechanic first, P4CrystallizeTime.Mechanic second) => _puddles.Any(p => p.soaker == first) ? second : first;
}

class P4CrystallizeTimeMaelstrom(BossModule module) : Components.GenericAOEs(module, AID.CrystallizeTimeMaelstrom)
{
    public readonly List<AOEInstance> AOEs = [];

    private static readonly AOEShapeCircle _shape = new(12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(2);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by other components

    // assuming that this component is activated when speed cast starts - all hourglasses should be already created, and tethers should have appeared few frames ago
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SorrowsHourglass)
        {
            AOEs.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(13.2f)));
            AOEs.SortBy(aoe => aoe.Activation);
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var delay = (TetherID)tether.ID switch
        {
            TetherID.UltimateRelativitySlow => 18.3f,
            TetherID.UltimateRelativityQuicken => 7.7f,
            _ => 0
        };
        if (delay != 0)
        {
            var index = AOEs.FindIndex(aoe => aoe.Origin.AlmostEqual(source.Position, 1));
            if (index >= 0)
            {
                AOEs.Ref(index).Activation = WorldState.FutureTime(delay);
                AOEs.SortBy(aoe => aoe.Activation);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class P4CrystallizeTimeDarkWater(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by other components

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkWater)
        {
            BitMask forbidden = default;
            if (Module.FindComponent<P4CrystallizeTime>() is var ct && ct != null)
            {
                for (int i = 0; i < ct.PlayerMechanics.Length; ++i)
                {
                    // should not be shared by eruption and all claws except air on slow side
                    forbidden[i] = ct.PlayerMechanics[i] switch
                    {
                        P4CrystallizeTime.Mechanic.FangEruption => true,
                        P4CrystallizeTime.Mechanic.ClawBlizzard => true,
                        P4CrystallizeTime.Mechanic.ClawAir => ct.ClawSides[i] * ct.NorthSlowHourglass.X > 0,
                        _ => false
                    };
                }
            }
            AddStack(actor, status.ExpireAt, forbidden);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DarkWater)
            Stacks.Clear();
    }
}

class P4CrystallizeTimeDarkEruption(BossModule module) : Components.GenericBaitAway(module, AID.DarkEruption)
{
    private static readonly AOEShapeCircle _shape = new(6);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by other components

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkEruption)
        {
            CurrentBaits.Add(new(actor, actor, _shape, status.ExpireAt));
        }
    }
}

class P4CrystallizeTimeDarkAero(BossModule module) : Components.Knockback(module, AID.CrystallizeTimeDarkAero) // TODO: not sure whether it actually ignores immunes, if so need to warn about immunity
{
    private readonly List<Actor> _sources = [];
    private DateTime _activation;

    private static readonly AOEShapeCircle _shape = new(15);

    public override IEnumerable<Source> Sources(int slot, Actor actor) => _sources.Exclude(actor).Select(s => new Source(s.Position, 30, _activation, _shape));

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkAero)
        {
            _sources.Add(actor);
            _activation = status.ExpireAt;
        }
    }
}

class P4CrystallizeTimeUnholyDarkness(BossModule module) : Components.UniformStackSpread(module, 6, 0, 5, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by other components

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingUnholyDarkness)
        {
            BitMask forbidden = default;
            if (Module.FindComponent<P4CrystallizeTime>() is var ct && ct != null)
            {
                for (int i = 0; i < ct.PlayerMechanics.Length; ++i)
                {
                    // should not be shared by all claws except blizzard on slow side
                    forbidden[i] = ct.PlayerMechanics[i] switch
                    {
                        P4CrystallizeTime.Mechanic.ClawBlizzard => ct.ClawSides[i] * ct.NorthSlowHourglass.X < 0,
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
        if ((AID)spell.Action.ID == AID.UltimateRelativityUnholyDarkness)
            Stacks.Clear();
    }
}

class P4CrystallizeTimeTidalLight : Components.Exaflare
{
    public List<(WPos pos, Angle dir)> StartingPositions = [];
    public WDir StartingOffsetSum;

    public P4CrystallizeTimeTidalLight(BossModule module) : base(module, new AOEShapeRect(10, 20))
    {
        ImminentColor = ArenaColor.AOE;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TidalLightAOEFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 10 * spell.Rotation.ToDirection(), Rotation = spell.Rotation, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2.1f, ExplosionsLeft = 4, MaxShownExplosions = 1 });
            StartingPositions.Add((caster.Position, spell.Rotation));
            StartingOffsetSum += caster.Position - Module.Center;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TidalLightAOEFirst or AID.TidalLightAOERest)
        {
            ++NumCasts;
            int index = Lines.FindIndex(item => item.Next.AlmostEqual(caster.Position, 1));
            if (index == -1)
            {
                ReportError($"Failed to find entry for {caster.InstanceID:X}");
                return;
            }

            AdvanceLine(Lines[index], caster.Position);
            if (Lines[index].ExplosionsLeft == 0)
                Lines.RemoveAt(index);
        }
    }
}

class P4CrystallizeTimeQuietus(BossModule module) : Components.CastCounter(module, AID.Quietus);

class P4CrystallizeTimeHints(BossModule module) : BossComponent(module)
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
            if (hint.offset.LengthSq() > 18 * 18)
                hint.offset *= 19.5f / 19;

            if (hint.hint.HasFlag(Hint.KnockbackFrom) && Raid.WithoutSlot().Any(p => p.PendingKnockbacks.Count > 0))
            {
                return; // don't even try moving until all knockbacks are resolved, that can fuck up others...
            }
            if (hint.hint.HasFlag(Hint.SafespotRough))
            {
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(Module.Center + hint.offset, 1), DateTime.MaxValue);
            }
            if (hint.hint.HasFlag(Hint.SafespotPrecise))
            {
                hints.PathfindMapBounds = FRU.PathfindHugBorderBounds;
                hints.AddForbiddenZone(ShapeContains.PrecisePosition(Module.Center + hint.offset, new(0, 1), Module.Bounds.MapResolution, actor.Position, 0.1f));
            }
            if (hint.hint.HasFlag(Hint.Maelstrom) && _hourglass != null)
            {
                foreach (var aoe in _hourglass.AOEs.Take(2))
                    hints.AddForbiddenZone(aoe.Shape.CheckFn(aoe.Origin, aoe.Rotation), aoe.Activation);
            }
            if (hint.hint.HasFlag(Hint.Heads) && _heads != null)
            {
                foreach (var h in _heads.Heads)
                    if (_heads.FindInterceptor(h.head, h.side) is var interceptor && interceptor != null && interceptor != actor)
                        hints.AddForbiddenZone(ShapeContains.Circle(interceptor.Position, 12));
            }
            if (hint.hint.HasFlag(Hint.Knockback) && _ct != null)
            {
                var source = _ct.FindPlayerByAssignment(P4CrystallizeTime.Mechanic.ClawAir, _ct.NorthSlowHourglass.X > 0 ? -1 : 1);
                var dest = Module.Center + SafeOffsetDarknessStack(_ct.NorthSlowHourglass.X > 0 ? 1 : -1);
                var pos = source != null ? source.Position + 2 * (dest - source.Position).Normalized() : Module.Center + hint.offset;
                hints.AddForbiddenZone(ShapeContains.PrecisePosition(pos, new(0, 1), Module.Bounds.MapResolution, actor.Position, 0.1f));
            }
            if (hint.hint.HasFlag(Hint.Mid) && _hourglass != null && !_hourglass.AOEs.Take(2).Any(aoe => aoe.Check(actor.Position)))
            {
                // stay on correct side
                var dest = Module.Center + new WDir(0, hint.offset.Z > 0 ? 18 : -18);
                hints.GoalZones.Add(hints.GoalSingleTarget(dest, 2, 0.5f));
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var safeOffset = CalculateHint(pcSlot).offset;
        if (safeOffset != default)
            Arena.AddCircle(Module.Center + safeOffset, 1, ArenaColor.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CrystallizeTimeDarkAero:
                KnockbacksResolve = WorldState.FutureTime(1); // it takes ~0.8s to resolve knockbacks
                break;
            case AID.UltimateRelativityUnholyDarkness:
                DarknessDone = true;
                break;
        }
    }

    // these are all possible 'raw' safespot offsets; they expect valid arguments
    private static readonly Angle IdealSecondHeadBaitAngle = 33.Degrees();
    private WDir SafeOffsetDodgeFirstHourglassSouth(int side) => 19 * (side * 40).Degrees().ToDirection();
    private WDir SafeOffsetPreKnockbackSouth(int side, float radius) => radius * (side * 30).Degrees().ToDirection();
    private WDir SafeOffsetDarknessStack(int side) => 19 * (side * 140).Degrees().ToDirection();
    private WDir SafeOffsetDodgeSecondHourglassSouth(int side) => 19 * (side * 20).Degrees().ToDirection();
    private WDir SafeOffsetDodgeSecondHourglassEW(int side) => 19 * (side * 80).Degrees().ToDirection(); // for ice that doesn't share unholy darkness
    private WDir SafeOffsetFirstHeadBait(int side) => 13 * (side * 90).Degrees().ToDirection();
    private WDir SafeOffsetSecondHeadBait(int side) => 13 * (side * IdealSecondHeadBaitAngle).ToDirection();
    private WDir SafeOffsetChillNorth(int side) => 6 * (side * 150).Degrees().ToDirection(); // final for non-airs
    private WDir SafeOffsetChillSouth(int side) => 6 * (side * 30).Degrees().ToDirection(); // final for 2 airs

    // these determine rough safespot offset (depending on player state and mechanic progress) for drawing on arena or adding ai hints
    private (WDir offset, Hint hint) CalculateHint(int slot)
    {
        if (_ct == null || _heads == null || _hourglass == null || _ct.NorthSlowHourglass.X == 0)
            return default;
        var clawSide = _ct.ClawSides[slot];
        var northSlowSide = _ct.NorthSlowHourglass.X > 0 ? 1 : -1;
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
            return (SafeOffsetPreKnockbackSouth(clawSide, 19), Hint.SafespotPrecise); // preposition to knock party across
        if (numHourglassesDone < 4 && clawSide == northSlowSide)
            return (SafeOffsetDodgeSecondHourglassSouth(clawSide), Hint.SafespotRough | Hint.Maelstrom); // dodge second hourglass; note that player on the slow side can already go intercept the head
        // by now, blizzards have booped their heads, so now it's our turn
        var head = _heads?.FindHead(clawSide);
        if (head != null)
        {
            var headOff = head.Position - Module.Center;
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
            return (SafeOffsetPreKnockbackSouth(-northSlowSide, 17), Hint.Knockback); // preposition to knockback across arena
        // from now on move together with eruption
        return HintFangEruption(northSlowSide, numHourglassesDone);
    }
}

class P4CrystallizeTimeRewind(BossModule module) : Components.Knockback(module)
{
    public bool RewindDone;
    public bool ReturnDone;
    private readonly P4CrystallizeTime? _ct = module.FindComponent<P4CrystallizeTime>();
    private readonly P4CrystallizeTimeTidalLight? _exalines = module.FindComponent<P4CrystallizeTimeTidalLight>();

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (!RewindDone && _ct != null && _exalines != null && _ct.Cleansed[slot])
            foreach (var s in _exalines.StartingPositions)
                yield return new(s.pos, 20, Direction: s.dir, Kind: Kind.DirForward);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);
        if (!RewindDone && _ct != null && _exalines != null && _ct.Cleansed[slot])
        {
            var players = Raid.WithoutSlot(excludeNPCs: true).ToList();
            players.SortBy(p => p.Position.X);
            var xOrder = players.IndexOf(actor);
            players.SortBy(p => p.Position.Z);
            var zOrder = players.IndexOf(actor);
            if (xOrder >= 0 && zOrder >= 0)
            {
                if (_exalines.StartingOffsetSum.X > 0)
                    xOrder = players.Count - 1 - xOrder;
                if (_exalines.StartingOffsetSum.Z > 0)
                    zOrder = players.Count - 1 - zOrder;

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
            hints.GoalZones.Add(hints.GoalProximity(midpoint, 15, 0.5f));
            var destPoint = midpoint + AssignedPositionOffset(actor, assignment);
            hints.GoalZones.Add(hints.GoalProximity(destPoint, 1, 1));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!RewindDone && _exalines != null)
        {
            var midpoint = SafeCorner();
            Arena.AddCircle(midpoint, 1, ArenaColor.Danger);
            var offset = AssignedPositionOffset(pc, Service.Config.Get<PartyRolesConfig>()[Module.Raid.Members[pcSlot].ContentId]);
            if (offset != default)
                Arena.AddCircle(midpoint + offset, 1, ArenaColor.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Return:
                RewindDone = true;
                break;
            case SID.Stun:
                ReturnDone = true;
                break;
        }
    }

    private WPos SafeCorner() => _exalines != null ? Module.Center + 0.5f * _exalines.StartingOffsetSum : default;

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
class P4CrystallizeTimeSpiritTaker(BossModule module) : SpiritTaker(module);

namespace BossMod.Dawntrail.Ultimate.FRU;

// TODO: hints and stuff...
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

class P4CrystallizeTimeDragonHead(BossModule module) : BossComponent(module)
{
    public bool ShowPuddles;
    private readonly P4CrystallizeTime? _ct = module.FindComponent<P4CrystallizeTime>();
    private readonly List<(Actor head, int side)> _heads = [];
    private readonly List<(Actor puddle, P4CrystallizeTime.Mechanic soaker)> _puddles = [];

    public Actor? FindHead(int side) => _heads.FirstOrDefault(v => v.side == side).head;
    public static int NumHeadHits(Actor? head) => head == null ? 2 : head.HitboxRadius < 2 ? 1 : 0;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var h in _heads)
        {
            Arena.Actor(h.head, ArenaColor.Object, true);
            var interceptor = _ct?.FindPlayerByAssignment(NumHeadHits(h.head) > 0 ? P4CrystallizeTime.Mechanic.ClawAir : P4CrystallizeTime.Mechanic.ClawBlizzard, h.side);
            if (interceptor != null)
                Arena.AddCircle(interceptor.Position, 12, ArenaColor.Danger);

        }

        if (ShowPuddles && _ct != null && !_ct.Cleansed[pcSlot])
        {
            var pcAssignment = _ct.PlayerMechanics[pcSlot];
            foreach (var p in _puddles)
                if (p.puddle.EventState != 7)
                    Arena.AddCircle(p.puddle.Position, 1, p.soaker == pcAssignment ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.DrachenWanderer:
                _heads.Add((actor, actor.Position.X > Module.Center.X ? 1 : -1));
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
        if ((AID)spell.Action.ID == AID.DrachenWandererDisappear)
            _heads.RemoveAll(h => h.head == caster);
    }

    private P4CrystallizeTime.Mechanic AssignPuddle(P4CrystallizeTime.Mechanic first, P4CrystallizeTime.Mechanic second) => _puddles.Any(p => p.soaker == first) ? second : first;
}

class P4CrystallizeTimeMaelstrom(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.CrystallizeTimeMaelstrom))
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCircle _shape = new(12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(2);

    // assuming that this component is activated when speed cast starts - all hourglasses should be already created, and tethers should have appeared few frames ago
    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.SorrowsHourglass)
            _aoes.Add(new(_shape, actor.Position, actor.Rotation, WorldState.FutureTime(13.2f)));
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
            var index = _aoes.FindIndex(aoe => aoe.Origin.AlmostEqual(source.Position, 1));
            if (index >= 0)
            {
                _aoes.Ref(index).Activation = WorldState.FutureTime(delay);
                _aoes.SortBy(aoe => aoe.Activation);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class P4CrystallizeTimeDarkWater(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
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

class P4CrystallizeTimeDarkEruption(BossModule module) : Components.GenericBaitAway(module, ActionID.MakeSpell(AID.DarkEruption))
{
    private static readonly AOEShapeCircle _shape = new(6);

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkEruption)
        {
            CurrentBaits.Add(new(actor, actor, _shape, status.ExpireAt));
        }
    }
}

class P4CrystallizeTimeDarkAero(BossModule module) : Components.Knockback(module, ActionID.MakeSpell(AID.CrystallizeTimeDarkAero)) // TODO: not sure whether it actually ignores immunes, if so need to warn about immunity
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
    public WDir StartingOffset;

    public P4CrystallizeTimeTidalLight(BossModule module) : base(module, new AOEShapeRect(10, 20))
    {
        ImminentColor = ArenaColor.AOE;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.TidalLightAOEFirst)
        {
            Lines.Add(new() { Next = caster.Position, Advance = 10 * spell.Rotation.ToDirection(), Rotation = spell.Rotation, NextExplosion = Module.CastFinishAt(spell), TimeToMove = 2.1f, ExplosionsLeft = 4, MaxShownExplosions = 1 });
            StartingOffset += caster.Position - Module.Center;
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

class P4CrystallizeTimeQuietus(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Quietus));

class P4CrystallizeTimeHints(BossModule module) : BossComponent(module)
{
    private readonly P4CrystallizeTime? _ct = module.FindComponent<P4CrystallizeTime>();
    private readonly P4CrystallizeTimeDragonHead? _heads = module.FindComponent<P4CrystallizeTimeDragonHead>();
    private readonly P4CrystallizeTimeMaelstrom? _hourglass = module.FindComponent<P4CrystallizeTimeMaelstrom>();
    private bool KnockbacksDone;
    private bool DarknessDone;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var safeOffset = SafeOffset(pcSlot);
        if (safeOffset != default)
            Arena.AddCircle(Module.Center + safeOffset, 1, ArenaColor.Safe);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.CrystallizeTimeDarkAero:
                KnockbacksDone = true;
                break;
            case AID.UltimateRelativityUnholyDarkness:
                DarknessDone = true;
                break;
        }
    }

    private WDir SafeOffset(int slot)
    {
        if (_ct == null || _heads == null || _hourglass == null || _ct.NorthSlowHourglass.X == 0)
            return default;
        var clawSide = _ct.ClawSides[slot];
        return _ct.PlayerMechanics[slot] switch
        {
            P4CrystallizeTime.Mechanic.ClawAir => clawSide != 0 ? SafeOffsetClawAir(clawSide, _hourglass.NumCasts, _ct.NorthSlowHourglass.X) : default,
            P4CrystallizeTime.Mechanic.ClawBlizzard => clawSide != 0 ? SafeOffsetClawBlizzard(clawSide, _hourglass.NumCasts, _ct.NorthSlowHourglass.X) : default,
            P4CrystallizeTime.Mechanic.FangEruption => SafeOffsetFangEruption(_ct.NorthSlowHourglass.X),
            _ => SafeOffsetFangOther(_hourglass.NumCasts, _ct.NorthSlowHourglass.X)
        };
    }

    private WDir SafeOffsetClawAir(int side, int numHourglassesDone, float northSlowX)
    {
        if (numHourglassesDone < 2)
            return 19 * (side * 40).Degrees().ToDirection(); // dodge first hourglass by the south side
        if (!KnockbacksDone)
            return 19 * (side * 30).Degrees().ToDirection(); // preposition to knock party across
        if (numHourglassesDone < 4)
            return 19 * (side * 20).Degrees().ToDirection(); // dodge second hourglass; note that player on the slow side can't really boop head earlier anyway
        // by now, blizzards have booped their heads, so now it's our turn
        var head = _heads?.FindHead(side);
        if (head != null)
            return head.Position - Module.Center;
        // head is done, so dodge between last two hourglasses
        return 6 * (northSlowX > 0 ? 30 : -30).Degrees().ToDirection();
    }

    private WDir SafeOffsetClawBlizzard(int side, int numHourglassesDone, float northSlowX)
    {
        var head = _heads?.FindHead(side);
        if (head != null && P4CrystallizeTimeDragonHead.NumHeadHits(head) == 0)
            return (head.Position - Module.Center).Length() * (side * 90).Degrees().ToDirection(); // intercept first head at E/W cardinal
        var shareDarknessStack = side * northSlowX > 0;
        if (shareDarknessStack)
            return SafeOffsetFangEruption(northSlowX); // go stack with eruption after intercepting head
        // dodge hourglasses
        return numHourglassesDone < 4 ? 19 * (side * 80).Degrees().ToDirection() : SafeOffsetFinalNonAir(northSlowX);
    }

    private WDir SafeOffsetFangEruption(float northSlowX) => !DarknessDone ? SafeOffsetDarknessStack(northSlowX) : SafeOffsetFinalNonAir(northSlowX);

    private WDir SafeOffsetFangOther(int numHourglassesDone, float northSlowX)
    {
        if (numHourglassesDone < 2)
            return 19 * (northSlowX > 0 ? -40 : 40).Degrees().ToDirection(); // dodge first hourglass by the south side
        if (!KnockbacksDone)
            return 17 * (northSlowX > 0 ? -30 : 30).Degrees().ToDirection(); // preposition to knockback across arena
        // from now on move together with eruption
        return SafeOffsetFangEruption(northSlowX);
    }

    private WDir SafeOffsetDarknessStack(float northSlowX) => 19 * (northSlowX > 0 ? 140 : -140).Degrees().ToDirection();
    private WDir SafeOffsetFinalNonAir(float northSlowX) => 6 * (northSlowX > 0 ? -150 : 150).Degrees().ToDirection();
}

class P4CrystallizeTimeRewind(BossModule module) : BossComponent(module)
{
    public bool Done;
    private readonly P4CrystallizeTime? _ct = module.FindComponent<P4CrystallizeTime>();
    private readonly P4CrystallizeTimeTidalLight? _exalines = module.FindComponent<P4CrystallizeTimeTidalLight>();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_ct != null && _exalines != null && _ct.Cleansed[pcSlot])
            Arena.AddCircle(Module.Center + 0.5f * _exalines.StartingOffset, 1, ArenaColor.Safe); // TODO: better hints...
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.Return)
            Done = true;
    }
}

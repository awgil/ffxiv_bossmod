namespace BossMod.Dawntrail.Ultimate.UMAD;

class P3VacuumWave(BossModule module) : Components.CastCounter(module, AID.VacuumWave);

class P3UmbraSmash(BossModule module) : Components.ProximityAOEs(module, AID.UmbraSmash, 20);

class P3UmbraSmashBait : Components.GenericBaitAway
{
    readonly DateTime _activation;

    public P3UmbraSmashBait(BossModule module) : base(module, AID.UmbraSmash, centerAtTarget: true)
    {
        _activation = WorldState.FutureTime(9.2f);
    }

    public override void Update()
    {
        CurrentBaits.Clear();

        if (((UMAD)Module).ChaosP3() is { } chaos && Raid.WithoutSlot().Farthest(chaos.Position) is { } target)
            CurrentBaits.Add(new(chaos, target, new AOEShapeCircle(20), _activation));
    }
}

class P3UltimaBlasterRaidwide : Components.RaidwideInstant
{
    public P3UltimaBlasterRaidwide(BossModule module) : base(module, AID.UltimaBlasterRaidwide, 10)
    {
        Activation = WorldState.FutureTime(Delay);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (NumCasts >= 8)
                Activation = default;
        }
    }
}

class P3UltimaBlasterCharge(BossModule module) : Components.UntelegraphedBait(module, AID.UltimaBlasterCharge)
{
    readonly int[] _order = Utils.MakeArray(8, -1);

    public bool Draw;

    WPos _cloneStart; // clones are spaced evenly about the arena edge
    Angle _cloneRotate;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.UltimaBlasterRaidwide)
        {
            if (_cloneStart == default)
                _cloneStart = caster.Position;
            else if (_cloneRotate == default)
            {
                var dir1 = _cloneStart - Arena.Center;
                var dir2 = caster.Position - Arena.Center;
                _cloneRotate = dir1.OrthoL().Dot(dir2) > 0 ? -45.Degrees() : 45.Degrees();
            }
        }

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            if (CurrentBaits.Count > 0)
                CurrentBaits.RemoveAt(0);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var order = (IconID)iconID switch
        {
            // these ids aren't contiguous, don't try to be clever
            IconID.Blaster1 => 0,
            IconID.Blaster2 => 1,
            IconID.Blaster3 => 2,
            IconID.Blaster4 => 3,
            IconID.Blaster5 => 4,
            IconID.Blaster6 => 5,
            IconID.Blaster7 => 6,
            IconID.Blaster8 => 7,
            _ => -1
        };

        if (order is >= 0 and < 8 && Raid.TryFindSlot(actor, out var slot))
        {
            _order[slot] = order;

            if (_cloneStart == default || _cloneRotate == default)
            {
                ReportError($"Got LC icon {order} for {actor}, but can't determine clone positions!");
                return;
            }

            var clone0Dir = _cloneStart - Arena.Center;
            var clonePos = Arena.Center + clone0Dir.Rotate(_cloneRotate * order);
            CurrentBaits.Add(new(clonePos, BitMask.Build(slot), new AOEShapeRect(100, 3), WorldState.FutureTime(12.1f + 0.2f * order)));
            CurrentBaits.SortBy(c => c.Activation);
        }
    }

    WPos SafeSpot(Bait b)
    {
        var fromBait = Arena.Center - b.Origin;
        return Arena.Center + fromBait.Normalized().Rotate(_cloneRotate * 0.5f) * 19;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!Draw)
            return;

        base.DrawArenaForeground(pcSlot, pc);

        foreach (var bait in BaitsOn(pcSlot))
        {
            Arena.AddCircle(bait.Origin, 30, ArenaColor.Object);
            Arena.AddCircle(SafeSpot(bait), 0.75f, ArenaColor.Safe);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        foreach (var bait in BaitsOn(slot))
            movementHints.Add(actor.Position, SafeSpot(bait), ArenaColor.Safe);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (Draw)
            base.DrawArenaBackground(pcSlot, pc);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!Draw)
            return;

        base.AddHints(slot, actor, hints);

        foreach (var bait in BaitsOn(slot))
            if (actor.Position.InCircle(bait.Origin, 30))
                hints.Add("Get away from clone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Draw)
            return;

        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var bait in BaitsOn(slot))
            hints.AddForbiddenZone(ShapeContains.Circle(bait.Origin, 30), bait.Activation);
    }
}

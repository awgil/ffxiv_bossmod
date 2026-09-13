namespace BossMod.Stormblood.Ultimate.UCOB;

class P3HeavensfallPreposition(BossModule module) : Components.CastCounter(module, AID.HeavensfallTrio)
{
    // heavensfall cast start to dive bait
    private readonly DateTime _diveAt = module.WorldState.FutureTime(8.5f);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (((UCOB)Module).BahamutPrime() is not { } baha)
            return;

        if (baha.IsTargetable)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 5), _diveAt.AddSeconds(-1));
        else
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 1), _diveAt);
    }
}

class P3HeavensfallTrio(BossModule module) : BossComponent(module)
{
    private Actor? _nael;
    private Actor? _twin;
    private Actor? _baha;
    private readonly WPos[] _safeSpots = new WPos[PartyState.MaxPartySize];

    public bool Active => _nael != null;

    private bool _divesActive;
    private bool _divesDone;
    private bool _puddlesActive;

    private static readonly Angle[] _offsetsNaelCenter = [10.Degrees(), 80.Degrees(), 100.Degrees(), 170.Degrees()];
    private static readonly Angle[] _offsetsNaelSide = [60.Degrees(), 80.Degrees(), 100.Degrees(), 120.Degrees()];

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_nael, ArenaColor.Object, true);
        var safespot = _safeSpots[pcSlot];
        if (safespot != default)
            Arena.AddCircle(safespot, 1, ArenaColor.Safe);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.NaelDeusDarnus && id == 0x1E43)
        {
            _nael = actor;
            InitIfReady();
        }
        else if ((OID)actor.OID == OID.Twintania && id == 0x1E44)
        {
            _twin = actor;
            InitIfReady();
        }
        else if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
        {
            _baha = actor;
            InitIfReady();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_divesActive)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(_safeSpots[slot], 1));

        if (!_puddlesActive && Module.FindComponent<P3Twister>() is { Predicted: true } or { Active: true })
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 8));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.MegaflareDive)
            _divesActive = true;

        if ((AID)spell.Action.ID == AID.MegaflarePuddle)
            _puddlesActive = true;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.TwistingDive)
        {
            _divesActive = false;
            _divesDone = true;
            Array.Fill(_safeSpots, default);
        }
    }

    private void InitIfReady()
    {
        if (_nael == null || _twin == null || _baha == null || _divesDone)
            return;

        var dirToNael = Angle.FromDirection(_nael.Position - Module.Center);
        var dirToTwin = Angle.FromDirection(_twin.Position - Module.Center);
        var dirToBaha = Angle.FromDirection(_baha.Position - Module.Center);

        var twinRel = (dirToTwin - dirToNael).Normalized();
        var bahaRel = (dirToBaha - dirToNael).Normalized();
        var (offsetSymmetry, offsets) = twinRel.Rad * bahaRel.Rad < 0 // twintania & bahamut are on different sides => nael is in center
            ? (0.Degrees(), _offsetsNaelCenter)
            : ((twinRel + bahaRel) * 0.5f, _offsetsNaelSide);
        var dirSymmetry = dirToNael + offsetSymmetry;
        foreach (var p in Service.Config.Get<UCOBConfig>().P3QuickmarchTrioAssignments.Resolve(Raid))
        {
            var left = p.group < 4;
            var order = p.group & 3;
            var offset = offsets[order];
            var dir = dirSymmetry + (left ? offset : -offset);
            _safeSpots[p.slot] = Module.Center + 20 * dir.ToDirection();
        }
    }
}

class P3Heavensfall(BossModule module) : Heavensfall(module)
{
    // no hints, handled by towers component
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}

class P3HeavensfallTowers(BossModule module) : Components.CastTowers(module, AID.MegaflareTower, 3)
{
    bool _knockbackDone;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.Heavensfall)
            _knockbackDone = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction && Towers.Count == 8)
        {
            var nael = Module.Enemies(OID.NaelDeusDarnus).FirstOrDefault();
            if (nael != null)
            {
                var dirToNael = Angle.FromDirection(nael.Position - Module.Center);
                var orders = Towers.Select(t => TowerSortKey(Angle.FromDirection(t.Position - Module.Center), dirToNael)).ToList();
                MemoryExtensions.Sort(orders.AsSpan(), Towers.AsSpan());
                foreach (var p in Service.Config.Get<UCOBConfig>().P3HeavensfallTrioTowers.Resolve(Raid))
                {
                    Towers.Ref(p.group).ForbiddenSoakers = new(~(1ul << p.slot));
                }
            }
        }
    }

    // order towers from nael's position CW
    private float TowerSortKey(Angle tower, Angle reference)
    {
        var cwDist = (reference - tower).Normalized().Deg;
        if (cwDist < -5f) // towers are ~22.5 degrees apart
            cwDist += 360;
        return cwDist;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_knockbackDone)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
        else if (Towers.FirstOrNull(t => !t.ForbiddenSoakers[slot]) is { } myTower)
        {
            var dir = myTower.Position - Arena.Center;
            var mySpot = Arena.Center + dir.Normalized() * 9;

            hints.GoalZones.Add(AIHints.GoalProximity(mySpot, 10, 5));

            hints.AddForbiddenZone(ShapeDistance.PrecisePosition(mySpot, new(0, 1), Arena.Bounds.MapResolution, actor.Position, 0.1f), myTower.Activation);
        }
    }
}

class P3HeavensfallFireball(BossModule module) : Components.StackWithIcon(module, (uint)IconID.Fireball, AID.Fireball, 4, 5.3f, 8)
{
    int _numHypernovas;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count > 0)
        {
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 2), Stacks[0].Activation);
            return;
        }

        // last hypernova to stack going off is 6 seconds
        switch (_numHypernovas)
        {
            case 3:
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 8), DateTime.MaxValue);
                break;
            case 2:
                hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 15), DateTime.MaxValue);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.Hypernova)
            _numHypernovas++;
    }
}

class P3ThermionicBurst(BossModule module) : P2ThermionicBurst(module)
{
    private readonly Angle[] _startingSlice = new Angle[PartyState.MaxPartySize];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.MegaflareTower)
        {
            foreach (var target in spell.Targets)
                if (Raid.TryFindSlot(target.ID, out var slot))
                    _startingSlice[slot] = (spell.TargetXZ - Arena.Center).ToAngle() - 11.25f.Degrees();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var numAoes = 0;
        foreach (var aoe in ActiveAOEs(slot, actor))
        {
            if (aoe.Activation > WorldState.CurrentTime || NumCasts < 2)
            {
                hints.AddForbiddenZone(aoe.Distance, aoe.Activation);
                if (++numAoes >= 2)
                    break;
            }
        }

        if (NumCasts < 16 && _startingSlice[slot] != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(Arena.Center, _startingSlice[slot], 40, -2, 1.5f));
    }
}

class P3HeavensfallHypernova(BossModule module) : P2Hypernova(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        foreach (var p in _predictedByEvent)
        {
            var dir = p.pos - Arena.Center;
            hints.AddForbiddenZone(ShapeDistance.Rect(p.pos, dir.ToAngle(), 50, 0, 5), p.time);
        }
    }
}

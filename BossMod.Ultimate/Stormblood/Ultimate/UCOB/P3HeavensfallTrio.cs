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
        if (_knockbackDone && actor.PendingKnockbacks.Count == 0)
            base.AddAIHints(slot, actor, assignment, hints);

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
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Stacks.Count > 0)
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(Arena.Center, 2), Stacks[0].Activation);
    }
}

class P3ThermionicBurst(BossModule module) : P2ThermionicBurst(module)
{
    bool _towersDone;
    int _rotation; // 0 if unknown, 1 cw, -1 ccw
    Angle? _start;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);

        if ((AID)spell.Action.ID == AID.MegaflareTower)
            _towersDone = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            if (Casters.Count == 0 && NumCasts == 0)
                _start = spell.Rotation;

            if (Casters.Count == 2)
                _rotation = Casters.Any(c => c.CastInfo!.Rotation.AlmostEqual(spell.Rotation + 22.5f.Degrees(), 0.1f)) ? 1 : -1;
        }

        base.OnCastStarted(caster, spell);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        //if (_towersDone && Casters.Count == 0 && NumCasts == 0)
        //    hints.GoalZones.Add(AIHints.GoalSingleTarget(Arena.Center, 13));

        if (_start is { } s)
        {
            if (_rotation != 0)
            {
                var ctr = Arena.Center;

                var bias = s + (_rotation * 5).Degrees();
                var h = 11.25f.Degrees().Cos();

                hints.GoalZones.Add(p =>
                {
                    var c = (p - ctr).Normalized().Dot(bias.ToDirection());
                    return MathF.Abs(c) >= h ? 1 : 0;
                });
            }
            else
            {
                hints.AddForbiddenZone(ShapeDistance.Intersection([ShapeDistance.InvertedCircle(Arena.Center + (s + 90.Degrees()).ToDirection() * 20, 2), ShapeDistance.InvertedCircle(Arena.Center + (s - 90.Degrees()).ToDirection() * 20, 2)]), DateTime.MaxValue);
            }
        }
    }
}

class P3HeavensfallHypernova(BossModule module) : P2Hypernova(module)
{
    int _numSpawned;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        if (_numSpawned >= 3)
            return;

        hints.GoalZones.Add(AIHints.GoalProximity(Arena.Center, 10, 5));

        var total = _predictedByEvent.Count + _sources.Count;
        if (total < 3)
            // preserve at least a 1 unit gap between possible hypernova positions and pillar voidzone
            hints.AddForbiddenZone(ShapeDistance.Rect(Arena.Center, default(Angle), 10, 10, 10));
    }

    public override void OnActorCreated(Actor actor)
    {
        base.OnActorCreated(actor);

        if ((OID)actor.OID == OID.VoidzoneHypernova)
            _numSpawned++;
    }
}

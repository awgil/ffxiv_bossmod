namespace BossMod.Stormblood.Ultimate.UWU;

class P3Geocrush1(BossModule module) : Components.StandardAOEs(module, AID.Geocrush1, new AOEShapeCircle(18));

// TODO: add prediction after PATE xxx - need non-interpolated actor rotation for that...
class P3Geocrush2(BossModule module) : Components.GenericAOEs(module, AID.Geocrush2)
{
    private WPos? _source;
    private WPos _bossSource;
    private AOEShapeDonut? _shapeReduced;
    private DateTime _playAction;

    //private static WDir[] _possibleOffsets = { new(14, 0), new(0, 14), new(-14, 0), new(0, -14) };
    private static readonly AOEShapeCircle _shapeCrush = new(24);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_shapeCrush, _source.Value, default, _playAction.AddSeconds(5.1f));
        if (_shapeReduced != null)
            yield return new(_shapeReduced, Module.Center);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _source = spell.LocXZ;
            _shapeReduced = new(NumCasts == 0 ? 16 : 12, Module.Bounds.Radius); // TODO: verify second radius
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _source = null;
            _bossSource = default;
            _playAction = default;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        // titan's non interpolated rotation isn't correct even at this point
        if ((OID)actor.OID == OID.Titan && id == 0x1E39)
        {
            _bossSource = actor.Position;
            _playAction = WorldState.CurrentTime;
        }
    }

    public override void Update()
    {
        if (_source == null && _playAction != default && WorldState.CurrentTime >= _playAction.AddSeconds(0.4f))
        {
            var dir = ((UWU)Module).Titan()!.ActualRotation;
            var toEdge = Intersect.RayCircle(_bossSource, dir.ToDirection(), Arena.Center, 14);
            _source = _bossSource + dir.ToDirection() * toEdge;
        }
    }
}

class P3GeocrushBury(BossModule module) : Components.GenericAOEs(module, AID.Bury)
{
    readonly List<AOEInstance> _predicted = [];

    public WPos? LastBoulder;

    // assuming proximity is true north
    static readonly WDir[] BombOffsets = [
        new(-5, +5),
        new(+5, +5),
        new(-5, +12),
        new(+5, +12),
        new(-11, +11),
        new(+11, +11)
    ];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
            if (_predicted.Count == 1)
            {
                ref var last = ref _predicted.Ref(0);
                LastBoulder = last.Origin;
                last.Activation = WorldState.FutureTime(8);
                last.Risky = false;
            }
        }

        if ((AID)spell.Action.ID == AID.Burst && _predicted.Count == 1)
            _predicted.Ref(0).Risky = true;

        if ((AID)spell.Action.ID == AID.Geocrush2)
        {
            var toN = (caster.Position - Arena.Center).ToAngle() - 180.Degrees();

            foreach (var off in BombOffsets)
                _predicted.Add(new(new AOEShapeCircle(4.3f), Arena.Center + off.Rotate(toN), default, WorldState.FutureTime(5.5f)));
        }
    }
}

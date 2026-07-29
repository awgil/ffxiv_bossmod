namespace BossMod.Stormblood.Ultimate.UCOB;

class P3GrandOctet(BossModule module) : Components.GenericAOEs(module)
{
    public List<Actor> Casters = [];
    private Actor? _nael;
    private Actor? _twin;
    private Actor? _baha;
    public List<AOEInstance> AOEs = [];
    private int _diveOrder; // 0 if not yet known, +1 if CCW, -1 if CW
    private WPos _initialSafespot;
    private readonly int[] _baitOrder = new int[PartyState.MaxPartySize];
    public int NumBaitsAssigned = 1; // reserve for lunar dive

    private static readonly AOEShapeRect _shapeNaelTwin = new(63.96f, 4f);
    private static readonly AOEShapeRect _shapeBahamut = new(64.2f, 6f);
    private static readonly AOEShapeRect _shapeDrake = new(52f, 10f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(AOEs);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_baitOrder[slot] >= NextBaitOrder)
            hints.Add($"Bait {_baitOrder[slot]}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_diveOrder != 0)
            hints.Add($"Move {(_diveOrder < 0 ? "CW" : "CCW")}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        // draw safespot
        if (NumCasts == 0 && AOEs.Count <= 1 && _initialSafespot != default)
            Arena.ZoneCircleOutline(_initialSafespot, 1f, Colors.Safe);

        // draw bait
        var order = _baitOrder[pcSlot];
        if (order >= NextBaitOrder && order <= Casters.Count)
        {
            var source = Casters[order - 1];
            Arena.Actor(source, Colors.Object, true);
            BaitShape(order).Outline(Arena, source.Position, Angle.FromDirection(pc.Position - source.Position));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.Firehorn or (uint)OID.Iceclaw or (uint)OID.Thunderwing or (uint)OID.TailOfDarkness or (uint)OID.FangOfLight)
        {
            Casters.Add(actor);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            AOEs.Add(new(shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell), actorID: caster.InstanceID));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            var count = Casters.Count;
            var id = caster.InstanceID;
            ++NumCasts;
            var aoes = CollectionsMarshal.AsSpan(AOEs);
            for (var i = 0; i < count; ++i)
            {
                ref var aoe = ref aoes[i];
                if (aoe.ActorID == id)
                {
                    AOEs.RemoveAt(i);
                    return;
                }
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
            return;

        switch (iconID)
        {
            case (uint)IconID.LunarDive: // this happens at the same time (so arbitrary order) as first cauterize
                _baitOrder[slot] = 1;
                break;
            case (uint)IconID.Cauterize:
                _baitOrder[slot] = ++NumBaitsAssigned;
                break;
            case (uint)IconID.MegaflareDive:
                _baitOrder[slot] = ++NumBaitsAssigned;
                if (NumBaitsAssigned == 7)
                {
                    var len = _baitOrder.Length;
                    for (var i = 0; i < len; ++i)
                    {
                        if (_baitOrder[i] == 0)
                        {
                            _baitOrder[i] = 8; // twintania bait
                        }
                    }
                }
                break;
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        var oid = actor.OID;
        if (oid == (uint)OID.NaelDeusDarnus && id == 0x1E43)
        {
            _nael = actor;
            InitIfReady();
        }
        else if (oid == (uint)OID.Twintania && id == 0x1E44)
        {
            _twin = actor;
            InitIfReady();
        }
        else if (oid == (uint)OID.BahamutPrime && id == 0x1E43)
        {
            _baha = actor;
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (_nael == null || _twin == null || _baha == null)
            return;

        // at this point NextCasters should contain 5 drakes, order is not yet known
        var center = Arena.Center;
        var dirToNael = Angle.FromDirection(_nael.Position - center);
        var dirToBaha = Angle.FromDirection(_baha.Position - center);

        // bahamut on cardinal => CCW dive order
        // bahamut on intercardinal => CW dive order
        var bahamutIntercardinal = ((int)MathF.Round(dirToBaha.Deg / 45f) & 1) != 0;
        _diveOrder = bahamutIntercardinal ? -1 : +1;

        var count = Casters.Count;
        var orders = new float[count];
        for (var i = 0; i < count; ++i)
        {
            var c = Casters[i];
            orders[i] = _diveOrder * CCWDirection(Angle.FromDirection(c.Position - center), dirToBaha);
        }

        MemoryExtensions.Sort(orders, Casters.AsSpan());
        Casters.Insert(0, _nael);
        Casters.Add(_baha);
        Casters.Add(_twin);

        // safespot is opposite of bahamut; if nael is there - adjusted 45 degrees
        var dirToSafespot = dirToBaha + 180f.Degrees();
        if (dirToSafespot.AlmostEqual(dirToNael, 0.1f))
            dirToSafespot += _diveOrder * 45f.Degrees();
        _initialSafespot = center + 20f * dirToSafespot.ToDirection();
    }

    private float CCWDirection(Angle direction, Angle reference)
    {
        var ccwDist = (direction - reference).Normalized().Deg;
        if (ccwDist < -5f)
            ccwDist += 360f;
        return ccwDist;
    }

    private int NextBaitOrder => AOEs.Count + NumCasts + 1;
    private static AOEShapeRect BaitShape(int order) => order switch
    {
        1 or 8 => _shapeNaelTwin,
        7 => _shapeBahamut,
        _ => _shapeDrake
    };

    private static AOEShapeRect? CastShape(ActionID aid) => aid.ID switch
    {
        (uint)AID.Cauterize1 => _shapeDrake,
        (uint)AID.Cauterize2 => _shapeDrake,
        (uint)AID.Cauterize3 => _shapeDrake,
        (uint)AID.Cauterize4 => _shapeDrake,
        (uint)AID.Cauterize5 => _shapeDrake,
        (uint)AID.LunarDive => _shapeNaelTwin,
        (uint)AID.TwistingDive => _shapeNaelTwin,
        (uint)AID.MegaflareDive => _shapeBahamut,
        _ => null
    };
}

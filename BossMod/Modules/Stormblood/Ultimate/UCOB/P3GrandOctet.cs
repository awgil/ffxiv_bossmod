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

    private static readonly AOEShapeRect _shapeNaelTwin = new(60, 4);
    private static readonly AOEShapeRect _shapeBahamut = new(60, 6);
    private static readonly AOEShapeRect _shapeDrake = new(52, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

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
            Arena.AddCircle(_initialSafespot, 1, ArenaColor.Safe);

        // draw bait
        var order = _baitOrder[pcSlot];
        if (order >= NextBaitOrder && order <= Casters.Count)
        {
            var source = Casters[order - 1];
            Arena.Actor(source, ArenaColor.Object, true);
            BaitShape(order).Outline(Arena, source.Position, Angle.FromDirection(pc.Position - source.Position));
        }
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.Firehorn or OID.Iceclaw or OID.Thunderwing or OID.TailOfDarkness or OID.FangOfLight)
            Casters.Add(actor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            AOEs.Add(new(shape, caster.Position, spell.Rotation, Module.CastFinishAt(spell)));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            ++NumCasts;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (!Raid.TryFindSlot(actor, out var slot))
            return;

        switch ((IconID)iconID)
        {
            case IconID.LunarDive: // this happens at the same time (so arbitrary order) as first cauterize
                _baitOrder[slot] = 1;
                break;
            case IconID.Cauterize:
                _baitOrder[slot] = ++NumBaitsAssigned;
                break;
            case IconID.MegaflareDive:
                _baitOrder[slot] = ++NumBaitsAssigned;
                if (NumBaitsAssigned == 7)
                    for (int i = 0; i < _baitOrder.Length; ++i)
                        if (_baitOrder[i] == 0)
                            _baitOrder[i] = 8; // twintania bait
                break;
        }
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

    private void InitIfReady()
    {
        if (_nael == null || _twin == null || _baha == null)
            return;

        // at this point NextCasters should contain 5 drakes, order is not yet known
        var dirToNael = Angle.FromDirection(_nael.Position - Module.Center);
        var dirToTwin = Angle.FromDirection(_twin.Position - Module.Center);
        var dirToBaha = Angle.FromDirection(_baha.Position - Module.Center);

        // bahamut on cardinal => CCW dive order
        // bahamut on intercardinal => CW dive order
        bool bahamutIntercardinal = ((int)MathF.Round(dirToBaha.Deg / 45) & 1) != 0;
        _diveOrder = bahamutIntercardinal ? -1 : +1;
        var orders = Casters.Select(c => _diveOrder * CCWDirection(Angle.FromDirection(c.Position - Module.Center), dirToBaha)).ToList();
        MemoryExtensions.Sort(orders.AsSpan(), Casters.AsSpan());
        Casters.Insert(0, _nael);
        Casters.Add(_baha);
        Casters.Add(_twin);

        // safespot is opposite of bahamut; if nael is there - adjusted 45 degrees
        var dirToSafespot = dirToBaha + 180.Degrees();
        if (dirToSafespot.AlmostEqual(dirToNael, 0.1f))
            dirToSafespot += _diveOrder * 45.Degrees();
        _initialSafespot = Module.Center + 20 * dirToSafespot.ToDirection();
    }

    private float CCWDirection(Angle direction, Angle reference)
    {
        var ccwDist = (direction - reference).Normalized().Deg;
        if (ccwDist < -5f)
            ccwDist += 360;
        return ccwDist;
    }

    private int NextBaitOrder => AOEs.Count + NumCasts + 1;
    private AOEShapeRect BaitShape(int order) => order switch
    {
        1 or 8 => _shapeNaelTwin,
        7 => _shapeBahamut,
        _ => _shapeDrake
    };

    private AOEShapeRect? CastShape(ActionID aid) => (AID)aid.ID switch
    {
        AID.Cauterize1 => _shapeDrake,
        AID.Cauterize2 => _shapeDrake,
        AID.Cauterize3 => _shapeDrake,
        AID.Cauterize4 => _shapeDrake,
        AID.Cauterize5 => _shapeDrake,
        AID.LunarDive => _shapeNaelTwin,
        AID.TwistingDive => _shapeNaelTwin,
        AID.MegaflareDive => _shapeBahamut,
        _ => null
    };
}

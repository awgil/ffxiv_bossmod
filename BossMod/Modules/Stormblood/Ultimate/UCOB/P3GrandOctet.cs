namespace BossMod.Stormblood.Ultimate.UCOB;

class P3GrandOctet : Components.GenericAOEs
{
    public List<Actor> Casters = new();
    private Actor? _nael;
    private Actor? _twin;
    private Actor? _baha;
    public List<AOEInstance> AOEs = new();
    private int _diveOrder; // 0 if not yet known, +1 if CCW, -1 if CW
    private WPos _initialSafespot;
    private int[] _baitOrder = new int[PartyState.MaxPartySize];
    public int NumBaitsAssigned = 1; // reserve for lunar dive

    private static readonly AOEShapeRect _shapeNaelTwin = new(60, 4);
    private static readonly AOEShapeRect _shapeBahamut = new(60, 6);
    private static readonly AOEShapeRect _shapeDrake = new(52, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => AOEs;

    public override void AddHints(BossModule module, int slot, Actor actor, TextHints hints, MovementHints? movementHints)
    {
        if (_baitOrder[slot] >= NextBaitOrder)
            hints.Add($"Bait {_baitOrder[slot]}", false);
        base.AddHints(module, slot, actor, hints, movementHints);
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (_diveOrder != 0)
            hints.Add($"Move {(_diveOrder < 0 ? "CW" : "CCW")}");
    }

    public override void DrawArenaForeground(BossModule module, int pcSlot, Actor pc, MiniArena arena)
    {
        // draw safespot
        if (NumCasts == 0 && AOEs.Count <= 1 && _initialSafespot != default)
            arena.AddCircle(_initialSafespot, 1, ArenaColor.Safe);

        // draw bait
        var order = _baitOrder[pcSlot];
        if (order >= NextBaitOrder && order <= Casters.Count)
        {
            var source = Casters[order - 1];
            arena.Actor(source, ArenaColor.Object, true);
            BaitShape(order).Outline(arena, source.Position, Angle.FromDirection(pc.Position - source.Position));
        }
    }

    public override void OnActorCreated(BossModule module, Actor actor)
    {
        if ((OID)actor.OID is OID.Firehorn or OID.Iceclaw or OID.Thunderwing or OID.TailOfDarkness or OID.FangOfLight)
            Casters.Add(actor);
    }

    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            AOEs.Add(new(shape, caster.Position, spell.Rotation, spell.NPCFinishAt));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var shape = CastShape(spell.Action);
        if (shape != null)
        {
            AOEs.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1));
            ++NumCasts;
        }
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var slot = module.Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
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

    public override void OnActorPlayActionTimelineEvent(BossModule module, Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.NaelDeusDarnus && id == 0x1E43)
        {
            _nael = actor;
            InitIfReady(module);
        }
        else if ((OID)actor.OID == OID.Twintania && id == 0x1E44)
        {
            _twin = actor;
            InitIfReady(module);
        }
        else if ((OID)actor.OID == OID.BahamutPrime && id == 0x1E43)
        {
            _baha = actor;
            InitIfReady(module);
        }
    }

    private void InitIfReady(BossModule module)
    {
        if (_nael == null || _twin == null || _baha == null)
            return;

        // at this point NextCasters should contain 5 drakes, order is not yet known
        var dirToNael = Angle.FromDirection(_nael.Position - module.Bounds.Center);
        var dirToTwin = Angle.FromDirection(_twin.Position - module.Bounds.Center);
        var dirToBaha = Angle.FromDirection(_baha.Position - module.Bounds.Center);

        // bahamut on cardinal => CCW dive order
        // bahamut on intercardinal => CW dive order
        bool bahamutIntercardinal = ((int)MathF.Round(dirToBaha.Deg / 45) & 1) != 0;
        _diveOrder = bahamutIntercardinal ? -1 : +1;
        var orders = Casters.Select(c => _diveOrder * CCWDirection(Angle.FromDirection(c.Position - module.Bounds.Center), dirToBaha)).ToList();
        MemoryExtensions.Sort(orders.AsSpan(), Casters.AsSpan());
        Casters.Insert(0, _nael);
        Casters.Add(_baha);
        Casters.Add(_twin);

        // safespot is opposite of bahamut; if nael is there - adjusted 45 degrees
        var dirToSafespot = dirToBaha + 180.Degrees();
        if (dirToSafespot.AlmostEqual(dirToNael, 0.1f))
            dirToSafespot += _diveOrder * 45.Degrees();
        _initialSafespot = module.Bounds.Center + 20 * dirToSafespot.ToDirection();
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

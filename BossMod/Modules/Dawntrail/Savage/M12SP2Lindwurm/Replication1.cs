namespace BossMod.Dawntrail.Savage.M12S2Lindwurm;

sealed class SnakingKick(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SnakingKick, new AOEShapeCone(40, 90.Degrees()));

sealed class WingedScourge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.WingedScourge, new AOEShapeCone(50, 15.Degrees()));

sealed class MightyMagicTopTierSlamFirstBait(BossModule module)
    : Components.UniformStackSpread(module, 5, 5, minStackSize: 2, maxStackSize: 2)
{
    record struct Caster(Actor Actor, bool Stack, DateTime Activation);

    readonly List<Caster> _casters = [];

    public int NumFire;
    public int NumDark;

    public override void Update()
    {
        Spreads.Clear();
        Stacks.Clear();

        var casters = CollectionsMarshal.AsSpan(_casters);
        var raid = Raid.WithoutSlot();
        var raidLen = raid.Length;

        for (var i = 0; i < casters.Length; ++i)
        {
            ref var entry = ref casters[i];
            var sourcePos = entry.Actor.Position.Quantized();
            var activation = entry.Activation;

            var needed = entry.Stack ? 1 : 2;

            Actor? first = null;
            Actor? second = null;
            var firstDist = float.MaxValue;
            var secondDist = float.MaxValue;

            for (var r = 0; r < raidLen; ++r)
            {
                var target = raid[r];
                var dist = (target.Position.Quantized() - sourcePos).LengthSq();

                if (dist < firstDist)
                {
                    second = first;
                    secondDist = firstDist;
                    first = target;
                    firstDist = dist;
                }
                else if (needed > 1 && dist < secondDist)
                {
                    second = target;
                    secondDist = dist;
                }
            }

            if (first != null)
            {
                if (entry.Stack)
                    AddStack(first, activation);
                else
                    AddSpread(first, activation);
            }

            if (!entry.Stack && second != null)
                AddSpread(second, activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamCast:
                _casters.Add(new(caster, true, Module.CastFinishAt(spell, 1)));
                break;

            case AID.MightyMagicCast:
                _casters.Add(new(caster, false, Module.CastFinishAt(spell, 1)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamStack:
                for (var i = _casters.Count - 1; i >= 0; --i)
                    if (_casters[i].Stack)
                        _casters.RemoveAt(i);
                ++NumFire;
                break;

            case AID.MightyMagicSpread:
                for (var i = _casters.Count - 1; i >= 0; --i)
                    if (!_casters[i].Stack)
                        _casters.RemoveAt(i);
                ++NumDark;
                break;
        }
    }
}

class Replication1SecondBait(BossModule module) : BossComponent(module)
{
    public enum Assignment
    {
        None,
        Fire,
        Dark
    }

    readonly Assignment[] _assignments = new Assignment[8];

    public record struct Clone(Actor Actor, Assignment Element);
    public readonly List<Clone> Clones = [];
    public record struct FinalClone(Actor Actor, WPos Position, Assignment Element);
    public readonly List<FinalClone> FinalClones = [];

    int _numFire;
    int _numDark;

    bool _highlightClone;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var assignment = _assignments[slot];
        if (assignment != Assignment.None)
            hints.Add($"Assignment: {assignment}", false);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (_numFire >= 1 && _numDark >= 2)
            return;

        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamCast:
            case AID.MightyMagicCast:
                {
                    if (Clones.Count == 0)
                        FinalClones.Clear();

                    var element = spell.Action.ID == (uint)AID.TopTierSlamCast
                        ? Assignment.Fire
                        : Assignment.Dark;

                    Clones.Add(new(caster, element));
                    break;
                }

            case AID.WingedScourgeCastVertical:
            case AID.WingedScourgeCastHorizontal:
                Clones.Add(new(caster, Assignment.None));
                break;
        }
    }
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TopTierSlamStack:
                ++_numFire;
                Assign();
                break;

            case AID.MightyMagicSpread:
                ++_numDark;
                Assign();
                break;

            case AID.Dash:
                {
                    _highlightClone = true;

                    var clones = CollectionsMarshal.AsSpan(Clones);

                    for (var i = 0; i < clones.Length; ++i)
                    {
                        ref var clone = ref clones[i];

                        if (clone.Actor == caster && clone.Element != Assignment.None)
                        {
                            // Record authoritative final position
                            FinalClones.Add(new(caster, spell.TargetXZ, clone.Element));
                            break;
                        }
                    }
                    break;
                }
        }
    }

    public override void OnActorPlayActionTimelineSync(Actor actor, List<(ulong InstanceID, ushort ID)> events)
    {
        var clones = CollectionsMarshal.AsSpan(Clones);

        for (var i = 0; i < clones.Length; ++i)
        {
            if (clones[i].Actor == actor)
            {
                var element = clones[i].Element;
                Clones.RemoveAt(i);

                var evs = CollectionsMarshal.AsSpan(events);
                for (var e = 0; e < evs.Length; ++e)
                {
                    ref var ev = ref evs[e];
                    if (ev.ID == 0x11D3)
                    {
                        var child = WorldState.Actors.Find(ev.InstanceID);
                        if (child != null)
                            Clones.Add(new(child, element));
                    }
                }
                return;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var clones = CollectionsMarshal.AsSpan(Clones);
        var playerAssignment = _assignments[pcSlot];

        for (var i = 0; i < clones.Length; ++i)
        {
            ref var clone = ref clones[i];
            var element = clone.Element;

            if (element == Assignment.None)
                continue;

            uint color = 0;

            if (playerAssignment != Assignment.None)
            {
                if (element == playerAssignment)
                    color = Colors.Object;
            }
            else
            {
                color = element == Assignment.Fire
                    ? Colors.Object
                    : Colors.Vulnerable;
            }

            if (color != 0)
            {
                Arena.ActorInsideBounds(clone.Actor.Position, clone.Actor.Rotation, color);

                if (_highlightClone)
                    Arena.ZoneCircleOutline(clone.Actor.Position, 1.25f, Colors.Danger);
            }
        }
    }

    void Assign()
    {
        if (_numFire != 1 || _numDark != 2)
            return;

        var party = Raid.WithSlot();
        var len = party.Length;

        for (var i = 0; i < len; ++i)
        {
            ref var entry = ref party[i];
            var slot = entry.Item1;
            var actor = entry.Item2;

            var hasDarkRes = actor.FindStatus(SID.DarkResistanceDownII, DateTime.MinValue) != null;
            _assignments[slot] = hasDarkRes ? Assignment.Fire : Assignment.Dark;
        }
    }
}

class WingedScourgeSecond(BossModule module) : Components.GenericAOEs(module)
{
    static readonly AOEShapeCone _shape = new(50, 15.Degrees());

    Angle _orientation;
    bool _recorded;
    bool _draw;

    readonly List<Actor> _clones = [];
    readonly List<(WPos Source, DateTime Activation)> _sources = [];
    readonly List<AOEInstance> _aoes = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_draw || _sources.Count == 0)
            return [];

        _aoes.Clear();

        var sources = CollectionsMarshal.AsSpan(_sources);
        var count = sources.Length;

        for (var i = 0; i < count; ++i)
        {
            ref var s = ref sources[i];

            _aoes.Add(new AOEInstance(_shape, s.Source, _orientation, s.Activation));
            _aoes.Add(new AOEInstance(_shape, s.Source, _orientation + 180.Degrees(), s.Activation));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var aid = (AID)spell.Action.ID;

        if (!_recorded && (aid == AID.WingedScourgeCastVertical || aid == AID.WingedScourgeCastHorizontal))
        {
            _clones.Add(caster);
            _orientation = aid == AID.WingedScourgeCastVertical
                ? default
                : 90.Degrees();
        }
    }

    public override void OnActorPlayActionTimelineSync(Actor actor, List<(ulong InstanceID, ushort ID)> events)
    {
        var clones = CollectionsMarshal.AsSpan(_clones);

        for (var i = 0; i < clones.Length; ++i)
        {
            if (clones[i] == actor)
            {
                _clones.RemoveAt(i);
                _recorded = true;

                var evs = CollectionsMarshal.AsSpan(events);
                for (var e = 0; e < evs.Length; ++e)
                {
                    ref var ev = ref evs[e];
                    if (ev.ID == 0x11D3)
                    {
                        var clone = WorldState.Actors.Find(ev.InstanceID);
                        if (clone != null)
                            _clones.Add(clone);
                    }
                }
                return;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID != AID.Dash)
            return;

        var clones = CollectionsMarshal.AsSpan(_clones);

        for (var i = 0; i < clones.Length; ++i)
        {
            if (clones[i] == caster)
            {
                _draw = true;
                _sources.Add((spell.TargetXZ, WorldState.FutureTime(7.2f)));
                return;
            }
        }
    }
}
class MightyMagicTopTierSlamSecondBait(BossModule module)
    : Components.UniformStackSpread(module, 5, 5, minStackSize: 2, maxStackSize: 2)
{
    readonly Replication1SecondBait _rep1Bait = module.FindComponent<Replication1SecondBait>()!;

    record struct Source(WPos Position, bool Stack, DateTime Activation);

    readonly List<Source> _sources = [];

    public int NumFire;
    public int NumDark;

    BitMask _fireVuln;
    BitMask _darkVuln;

    public override void Update()
    {
        Spreads.Clear();
        Stacks.Clear();

        var sources = CollectionsMarshal.AsSpan(_sources);
        var raid = Raid.WithoutSlot();
        var raidLen = raid.Length;

        for (var i = 0; i < sources.Length; ++i)
        {
            ref var s = ref sources[i];

            var origin = s.Position.Quantized();
            var activation = s.Activation;
            var needed = s.Stack ? 1 : 2;

            Actor? first = null;
            Actor? second = null;
            var firstDist = float.MaxValue;
            var secondDist = float.MaxValue;

            for (var r = 0; r < raidLen; ++r)
            {
                var target = raid[r];
                var dist = (target.Position.Quantized() - origin).LengthSq();

                if (dist < firstDist)
                {
                    second = first;
                    secondDist = firstDist;
                    first = target;
                    firstDist = dist;
                }
                else if (needed > 1 && dist < secondDist)
                {
                    second = target;
                    secondDist = dist;
                }
            }

            if (first != null)
            {
                if (s.Stack)
                    AddStack(first, activation, _fireVuln);
                else
                    AddSpread(first, activation);
            }

            if (!s.Stack && second != null)
                AddSpread(second, activation);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_fireVuln[slot] && IsStackTarget(actor))
            hints.Add("Avoid baiting fire!");

        if (_darkVuln[slot] && IsSpreadTarget(actor))
            hints.Add("Avoid baiting dark!");
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
            return;

        switch ((SID)status.ID)
        {
            case SID.FireResistanceDownII:
                _fireVuln.Set(slot);
                break;

            case SID.DarkResistanceDownII:
                _darkVuln.Set(slot);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Dash:
                {
                    var clones = CollectionsMarshal.AsSpan(_rep1Bait.Clones);

                    for (var i = 0; i < clones.Length; ++i)
                    {
                        ref var clone = ref clones[i];
                        if (clone.Actor == caster && clone.Element != Replication1SecondBait.Assignment.None)
                        {
                            var isStack = clone.Element == Replication1SecondBait.Assignment.Fire;
                            _sources.Add(new(spell.TargetXZ, isStack, WorldState.FutureTime(10))); // FIXME timing
                            break;
                        }
                    }
                    break;
                }

            case AID.TopTierSlamStack:
                ++NumFire;
                for (var i = _sources.Count - 1; i >= 0; --i)
                    if (_sources[i].Stack)
                        _sources.RemoveAt(i);
                break;

            case AID.MightyMagicSpread:
                ++NumDark;
                for (var i = _sources.Count - 1; i >= 0; --i)
                    if (!_sources[i].Stack)
                        _sources.RemoveAt(i);
                break;
        }
    }
}

sealed class DoubleSobatBuster(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(40, 90.Degrees()), (uint)IconID.DoubleSobat)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DoubleSobatBuster1:
            case AID.DoubleSobatBuster2:
            case AID.DoubleSobatBuster3:
            case AID.DoubleSobatBuster4:
                ++NumCasts;
                CurrentBaits.Clear();
                break;
        }
    }
}

sealed class DoubleSobatRepeat(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DoubleSobatRepeat, new AOEShapeCone(40, 90.Degrees()));

sealed class EsotericFinisher : Components.GenericBaitAway
{
    static readonly AOEShapeCircle _shape = new(10);

    public EsotericFinisher(BossModule module)
        : base(module, (uint)AID.EsotericFinisher, centerAtTarget: true)
    {
        var party = module.Raid.WithoutSlot();
        var len = party.Length;

        Actor? first = null;
        Actor? second = null;

        // First pass: pick tanks
        for (var i = 0; i < len; ++i)
        {
            var p = party[i];
            if (p.Role == Role.Tank)
            {
                if (first == null)
                    first = p;
                else
                {
                    second = p;
                    break;
                }
            }
        }

        if (second == null)
        {
            for (var i = 0; i < len; ++i)
            {
                var p = party[i];

                if (p == first)
                    continue;

                if (first == null)
                {
                    first = p;
                }
                else
                {
                    second = p;
                    break;
                }
            }
        }

        var activation = WorldState.FutureTime(10);

        if (first != null)
            CurrentBaits.Add(new(module.PrimaryActor, first, _shape, activation));

        if (second != null)
            CurrentBaits.Add(new(module.PrimaryActor, second, _shape, activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.EsotericFinisher)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

sealed class Replication1Guidance : BossComponent
{
    const float InitialRadius = 7.5f;
    const float PairOffset = 2.25f;
    const float FireCenterOffset = 1.5f;
    const float NearDarkOffset = 1.0f;
    // Spread-marker radius (matches UniformStackSpread(module, 5, 5) used by the bait components).
    const float StaticSpreadRadius = 5f;

    const float StaticWaymarkRadius = 13.65f;

    readonly Replication1SecondBait _bait;
    readonly PartyRolesConfig _prc = Service.Config.Get<PartyRolesConfig>();
    readonly PartyRolesConfig.Assignment[] _assignments;
    readonly M12S2LindwurmConfig.Replication1Effective _rep1;

    // Cached on first non-null hit so we don't pay FindComponent's type-lookup every frame. The two components are
    // activated AFTER this one, so the lookup has to be lazy rather than done in the constructor.
    WingedScourge? _wingedScourge;
    WingedScourgeSecond? _wingedScourgeSecond;

    WPos Center => Module.Center;

    // ------------------------------------------------------------
    // FIXED CARDINAL MARKERS
    // ------------------------------------------------------------

    static readonly WPos[] Outer =
    [
        new(100, 86),  // 0 N
        new(114, 100), // 1 E
        new(100, 114), // 2 S
        new(86, 100)   // 3 W
    ];

    static readonly WPos[] Inner =
    [
        new(100, 92),
        new(108, 100),
        new(100, 108),
        new(92, 100)
    ];

    // DN selection orders (no stackalloc)
    static readonly int[] OrderNESW = [0, 1, 2, 3];
    static readonly int[] OrderWSEN = [3, 2, 1, 0];

    // Static strategy offsets in the new-north frame (X = rel-East, Z = rel-North).
    static readonly WDir StaticH2R2Fire = StaticWaymarkRadius * new WDir(1f, 1f).Normalized(); // rel-NE flank (right) = A@4
    static readonly WDir StaticH1R1Fire = StaticWaymarkRadius * new WDir(-1f, 1f).Normalized(); // rel-NW flank (left)  = D@4
    static readonly WDir StaticRangedDark = StaticWaymarkRadius * new WDir(-1f, -1f).Normalized(); // rel-SW behind-left  = C@4
    static readonly WDir StaticMeleeFire = new(0f, 1.5f); // baits fire: stack on the center box's north line

    public Replication1Guidance(BossModule module) : base(module)
    {
        _bait = module.FindComponent<Replication1SecondBait>()!;
        _assignments = _prc.AssignmentsPerSlot(Module.WorldState.Party);
        _rep1 = Service.Config.Get<M12S2LindwurmConfig>().GetReplication1();
    }

    // ------------------------------------------------------------
    // STATUS → BAIT TYPE
    // ------------------------------------------------------------

    Replication1SecondBait.Assignment BaitType(Actor actor)
    {
        if (actor.FindStatus(SID.DarkResistanceDownII, DateTime.MinValue) != null)
            return Replication1SecondBait.Assignment.Fire;

        if (actor.FindStatus(SID.FireResistanceDownII, DateTime.MinValue) != null)
            return Replication1SecondBait.Assignment.Dark;

        return Replication1SecondBait.Assignment.Dark;
    }

    // ------------------------------------------------------------
    // INITIAL POSITIONS
    // ------------------------------------------------------------

    WPos InitialPosition(PartyRolesConfig.Assignment a)
    {
        if (_rep1.IsStatic)
            return StaticInitialPosition(a);
        else
        {
            return CloneRelativeOrDNInitialPosition(a);
        }
    }

    WPos CloneRelativeOrDNInitialPosition(PartyRolesConfig.Assignment a)
    {
        var quadrant = a switch
        {
            PartyRolesConfig.Assignment.H1 or PartyRolesConfig.Assignment.R1 => 0,
            PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.MT => 1,
            PartyRolesConfig.Assignment.H2 or PartyRolesConfig.Assignment.R2 => 2,
            PartyRolesConfig.Assignment.OT or PartyRolesConfig.Assignment.M2 => 3,
            _ => 0
        };

        var angle = quadrant switch
        {
            0 => -135.Degrees(),
            1 => 135.Degrees(),
            2 => 45.Degrees(),
            3 => -45.Degrees(),
            _ => default
        };

        var center = Center;
        var anchor = center + InitialRadius * angle.ToDirection();

        var dir = (anchor - center).Normalized();
        var perp = new WDir(-dir.Z, dir.X);

        float sign = a switch
        {
            PartyRolesConfig.Assignment.H1 or
            PartyRolesConfig.Assignment.M1 or
            PartyRolesConfig.Assignment.OT or
            PartyRolesConfig.Assignment.H2 => +1,
            _ => -1
        };

        return anchor + sign * PairOffset * perp;
    }

    // Static preposition, expressed with angles like CloneRelativeOrDNInitialPosition.
    // Fire stacks group up:  Melee M1/M2 share an SE point, Tanks MT/OT share an NW point (single shared point each).
    // Dark spreads fan out:  Ranged R1/R2 around NE, Healers H1/H2 around SW, pushed out to the edge of
    //                        their spread marker (InitialRadius + StaticSpreadRadius) and split by PairOffset.
    WPos StaticInitialPosition(PartyRolesConfig.Assignment assignment)
    {
        var edgeRadius = InitialRadius + StaticSpreadRadius; // 12.5

        var (angleDeg, radius, splitSign) = assignment switch
        {
            PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2 => (45f, InitialRadius, 0f),    // SE stack
            PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT => (-135f, InitialRadius, 0f),  // NW stack
            PartyRolesConfig.Assignment.R1 => (135f, edgeRadius, +1f),                                       // NE
            PartyRolesConfig.Assignment.R2 => (135f, edgeRadius, -1f),
            PartyRolesConfig.Assignment.H1 => (-45f, edgeRadius, -1f),                                       // SW
            PartyRolesConfig.Assignment.H2 => (-45f, edgeRadius, +1f),
            _ => (0f, InitialRadius, 0f)
        };

        var dir = angleDeg.Degrees().ToDirection();
        var anchor = Center + radius * dir;
        if (splitSign == 0f)
            return anchor;

        var perp = new WDir(-dir.Z, dir.X);
        return anchor + splitSign * PairOffset * perp;
    }

    // ------------------------------------------------------------
    // FORMATION FROM FINAL CLONES
    // ------------------------------------------------------------

    bool TryGetFormation(
        out WPos nearFire, out WPos farFire,
        out WPos nearDark, out WPos farDark)
    {
        nearFire = farFire = nearDark = farDark = default;

        var list = _bait.FinalClones;
        if (list.Count != 4)
            return false;

        var span = CollectionsMarshal.AsSpan(list);
        var center = Center;

        WPos f0 = default, f1 = default;
        WPos d0 = default, d1 = default;
        int nf = 0, nd = 0;

        for (var i = 0; i < span.Length; ++i)
        {
            ref readonly var c = ref span[i];

            if (c.Element == Replication1SecondBait.Assignment.Fire)
            {
                if (nf == 0)
                    f0 = c.Position;
                else
                    f1 = c.Position;
                ++nf;
            }
            else if (c.Element == Replication1SecondBait.Assignment.Dark)
            {
                if (nd == 0)
                    d0 = c.Position;
                else
                    d1 = c.Position;
                ++nd;
            }
        }

        if (nf != 2 || nd != 2)
            return false;

        nearFire = (f0 - center).LengthSq() < (f1 - center).LengthSq() ? f0 : f1;
        farFire = nearFire.Equals(f0) ? f1 : f0;

        nearDark = (d0 - center).LengthSq() < (d1 - center).LengthSq() ? d0 : d1;
        farDark = nearDark.Equals(d0) ? d1 : d0;

        return true;
    }

    // ------------------------------------------------------------
    // INNER DARK OFFSET
    // ------------------------------------------------------------

    WPos CornerOffset(int idx, WPos marker, WPos clone)
    {
        var x = marker.X;
        var z = marker.Z;
        var center = Center;

        switch (idx)
        {
            case 0:
            case 2:
                z += (center.Z - marker.Z > 0 ? NearDarkOffset : -NearDarkOffset);
                x += (marker.X - clone.X > 0 ? NearDarkOffset : -NearDarkOffset);
                break;

            case 1:
            case 3:
                x += center.X - marker.X > 0 ? NearDarkOffset : -NearDarkOffset;
                z += marker.Z - clone.Z > 0 ? NearDarkOffset : -NearDarkOffset;
                break;
        }

        return new WPos(x, z);
    }

    // ------------------------------------------------------------
    // QUADRANT → FLANK CARDINALS
    // ------------------------------------------------------------

    void FlankingCardinals(WPos pos, out int cw, out int ccw)
    {
        var v = pos - Center;

        if (v.X >= 0)
        {
            if (v.Z < 0)
            {
                cw = 1;
                ccw = 0;
            } // NE → E, N
            else
            {
                cw = 2;
                ccw = 1;
            } // SE → S, E
        }
        else
        {
            if (v.Z >= 0)
            {
                cw = 3;
                ccw = 2;
            } // SW → W, S
            else
            {
                cw = 0;
                ccw = 3;
            } // NW → N, W
        }
    }

    static int SelectByOrder(int a, int b, int[] order)
    {
        for (var i = 0; i < order.Length; ++i)
        {
            var v = order[i];
            if (v == a || v == b)
                return v;
        }
        return a;
    }

    // ------------------------------------------------------------
    // FINAL POSITION
    // ------------------------------------------------------------

    WPos? FinalPosition(int slot, Actor actor)
    {
        if (!TryGetFormation(out var nearFire, out var farFire,
                             out var nearDark, out var farDark))
            return null;

        return _rep1.IsStatic ? StaticFinalPosition(slot, actor, farDark, nearDark)
                              : CloneRelativeOrDNFinalPosition(slot, actor, nearFire, farFire, nearDark, farDark);
    }

    WPos? CloneRelativeOrDNFinalPosition(int slot, Actor actor, WPos nearFire, WPos farFire, WPos nearDark, WPos farDark)
    {
        var assign = _assignments[slot];
        var mech = BaitType(actor);

        var inner =
            assign is PartyRolesConfig.Assignment.MT
                   or PartyRolesConfig.Assignment.OT
                   or PartyRolesConfig.Assignment.M1
                   or PartyRolesConfig.Assignment.M2;

        var support =
            assign is PartyRolesConfig.Assignment.MT
                   or PartyRolesConfig.Assignment.OT
                   or PartyRolesConfig.Assignment.H1
                   or PartyRolesConfig.Assignment.H2;

        var center = Center;

        // ================= FIRE =================

        if (mech == Replication1SecondBait.Assignment.Fire)
        {
            if (inner)
            {
                var dir = (nearFire - center).Normalized();
                return center + FireCenterOffset * dir;
            }

            // OUTER FIRE — candidates ALWAYS from farFire

            FlankingCardinals(farFire, out var cwFire, out var ccwFire);

            int target;

            if (_rep1.IsDN)
            {
                // DN: pick first valid in N→E→S→W
                target = SelectByOrder(cwFire, ccwFire, OrderNESW);
            }
            else
            {
                // Clone Relative: always CCW from clone
                target = ccwFire;
            }

            return Outer[target];
        }

        // ================= DARK =================

        var clonePos = inner ? nearDark : farDark;

        FlankingCardinals(clonePos, out var cwDark, out var ccwDark);

        int targetDark;

        if (_rep1.IsDN)
        {
            targetDark = support
                ? SelectByOrder(cwDark, ccwDark, OrderNESW)
                : SelectByOrder(cwDark, ccwDark, OrderWSEN);
        }
        else
        {
            // Clone Relative (Static handled earlier)
            targetDark = support ? ccwDark : cwDark;
        }

        if (inner)
        {
            var marker = Inner[targetDark];
            return CornerOffset(targetDark, marker, clonePos);
        }

        return Outer[targetDark];
    }

    // ------------------------------------------------------------
    // STATIC STRATEGY
    // ------------------------------------------------------------
    //
    // The dark clone closest to the wall (farDark) defines a "new North".
    // All 8 safe spots are expressed as offsets in that rotated local frame,
    // where +Z = relative-North (toward farDark) and +X = relative-East (90° clockwise).
    //
    // Ranged/healer:
    //   - Dark (H1/R1 & H2/R2)  → behind-left cardinal waymark (rel-SW). new north @4 → C, @1 → D, ...
    //   - Fire H1/R1            → left  flank cardinal waymark (rel-NW). new north @4 → D
    //   - Fire H2/R2            → right flank cardinal waymark (rel-NE). new north @4 → A
    //
    // Melee/tank:
    //   - Fire debuff (baits dark) → on the boss's edge in melee range, split MT/M1 left (rel-West),
    //                                OT/M2 right (rel-East), away from the stacked fire-baiters.
    //   - Dark debuff (baits fire) → stack both together on the center box's north line.

    bool BuildNewNorthBasis(WPos farDark, out WDir north, out WDir east)
    {
        var v = farDark - Center;
        var lenSq = v.LengthSq();
        if (lenSq < 0.0001f)
        {
            north = default;
            east = default;
            return false;
        }
        north = v / MathF.Sqrt(lenSq);
        east = new WDir(-north.Z, north.X); // 90° clockwise
        return true;
    }

    static bool IsMeleeOrTank(PartyRolesConfig.Assignment assignment)
        => assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.OT
                      or PartyRolesConfig.Assignment.M1 or PartyRolesConfig.Assignment.M2;

    WPos? StaticFinalPosition(int slot, Actor actor, WPos farDark, WPos nearDark)
    {
        if (!BuildNewNorthBasis(farDark, out var north, out var east))
            return null;

        var assignment = _assignments[slot];
        var baitsDark = BaitType(actor) == Replication1SecondBait.Assignment.Dark;

        // Bait-dark melee/tank hug the non-new-north dark clone, just off it on their side, in the cone's safe gap.
        if (baitsDark && IsMeleeOrTank(assignment))
        {
            var side = assignment is PartyRolesConfig.Assignment.MT or PartyRolesConfig.Assignment.M1 ? -1f : 1f;
            return MeleeBaitDarkSpot(nearDark, farDark, side, slot, actor);
        }

        var localOffset = (assignment, baitsDark) switch
        {
            // Dark baits — ranged/healers share the behind-left cardinal waymark
            (PartyRolesConfig.Assignment.H1, false) or (PartyRolesConfig.Assignment.R1, false) or
            (PartyRolesConfig.Assignment.H2, false) or (PartyRolesConfig.Assignment.R2, false) => StaticRangedDark,

            // Fire ranged/healer flanks
            (PartyRolesConfig.Assignment.H1, true) or (PartyRolesConfig.Assignment.R1, true) => StaticH1R1Fire,
            (PartyRolesConfig.Assignment.H2, true) or (PartyRolesConfig.Assignment.R2, true) => StaticH2R2Fire,

            // Melee/tank baiting fire (dark debuff): stack both together on the center box's north line
            (PartyRolesConfig.Assignment.MT, false) or (PartyRolesConfig.Assignment.M1, false) or
            (PartyRolesConfig.Assignment.OT, false) or (PartyRolesConfig.Assignment.M2, false) => StaticMeleeFire,

            _ => default
        };

        // Ranged/healers stand EXACTLY on their cardinal waymark (A/C/D rotated to new north) — never nudged
        // off it for cones: the player body doesn't trigger the cone, only their AoE circle would.
        return Center + localOffset.X * east + localOffset.Z * north;
    }

    // Edge melee bait-dark spot: stand at nearDark, facing new north (farDark) as 0°, then head out 45° to the
    // side — left (CCW) for MT/M1 (side -1), right (CW) for OT/M2 (side +1). If that point is inside a cone,
    // move further out and rotate a bit further outward until it lands in the cone's safe gap.
    WPos MeleeBaitDarkSpot(WPos nearDark, WPos farDark, float side, int slot, Actor actor)
    {
        var forwardAngle = (farDark - nearDark).ToAngle(); // 0° = facing new north

        const float coneCushion = 0.5f; // only the player point must be outside the cone; the bait circle may clip it
        const float minRadius = 5f;     // hug the dark clone — only move out if the close spot is in a cone
        const float maxRadius = 12f;
        const float rStep = 0.5f;
        const float angStep = 5f;
        const int angleStepCount = 11;  // angles 0°, 5°, ..., 50° beyond the base 45°

        // Precompute one direction per outward-rotation step — these depend on 'extra' only, not on 'r',
        // so we hoist the sin/cos work out of the radius loop.
        Span<WDir> directions = stackalloc WDir[angleStepCount];
        for (var k = 0; k < angleStepCount; ++k)
            directions[k] = (forwardAngle + (-side * (45f + k * angStep)).Degrees()).ToDirection();

        var coneFields = ActiveConeFields(slot, actor);
        // No active cones - the base spot is always safe; skip the search entirely.
        if (coneFields.Length == 0)
            return nearDark + minRadius * directions[0];

        for (var r = minRadius; r <= maxRadius; r += rStep)
            for (var k = 0; k < angleStepCount; ++k)
            {
                var candidate = nearDark + r * directions[k];
                if (IsClearOf(coneFields, candidate, coneCushion))
                    return candidate;
            }

        return nearDark + minRadius * directions[0]; // fallback: base 45° ray at the closest radius
    }

    // Signed-distance field per active cone, gathered from both WingedScourge components. Negative = inside.
    // Both components are activated after this one, so the lookups are lazy — '??=' keeps probing until each
    // component exists, then sticks with the cached reference for the rest of the mechanic.
    ShapeDistance[] ActiveConeFields(int slot, Actor actor)
    {
        _wingedScourge ??= Module.FindComponent<WingedScourge>();
        _wingedScourgeSecond ??= Module.FindComponent<WingedScourgeSecond>();

        var primaryCones = _wingedScourge is { } primary ? primary.ActiveAOEs(slot, actor) : default;
        var secondaryCones = _wingedScourgeSecond is { } secondary ? secondary.ActiveAOEs(slot, actor) : default;
        var total = primaryCones.Length + secondaryCones.Length;
        if (total == 0)
            return [];

        var fields = new ShapeDistance[total];
        var next = 0;
        foreach (ref readonly var cone in primaryCones)
            fields[next++] = cone.Shape.Distance(cone.Origin, cone.Rotation);
        foreach (ref readonly var cone in secondaryCones)
            fields[next++] = cone.Shape.Distance(cone.Origin, cone.Rotation);
        return fields;
    }

    // True when 'point' stays at least 'cushion' units outside every shape in 'fields' (cones and/or circles),
    // i.e. it's clear of all those hazards with that safety margin. False if it's inside, or within the
    // cushion of, any of them.
    static bool IsClearOf(ShapeDistance[] fields, WPos point, float cushion)
    {
        for (var i = 0; i < fields.Length; ++i)
            if (fields[i].Distance(point) < cushion)
                return false;
        return true;
    }

    // ------------------------------------------------------------
    // DRAW
    // ------------------------------------------------------------

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_assignments.Length != 8)
            return;

        // Mark the wall-dark clone as "new North" for Static strategy.
        if (_rep1.IsStatic && TryGetFormation(out _, out _, out _, out var farDark))
            Arena.ZoneCircleOutline(farDark, 1.25f, Colors.Other1);

        var pos = DebuffsAssigned()
            ? FinalPosition(pcSlot, pc)
            : InitialPosition(_assignments[pcSlot]);

        if (pos == null)
            return;

        Arena.ZoneCircleOutline(pos.Value, 1.1f, Colors.Safe);
        Arena.AddLine(pc.Position, pos.Value, Colors.Safe);
    }

    // ------------------------------------------------------------
    // HINTS
    // ------------------------------------------------------------

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!DebuffsAssigned())
            return;

        var mech = BaitType(actor);
        hints.Add($"Bait {mech}", false);
    }

    bool DebuffsAssigned()
    {
        var raid = Raid.WithoutSlot();
        for (var i = 0; i < raid.Length; ++i)
        {
            var p = raid[i];
            if (p.FindStatus(SID.FireResistanceDownII, DateTime.MinValue) != null)
                return true;
            if (p.FindStatus(SID.DarkResistanceDownII, DateTime.MinValue) != null)
                return true;
        }
        return false;
    }
}

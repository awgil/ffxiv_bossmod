namespace BossMod.Dawntrail.Ultimate.FRU;

class P3UltimateRelativity(BossModule module) : Components.CastCounter(module, default)
{
    public struct PlayerState
    {
        public int FireOrder;
        public int RewindOrder;
        public int LaserOrder;
        public bool HaveDarkEruption; // all dark eruptions have rewind order 1
        public bool HaveDarkBlizzard;
        public WDir AssignedDir;
        public WPos ReturnPos;
    }

    public readonly PlayerState[] States = new PlayerState[PartyState.MaxPartySize];
    public readonly List<(Actor origin, Angle rotation, DateTime activation)> LaserRotations = [];
    public int NumReturnStuns;
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private WDir _relNorth;
    private int _numYellowTethers;
    private DateTime _nextImminent = module.WorldState.FutureTime(21.9f - 2.5f); // approx 2.5s before next step resolves

    public const float RangeHintOut = 12; // explosion radius is 8
    public const float RangeHintStack = 1;
    public const float RangeHintLaser = 9.5f; // hourglass location
    public const float RangeHintDarkEruption = 9; // radius is 6, especially for fire-order 2 has to be < 9.5, otherwise will be clipped by own laser
    public const float RangeHintDarkWater = 1;
    public const float RangeHintEye = 2;
    public const float RangeHintChill = -1; // simplifies looking outside and hitting boss

    public Angle LaserRotationAt(WPos pos) => LaserRotations.FirstOrDefault(r => r.origin.Position.AlmostEqual(pos, 1)).rotation;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (States[slot].RewindOrder == 0)
            return;

        var isSupport = actor.Class.IsSupport();
        var hint = string.Join(" > ", Enumerable.Range(NumCasts, 7 - NumCasts).Select(i => Hint(States[slot], isSupport, i)));
        hints.Add(hint, false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (States[slot].AssignedDir != default)
        {
            var range = RangeHint(States[slot], actor.Class.IsSupport(), NumCasts);
            switch (range)
            {
                case RangeHintOut:
                    if (WorldState.CurrentTime < _nextImminent)
                    {
                        // there's still time, around maxmelee at assigned direction
                        hints.AddForbiddenZone(ShapeContains.InvertedCircle(SafeSpot(slot, 9), 1), _nextImminent);
                    }
                    else
                    {
                        // ok, out is imminent, gtfo - we need to avoid clipping people, avoid dark blizzard (if it's being resolved now), and avoid lasers (if any)
                        var avoidBlizzard = NumCasts == 2;
                        foreach (var (i, p) in Raid.WithSlot().Exclude(slot))
                        {
                            var avoidRadius = avoidBlizzard && States[i].HaveDarkBlizzard ? 12 : 8;
                            hints.AddForbiddenZone(ShapeContains.Circle(p.Position, avoidRadius));
                        }
                        var lasers = Module.FindComponent<P3UltimateRelativitySinboundMeltdownAOE>();
                        if (lasers != null)
                            foreach (var laser in lasers.ActiveAOEs(slot, actor))
                                hints.AddForbiddenZone(laser.Shape.CheckFn(laser.Origin, laser.Rotation), laser.Activation);
                    }
                    break;
                case RangeHintLaser:
                case RangeHintDarkEruption:
                case RangeHintEye:
                    // go to exact safespot
                    hints.AddForbiddenZone(ShapeContains.PrecisePosition(SafeSpot(slot, range), new(0, 1), Module.Bounds.MapResolution, actor.Position, 0.1f), _nextImminent);
                    break;
                default:
                    // go to mid, staying as tightly as possible to allow for better uptime; after all mechanics, go opposite to dodge eyes more naturally
                    var dest = NumCasts >= 6 ? SafeSpot(slot, range) : Module.Center;
                    hints.AddForbiddenZone(ShapeContains.PrecisePosition(dest, new(0, 1), Module.Bounds.MapResolution, actor.Position, 0.1f), _nextImminent);
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var assignedDir = States[pcSlot].AssignedDir;
        if (assignedDir != default && NumCasts < 6)
        {
            Arena.AddLine(Module.Center, Module.Center + Module.Bounds.Radius * assignedDir, ArenaColor.Safe);
            Arena.AddCircle(SafeSpot(pcSlot, RangeHint(States[pcSlot], pc.Class.IsSupport(), NumCasts)), 1, ArenaColor.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SpellInWaitingDarkFire:
                if (Raid.TryFindSlot(actor, out var slot))
                {
                    ref var state = ref States[slot];
                    state.FireOrder = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
                    {
                        < 15 => 1,
                        < 25 => 2,
                        _ => 3
                    };
                    state.LaserOrder = state.FireOrder switch
                    {
                        3 => 1,
                        1 => 2,
                        _ => 3,
                    };
                }
                break;
            case SID.SpellInWaitingDarkBlizzard:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    States[slot].LaserOrder = actor.Class.IsSupport() ? 2 : 1;
                    States[slot].HaveDarkBlizzard = true;
                }
                break;
            case SID.SpellInWaitingDarkEruption:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].HaveDarkEruption = true;
                break;
            case SID.SpellInWaitingReturn:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].RewindOrder = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 20 ? 1 : 2;
                break;
            case SID.DelightsHourglassRotation:
                var rot = status.Extra switch
                {
                    0x10D => 15.Degrees(),
                    0x15C => -15.Degrees(),
                    _ => default
                };
                if (rot != default)
                    LaserRotations.Add((actor, rot, status.ExpireAt));
                else
                    ReportError($"Unexpected rotation status param: {status.Extra:X}");
                break;
            case SID.Return:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].ReturnPos = actor.Position;
                break;
            case SID.Stun:
                ++NumReturnStuns;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.Stun:
                --NumReturnStuns;
                break;
        }
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((OID)source.OID == OID.DelightsHourglass && tether.ID == (uint)TetherID.UltimateRelativityQuicken)
        {
            _relNorth += source.Position - Module.Center;
            if (++_numYellowTethers == 3)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.UltimateRelativityUnholyDarkness or AID.UltimateRelativitySinboundMeltdownAOEFirst && WorldState.CurrentTime > _nextImminent)
        {
            ++NumCasts;
            _nextImminent = WorldState.FutureTime(2.5f);
        }
    }

    private void InitAssignments()
    {
        // determine who flexes
        Span<(int slot, int prio)> eastFlex = [(-1, -1), (-1, -1)]; // [support, dd]
        foreach (var (slot, group) in _config.P3UltimateRelativityAssignment.Resolve(Raid))
        {
            var player = Raid[slot];
            if (player == null)
                continue;

            var isSupport = player.Class.IsSupport();
            if (States[slot].FireOrder != (isSupport ? 3 : 1))
                continue; // fixed assignment

            ref var flex = ref eastFlex[isSupport ? 0 : 1];
            if (group > flex.prio)
                flex = (slot, group);
        }

        // calculate positions
        var northDir = Angle.FromDirection(_relNorth);
        for (int i = 0; i < States.Length; ++i)
        {
            var player = Raid[i];
            if (player == null)
                continue;

            ref var state = ref States[i];
            var isSupport = player.Class.IsSupport();
            var eastSlot = eastFlex[isSupport ? 0 : 1].slot;
            bool? preferEast = eastSlot >= 0 ? eastSlot == i : null;
            var dir = player.Class.IsSupport() ? SupportDir(state.FireOrder, preferEast) : DDDir(state.FireOrder, preferEast);
            if (dir != null)
                state.AssignedDir = (northDir + dir.Value).ToDirection();
        }
    }

    private Angle? SupportDir(int fireOrder, bool? preferEast) => fireOrder switch
    {
        2 => 90.Degrees(),
        3 => preferEast == null ? null : preferEast.Value ? -45.Degrees() : 45.Degrees(),
        _ => 0.Degrees()
    };

    private Angle? DDDir(int fireOrder, bool? preferEast) => fireOrder switch
    {
        1 => preferEast == null ? null : preferEast.Value ? -135.Degrees() : 135.Degrees(),
        2 => -90.Degrees(),
        _ => 180.Degrees()
    };

    private bool IsBaitingLaser(in PlayerState state, int order) => order switch
    {
        1 => state.LaserOrder == 1,
        3 => state.LaserOrder == 2,
        5 => state.LaserOrder == 3,
        _ => false
    };

    private float RangeHint(in PlayerState state, bool isSupport, int order) => order switch
    {
        0 => state.FireOrder == 1 ? RangeHintOut : RangeHintStack,
        1 => state.LaserOrder == 1 ? RangeHintLaser : state.HaveDarkEruption ? RangeHintDarkEruption : RangeHintDarkWater,
        2 => state.FireOrder == 2 ? RangeHintOut : RangeHintStack,
        3 => state.LaserOrder == 2 ? RangeHintLaser : state.RewindOrder == 2 ? RangeHintEye : RangeHintChill,
        4 => state.FireOrder == 3 ? RangeHintOut : RangeHintStack,
        5 => state.LaserOrder == 3 ? RangeHintLaser : RangeHintChill,
        _ => RangeHintChill
    };

    private string Hint(in PlayerState state, bool isSupport, int order) => order switch
    {
        0 => state.FireOrder == 1 ? "Out" : "Stack", // 10s
        1 => state.LaserOrder == 1 ? "Laser" : state.HaveDarkEruption ? "Hourglass" : "Mid", // 15s - at this point everyone either baits laser or does rewind (eruption or water)
        2 => state.FireOrder == 2 ? "Out" : "Stack", // 20s
        3 => state.LaserOrder == 2 ? "Laser" : state.RewindOrder == 2 ? "Mid" : "Chill", // 25s - at this point people bait lasers, rewind eyes or chill
        4 => state.FireOrder == 3 ? "Out" : "Stack", // 30s
        5 => state.LaserOrder == 3 ? "Laser" : "Chill", // 35s
        _ => "Look out"
    };

    private WPos SafeSpot(int slot, float range)
    {
        var assignedDir = States[slot].AssignedDir;
        var safespot = Module.Center + range * assignedDir;
        if (IsBaitingLaser(States[slot], NumCasts) && LaserRotationAt(safespot) is var rot && rot != default)
            safespot += 1.5f * (Angle.FromDirection(assignedDir) - 4.5f * rot).ToDirection();
        return safespot;
    }
}

class P3UltimateRelativityDarkFireUnholyDarkness(BossModule module) : Components.UniformStackSpread(module, 6, 8, 5, alwaysShowSpreads: true)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by main component

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SpellInWaitingUnholyDarkness:
                if ((status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 10)
                    AddStack(actor, status.ExpireAt);
                break;
            case SID.SpellInWaitingDarkFire:
                if ((status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 10)
                    AddSpread(actor, status.ExpireAt);
                break;
        }
    }
}

class P3UltimateRelativitySinboundMeltdownBait(BossModule module) : Components.GenericBaitAway(module, AID.UltimateRelativitySinboundMeltdownAOEFirst)
{
    private readonly P3UltimateRelativity? _rel = module.FindComponent<P3UltimateRelativity>();

    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_rel != null)
        {
            for (int i = NumCasts; i < _rel.LaserRotations.Count; ++i)
            {
                var closest = Raid.WithoutSlot().Closest(_rel.LaserRotations[i].origin.Position);
                if (closest != null)
                {
                    CurrentBaits.Add(new(_rel.LaserRotations[i].origin, closest, _shape, _rel.LaserRotations[i].activation));
                }
            }
        }
    }

    // TODO: hints for proper baiting?..
    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;

        var shouldBait = (_rel?.States[slot].LaserOrder ?? 0) == CurrentOrder;
        var baitIndex = CurrentBaits.FindIndex(b => b.Target == actor);
        var isBaiting = baitIndex >= 0;
        if (isBaiting != shouldBait)
        {
            hints.Add(shouldBait ? "Bait laser!" : "GTFO from hourglass!");
        }

        if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from baited aoe!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by main component

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var bait in ActiveBaitsOn(pc))
        {
            if (bait.Source.Position.AlmostEqual(AssignedHourglass(pcSlot), 1) && _rel != null)
            {
                // draw extra rotation hints for correctly baited hourglass
                // note: we don't want to draw 'short' edges of the rectangle (farther one is far outside arena bounds anyway, and closer one messes visualization up too much
                var rot = _rel.LaserRotationAt(bait.Source.Position);
                for (int i = 0; i < 10; ++i)
                {
                    var dir = (bait.Rotation + i * rot).ToDirection();
                    var side = _shape.HalfWidth * dir.OrthoR();
                    var end = bait.Source.Position + _shape.LengthFront * dir;
                    Arena.AddLine(bait.Source.Position + side, end + side, ArenaColor.Danger);
                    Arena.AddLine(bait.Source.Position - side, end - side, ArenaColor.Danger);
                }
            }
            else
            {
                // just draw default hint
                bait.Shape.Outline(Arena, bait.Source.Position, bait.Rotation);
            }
        }
    }

    private int CurrentOrder => NumCasts switch
    {
        < 3 => 1,
        < 6 => 2,
        < 8 => 3,
        _ => 0
    };

    private WPos AssignedHourglass(int slot) => Module.Center + 9.5f * (_rel?.States[slot].AssignedDir ?? default);
}

class P3UltimateRelativitySinboundMeltdownAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly P3UltimateRelativity? _rel = module.FindComponent<P3UltimateRelativity>();
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(50, 2.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by main component

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.UltimateRelativitySinboundMeltdownAOEFirst:
                var rot = _rel?.LaserRotationAt(caster.Position) ?? default;
                for (int i = 1; i < 10; ++i)
                    _aoes.Add(new(_shape, caster.Position, spell.Rotation + i * rot, WorldState.FutureTime(i + 1)));
                break;
            case AID.UltimateRelativitySinboundMeltdownAOERest:
                ++NumCasts;
                var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1) && aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
                if (count != 1)
                    ReportError($"Unexpected aoe at {caster.Position} -> {spell.Rotation}");
                break;
        }
    }
}

class P3UltimateRelativityDarkBlizzard(BossModule module) : Components.GenericAOEs(module, AID.UltimateRelativityDarkBlizzard)
{
    private readonly List<Actor> _sources = [];
    private DateTime _activation;

    private static readonly AOEShapeDonut _shape = new(3, 12); // TODO: verify inner radius

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sources.Select(s => new AOEInstance(_shape, s.Position, default, _activation));

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by main component

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.SpellInWaitingDarkBlizzard)
        {
            _sources.Add(actor);
            _activation = status.ExpireAt;
        }
    }
}

class P3UltimateRelativityShadoweye(BossModule module) : BossComponent(module)
{
    private readonly P3UltimateRelativity? _rel = module.FindComponent<P3UltimateRelativity>();
    private readonly List<WPos> _eyes = [];
    private DateTime _activation;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var pos = _rel?.States[slot].ReturnPos ?? actor.Position;
        if (_eyes.Any(eye => eye != pos && HitByEye(pos, actor.Rotation, eye)))
            hints.Add("Turn away from gaze!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var pos = _rel?.States[slot].ReturnPos ?? actor.Position;
        foreach (var eye in _eyes)
            if (eye != pos)
                hints.ForbiddenDirections.Add((Angle.FromDirection(eye - pos), 45.Degrees(), _activation));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pos = _rel?.States[pcSlot].ReturnPos ?? pc.Position;
        Arena.Actor(pos, pc.Rotation, ArenaColor.Object);
        Arena.AddLine(pos, pc.Position, ArenaColor.Safe);
        foreach (var eye in _eyes)
        {
            if (eye == pos)
                continue;

            bool danger = HitByEye(pos, pc.Rotation, eye);
            var eyeCenter = Arena.WorldPositionToScreenPosition(eye);
            Components.GenericGaze.DrawEye(eyeCenter, danger);

            var (min, max) = (-45, 45);
            Arena.PathArcTo(pos, 1, (pc.Rotation + min.Degrees()).Rad, (pc.Rotation + max.Degrees()).Rad);
            Arena.PathStroke(false, ArenaColor.Enemy);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.SpellInWaitingShadoweye:
                if (_rel != null && Raid.TryFindSlot(actor, out var slot))
                    _eyes.Add(_rel.States[slot].ReturnPos);
                break;
            case SID.Return:
                _activation = status.ExpireAt;
                return;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.UltimateRelativityShadoweye)
            _eyes.Clear();
    }

    private bool HitByEye(WPos pos, Angle rot, WPos eye) => rot.ToDirection().Dot((eye - pos).Normalized()) >= 0.707107f; // 45-degree
}

class P3ShellCrusher(BossModule module) : Components.UniformStackSpread(module, 6, 0, 8, 8, includeDeadTargets: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ShellCrusher)
        {
            // note: target is random?..
            var target = WorldState.Actors.Find(caster.TargetID);
            if (target != null)
                AddStack(target, Module.CastFinishAt(spell, 0.4f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ShellCrusherAOE)
        {
            Stacks.Clear();
        }
    }
}

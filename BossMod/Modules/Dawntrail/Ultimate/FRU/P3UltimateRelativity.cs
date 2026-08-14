namespace BossMod.Dawntrail.Ultimate.FRU;

sealed class P3UltimateRelativity(BossModule module) : Components.CastCounter(module, default)
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

    public const float RangeHintOut = 12f; // explosion radius is 8
    public const float RangeHintStack = 1f;
    public const float RangeHintLaser = 9.5f; // hourglass location
    public const float RangeHintDarkEruption = 9f; // radius is 6, especially for fire-order 2 has to be < 9.5, otherwise will be clipped by own laser
    public const float RangeHintDarkWater = 1f;
    public const float RangeHintEye = 2f;
    public const float RangeHintChill = -1f; // simplifies looking outside and hitting boss

    public Angle LaserRotationAt(WPos pos) => LaserRotations.FirstOrDefault(r => r.origin.Position.AlmostEqual(pos, 1f)).rotation;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (States[slot].RewindOrder == 0)
            return;

        var isSupport = actor.Class.IsSupport();
        var hintBuilder = new StringBuilder();

        for (var i = NumCasts; i < 7; ++i)
        {
            if (i > NumCasts)
            {
                hintBuilder.Append(" > ");
            }
            hintBuilder.Append(Hint(States[slot], isSupport, i));
        }

        hints.Add(hintBuilder.ToString(), false);
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
                        hints.AddForbiddenZone(new SDInvertedCircle(SafeSpot(slot, 9f), 1f), _nextImminent);
                    }
                    else
                    {
                        // ok, out is imminent, gtfo - we need to avoid clipping people, avoid dark blizzard (if it's being resolved now), and avoid lasers (if any)
                        var avoidBlizzard = NumCasts == 2;
                        foreach (var (i, p) in Raid.WithSlot(false, true, true).Exclude(slot))
                        {
                            var avoidRadius = avoidBlizzard && States[i].HaveDarkBlizzard ? 12f : 8f;
                            hints.AddForbiddenZone(new SDCircle(p.Position, avoidRadius));
                        }
                        var lasers = Module.FindComponent<P3UltimateRelativitySinboundMeltdownAOE>();
                        if (lasers != null)
                            foreach (var laser in lasers.ActiveAOEs(slot, actor))
                                hints.AddForbiddenZone(laser.Shape.Distance(laser.Origin, laser.Rotation), laser.Activation);
                    }
                    break;
                case RangeHintLaser:
                case RangeHintDarkEruption:
                case RangeHintEye:
                    // go to exact safespot
                    hints.AddForbiddenZone(new SDPrecisePosition(SafeSpot(slot, range), new(default, 1f), Arena.Bounds.MapResolution, actor.Position, 0.1f), _nextImminent);
                    break;
                default:
                    // go to mid, staying as tightly as possible to allow for better uptime; after all mechanics, go opposite to dodge eyes more naturally
                    var dest = NumCasts >= 6 ? SafeSpot(slot, range) : Arena.Center;
                    hints.AddForbiddenZone(new SDPrecisePosition(dest, new(default, 1f), Arena.Bounds.MapResolution, actor.Position, 0.1f), _nextImminent);
                    break;
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var assignedDir = States[pcSlot].AssignedDir;
        if (assignedDir != default && NumCasts < 6)
        {
            Arena.AddLine(Arena.Center, Arena.Center + 20f * assignedDir, Colors.Safe);
            Arena.ZoneCircleOutline(SafeSpot(pcSlot, RangeHint(States[pcSlot], pc.Class.IsSupport(), NumCasts)), 1f, Colors.Safe);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SpellInWaitingDarkFire:
                var slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    ref var state = ref States[slot];
                    state.FireOrder = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds switch
                    {
                        < 15d => 1,
                        < 25d => 2,
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
            case (uint)SID.SpellInWaitingDarkBlizzard:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                {
                    States[slot].LaserOrder = actor.Class.IsSupport() ? 2 : 1;
                    States[slot].HaveDarkBlizzard = true;
                }
                break;
            case (uint)SID.SpellInWaitingDarkEruption:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].HaveDarkEruption = true;
                break;
            case (uint)SID.SpellInWaitingReturn:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].RewindOrder = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 20 ? 1 : 2;
                break;
            case (uint)SID.DelightsHourglassRotation:
                var rot = status.Extra switch
                {
                    0x10D => 15f.Degrees(),
                    0x15C => -15f.Degrees(),
                    _ => default
                };
                if (rot != default)
                    LaserRotations.Add((actor, rot, status.ExpireAt));
                break;
            case (uint)SID.Return:
                slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0)
                    States[slot].ReturnPos = actor.Position;
                break;
            case (uint)SID.Stun:
                ++NumReturnStuns;
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.Stun:
                --NumReturnStuns;
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (source.OID == (uint)OID.DelightsHourglass && tether.ID == (uint)TetherID.UltimateRelativityQuicken)
        {
            _relNorth += source.Position - Arena.Center;
            if (++_numYellowTethers == 3)
                InitAssignments();
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.UltimateRelativityUnholyDarkness or (uint)AID.UltimateRelativitySinboundMeltdownAOEFirst && WorldState.CurrentTime > _nextImminent)
        {
            ++NumCasts;
            _nextImminent = WorldState.FutureTime(2.5d);
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
        for (var i = 0; i < States.Length; ++i)
        {
            var player = Raid[i];
            if (player == null)
                continue;

            ref var state = ref States[i];
            var isSupport = player.Class.IsSupport();
            var eastSlot = eastFlex[isSupport ? 0 : 1].slot;
            bool? preferEast = eastSlot >= 0 ? eastSlot == i : null;
            var dir = player.Class.IsSupport() ? SupportDir(state.FireOrder, preferEast) : DDDir(state.FireOrder, preferEast);
            //if (dir != null)
            //    state.AssignedDir = (northDir + dir.Value).ToDirection();
            // Fixing support fire 10 assignment not drawing - Topas 2026/05/03
            state.AssignedDir = dir != null ? (northDir + dir.Value).ToDirection() : northDir.ToDirection();
        }
    }

    private static Angle? SupportDir(int fireOrder, bool? preferEast) => fireOrder switch
    {
        2 => 90f.Degrees(),
        3 => preferEast == null ? null : preferEast.Value ? -45f.Degrees() : 45f.Degrees(),
        _ => default
    };

    private static Angle? DDDir(int fireOrder, bool? preferEast) => fireOrder switch
    {
        1 => preferEast == null ? null : preferEast.Value ? -135f.Degrees() : 135f.Degrees(),
        2 => -90f.Degrees(),
        _ => 180f.Degrees()
    };

    private static bool IsBaitingLaser(in PlayerState state, int order) => order switch
    {
        1 => state.LaserOrder == 1,
        3 => state.LaserOrder == 2,
        5 => state.LaserOrder == 3,
        _ => false
    };

    private static float RangeHint(in PlayerState state, bool isSupport, int order) => order switch
    {
        0 => state.FireOrder == 1 ? RangeHintOut : RangeHintStack,
        1 => state.LaserOrder == 1 ? RangeHintLaser : state.HaveDarkEruption ? RangeHintDarkEruption : RangeHintDarkWater,
        2 => state.FireOrder == 2 ? RangeHintOut : RangeHintStack,
        3 => state.LaserOrder == 2 ? RangeHintLaser : state.RewindOrder == 2 ? RangeHintEye : RangeHintChill,
        4 => state.FireOrder == 3 ? RangeHintOut : RangeHintStack,
        5 => state.LaserOrder == 3 ? RangeHintLaser : RangeHintChill,
        _ => RangeHintChill
    };

    private static string Hint(in PlayerState state, bool isSupport, int order) => order switch
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
        var safespot = Arena.Center + range * assignedDir;
        if (IsBaitingLaser(States[slot], NumCasts) && LaserRotationAt(safespot) is var rot && rot != default)
            safespot += 1.5f * (Angle.FromDirection(assignedDir) - 4.5f * rot).ToDirection();
        return safespot;
    }
}

sealed class P3UltimateRelativityDarkFireUnholyDarkness(BossModule module) : Components.UniformStackSpread(module, 6f, 8f, 5)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by main component

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SpellInWaitingUnholyDarkness:
                if ((status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 10d)
                    AddStack(actor, status.ExpireAt);
                break;
            case (uint)SID.SpellInWaitingDarkFire:
                if ((status.ExpireAt - WorldState.CurrentTime).TotalSeconds < 10d)
                    AddSpread(actor, status.ExpireAt);
                break;
        }
    }
}

sealed class P3UltimateRelativitySinboundMeltdownBait(BossModule module) : Components.GenericBaitAway(module, (uint)AID.UltimateRelativitySinboundMeltdownAOEFirst)
{
    private readonly P3UltimateRelativity? _rel = module.FindComponent<P3UltimateRelativity>();

    private static readonly AOEShapeRect _shape = new(60, 2.5f);

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_rel != null)
        {
            for (var i = NumCasts; i < _rel.LaserRotations.Count; ++i)
            {
                var closest = Raid.WithoutSlot(false, true, true).Closest(_rel.LaserRotations[i].origin.Position);
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

        if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, ref b)))
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
                for (var i = 0; i < 10; ++i)
                {
                    var dir = (bait.Rotation + i * rot).ToDirection();
                    var side = _shape.HalfWidth * dir.OrthoR();
                    var end = bait.Source.Position + _shape.LengthFront * dir;
                    Arena.AddLine(bait.Source.Position + side, end + side, Colors.Danger);
                    Arena.AddLine(bait.Source.Position - side, end - side, Colors.Danger);
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

    private WPos AssignedHourglass(int slot) => Arena.Center + 9.5f * (_rel?.States[slot].AssignedDir ?? default);
}

sealed class P3UltimateRelativitySinboundMeltdownAOE(BossModule module) : Components.GenericAOEs(module)
{
    private readonly P3UltimateRelativity? _rel = module.FindComponent<P3UltimateRelativity>();
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(50, 2.5f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_aoes);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by main component

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.UltimateRelativitySinboundMeltdownAOEFirst:
                var rot = _rel?.LaserRotationAt(caster.Position) ?? default;
                for (var i = 1; i < 10; ++i)
                    _aoes.Add(new(_shape, caster.Position, spell.Rotation + i * rot, WorldState.FutureTime(i + 1)));
                break;
            case (uint)AID.UltimateRelativitySinboundMeltdownAOERest:
                ++NumCasts;
                var count = _aoes.RemoveAll(aoe => aoe.Origin.AlmostEqual(caster.Position, 1) && aoe.Rotation.AlmostEqual(spell.Rotation, 0.1f));
                if (count != 1)
                    ReportError($"Unexpected aoe at {caster.Position} -> {spell.Rotation}");
                break;
        }
    }
}

sealed class P3UltimateRelativityDarkBlizzard(BossModule module) : Components.GenericAOEs(module, (uint)AID.UltimateRelativityDarkBlizzard)
{
    private readonly List<Actor> _sources = [];
    private DateTime _activation;

    private static readonly AOEShapeDonut _shape = new(3f, 12f); // TODO: verify inner radius

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _sources.Count;
        if (count == 0)
            return [];
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(_shape, _sources[i].Position, default, _activation);
        }
        return aoes;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { } // handled by main component

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.SpellInWaitingDarkBlizzard)
        {
            _sources.Add(actor);
            _activation = status.ExpireAt;
        }
    }
}

sealed class P3UltimateRelativityShadoweye(BossModule module) : BossComponent(module)
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
                hints.ForbiddenDirections.Add((Angle.FromDirection(eye - pos), 45f.Degrees(), _activation));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var pos = _rel?.States[pcSlot].ReturnPos ?? pc.Position;
        Arena.Actor(pos, pc.Rotation, Colors.Object);
        Arena.AddLine(pos, pc.Position, Colors.Safe);
        foreach (var eye in _eyes)
        {
            if (eye == pos)
                continue;

            var danger = HitByEye(pos, pc.Rotation, eye);
            var eyeCenter = Arena.WorldPositionToScreenPosition(eye);
            Components.GenericGaze.DrawEye(eyeCenter, danger);

            var (min, max) = (-45f, 45f);
            Arena.PathArcTo(pos, 1, (pc.Rotation + min.Degrees()).Rad, (pc.Rotation + max.Degrees()).Rad);
            MiniArena.PathStroke(false, Colors.Enemy);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.SpellInWaitingShadoweye:
                var slot = Raid.FindSlot(actor.InstanceID);
                if (slot >= 0 && _rel != null)
                    _eyes.Add(_rel.States[slot].ReturnPos);
                break;
            case (uint)SID.Return:
                _activation = status.ExpireAt;
                return;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.UltimateRelativityShadoweye)
            _eyes.Clear();
    }

    private bool HitByEye(WPos pos, Angle rot, WPos eye) => rot.ToDirection().Dot((eye - pos).Normalized()) >= 0.707107f; // 45-degree
}

sealed class P3ShellCrusher(BossModule module) : Components.UniformStackSpread(module, 6f, default, 8, 8, includeDeadTargets: true)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.ShellCrusher)
        {
            // note: target is random?..
            var target = WorldState.Actors.Find(caster.TargetID);
            if (target != null)
                AddStack(target, Module.CastFinishAt(spell, 0.4f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.ShellCrusherAOE)
        {
            Stacks.Clear();
        }
    }
}

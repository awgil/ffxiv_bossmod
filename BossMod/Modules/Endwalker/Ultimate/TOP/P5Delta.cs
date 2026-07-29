namespace BossMod.Endwalker.Ultimate.TOP;

// note: this is all very tied to LPDU strat
sealed class P5Delta(BossModule module) : BossComponent(module)
{
    public enum PairAssignment { None, Inner, Outer }
    public enum SideAssignment { None, North, South }

    public struct PlayerState
    {
        public int PartnerSlot;
        public bool IsLocal; // true = local smell aka green tether, false = remote smell aka blue tether
        public bool TetherBroken;
        public Actor? RocketPunch;
        public PairAssignment PairAssignment;
        public SideAssignment SideAssignment;
    }

    public int NumPunchesSpawned;
    public int NumTethersBroken;
    public bool TethersActive;
    public bool ExplosionsBaited;
    public Angle[] ArmRotations = new Angle[6]; // [0] = at rel north, then CCW
    public PlayerState[] Players = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { PartnerSlot = -1 });
    private Actor? _nearWorld;
    private Actor? _distantWorld;
    private Actor? _monitorTarget;
    private Actor? _beyondDefenceTarget;
    private WDir _eyeDir; // relative north; beetle is rel west, final is rel east
    private WDir _monitorSafeDir;
    private WDir _swivelCannonSafeDir;
    private readonly List<(int, int)> _localTethers = [];
    private readonly List<(int, int)> _remoteTethers = [];

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
    {
        var pcState = Players[pcSlot];
        var playerState = Players[playerSlot];
        return pcState.PartnerSlot == playerSlot ? PlayerPriority.Interesting
            : !ExplosionsBaited && pcState.SideAssignment != SideAssignment.None && pcState.IsLocal == playerState.IsLocal && pcState.SideAssignment == playerState.SideAssignment ? PlayerPriority.Danger
            : PlayerPriority.Normal;
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var p = Players[pcSlot];
        var partner = p.TetherBroken ? null : Raid[p.PartnerSlot];
        if (partner != null)
            Arena.AddLine(pc.Position, partner.Position);

        foreach (var safeSpot in SafeSpotOffsets(pcSlot))
            Arena.ZoneCircleOutline(Arena.Center + safeSpot, 1f, Colors.Safe);
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.RocketPunch1 or (uint)OID.RocketPunch2)
        {
            var (closestSlot, closestPlayer) = Raid.WithSlot(true, true, true).Closest(actor.Position);
            if (closestPlayer != null)
            {
                if (Players[closestSlot].RocketPunch != null)
                    ReportError($"Multiple punches spawned for player #{closestSlot}");
                Players[closestSlot].RocketPunch = actor;
            }

            if (++NumPunchesSpawned == PartyState.MaxPartySize)
            {
                InitAssignments();
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.HelloNearWorld:
                _nearWorld = actor;
                break;
            case (uint)SID.HelloDistantWorld:
                _distantWorld = actor;
                break;
            case (uint)SID.OversampledWaveCannonLoadingR:
            case (uint)SID.OversampledWaveCannonLoadingL:
                _monitorTarget = actor;
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        switch (tether.ID)
        {
            case (uint)TetherID.HWPrepLocalTether:
            case (uint)TetherID.HWPrepRemoteTether:
                var s1 = Raid.FindSlot(source.InstanceID);
                var s2 = Raid.FindSlot(tether.Target);
                if (s1 >= 0 && s2 >= 0)
                {
                    var isLocal = tether.ID == (uint)TetherID.HWPrepLocalTether;
                    Players[s1].PartnerSlot = s2;
                    Players[s2].PartnerSlot = s1;
                    Players[s1].IsLocal = Players[s2].IsLocal = isLocal;
                    (isLocal ? _localTethers : _remoteTethers).Add((s1, s2));
                }
                break;
            case (uint)TetherID.HWLocalTether:
            case (uint)TetherID.HWRemoteTether:
                TethersActive = true;
                break;
        }
    }

    public override void OnUntethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID is (uint)TetherID.HWLocalTether or (uint)TetherID.HWRemoteTether)
        {
            var s1 = Raid.FindSlot(source.InstanceID);
            var s2 = Raid.FindSlot(tether.Target);
            if (s1 >= 0 && s2 >= 0)
            {
                Players[s1].TetherBroken = Players[s2].TetherBroken = true;
                ++NumTethersBroken;
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.DeltaExplosion:
                ExplosionsBaited = true;
                break;
            case (uint)AID.DeltaOversampledWaveCannonR:
                _monitorSafeDir = -(spell.Rotation - 90f.Degrees()).ToDirection();
                break;
            case (uint)AID.DeltaOversampledWaveCannonL:
                _monitorSafeDir = -(spell.Rotation + 90f.Degrees()).ToDirection();
                break;
            case (uint)AID.SwivelCannonR:
            case (uint)AID.SwivelCannonL:
                _swivelCannonSafeDir = -spell.Rotation.ToDirection();
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.BeyondDefenseAOE)
            _beyondDefenceTarget = WorldState.Actors.Find(spell.MainTargetID);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43)
        {
            switch (actor.OID)
            {
                case (uint)OID.BeetleHelper:
                    _eyeDir = (actor.Position - Arena.Center).Normalized().OrthoR();
                    break;
                case (uint)OID.FinalHelper:
                    _eyeDir = (actor.Position - Arena.Center).Normalized().OrthoL();
                    break;
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor.OID is (uint)OID.LeftArmUnit or (uint)OID.RightArmUnit)
        {
            var rotation = iconID switch
            {
                (uint)IconID.RotateCW => -20f.Degrees(),
                (uint)IconID.RotateCCW => 20f.Degrees(),
                _ => default
            };
            if (rotation != default)
                ArmRotations[ArmIndex(actor.Position - Arena.Center)] = rotation;
        }
    }

    public WDir ArmOffset(int index) => 20f * (Angle.FromDirection(_eyeDir) + index * 60f.Degrees()).ToDirection();
    public int ArmIndex(WDir offset) => _eyeDir.Dot(offset) switch
    {
        > 19 => 0,
        < -19 => 3,
        > 0 => _eyeDir.OrthoL().Dot(offset) > 0 ? 1 : 5,
        _ => _eyeDir.OrthoL().Dot(offset) > 0 ? 2 : 4,
    };

    private void InitAssignments()
    {
        // 1. assign initial inner/outer
        float slotToOffsetX(int slot) => _eyeDir.OrthoR().Dot((Raid[slot]?.Position ?? Arena.Center) - Arena.Center);
        float pairToOffsetX((int s1, int s2) slots) => Math.Abs(slotToOffsetX(slots.s1) + slotToOffsetX(slots.s2));
        var outerLocal = _localTethers.MaxBy(pairToOffsetX);
        var outerRemote = _remoteTethers.MaxBy(pairToOffsetX);
        foreach (ref var p in Players.AsSpan())
        {
            var outerSlots = p.IsLocal ? outerLocal : outerRemote;
            p.PairAssignment = p.PartnerSlot == outerSlots.Item1 || p.PartnerSlot == outerSlots.Item2 ? PairAssignment.Outer : PairAssignment.Inner;
        }
        // 2. assign initial north/south
        foreach (var (s1, s2) in _localTethers.Concat(_remoteTethers))
        {
            var p1 = Raid[s1];
            var p2 = Raid[s2];
            if (p1 != null && p2 != null)
            {
                var p12n = _eyeDir.Dot(p1.Position - p2.Position) > 0;
                Players[s1].SideAssignment = p12n ? SideAssignment.North : SideAssignment.South;
                Players[s2].SideAssignment = p12n ? SideAssignment.South : SideAssignment.North;
            }
        }
        // 3. swap inner north/south if needed
        foreach (ref var p in Players.AsSpan())
        {
            if (p.PairAssignment != PairAssignment.Inner)
                continue; // outer stay where they are

            var outerSlots = p.IsLocal ? outerLocal : outerRemote;
            var nearbyOuterSlot = Players[outerSlots.Item1].SideAssignment == p.SideAssignment ? outerSlots.Item1 : outerSlots.Item2;
            if (p.RocketPunch?.OID == Players[nearbyOuterSlot].RocketPunch?.OID)
            {
                // swap side
                p.SideAssignment = p.SideAssignment == SideAssignment.North ? SideAssignment.South : SideAssignment.North;
            }
        }
    }

    private WDir[] SafeSpotOffsets(int slot)
    {
        var p = Players[slot];
        if (p.PartnerSlot < 0 || _eyeDir == default)
            return []; // no safe spots yet
        if (NumPunchesSpawned < PartyState.MaxPartySize)
        {
            // no punches yet, show all 4 possible spots
            if (p.IsLocal)
            {
                // green tethers go to final side
                return
                [
                    TransformRelNorth(9f, -11f),
                    TransformRelNorth(9f, +11f),
                    TransformRelNorth(13f, -11f),
                    TransformRelNorth(13f, +11f)
                ];
            }
            else
            {
                // blue tethers go to beetle side
                return
                [
                    TransformRelNorth(-9f, -8f),
                    TransformRelNorth(-9f, +8f),
                    TransformRelNorth(-13f, -4f),
                    TransformRelNorth(-13f, +4f)
                ];
            }
        }

        if (p.SideAssignment == SideAssignment.None || p.PairAssignment == PairAssignment.None)
            return []; // we should have correct assignments by now

        var dirZ = p.SideAssignment == SideAssignment.North ? -1 : 1; // fully symmetrical
        if (!ExplosionsBaited)
        {
            // now we should have correct assignments
            if (p.IsLocal)
                return [TransformRelNorth(11f, 11f * dirZ)];
            else
                return [TransformRelNorth(-13f, (p.PairAssignment == PairAssignment.Inner && NumTethersBroken == 0 ? 8f : 4f) * dirZ)];
        }

        if (_beyondDefenceTarget == null)
        {
            // bait spots near arms
            if (p.IsLocal)
            {
                if (p.PairAssignment == PairAssignment.Inner)
                    return [BaitOffset(dirZ > 0 ? 3 : 0)];
                else
                    return [BaitOffset(dirZ > 0 ? 4 : 5)];
            }
            else
            {
                if (p.PairAssignment == PairAssignment.Inner)
                    return [TransformRelNorth(0, 5f * dirZ)];
                else
                    return [BaitOffset(dirZ > 0 ? 2 : 1)];
            }
        }

        if (_swivelCannonSafeDir == default)
        {
            if (p.IsLocal)
            {
                // monitor soak spots
                var dirX = p.PairAssignment == PairAssignment.Inner ? -1 : +1;
                return [TransformRelNorth(7f * dirX, 13f * dirZ)];
            }
            else if (Raid[slot] != _beyondDefenceTarget)
            {
                // central stack
                return [(Raid[slot] == _monitorTarget ? 5f : 2.5f) * _monitorSafeDir];
            }
            else
            {
                // beyond defense target wants to run outside stack (TODO: select direction that is convenient for monitor target)
                var stackPos = (Raid[slot] == _monitorTarget ? 5f : 2.5f) * _monitorSafeDir;
                var horizOffset = TransformRelNorth(15f, default);
                return
                    [
                        stackPos + horizOffset,
                        stackPos - horizOffset
                    ];
            }
        }
        var relNorthSafe = _swivelCannonSafeDir.Dot(_eyeDir) > 0f;
        var safeDirZ = relNorthSafe ? -1 : 1;
        if (p.IsLocal)
        {
            var startingFromSafe = p.SideAssignment == SideAssignment.North == relNorthSafe;
            if (p.PairAssignment == PairAssignment.Inner)
                return [TransformRelNorth(-10f, (startingFromSafe ? 12f : 6f) * safeDirZ)];
            else if (startingFromSafe)
                return [TransformRelNorth(15f, 11f * safeDirZ)];
            else
                return [TransformRelNorth(-18f, 2f * safeDirZ)];
        }
        else if (_distantWorld == Raid[slot])
        {
            return [TransformRelNorth(default, 19f * safeDirZ)];
        }
        else if (_nearWorld == Raid[slot])
        {
            return [TransformRelNorth(default, 6f * safeDirZ)];
        }
        else
        {
            return [TransformRelNorth(9f, 15f * safeDirZ)];
        }
    }

    // x positive is east (final side), z positive is south
    private WDir TransformRelNorth(float x, float z) => x * _eyeDir.OrthoR() - z * _eyeDir;
    private WDir BaitOffset(int index) => 19f * (Angle.FromDirection(_eyeDir) + index * 60f.Degrees() - 0.15f * ArmRotations[index]).ToDirection(); // 5 degrees offset in correct direction
}

sealed class P5DeltaOpticalLaser(BossModule module) : Components.GenericAOEs(module, (uint)AID.OpticalLaser)
{
    public Actor? Source;
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(100f, 8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (Source != null)
            return new AOEInstance[1] { new(_shape, Source.Position, Source.Rotation, _activation) };
        return [];
    }

    // at this point eye is in correct position
    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id == 0x1E43 && actor.OID is (uint)OID.BeetleHelper or (uint)OID.FinalHelper)
        {
            Source ??= Module.Enemies((uint)OID.OpticalUnit)[0];
            _activation = WorldState.FutureTime(20d);
        }
    }
}

sealed class P5DeltaExplosion(BossModule module) : Components.SimpleAOEs(module, (uint)AID.DeltaExplosion, 3f)
{
    private readonly P5Delta? _delta = module.FindComponent<P5Delta>();

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_delta == null || Casters.Count > 0)
            return;
        var ps = _delta.Players[pcSlot];
        var partner = Raid.WithSlot(true, true, true).WhereSlot(i => _delta.Players[i].IsLocal == ps.IsLocal && i != ps.PartnerSlot && _delta.Players[i].RocketPunch?.OID != ps.RocketPunch?.OID).FirstOrDefault().Item2;
        if (partner != null)
            Arena.ZoneCircleOutline(partner.Position, 3f, Colors.Safe);
    }
}

sealed class P5DeltaHyperPulse(BossModule module) : Components.GenericAOEs(module)
{
    private readonly P5Delta? _delta = module.FindComponent<P5Delta>();
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _shape = new(100f, 4f);
    private const uint _numRepeats = 6u;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoes.Count != 0)
        {
            return CollectionsMarshal.AsSpan(_aoes);
        }
        else if (_delta != null)
        {
            var len = _delta.ArmRotations.Length;
            var aoes = new List<AOEInstance>(len);
            for (var i = 0; i < len; ++i)
            {
                var pos = (Arena.Center + _delta.ArmOffset(i)).Quantized();
                if (Raid.WithoutSlot(false, true, true).Closest(pos) == actor)
                {
                    var angle = Angle.FromDirection(actor.Position - pos);
                    for (var j = 0u; j < _numRepeats; ++j)
                    {
                        aoes.Add(new(_shape, pos, angle + j * _delta.ArmRotations[i], risky: false));
                    }
                }
            }
            return CollectionsMarshal.AsSpan(aoes);
        }
        return [];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.DeltaHyperPulseFirst && _delta != null)
        {
            var rot = _delta.ArmRotations[_delta.ArmIndex(caster.Position - Arena.Center)];
            for (var i = 0u; i < _numRepeats; ++i)
            {
                _aoes.Add(new(_shape, spell.LocXZ, (spell.Rotation + i * rot).Normalized(), Module.CastFinishAt(spell, i * 0.6d), actorID: caster.InstanceID));
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.DeltaHyperPulseFirst or (uint)AID.DeltaHyperPulseRest)
        {
            ++NumCasts;
            var count = _aoes.Count;
            var id = caster.InstanceID;
            for (var i = 0; i < count; ++i)
            {
                var aoe = _aoes[i];
                if (aoe.ActorID == id && aoe.Rotation.AlmostEqual(caster.Rotation, Angle.DegToRad))
                {
                    _aoes.RemoveAt(i);
                    return;
                }
            }
            ReportError($"Single cast removed zero aoes");
        }
    }
}

sealed class P5DeltaOversampledWaveCannon(BossModule module) : Components.UniformStackSpread(module, default, 7f)
{
    private readonly P5Delta? _delta = module.FindComponent<P5Delta>();
    private Actor? _boss;
    private Angle _bossAngle;
    private BitMask _bossIntendedTargets;
    private Actor? _player;
    private Angle _playerAngle;
    private BitMask _playerIntendedTargets;

    private static readonly AOEShapeRect _shape = new(50, 50);

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        base.AddHints(slot, actor, hints);

        if (_player == actor)
        {
            // ensure we hit only two intended targets
            hints.Add("Aim monitor!", Raid.WithSlot(false, true, true).Exclude(actor).Any(ip => _shape.Check(ip.Item2.Position, actor.Position, actor.Rotation + _playerAngle) != _playerIntendedTargets[ip.Item1]));
        }
        else if (_player != null)
        {
            var hit = _shape.Check(actor.Position, _player.Position, _player.Rotation + _playerAngle);
            if (hit != _playerIntendedTargets[slot])
                hints.Add(hit ? "GTFO from player monitor!" : "Soak player monitor!");
        }

        if (_boss != null)
        {
            var hit = _shape.Check(actor.Position, _boss.Position, _boss.Rotation + _bossAngle);
            if (hit != _bossIntendedTargets[slot])
                hints.Add(hit ? "GTFO from boss monitor!" : "Soak boss monitor!");
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_boss != null)
            _shape.Draw(Arena, _boss.Position, _boss.Rotation + _bossAngle, _bossIntendedTargets[pcSlot] ? Colors.SafeFromAOE : default);
        if (_player != null)
            _shape.Draw(Arena, _player.Position, _player.Rotation + _playerAngle, _playerIntendedTargets[pcSlot] ? Colors.SafeFromAOE : default);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        var angle = status.ID switch
        {
            (uint)SID.OversampledWaveCannonLoadingL => 90f.Degrees(),
            (uint)SID.OversampledWaveCannonLoadingR => -90f.Degrees(),
            _ => default
        };
        if (angle != default && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0)
        {
            _player = actor;
            _playerAngle = angle;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var angle = spell.Action.ID switch
        {
            (uint)AID.DeltaOversampledWaveCannonL => 90f.Degrees(),
            (uint)AID.DeltaOversampledWaveCannonR => -90f.Degrees(),
            _ => default
        };
        if (angle == default)
            return;
        _boss = caster;
        _bossAngle = angle;
        if (_delta == null)
            return;
        var bossSide = angle.Rad > 0 ? P5Delta.SideAssignment.South : P5Delta.SideAssignment.North;
        foreach (var (i, p) in Raid.WithSlot(true, true, true))
        {
            var ps = _delta.Players[i];
            if (ps.IsLocal)
            {
                AddSpread(p, Module.CastFinishAt(spell)); // assume only intended targets will be hit, otherwise chances are it will be all random
                if (ps.SideAssignment == bossSide)
                    _bossIntendedTargets[i] = true;
                else
                    _playerIntendedTargets[i] = true;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.OversampledWaveCannonAOE)
            Spreads.Clear();
    }
}

sealed class P5DeltaSwivelCannon(BossModule module) : Components.GenericAOEs(module)
{
    public AOEInstance[] AOE = [];

    private static readonly AOEShapeCone _shape = new(60f, 105f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOE;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SwivelCannonR or (uint)AID.SwivelCannonL)
        {
            AOE = [new(_shape, spell.LocXZ, spell.Rotation, Module.CastFinishAt(spell))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.SwivelCannonR or (uint)AID.SwivelCannonL)
        {
            AOE = [];
        }
    }
}

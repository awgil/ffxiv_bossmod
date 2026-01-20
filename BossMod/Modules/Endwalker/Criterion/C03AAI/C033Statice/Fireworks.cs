namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

class Fireworks(BossModule module) : Components.UniformStackSpread(module, 3, 20, 2, 2, true)
{
    public Actor?[] TetheredAdds = new Actor?[4];
    public WPos?[] TetheredStartPositions = new WPos?[4];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (TetheredAdds[slot] is var add && add != null)
            hints.Add($"Tether: {((OID)add.OID is OID.NSurprisingMissile or OID.SSurprisingMissile ? "missile" : "claw")}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (TetheredAdds[pcSlot] is var add && add != null)
        {
            Arena.Actor(add, ArenaColor.Object, true);
            Arena.AddLine(add.Position, pc.Position, ArenaColor.Danger);
        }
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.Follow && Raid.TryFindSlot(tether.Target, out var slot) && slot < TetheredAdds.Length)
        {
            TetheredAdds[slot] = source;
            TetheredStartPositions[slot] = source.Position;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch ((IconID)iconID)
        {
            case IconID.FireworksSpread:
                AddSpread(actor, WorldState.FutureTime(9.1f));
                break;
            case IconID.FireworksEnumeration:
                AddStack(actor, WorldState.FutureTime(8.2f));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NFireworksStack:
            case AID.SFireworksStack:
                Stacks.Clear();
                break;
            case AID.NFireworksSpread:
            case AID.SFireworksSpread:
                Spreads.Clear();
                break;
        }
    }
}

class BurningChains(BossModule module) : BossComponent(module)
{
    private BitMask _tethers;
    private BitMask _chains;

    public bool Preparing => _tethers.Any();
    public bool Active => _chains.Any();
    public bool Chained(int slot) => _chains[slot];
    public bool Tethered(int slot) => _tethers[slot];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (Tethered(slot))
            hints.Add("Stay with partner!");
        else if (Chained(slot))
            hints.Add("Break chains!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (Tethered(pcSlot))
            Arena.AddCircle(Module.Center, 1, ArenaColor.Safe);
        else if (Chained(pcSlot))
        {
            var partner = Raid[_chains.WithoutBit(pcSlot).LowestSetBit()];
            if (partner != null)
                Arena.AddLine(pc.Position, partner.Position, ArenaColor.Safe, 1);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (Tethered(slot))
            movementHints.Add(actor.Position, Module.Center, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BurningChains)
        {
            var slot = Raid.FindSlot(actor.InstanceID);
            _chains.Set(slot);
            _tethers.Clear(slot);
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BurningChains)
            _chains.Clear(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID == IconID.FireworksSpread)
            _tethers.Set(Raid.FindSlot(actor.InstanceID));
    }

    // Shortcuts for early chain hints in Fireworks1
    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        // Ensure "preparing" is cleared if the status somehow doesn't happen
        if ((OID)source.OID is OID.NSurprisingMissile or OID.SSurprisingMissile
              && (TetherID)tether.ID == TetherID.Follow && Raid.TryFindSlot(tether.Target, out var slot))
            _tethers.Clear(slot);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        // Missile indicates an upcoming chain
        if ((OID)source.OID is OID.NSurprisingMissile or OID.SSurprisingMissile
              && (TetherID)tether.ID == TetherID.Follow && Raid.TryFindSlot(tether.Target, out var slot))
            _tethers.Set(slot);
    }
}

class FireSpread(BossModule module) : Components.GenericAOEs(module)
{
    public struct Sequence
    {
        public Angle NextRotation;
        public int RemainingExplosions;
        public DateTime NextActivation;
    }

    public List<Sequence> Sequences = [];
    private Angle _rotation;

    private static readonly AOEShapeRect _shape = new(20, 2.5f, -8);
    private const int _maxShownExplosions = 3;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        // future explosions
        foreach (var s in Sequences)
        {
            var rot = s.NextRotation + _rotation;
            var act = s.NextActivation.AddSeconds(1.1f);
            var max = Math.Min(s.RemainingExplosions, _maxShownExplosions);
            for (int i = 1; i < max; ++i)
            {
                yield return new(_shape, Module.Center, rot, act);
                rot += _rotation;
                act = act.AddSeconds(1.1f);
            }
        }

        // imminent explosions
        foreach (var s in Sequences)
        {
            if (s.RemainingExplosions > 0)
            {
                yield return new(_shape, Module.Center, s.NextRotation, s.NextActivation, ArenaColor.Danger);
            }
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var rot = (IconID)iconID switch
        {
            IconID.RotateCW => -10.Degrees(),
            IconID.RotateCCW => 10.Degrees(),
            _ => default
        };
        if (rot != default)
            _rotation = rot;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NFireSpreadFirst or AID.SFireSpreadFirst)
            Sequences.Add(new() { NextRotation = spell.Rotation, RemainingExplosions = 12, NextActivation = Module.CastFinishAt(spell) });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.NFireSpreadFirst or AID.NFireSpreadRest or AID.SFireSpreadFirst or AID.SFireSpreadRest)
        {
            ++NumCasts;
            var index = Sequences.FindIndex(s => s.NextRotation.AlmostEqual(caster.Rotation, 0.1f));
            if (index >= 0)
            {
                ref var s = ref Sequences.AsSpan()[index];
                if (--s.RemainingExplosions > 0)
                {
                    s.NextRotation += _rotation;
                    s.NextActivation = WorldState.FutureTime(1.1f);
                }
                else
                {
                    Sequences.RemoveAt(index);
                }
            }
            else
            {
                ReportError($"Failed to find sequence with rotation {caster.Rotation}");
            }
        }
    }
}

class Fireworks1Hints(BossModule module) : BossComponent(module)
{
    private readonly RingARingOExplosions? _bombs = module.FindComponent<RingARingOExplosions>();
    private readonly Fireworks? _fireworks = module.FindComponent<Fireworks>();
    private readonly BurningChains? _chains = module.FindComponent<BurningChains>();
    private SplitType _split;
    private BitMask _pattern;
    private readonly List<WPos> _safeSpotsClaw = [];
    private readonly List<WPos> _safeSpotsMissile = [];
    private static readonly uint[] colorProgression = [ArenaColor.Safe, ArenaColor.Danger, ArenaColor.Enemy];

    public override void Update()
    {
        if (_pattern.Any() || _bombs == null || _bombs.ActiveBombs.Count < 3)
            return;

        // there are always 6 bombs: one in center, one on north, and other 4 at equal distance (72 degrees apart)
        // one of the active bombs is always in center, two others are then either 72deg ('easy' pattern) or 144deg ('hard' pattern) apart
        // we assign index 0 to N bomb, then increment in CW order
        // so in total there are 10 patterns, 4 of which are mirrored
        // off-center bombs are at radius 15: offsets are (0, -15), (+/- 14.28, -4.64) and (+/- 8.82, +12.14)
        // sum of offsets is 2 * 15 * cos(angle/2) = 9.27 or 24.27
        // staffs are always in cardinals at radius 8
        foreach (var b in _bombs.ActiveBombs)
        {
            var offset = b.Position - Module.Center;
            if (offset.Z < -14)
                _pattern.Set(0); // N
            else if (offset.Z < -4)
                _pattern.Set(offset.X > 0 ? 1 : 4);
            else if (offset.Z > 12)
                _pattern.Set(offset.X > 0 ? 2 : 3);
        }

        switch (_pattern.Raw)
        {
            case 0x3: // 0+1: easy pattern, N + CW
                _split = SplitType.Lateral;
                AddSafeSpot(_safeSpotsClaw, -45.Degrees());
                AddSafeSpot(_safeSpotsMissile, -135.Degrees());
                AddSafeSpot(_safeSpotsMissile, 50.Degrees());
                break;
            case 0x11: // 0+4: easy pattern, N + CCW
                _split = SplitType.Lateral;
                AddSafeSpot(_safeSpotsClaw, 45.Degrees());
                AddSafeSpot(_safeSpotsMissile, 135.Degrees());
                AddSafeSpot(_safeSpotsMissile, -50.Degrees());
                break;
            case 0x5: // 0+2: hard pattern, N + CW
                _split = SplitType.Vertical;
                AddSafeSpot(_safeSpotsClaw, -135.Degrees());
                AddSafeSpot(_safeSpotsMissile, 125.Degrees());
                AddSafeSpot(_safeSpotsMissile, -35.Degrees());
                break;
            case 0x9: // 0+3: hard pattern, N + CCW
                _split = SplitType.Vertical;
                AddSafeSpot(_safeSpotsClaw, 135.Degrees());
                AddSafeSpot(_safeSpotsMissile, -125.Degrees());
                AddSafeSpot(_safeSpotsMissile, 35.Degrees());
                break;
            case 0x6: // 1+2: easy pattern, E
                _split = SplitType.Lateral;
                AddSafeSpot(_safeSpotsClaw, -135.Degrees());
                AddSafeSpot(_safeSpotsMissile, 150.Degrees());
                AddSafeSpot(_safeSpotsMissile, -45.Degrees());
                break;
            case 0x18: // 3+4: easy pattern, W
                _split = SplitType.Lateral;
                AddSafeSpot(_safeSpotsClaw, 135.Degrees());
                AddSafeSpot(_safeSpotsMissile, -150.Degrees());
                AddSafeSpot(_safeSpotsMissile, 45.Degrees());
                break;
            case 0xA: // 1+3: hard pattern, NE + SW
                _split = SplitType.Vertical;
                AddSafeSpot(_safeSpotsClaw, 150.Degrees());
                AddSafeSpot(_safeSpotsMissile, 30.Degrees());
                AddSafeSpot(_safeSpotsMissile, -120.Degrees());
                break;
            case 0x14: // 2+4: hard pattern, NW + SE
                _split = SplitType.Vertical;
                AddSafeSpot(_safeSpotsClaw, -150.Degrees());
                AddSafeSpot(_safeSpotsMissile, -30.Degrees());
                AddSafeSpot(_safeSpotsMissile, 120.Degrees());
                break;
            case 0x12: // 1+4: never seen
            case 0xC: // 2+3: never seen
            default:
                ReportError($"Unexpected bomb pattern: {_pattern}");
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_pattern.Any())
            hints.Add($"Pattern: {_pattern}");
    }

    private WPos NearestCardinal(WPos pos)
    {
        var north = _bombs?.RelativeNorth ?? 180.Degrees();
        if (_split is SplitType.Lateral) // Rotate relative north so later calculations can always assume n/s
            north += 90.Degrees();
        var destAngle = Angle.FromDirection(pos - Module.Center);
        return GetEdgePos(destAngle.DistanceToAngle(north).Abs().Rad < 90.Degrees().Rad ? north : north + 180.Degrees());
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        WPos[] spotProgression = [.. GetAssignedSafeSpots(pcSlot)];
        foreach (var (i, spot) in spotProgression.Index())
        {
            Arena.AddCircle(spot, 1, colorProgression[i]);
        }
        if (spotProgression.Length == 0)
        {
            foreach (var p in _safeSpotsClaw)
                Arena.AddCircle(p, 1, ArenaColor.Enemy);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        WPos[] spotProgression = [.. GetAssignedSafeSpots(slot)];
        var lastPos = actor.Position;
        foreach (var (i, spot) in spotProgression.Index())
        {
            movementHints.Add(lastPos, spot, colorProgression[i]);
            lastPos = spot;
        }
    }

    private WPos GetEdgePos(Angle angle) => Module.Center + 19 * angle.ToDirection();

    private void AddSafeSpot(List<WPos> list, Angle angle) => list.Add(GetEdgePos(angle));

    private IEnumerable<WPos> GetAssignedSafeSpots(int slot)
    {
        if (_fireworks?.TetheredAdds[slot] is { } add && _fireworks?.TetheredStartPositions[slot] is { } start)
        {
            if (_chains?.Tethered(slot) is true)
                yield return Module.Center;
            var safeSpots = (OID)add.OID is OID.NSurprisingClaw or OID.SSurprisingClaw ? _safeSpotsClaw : _safeSpotsMissile;
            var closestSafeSpot = safeSpots.MaxBy(spot => (spot - start).LengthSq()); // Furthest from add starting position
            // If we're still chained, we head cardinal first
            if (_chains?.Tethered(slot) is true || _chains?.Chained(slot) is true)
                yield return NearestCardinal(closestSafeSpot);
            // Finally, return our end position
            yield return closestSafeSpot;
        }
    }
}

class Fireworks2Hints(BossModule module) : BossComponent(module)
{
    private readonly C033SStaticeConfig _config = Service.Config.Get<C033SStaticeConfig>();
    private readonly Fireworks? _fireworks = module.FindComponent<Fireworks>();
    private readonly Dartboard? _dartboard = module.FindComponent<Dartboard>();
    private readonly BurningChains? _chains = module.FindComponent<BurningChains>();
    private readonly FireSpread? _fireSpread = module.FindComponent<FireSpread>();
    private Angle? _relNorth;

    public override void Update()
    {
        if (_relNorth == null && _fireSpread?.Sequences.Count == 3 && _dartboard != null && _dartboard.ForbiddenColor != Dartboard.Color.None)
        {
            // relative north is the direction to fire spread that has two non-forbidden colors at both sides
            _relNorth = _fireSpread.Sequences.FirstOrDefault(s => _dartboard.DirToColor(s.NextRotation - 5.Degrees(), false) != _dartboard.ForbiddenColor && _dartboard.DirToColor(s.NextRotation + 5.Degrees(), false) != _dartboard.ForbiddenColor).NextRotation;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_fireworks?.Spreads.Count > 0)
        {
            // Leave the hint yellow until the chains lock in
            var hintColor = _chains?.Tethered(pcSlot) is true ? ArenaColor.Danger : ArenaColor.Safe;
            foreach (var dir in SafeSpots(pcSlot, pc))
                Arena.AddCircle(Module.Center + 19 * dir.ToDirection(), 1, hintColor);
        }
        else if (_relNorth != null)
        {
            // show rel north before assignments are done
            Arena.AddCircle(Module.Center + 19 * _relNorth.Value.ToDirection(), 1, ArenaColor.Enemy);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        if (_fireworks?.Spreads.Count > 0)
        {
            if (_chains?.Tethered(slot) is true)
            {
                movementHints.Add(actor.Position, Module.Center, ArenaColor.Safe);
                foreach (var dir in SafeSpots(slot, actor))
                    movementHints.Add(Module.Center, Module.Center + 19 * dir.ToDirection(), ArenaColor.Danger);
            }
            else
            {
                foreach (var dir in SafeSpots(slot, actor))
                    movementHints.Add(actor.Position, Module.Center + 19 * dir.ToDirection(), ArenaColor.Safe);
            }
        }
        else if (_relNorth != null)
        {
            // show rel north before assignments are done
            var pos = Module.Center + 19 * _relNorth.Value.ToDirection();
            movementHints.Add(pos, pos, ArenaColor.Enemy);
        }
    }

    private IEnumerable<Angle> SafeSpots(int slot, Actor actor)
    {
        if (_fireworks?.Spreads.Count > 0 && _dartboard != null && _relNorth != null)
        {
            if (_fireworks.IsSpreadTarget(actor))
            {
                // spreads always go slightly S of rel E/W
                bool west = ShouldGoWest(actor);
                yield return _relNorth.Value + (west ? 95 : -95).Degrees();
            }
            else if (!_dartboard.Bullseye[slot])
            {
                // non-dartboard non-spread should just go north
                yield return _relNorth.Value;
            }
            else if (Raid[_dartboard.Bullseye.WithoutBit(slot).LowestSetBit()] is var partner && partner != null)
            {
                bool west = ShouldGoWest(actor);
                if (_fireworks.IsSpreadTarget(partner) && ShouldGoWest(partner) == west)
                    west = !west; // adjust to opposite color
                yield return _relNorth.Value + (west ? 5 : -5).Degrees();
            }
        }
    }

    private bool ShouldGoWest(Actor actor) => _config.Fireworks2Invert ? actor.Class.IsSupport() : actor.Class.IsDD();
}

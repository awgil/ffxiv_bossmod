namespace BossMod.Endwalker.Criterion.C03AAI.C033Statice;

class Fireworks(BossModule module) : Components.UniformStackSpread(module, 3, 20, 2, 2, true)
{
    public Actor?[] TetheredAdds = new Actor?[4];

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
        if ((TetherID)tether.ID == TetherID.Follow && Raid.FindSlot(tether.Target) is var slot && slot >= 0 && slot < TetheredAdds.Length)
            TetheredAdds[slot] = source;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
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
    private BitMask _chains;

    public bool Active => _chains.Any();

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_chains[slot])
            hints.Add("Break chains!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_chains[pcSlot])
        {
            var partner = Raid[_chains.WithoutBit(pcSlot).LowestSetBit()];
            if (partner != null)
                Arena.AddLine(pc.Position, partner.Position, ArenaColor.Safe, 1);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BurningChains)
            _chains.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.BurningChains)
            _chains.Clear(Raid.FindSlot(actor.InstanceID));
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

    public override void OnEventIcon(Actor actor, uint iconID)
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
            Sequences.Add(new() { NextRotation = spell.Rotation, RemainingExplosions = 12, NextActivation = spell.NPCFinishAt });
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

// TODO: assign spread safespots based on initial missile position
class Fireworks1Hints(BossModule module) : BossComponent(module)
{
    private readonly RingARingOExplosions? _bombs = module.FindComponent<RingARingOExplosions>();
    private readonly Fireworks? _fireworks = module.FindComponent<Fireworks>();
    private BitMask _pattern;
    private readonly List<WPos> _safeSpotsClaw = [];
    private readonly List<WPos> _safeSpotsMissile = [];

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
                AddSafeSpot(_safeSpotsClaw, -45.Degrees());
                AddSafeSpot(_safeSpotsMissile, -135.Degrees());
                AddSafeSpot(_safeSpotsMissile, 50.Degrees());
                break;
            case 0x11: // 0+4: easy pattern, N + CCW
                AddSafeSpot(_safeSpotsClaw, 45.Degrees());
                AddSafeSpot(_safeSpotsMissile, 135.Degrees());
                AddSafeSpot(_safeSpotsMissile, -50.Degrees());
                break;
            case 0x5: // 0+2: hard pattern, N + CW
                AddSafeSpot(_safeSpotsClaw, -135.Degrees());
                AddSafeSpot(_safeSpotsMissile, 125.Degrees());
                AddSafeSpot(_safeSpotsMissile, -35.Degrees());
                break;
            case 0x9: // 0+3: hard pattern, N + CCW
                AddSafeSpot(_safeSpotsClaw, 135.Degrees());
                AddSafeSpot(_safeSpotsMissile, -125.Degrees());
                AddSafeSpot(_safeSpotsMissile, 35.Degrees());
                break;
            case 0x6: // 1+2: easy pattern, E
                AddSafeSpot(_safeSpotsClaw, -135.Degrees());
                AddSafeSpot(_safeSpotsMissile, 150.Degrees());
                AddSafeSpot(_safeSpotsMissile, -45.Degrees());
                break;
            case 0x18: // 3+4: easy pattern, W
                AddSafeSpot(_safeSpotsClaw, 135.Degrees());
                AddSafeSpot(_safeSpotsMissile, -150.Degrees());
                AddSafeSpot(_safeSpotsMissile, 45.Degrees());
                break;
            case 0xA: // 1+3: hard pattern, NE + SW
                AddSafeSpot(_safeSpotsClaw, 150.Degrees());
                AddSafeSpot(_safeSpotsMissile, 30.Degrees());
                AddSafeSpot(_safeSpotsMissile, -120.Degrees());
                break;
            case 0x14: // 2+4: hard pattern, NW + SE
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

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_fireworks?.TetheredAdds[pcSlot] is var add && add != null)
        {
            var safeSpots = (OID)add.OID is OID.NSurprisingClaw or OID.SSurprisingClaw ? _safeSpotsClaw : _safeSpotsMissile;
            foreach (var p in safeSpots)
                Arena.AddCircle(p, 1, ArenaColor.Safe);
        }
        else
        {
            foreach (var p in _safeSpotsClaw)
                Arena.AddCircle(p, 1, ArenaColor.Enemy);
        }
    }

    private void AddSafeSpot(List<WPos> list, Angle angle) => list.Add(Module.Center + 19 * angle.ToDirection());
}

// TODO: currently this assumes that DD always go rel-west, supports rel-east
class Fireworks2Hints(BossModule module) : BossComponent(module)
{
    private readonly Fireworks? _fireworks = module.FindComponent<Fireworks>();
    private readonly Dartboard? _dartboard = module.FindComponent<Dartboard>();
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
            foreach (var dir in SafeSpots(pcSlot, pc))
                Arena.AddCircle(Module.Center + 19 * dir.ToDirection(), 1, ArenaColor.Safe);
        }
        else if (_relNorth != null)
        {
            // show rel north before assignments are done
            Arena.AddCircle(Module.Center + 19 * _relNorth.Value.ToDirection(), 1, ArenaColor.Enemy);
        }
    }

    private IEnumerable<Angle> SafeSpots(int slot, Actor actor)
    {
        if (_fireworks?.Spreads.Count > 0 && _dartboard != null && _relNorth != null)
        {
            if (_fireworks.IsSpreadTarget(actor))
            {
                // spreads always go slightly S of rel E/W
                bool west = actor.Class.IsDD(); // note: this is arbitrary
                yield return _relNorth.Value + (west ? 95 : -95).Degrees();
            }
            else if (!_dartboard.Bullseye[slot])
            {
                // non-dartboard non-spread should just go north
                yield return _relNorth.Value;
            }
            else if (Raid[_dartboard.Bullseye.WithoutBit(slot).LowestSetBit()] is var partner && partner != null)
            {
                bool west = actor.Class.IsDD(); // note: this is arbitrary
                if (_fireworks.IsSpreadTarget(partner) && partner.Class.IsDD() == west)
                    west = !west; // adjust to opposite color
                yield return _relNorth.Value + (west ? 5 : -5).Degrees();
            }
        }
    }
}

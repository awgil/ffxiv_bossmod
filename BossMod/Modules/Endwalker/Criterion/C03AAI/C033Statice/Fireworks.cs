namespace BossMod.Endwalker.VariantCriterion.C03AAI.C033Statice;

class Fireworks(BossModule module) : Components.UniformStackSpread(module, 3f, 20f, 2, 2)
{
    public Actor?[] TetheredAdds = new Actor?[4];

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (TetheredAdds[slot] is var add && add != null)
            hints.Add($"Tether: {(add.OID is (uint)OID.NSurprisingMissile or (uint)OID.SSurprisingMissile ? "missile" : "claw")}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (TetheredAdds[pcSlot] is var add && add != null)
        {
            Arena.Actor(add, Colors.Object, true);
            Arena.AddLine(add.Position, pc.Position);
        }
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.Follow && Raid.FindSlot(tether.Target) is var slot && slot >= 0 && slot < TetheredAdds.Length)
            TetheredAdds[slot] = source;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        switch (iconID)
        {
            case (uint)IconID.FireworksSpread:
                AddSpread(actor, WorldState.FutureTime(9.1d));
                break;
            case (uint)IconID.FireworksEnumeration:
                AddStack(actor, WorldState.FutureTime(8.2d));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NFireworksStack:
            case (uint)AID.SFireworksStack:
                Stacks.Clear();
                break;
            case (uint)AID.NFireworksSpread:
            case (uint)AID.SFireworksSpread:
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
                Arena.AddLine(pc.Position, partner.Position, Colors.Safe, 1f);
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BurningChains)
            _chains[Raid.FindSlot(actor.InstanceID)] = true;
    }

    public override void OnStatusLose(Actor actor, ref ActorStatus status)
    {
        if (status.ID == (uint)SID.BurningChains)
            _chains[Raid.FindSlot(actor.InstanceID)] = false;
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

    private static readonly AOEShapeRect _shape = new(20f, 2.5f, -8f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = 0;

        var countS = Sequences.Count;
        for (var i = 0; i < countS; ++i)
        {
            var s = Sequences[i];
            count += Math.Min(s.RemainingExplosions, 3);
        }

        if (count == 0)
            return [];

        var aoes = new AOEInstance[count];
        var index = 0;
        var center = Arena.Center.Quantized();
        for (var i = 0; i < countS; ++i)
        {
            var s = Sequences[i];
            var rot = s.NextRotation + _rotation;
            var act = s.NextActivation.AddSeconds(1.1d);
            var max = Math.Min(s.RemainingExplosions, 3);
            for (var j = 1; j < max; ++j)
            {
                aoes[index++] = new(_shape, center, rot, act);
                rot += _rotation;
                act = act.AddSeconds(1.1d);
            }
            if (s.RemainingExplosions > 0)
            {
                aoes[index++] = new(_shape, center, s.NextRotation, s.NextActivation, Colors.Danger);
            }
        }
        return aoes;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var rot = iconID switch
        {
            (uint)IconID.RotateCW => -10f.Degrees(),
            (uint)IconID.RotateCCW => 10f.Degrees(),
            _ => default
        };
        if (rot != default)
            _rotation = rot;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NFireSpreadFirst or (uint)AID.SFireSpreadFirst)
            Sequences.Add(new() { NextRotation = spell.Rotation, RemainingExplosions = 12, NextActivation = Module.CastFinishAt(spell) });
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.NFireSpreadFirst or (uint)AID.NFireSpreadRest or (uint)AID.SFireSpreadFirst or (uint)AID.SFireSpreadRest)
        {
            ++NumCasts;
            var index = Sequences.FindIndex(s => s.NextRotation.AlmostEqual(caster.Rotation, 0.1f));
            if (index >= 0)
            {
                ref var s = ref Sequences.AsSpan()[index];
                if (--s.RemainingExplosions > 0)
                {
                    s.NextRotation += _rotation;
                    s.NextActivation = WorldState.FutureTime(1.1d);
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
            var offset = b.Position - Arena.Center;
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
                AddSafeSpot(_safeSpotsClaw, -45f.Degrees());
                AddSafeSpot(_safeSpotsMissile, -135f.Degrees());
                AddSafeSpot(_safeSpotsMissile, 50f.Degrees());
                break;
            case 0x11: // 0+4: easy pattern, N + CCW
                AddSafeSpot(_safeSpotsClaw, 45f.Degrees());
                AddSafeSpot(_safeSpotsMissile, 135f.Degrees());
                AddSafeSpot(_safeSpotsMissile, -50f.Degrees());
                break;
            case 0x5: // 0+2: hard pattern, N + CW
                AddSafeSpot(_safeSpotsClaw, -135f.Degrees());
                AddSafeSpot(_safeSpotsMissile, 125f.Degrees());
                AddSafeSpot(_safeSpotsMissile, -35f.Degrees());
                break;
            case 0x9: // 0+3: hard pattern, N + CCW
                AddSafeSpot(_safeSpotsClaw, 135f.Degrees());
                AddSafeSpot(_safeSpotsMissile, -125f.Degrees());
                AddSafeSpot(_safeSpotsMissile, 35f.Degrees());
                break;
            case 0x6: // 1+2: easy pattern, E
                AddSafeSpot(_safeSpotsClaw, -135f.Degrees());
                AddSafeSpot(_safeSpotsMissile, 150f.Degrees());
                AddSafeSpot(_safeSpotsMissile, -45f.Degrees());
                break;
            case 0x18: // 3+4: easy pattern, W
                AddSafeSpot(_safeSpotsClaw, 135f.Degrees());
                AddSafeSpot(_safeSpotsMissile, -150f.Degrees());
                AddSafeSpot(_safeSpotsMissile, 45f.Degrees());
                break;
            case 0xA: // 1+3: hard pattern, NE + SW
                AddSafeSpot(_safeSpotsClaw, 150f.Degrees());
                AddSafeSpot(_safeSpotsMissile, 30f.Degrees());
                AddSafeSpot(_safeSpotsMissile, -120f.Degrees());
                break;
            case 0x14: // 2+4: hard pattern, NW + SE
                AddSafeSpot(_safeSpotsClaw, -150f.Degrees());
                AddSafeSpot(_safeSpotsMissile, -30f.Degrees());
                AddSafeSpot(_safeSpotsMissile, 120f.Degrees());
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
            var safeSpots = add.OID is (uint)OID.NSurprisingClaw or (uint)OID.SSurprisingClaw ? _safeSpotsClaw : _safeSpotsMissile;
            foreach (var p in safeSpots)
                Arena.ZoneCircleOutline(p, 1f, Colors.Safe);
        }
        else
        {
            foreach (var p in _safeSpotsClaw)
                Arena.ZoneCircleOutline(p, 1f, Colors.Enemy);
        }
    }

    private void AddSafeSpot(List<WPos> list, Angle angle) => list.Add(Arena.Center + 19f * angle.ToDirection());
}

class Fireworks2Hints(BossModule module) : BossComponent(module)
{
    private readonly C033SStaticeConfig _config = Service.Config.Get<C033SStaticeConfig>();
    private readonly Fireworks? _fireworks = module.FindComponent<Fireworks>();
    private readonly Dartboard? _dartboard = module.FindComponent<Dartboard>();
    private readonly FireSpread? _fireSpread = module.FindComponent<FireSpread>();
    private Angle? _relNorth;

    public override void Update()
    {
        if (_relNorth == null && _fireSpread?.Sequences.Count == 3 && _dartboard != null && _dartboard.ForbiddenColor != Dartboard.Color.None)
        {
            // relative north is the direction to fire spread that has two non-forbidden colors at both sides
            _relNorth = _fireSpread.Sequences.FirstOrDefault(s => _dartboard.DirToColor(s.NextRotation - 5f.Degrees(), false) != _dartboard.ForbiddenColor && _dartboard.DirToColor(s.NextRotation + 5.Degrees(), false) != _dartboard.ForbiddenColor).NextRotation;
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_fireworks?.Spreads.Count > 0)
        {
            foreach (var dir in SafeSpots(pcSlot, pc))
                Arena.ZoneCircleOutline(Arena.Center + 19f * dir.ToDirection(), 1f, Colors.Safe);
        }
        else if (_relNorth != null)
        {
            // show rel north before assignments are done
            Arena.ZoneCircleOutline(Arena.Center + 19f * _relNorth.Value.ToDirection(), 1f, Colors.Enemy);
        }
    }

    private Angle[] SafeSpots(int slot, Actor actor)
    {
        if (_fireworks?.Spreads.Count > 0 && _dartboard != null && _relNorth != null)
        {
            if (_fireworks.IsSpreadTarget(actor))
            {
                // spreads always go slightly S of rel E/W
                var west = ShouldGoWest(actor);
                return [_relNorth.Value + (west ? 95f : -95f).Degrees()];
            }
            else if (!_dartboard.Bullseye[slot])
            {
                // non-dartboard non-spread should just go north
                return [_relNorth.Value];
            }
            else if (Raid[_dartboard.Bullseye.WithoutBit(slot).LowestSetBit()] is var partner && partner != null)
            {
                var west = ShouldGoWest(actor);
                if (_fireworks.IsSpreadTarget(partner) && ShouldGoWest(partner) == west)
                    west = !west; // adjust to opposite color
                return [_relNorth.Value + (west ? 5f : -5f).Degrees()];
            }
        }
        return [];
    }

    private bool ShouldGoWest(Actor actor) => _config.Fireworks2Invert ? actor.Class.IsSupport() : actor.Class.IsDD();
}

namespace BossMod.Endwalker.Ultimate.TOP;

// note: this is all very tied to LPDU strat
class P5Sigma(BossModule module) : BossComponent(module)
{
    public enum Glitch { Unknown, Mid, Remote }

    public struct PlayerState
    {
        public int Order;
        public int PartnerSlot;
        public bool WaveCannonTarget;
        public Angle SpreadAngle;
    }

    public Glitch ActiveGlitch;
    public PlayerState[] Players = Utils.MakeArray(PartyState.MaxPartySize, new PlayerState() { PartnerSlot = -1 });
    private WDir _waveCannonNorthDir;
    private int _numWaveCannonTargets;
    private bool _waveCannonsDone;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var ps = Players[slot];
        if (ps.Order > 0)
            hints.Add($"Order: {ps.Order}", false);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (ActiveGlitch != Glitch.Unknown)
            hints.Add($"Glitch: {ActiveGlitch}");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var partner = Raid[Players[pcSlot].PartnerSlot];
        if (partner != null)
        {
            var distSq = (partner.Position - pc.Position).LengthSq();
            var range = DistanceRange;
            Arena.AddLine(pc.Position, partner.Position, distSq < range.min * range.min || distSq > range.max * range.max ? ArenaColor.Danger : ArenaColor.Safe);
        }

        foreach (var safeSpot in SafeSpotOffsets(pcSlot))
            Arena.AddCircle(Module.Center + safeSpot, 1, ArenaColor.Safe);
    }

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PartySynergy)
        {
            if (Raid.TryFindSlot(source.InstanceID, out var s1) && Raid.TryFindSlot(tether.Target, out var s2))
            {
                Players[s1].PartnerSlot = s2;
                Players[s2].PartnerSlot = s1;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        switch ((SID)status.ID)
        {
            case SID.MidGlitch:
                ActiveGlitch = Glitch.Mid;
                break;
            case SID.RemoteGlitch:
                ActiveGlitch = Glitch.Remote;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SigmaWaveCannonAOE)
            _waveCannonsDone = true;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (!Raid.TryFindSlot(actor, out var slot))
            return;

        // assuming standard 'blue-purple-orange-green' order
        var order = (IconID)iconID switch
        {
            IconID.PartySynergyCross => 1,
            IconID.PartySynergySquare => 2,
            IconID.PartySynergyCircle => 3,
            IconID.PartySynergyTriangle => 4,
            _ => 0
        };
        if (order > 0)
        {
            Players[slot].Order = order;
        }
        else if (iconID == (uint)IconID.SigmaWaveCannon)
        {
            Players[slot].WaveCannonTarget = true;
            if (++_numWaveCannonTargets == 6)
                InitSpreadPositions();
        }
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        switch ((OID)actor.OID)
        {
            case OID.RightArmUnit: // TODO: can it be left unit instead?..
                if (id == 0x1E43)
                    _waveCannonNorthDir -= actor.Position - Module.Center;
                break;
            case OID.BossP5:
                if (id == 0x1E43)
                    _waveCannonNorthDir = actor.Position - Module.Center; // just in case...
                break;
        }
    }

    private (float min, float max) DistanceRange => ActiveGlitch switch
    {
        Glitch.Mid => (20, 26),
        Glitch.Remote => (34, 50),
        _ => (0, 50)
    };

    private void InitSpreadPositions()
    {
        var northAngle = Angle.FromDirection(_waveCannonNorthDir);
        var waveCannonsPerPair = new BitMask[4];
        for (int i = 0; i < Players.Length; ++i)
        {
            var ps = Players[i];
            if (ps.WaveCannonTarget && ps.Order > 0)
                waveCannonsPerPair[ps.Order - 1].Set(i);
        }
        int nextSingle = 0;
        int nextDouble = 0;
        foreach (var mask in waveCannonsPerPair)
        {
            if (mask.NumSetBits() == 2)
            {
                var s1 = mask.LowestSetBit();
                var s2 = mask.HighestSetBit();
                var dir = (Raid[s2]?.Position ?? default) - (Raid[s1]?.Position ?? default); // s1 to s2
                if (_waveCannonNorthDir.OrthoL().Dot(dir) > 0)
                    Utils.Swap(ref s1, ref s2); // s1 is now N/W, s2 is S/E
                if (nextSingle == 0)
                {
                    Players[s1].SpreadAngle = northAngle;
                    Players[s2].SpreadAngle = northAngle + 180.Degrees();
                }
                else
                {
                    Players[s1].SpreadAngle = northAngle + 90.Degrees();
                    Players[s2].SpreadAngle = northAngle - 90.Degrees();
                }
                ++nextSingle;
            }
            else
            {
                var s1 = mask.LowestSetBit();
                var s2 = Players[s1].PartnerSlot;
                if (nextDouble == 0)
                {
                    Players[s1].SpreadAngle = northAngle - 135.Degrees();
                    Players[s2].SpreadAngle = northAngle + 45.Degrees();
                }
                else
                {
                    Players[s1].SpreadAngle = northAngle + 135.Degrees();
                    Players[s2].SpreadAngle = northAngle - 45.Degrees();
                }
                ++nextDouble;
            }
        }

        foreach (ref var p in Players.AsSpan())
            p.SpreadAngle = p.SpreadAngle.Normalized();
    }

    private IEnumerable<WDir> SafeSpotOffsets(int slot)
    {
        var p = Players[slot];
        if (_waveCannonNorthDir == default)
            yield break; // no safe spots yet

        if (_numWaveCannonTargets < 6)
        {
            var dir = _waveCannonNorthDir.Normalized();
            yield return (20 - 2.5f * p.Order) * dir + 2 * dir.OrthoL();
            yield return (20 - 2.5f * p.Order) * dir + 2 * dir.OrthoR();
            yield break;
        }

        if (!_waveCannonsDone)
        {
            yield return (ActiveGlitch == Glitch.Mid ? 11 : 19) * p.SpreadAngle.ToDirection();
            yield break;
        }
    }
}

class P5SigmaHyperPulse(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(100, 3), (uint)TetherID.SigmaHyperPulse, AID.SigmaHyperPulse)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in CurrentBaits)
            Arena.Actor(b.Source, ArenaColor.Object, true);
        base.DrawArenaForeground(pcSlot, pc);
    }
}

class P5SigmaWaveCannon(BossModule module) : Components.GenericBaitAway(module, AID.SigmaWaveCannonAOE)
{
    private BitMask _waveCannonTargets;

    private static readonly AOEShapeCone _shapeWaveCannon = new(100, 22.5f.Degrees()); // TODO: verify angle

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SigmaWaveCannon)
            foreach (var p in Raid.WithSlot(true).IncludedInMask(_waveCannonTargets).Actors())
                CurrentBaits.Add(new(caster, p, _shapeWaveCannon));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SigmaWaveCannon)
            _waveCannonTargets.Set(Raid.FindSlot(actor.InstanceID));
    }
}

class P5SigmaTowers(BossModule module) : Components.GenericTowers(module)
{
    private int _soakerSum;

    public override void OnActorCreated(Actor actor)
    {
        var numSoakers = (OID)actor.OID switch
        {
            OID.Tower2 => 1,
            OID.Tower3 => 2,
            _ => 0,
        };
        if (numSoakers == 0)
            return;

        Towers.Add(new(actor.Position, 3, numSoakers, numSoakers));
        _soakerSum += numSoakers;
        if (_soakerSum == PartyState.MaxPartySize)
            InitAssignments();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.StorageViolation1 or AID.StorageViolation2 or AID.StorageViolationObliteration)
        {
            ++NumCasts;
            Towers.Clear();
        }
    }

    private void InitAssignments()
    {
        var sigma = Module.FindComponent<P5Sigma>();
        if (sigma == null)
            return;

        WDir relNorth = default;
        foreach (var t in Towers)
            relNorth -= t.Position - Module.Center;

        foreach (ref var tower in Towers.AsSpan())
        {
            var offset = tower.Position - Module.Center;
            var left = relNorth.OrthoL().Dot(offset) > 0;
            if (Towers.Count == 5)
            {
                // 1's are rel W (A) and E (D), 2's are N (12), SW (B3), SE (C4)
                if (tower.MinSoakers == 1)
                    AssignPlayers(sigma, ref tower, (left ? 180 : -90).Degrees()); // A/D
                else if (relNorth.Dot(offset) > 0)
                    AssignPlayers(sigma, ref tower, 135.Degrees(), 45.Degrees()); // 1/2
                else if (left)
                    AssignPlayers(sigma, ref tower, 90.Degrees(), -45.Degrees()); // B/3
                else
                    AssignPlayers(sigma, ref tower, 0.Degrees(), -135.Degrees()); // C/4
            }
            else
            {
                // 1's are NW (1), NE (2), SW (B), SE (C), 2's are W (A3) and E (D4)
                if (tower.MinSoakers == 2)
                    AssignPlayers(sigma, ref tower, (left ? 180 : -90).Degrees(), (left ? -45 : -135).Degrees()); // A3/D4
                else if (relNorth.Dot(offset) > 0)
                    AssignPlayers(sigma, ref tower, (left ? 135 : 45).Degrees()); // 1/2
                else
                    AssignPlayers(sigma, ref tower, (left ? 90 : 0).Degrees()); // B/C
            }
        }
    }

    private void AssignPlayers(P5Sigma sigma, ref Tower tower, params Angle[] angles)
    {
        for (int i = 0; i < sigma.Players.Length; ++i)
            if (!angles.Any(a => a.AlmostEqual(sigma.Players[i].SpreadAngle, 0.1f)))
                tower.ForbiddenSoakers.Set(i);
    }
}

class P5SigmaRearLasers(BossModule module) : Components.GenericAOEs(module)
{
    public Angle StartingDir { get; private set; }
    public Angle Rotation { get; private set; }
    private DateTime _activation;

    private static readonly AOEShapeRect _shape = new(25, 6, 25);

    public bool Active => Rotation != default;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!Active)
            yield break;
        for (int i = NumCasts + 1; i < 14; ++i)
            yield return new(_shape, Module.Center, StartingDir + i * Rotation, _activation.AddSeconds(0.6 * i), Risky: false);
        if (NumCasts < 14)
            yield return new(_shape, Module.Center, StartingDir + NumCasts * Rotation, _activation.AddSeconds(0.6 * NumCasts), ArenaColor.Danger);
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((OID)actor.OID != OID.RearPowerUnit)
            return;
        var rot = (IconID)iconID switch
        {
            IconID.RotateCW => -9.Degrees(),
            IconID.RotateCCW => 9.Degrees(),
            _ => default
        };
        if (rot == default)
            return;
        StartingDir = actor.Rotation;
        Rotation = rot;
        _activation = WorldState.FutureTime(10.1f);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.RearLasersFirst or AID.RearLasersRest)
            ++NumCasts;
    }
}

class P5SigmaDoubleAOEs(BossModule module) : Components.GenericAOEs(module)
{
    public bool Show;
    public List<AOEInstance> AOEs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Show ? AOEs : Enumerable.Empty<AOEInstance>();

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SuperliminalSteel or AID.OptimizedBlizzard)
            ++NumCasts;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E43 || (OID)actor.OID != OID.BossP5)
            return;
        if (actor.ModelState.ModelState == 4)
        {
            AOEs.Add(new(new AOEShapeRect(40, 40, -4), actor.Position, actor.Rotation + 90.Degrees(), WorldState.FutureTime(15.1f)));
            AOEs.Add(new(new AOEShapeRect(40, 40, -4), actor.Position, actor.Rotation - 90.Degrees(), WorldState.FutureTime(15.1f)));
        }
        else
        {
            AOEs.Add(new(new AOEShapeCross(100, 5), actor.Position, actor.Rotation, WorldState.FutureTime(15.1f)));
            Show = true; // cross can be shown from the start
        }
    }
}

class P5SigmaNearDistantWorld(BossModule module) : P5NearDistantWorld(module)
{
    private readonly P5SigmaRearLasers? _lasers = module.FindComponent<P5SigmaRearLasers>();
    private BitMask _dynamisStacks;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pcSlot, pc))
            Arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        base.OnStatusGain(actor, status);
        if ((SID)status.ID == SID.QuickeningDynamis)
            _dynamisStacks.Set(Raid.FindSlot(actor.InstanceID));
    }

    private IEnumerable<WPos> SafeSpots(int slot, Actor actor)
    {
        if (_lasers == null) // note: we assume StartingDir is relative south, Rotation is +- 9 degrees
            yield break;

        if (actor == NearWorld)
        {
            yield return Module.Center + 10 * (_lasers.StartingDir + 10 * _lasers.Rotation).ToDirection();
        }
        else if (actor == DistantWorld)
        {
            yield return Module.Center + 10 * _lasers.StartingDir.ToDirection();
        }
        else
        {
            // TODO: figure out a way to assign safespots - for now, assume no-dynamis always go south (and so can be second far baiters or any near baiters), dynamis can go anywhere
            yield return Module.Center + 19 * _lasers.StartingDir.ToDirection(); // '4' - second far bait spot
            yield return Module.Center + 19 * (_lasers.StartingDir + 9 * _lasers.Rotation).ToDirection(); // '2' - first near bait spot
            yield return Module.Center + 19 * (_lasers.StartingDir + 11 * _lasers.Rotation).ToDirection(); // '3' - second near bait spot
            if (_dynamisStacks[slot])
            {
                yield return Module.Center - 19 * _lasers.StartingDir.ToDirection(); // '1' - first far bait spot
                yield return Module.Center - 19 * (_lasers.StartingDir + 5 * _lasers.Rotation).ToDirection(); // first (far) laser bait spot
                yield return Module.Center - 19 * (_lasers.StartingDir - 5 * _lasers.Rotation).ToDirection(); // second (stay) laser bait spot
            }
        }
    }
}

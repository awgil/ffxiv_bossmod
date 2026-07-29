namespace BossMod.Endwalker.Ultimate.TOP;

// note: this is all very tied to LPDU strat
sealed class P5Sigma(BossModule module) : BossComponent(module)
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
            Arena.AddLine(pc.Position, partner.Position, distSq < range.min * range.min || distSq > range.max * range.max ? 0 : Colors.Safe);
        }

        foreach (var safeSpot in SafeSpotOffsets(pcSlot))
            Arena.ZoneCircleOutline(Arena.Center + safeSpot, 1, Colors.Safe);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (tether.ID == (uint)TetherID.PartySynergy)
        {
            var s1 = Raid.FindSlot(source.InstanceID);
            var s2 = Raid.FindSlot(tether.Target);
            if (s1 >= 0 && s2 >= 0)
            {
                Players[s1].PartnerSlot = s2;
                Players[s2].PartnerSlot = s1;
            }
        }
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        switch (status.ID)
        {
            case (uint)SID.MidGlitch:
                ActiveGlitch = Glitch.Mid;
                break;
            case (uint)SID.RemoteGlitch:
                ActiveGlitch = Glitch.Remote;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID == (uint)AID.SigmaWaveCannonAOE)
            _waveCannonsDone = true;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var slot = Raid.FindSlot(actor.InstanceID);
        if (slot < 0)
            return;

        // assuming standard 'blue-purple-orange-green' order
        var order = iconID switch
        {
            (uint)IconID.PartySynergyCross => 1,
            (uint)IconID.PartySynergySquare => 2,
            (uint)IconID.PartySynergyCircle => 3,
            (uint)IconID.PartySynergyTriangle => 4,
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
        switch (actor.OID)
        {
            case (uint)OID.RightArmUnit: // TODO: can it be left unit instead?..
                if (id == 0x1E43)
                    _waveCannonNorthDir -= actor.Position - Arena.Center;
                break;
            case (uint)OID.BossP5:
                if (id == 0x1E43)
                    _waveCannonNorthDir = actor.Position - Arena.Center; // just in case...
                break;
        }
    }

    private (float min, float max) DistanceRange => ActiveGlitch switch
    {
        Glitch.Mid => (20f, 26f),
        Glitch.Remote => (34f, 50f),
        _ => (0f, 50f)
    };

    private void InitSpreadPositions()
    {
        var northAngle = Angle.FromDirection(_waveCannonNorthDir);
        var waveCannonsPerPair = new BitMask[4];
        for (var i = 0; i < Players.Length; ++i)
        {
            var ps = Players[i];
            if (ps.WaveCannonTarget && ps.Order > 0)
                waveCannonsPerPair[ps.Order - 1].Set(i);
        }
        var nextSingle = 0;
        var nextDouble = 0;
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
                    Players[s2].SpreadAngle = northAngle + 180f.Degrees();
                }
                else
                {
                    Players[s1].SpreadAngle = northAngle + 90f.Degrees();
                    Players[s2].SpreadAngle = northAngle - 90f.Degrees();
                }
                ++nextSingle;
            }
            else
            {
                var s1 = mask.LowestSetBit();
                var s2 = Players[s1].PartnerSlot;
                if (nextDouble == 0)
                {
                    Players[s1].SpreadAngle = northAngle - 135f.Degrees();
                    Players[s2].SpreadAngle = northAngle + 45f.Degrees();
                }
                else
                {
                    Players[s1].SpreadAngle = northAngle + 135f.Degrees();
                    Players[s2].SpreadAngle = northAngle - 45f.Degrees();
                }
                ++nextDouble;
            }
        }

        foreach (ref var p in Players.AsSpan())
            p.SpreadAngle = p.SpreadAngle.Normalized();
    }

    private List<WDir> SafeSpotOffsets(int slot)
    {
        var p = Players[slot];
        if (_waveCannonNorthDir == default)
            return []; // no safe spots yet

        if (_numWaveCannonTargets < 6)
        {
            var safespots = new List<WDir>(2);
            var dir = _waveCannonNorthDir.Normalized();
            var offset = (20f - 2.5f * p.Order) * dir;
            safespots.Add(offset + 2f * dir.OrthoL());
            safespots.Add(offset + 2f * dir.OrthoR());
            return safespots;
        }

        if (!_waveCannonsDone)
        {
            return [(ActiveGlitch == Glitch.Mid ? 11f : 19f) * p.SpreadAngle.ToDirection()];
        }
        return [];
    }
}

sealed class P5SigmaHyperPulse(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(100f, 3f), (uint)TetherID.SigmaHyperPulse, (uint)AID.SigmaHyperPulse)
{
    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var b in CurrentBaits)
            Arena.Actor(b.Source, Colors.Object, true);
        base.DrawArenaForeground(pcSlot, pc);
    }
}

sealed class P5SigmaWaveCannon(BossModule module) : Components.GenericBaitAway(module, (uint)AID.SigmaWaveCannonAOE)
{
    private BitMask _waveCannonTargets;

    private static readonly AOEShapeCone _shapeWaveCannon = new(100f, 22.5f.Degrees()); // TODO: verify angle

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.SigmaWaveCannon)
            foreach (var p in Raid.WithSlot(true, true, true).IncludedInMask(_waveCannonTargets).Actors())
                CurrentBaits.Add(new(caster, p, _shapeWaveCannon));
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.SigmaWaveCannon)
            _waveCannonTargets.Set(Raid.FindSlot(actor.InstanceID));
    }
}

sealed class P5SigmaTowers(BossModule module) : Components.GenericTowers(module)
{
    private int _soakerSum;

    public override void OnActorCreated(Actor actor)
    {
        var numSoakers = actor.OID switch
        {
            (uint)OID.Tower2 => 1,
            (uint)OID.Tower3 => 2,
            _ => 0,
        };
        if (numSoakers == 0)
            return;

        Towers.Add(new(actor.Position, 3f, numSoakers, numSoakers));
        _soakerSum += numSoakers;
        if (_soakerSum == PartyState.MaxPartySize)
            InitAssignments();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.StorageViolation1 or (uint)AID.StorageViolation2 or (uint)AID.StorageViolationObliteration)
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
            relNorth -= t.Position - Arena.Center;

        foreach (ref var tower in Towers.AsSpan())
        {
            var offset = tower.Position - Arena.Center;
            var left = relNorth.OrthoL().Dot(offset) > 0;
            if (Towers.Count == 5)
            {
                // 1's are rel W (A) and E (D), 2's are N (12), SW (B3), SE (C4)
                if (tower.MinSoakers == 1)
                    AssignPlayers(sigma, ref tower, (left ? 180f : -90f).Degrees()); // A/D
                else if (relNorth.Dot(offset) > 0)
                    AssignPlayers(sigma, ref tower, 135f.Degrees(), 45f.Degrees()); // 1/2
                else if (left)
                    AssignPlayers(sigma, ref tower, 90f.Degrees(), -45f.Degrees()); // B/3
                else
                    AssignPlayers(sigma, ref tower, default, -135f.Degrees()); // C/4
            }
            else
            {
                // 1's are NW (1), NE (2), SW (B), SE (C), 2's are W (A3) and E (D4)
                if (tower.MinSoakers == 2)
                    AssignPlayers(sigma, ref tower, (left ? 180f : -90f).Degrees(), (left ? -45f : -135f).Degrees()); // A3/D4
                else if (relNorth.Dot(offset) > 0)
                    AssignPlayers(sigma, ref tower, (left ? 135f : 45f).Degrees()); // 1/2
                else
                    AssignPlayers(sigma, ref tower, (left ? 90f : default).Degrees()); // B/C
            }
        }
    }

    private static void AssignPlayers(P5Sigma sigma, ref Tower tower, params Angle[] angles)
    {
        var len = sigma.Players.Length;
        var len2 = angles.Length;
        for (var i = 0; i < len; ++i)
        {
            var found = false;
            ref readonly var sigmaP = ref sigma.Players[i].SpreadAngle;
            for (var j = 0; j < len2; ++j)
            {
                if (angles[j].AlmostEqual(sigmaP, 0.1f))
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                tower.ForbiddenSoakers[i] = true;
        }
    }
}

sealed class P5SigmaRearLasers(BossModule module) : Components.GenericRotatingAOE(module)
{
    private static readonly AOEShapeRect _shape = new(25f, 6f, 25f);
    public Angle StartingDir;
    public Angle Rotation;
    public bool Active => Sequences.Count != 0;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor.OID != (uint)OID.RearPowerUnit)
            return;
        var rot = iconID switch
        {
            (uint)IconID.RotateCW => -9f.Degrees(),
            (uint)IconID.RotateCCW => 9f.Degrees(),
            _ => default
        };
        if (rot == default)
            return;
        StartingDir = actor.Rotation;
        Rotation = rot;
        Sequences.Add(new(_shape, actor.Position, actor.Rotation, rot, WorldState.FutureTime(10.1d), 0.6d, 14, 9));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.RearLasersFirst or (uint)AID.RearLasersRest)
            AdvanceSequence(0, WorldState.CurrentTime);
    }
}

sealed class P5SigmaDoubleAOEs(BossModule module) : Components.GenericAOEs(module)
{
    public bool Show;
    public List<AOEInstance> AOEs = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => Show ? CollectionsMarshal.AsSpan(AOEs) : [];

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action.ID is (uint)AID.SuperliminalSteel or (uint)AID.OptimizedBlizzard)
            ++NumCasts;
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if (id != 0x1E43 || actor.OID != (uint)OID.BossP5)
            return;
        var pos = actor.Position.Quantized();
        var rot = actor.Rotation;
        var act = WorldState.FutureTime(15.1d);
        if (actor.ModelState.ModelState == 4)
        {
            AOEs.Add(new(P5OmegaDoubleAOEs.Shapes[2], pos, rot + 90f.Degrees(), act));
            AOEs.Add(new(P5OmegaDoubleAOEs.Shapes[2], pos, rot - 90f.Degrees(), act));
        }
        else
        {
            AOEs.Add(new(P5OmegaDoubleAOEs.Shapes[3], pos, rot, act));
            Show = true; // cross can be shown from the start
        }
    }
}

sealed class P5SigmaNearDistantWorld(BossModule module) : P5NearDistantWorld(module)
{
    private readonly P5SigmaRearLasers? _lasers = module.FindComponent<P5SigmaRearLasers>();
    private BitMask _dynamisStacks;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        foreach (var p in SafeSpots(pcSlot, pc))
            Arena.ZoneCircleOutline(p, 1f, Colors.Safe);
    }

    public override void OnStatusGain(Actor actor, ref ActorStatus status)
    {
        base.OnStatusGain(actor, ref status);
        if (status.ID == (uint)SID.QuickeningDynamis)
            _dynamisStacks[Raid.FindSlot(actor.InstanceID)] = true;
    }

    private List<WPos> SafeSpots(int slot, Actor actor)
    {
        if (_lasers == null) // note: we assume StartingDir is relative south, Rotation is +- 9 degrees
            return [];
        var center = Arena.Center;
        var startdir = _lasers.StartingDir;
        var rot = _lasers.Rotation;
        if (actor == NearWorld)
        {
            return [center + 10f * (startdir + 10f * rot).ToDirection()];
        }
        else if (actor == DistantWorld)
        {
            return [center + 10f * startdir.ToDirection()];
        }
        else
        {
            var safespots = new List<WPos>
            {
                // TODO: figure out a way to assign safespots - for now, assume no-dynamis always go south (and so can be second far baiters or any near baiters), dynamis can go anywhere
                center + 19f * startdir.ToDirection(), // '4' - second far bait spot
                center + 19f * (startdir + 9f * rot).ToDirection(), // '2' - first near bait spot
                center + 19f * (startdir + 11f * rot).ToDirection() // '3' - second near bait spot
            };
            if (_dynamisStacks[slot])
            {
                safespots.Add(center - 19f * startdir.ToDirection()); // '1' - first far bait spot
                safespots.Add(center - 19f * (startdir + 5f * rot).ToDirection()); // first (far) laser bait spot
                safespots.Add(center - 19f * (startdir - 5f * rot).ToDirection()); // second (stay) laser bait spot
            }
            return safespots;
        }
    }
}

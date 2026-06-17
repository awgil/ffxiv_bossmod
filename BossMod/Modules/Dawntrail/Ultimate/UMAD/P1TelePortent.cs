namespace BossMod.Dawntrail.Ultimate.UMAD;

class P1TelePortent(BossModule module) : BossComponent(module)
{
    readonly UMADConfig _config = Service.Config.Get<UMADConfig>();

    [Flags]
    enum Direction : byte
    {
        None,
        Up = 1,
        Down = 2,
        Right = 4,
        Left = 8
    }

    class Directions
    {
        public Direction All;

        public (Direction Dir, DateTime Time) D1;
        public (Direction Dir, DateTime Time) D2;
    }

    readonly Directions[] _debuffs = Utils.GenArray<Directions>(8, () => new());
    readonly List<(Direction, WPos)>[] _hintSpots = Utils.GenArray<List<(Direction, WPos)>>(8, () => []);
    readonly int[] _timesHit = new int[8];

    static WDir ToWDir(Direction d)
    {
        var wd = new WDir(0, 0);
        if (d.HasFlag(Direction.Up))
            wd.Z -= 1;
        if (d.HasFlag(Direction.Down))
            wd.Z += 1;
        if (d.HasFlag(Direction.Right))
            wd.X += 1;
        if (d.HasFlag(Direction.Left))
            wd.X -= 1;
        return wd;
    }

    static Direction FromWDir(WDir d)
    {
        d = d.Normalized().Rounded();
        if (d.X < 0)
            return Direction.Left;
        if (d.X > 0)
            return Direction.Right;
        if (d.Z > 0)
            return Direction.Down;
        if (d.Z < 0)
            return Direction.Up;
        return Direction.None;
    }

    public int NumArrows { get; private set; }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        var newDir = (SID)status.ID switch
        {
            SID.TelePortentN1 or SID.TelePortentN2 => Direction.Up,
            SID.TelePortentS1 or SID.TelePortentS2 => Direction.Down,
            SID.TelePortentE1 or SID.TelePortentE2 => Direction.Right,
            SID.TelePortentW1 or SID.TelePortentW2 => Direction.Left,
            _ => default
        };

        if (newDir == default || !Raid.TryFindSlot(actor.InstanceID, out var slot))
            return;

        var dir = _debuffs[slot];

        dir.All |= newDir;

        var duration = (status.ExpireAt - WorldState.CurrentTime).TotalSeconds;
        if (duration > 8)
            dir.D2 = (newDir, status.ExpireAt);
        else
            dir.D1 = (newDir, status.ExpireAt);

        if (dir.D1 == default || dir.D2 == default)
            return;

        switch (_config.P1Arrows)
        {
            case UMADConfig.P1ArrowShape.BigBox:
            case UMADConfig.P1ArrowShape.Freaky:
                var wd = ToWDir(dir.All);

                if (dir.All == dir.D1.Dir)
                {
                    // matched arrows
                    var cardinal = wd.OrthoL() * (_config.P1Arrows == UMADConfig.P1ArrowShape.Freaky ? 11 : 12);
                    var preCardinal = (wd + wd.OrthoL() * 0.5f).OrthoL() * 12;
                    _hintSpots[slot].AddRange([(dir.All, Arena.Center + cardinal), (dir.All, Arena.Center + preCardinal)]);
                }
                else
                {
                    // unmatched arrows
                    var intercard = wd.OrthoL() * 6;
                    var preIntercard = intercard + (wd + wd.OrthoL()).Sign().OrthoL() * 6;
                    _hintSpots[slot].AddRange([(FromWDir(wd.Rotate(-45.Degrees())), Arena.Center + intercard * 2), (FromWDir(wd.Rotate(45.Degrees())), Arena.Center + preIntercard)]);
                }
                break;
        }
    }

    public override void OnStatusLose(Actor actor, ActorStatus status)
    {
        var missingDir = (SID)status.ID switch
        {
            SID.TelePortentN1 or SID.TelePortentN2 => Direction.Up,
            SID.TelePortentS1 or SID.TelePortentS2 => Direction.Down,
            SID.TelePortentE1 or SID.TelePortentE2 => Direction.Right,
            SID.TelePortentW1 or SID.TelePortentW2 => Direction.Left,
            _ => default
        };

        if (missingDir == default)
            return;

        NumArrows++;

        if (!Raid.TryFindSlot(actor, out var slot))
            return;

        _timesHit[slot]++;
        var matchingSpot = _hintSpots[slot].Where(s => s.Item1 == missingDir).MinBy(s => (actor.Position - s.Item2).LengthSq());
        _hintSpots[slot].Remove(matchingSpot);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var d = _debuffs[slot];
        if (d.D2 == default)
            return;
        hints.Add($"{d.D1.Dir} => {d.D2.Dir}", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_timesHit[slot] > 1)
            return;

        var spotsNow = _hintSpots[slot].Where(h =>
        {
            var toCheck = _timesHit[slot] == 0 ? _debuffs[slot].D1.Dir : _debuffs[slot].D2.Dir;
            return h.Item1 == toCheck;
        }).Select(s => ShapeContains.InvertedCircle(s.Item2, 1)).ToList();

        var deadline = _timesHit[slot] == 0 ? _debuffs[slot].D1.Time : _debuffs[slot].D2.Time;

        hints.AddForbiddenZone(ShapeContains.Intersection(spotsNow), deadline);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var (sd, sp) in _hintSpots[pcSlot])
        {
            var toCheck = _timesHit[pcSlot] == 0 ? _debuffs[pcSlot].D1.Dir : _debuffs[pcSlot].D2.Dir;

            Arena.AddCircle(sp, 0.75f, sd == toCheck ? ArenaColor.Safe : ArenaColor.Danger);
        }
    }

    public override void AddMovementHints(int slot, Actor actor, MovementHints movementHints)
    {
        var toCheck = _timesHit[slot] == 0 ? _debuffs[slot].D1.Dir : _debuffs[slot].D2.Dir;

        WPos last = default;

        foreach (var (d, s) in _hintSpots[slot].OrderBy(s => s.Item1 != toCheck))
        {
            movementHints.Add(d == toCheck ? actor.Position : last, s, d == toCheck ? ArenaColor.Safe : ArenaColor.Danger);
            last = s;
        }
    }
}

class P1Arrow(BossModule module) : BossComponent(module)
{
    readonly List<Actor> _arrows = [];

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.TelePortent)
            _arrows.Add(actor);
    }

    public override void OnActorDestroyed(Actor actor)
    {
        if ((OID)actor.OID == OID.TelePortent)
            _arrows.Remove(actor);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var positions = _arrows.Select(r => r.Position).ToList();
        hints.AddForbiddenZone(p => positions.Any(p2 => p2.InCircle(p, 2)));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        foreach (var arr in _arrows)
        {
            if (Arena.Config.ShowOutlinesAndShadows)
                Arena.AddCircle(arr.Position, 2, 0xFF000000, 2);
            Arena.AddCircle(arr.Position, 2, ArenaColor.Danger);
            Arena.AddLine(arr.Position + new WDir(0, 1).Rotate(arr.Rotation), arr.Position + new WDir(1, 0).Rotate(arr.Rotation), ArenaColor.Danger);
            Arena.AddLine(arr.Position + new WDir(0, 1).Rotate(arr.Rotation), arr.Position + new WDir(-1, 0).Rotate(arr.Rotation), ArenaColor.Danger);
            Arena.AddLine(arr.Position + new WDir(0, 1).Rotate(arr.Rotation), arr.Position + new WDir(0, -1).Rotate(arr.Rotation), ArenaColor.Danger);
        }
    }

    public override void Update()
    {
        _arrows.RemoveAll(a => a.EventState == 7);
    }
}

class P1IndulgentWill(BossModule module) : BossComponent(module)
{
    BitMask _targets;
    public bool Draw;

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GravenImage && source.Position.AlmostEqual(new(95, 25), 5) && Raid.TryFindSlot(tether.Target, out var target))
            _targets.Set(target);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_targets[pcSlot] || !Draw)
            return;

        var closestAlly = Raid.WithoutSlot().Exclude(pc).Closest(pc.Position);
        if (closestAlly != null)
        {
            var dir = closestAlly.Position - pc.Position;
            var len = MathF.Max(1, dir.Length() - 3);
            var dest = pc.Position + dir.Normalized() * len;

            Arena.AddLine(pc.Position, dest, ArenaColor.Danger);
            Arena.ActorProjected(pc.Position, dest, dir.ToAngle(), ArenaColor.Danger);
        }
    }
}

class P1IdyllicWill(BossModule module) : Components.UniformStackSpread(module, 0, 5)
{
    readonly List<Spread> _stored = [];

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID == TetherID.GravenImage && source.Position.AlmostEqual(new(107, 43), 5) && WorldState.Actors.Find(tether.Target) is { } target)
            _stored.Add(new(target, 5, WorldState.FutureTime(9)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.IdyllicWill)
            Spreads.Clear();
    }

    public void Activate()
    {
        Spreads.AddRange(_stored);
        _stored.Clear();
    }
}

class P1IdyllicWillCounter(BossModule module) : Components.CastCounter(module, AID.IdyllicWill);

// Indolent Will: 0x1EBFBE plays 00400080 at 105.25, 34
// Ave Maria: 0x1EBFBF plays 00400080

class P1StatueGaze : Components.GenericGaze
{
    WPos? _source;
    DateTime _activation;

    public P1StatueGaze(BossModule module) : base(module)
    {
        EnableHints = false;
    }

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_source.HasValue)
            yield return new(_source.Value, _activation);
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == 0x1EBFBE && state == 0x00400080)
        {
            _source = new(105.25f, 34);
            _activation = WorldState.FutureTime(9.9f);
        }

        if (actor.OID == 0x1EBFBF && state == 0x00400080)
        {
            _source = new(95, 25);
            _activation = WorldState.FutureTime(9.9f);
            Inverted = true;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.IndolentWill or AID.AveMaria)
        {
            NumCasts++;
            _source = null;
        }
    }
}

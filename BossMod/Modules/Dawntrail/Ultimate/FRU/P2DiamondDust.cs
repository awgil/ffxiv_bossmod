namespace BossMod.Dawntrail.Ultimate.FRU;

class P2AxeKick(BossModule module) : Components.StandardAOEs(module, AID.AxeKick, new AOEShapeCircle(16));
class P2ScytheKick(BossModule module) : Components.StandardAOEs(module, AID.ScytheKick, new AOEShapeDonut(4, 20));

class P2IcicleImpact(BossModule module) : Components.GenericAOEs(module, AID.IcicleImpact)
{
    public readonly List<AOEInstance> AOEs = []; // note: we don't remove finished aoes, since we use them in other components to detect safespots

    private static readonly AOEShapeCircle _shape = new(10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Skip(NumCasts);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            // initially all aoes start as non-risky
            AOEs.Add(new(_shape, caster.Position, default, Module.CastFinishAt(spell), 0, false));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IcicleImpact:
                ++NumCasts;
                break;
            case AID.HouseOfLight:
                // after proteans are baited, first two aoes become risky; remaining are still not - stones are supposed to be baited into them
                MarkAsRisky(0, Math.Min(2, AOEs.Count));
                break;
            case AID.FrigidStone:
                // after stones are baited, all aoes should be marked as risky
                MarkAsRisky(2, AOEs.Count);
                break;
        }
    }

    private void MarkAsRisky(int start, int end)
    {
        for (int i = start; i < end; ++i)
            AOEs.Ref(i).Risky = true;
    }
}

class P2FrigidNeedleCircle(BossModule module) : Components.StandardAOEs(module, AID.FrigidNeedleCircle, new AOEShapeCircle(5));
class P2FrigidNeedleCross(BossModule module) : Components.StandardAOEs(module, AID.FrigidNeedleCross, new AOEShapeCross(40, 2.5f));

class P2FrigidStone : Components.BaitAwayIcon
{
    public P2FrigidStone(BossModule module) : base(module, new AOEShapeCircle(5), (uint)IconID.FrigidStone, AID.FrigidStone, 8.1f, true)
    {
        EnableHints = false;
        IgnoreOtherBaits = true;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }
}

class P2DiamondDustHouseOfLight(BossModule module) : Components.GenericBaitAway(module, AID.HouseOfLight)
{
    private Actor? _source;
    private DateTime _activation;

    private static readonly AOEShapeCone _shape = new(60, 15.Degrees());

    public override void Update()
    {
        CurrentBaits.Clear();
        if (_source != null && ForbiddenPlayers.Any())
            foreach (var p in Raid.WithoutSlot().SortedByRange(_source.Position).Take(4))
                CurrentBaits.Add(new(_source, p, _shape, _activation));
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (CurrentBaits.Count == 0)
            return;

        var baitIndex = CurrentBaits.FindIndex(b => b.Target == actor);
        if (ForbiddenPlayers[slot])
        {
            if (baitIndex >= 0)
                hints.Add("Stay farther away!");
        }
        else
        {
            if (baitIndex < 0)
                hints.Add("Stay closer to bait!");
            else if (PlayersClippedBy(CurrentBaits[baitIndex]).Any())
                hints.Add("Bait cone away from raid!");
        }

        if (ActiveBaitsNotOn(actor).Any(b => IsClippedBy(actor, b)))
            hints.Add("GTFO from baited cone!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints) { }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.AxeKick or AID.ScytheKick)
        {
            _source = caster;
            _activation = Module.CastFinishAt(spell, 0.8f);
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FrigidStone)
            ForbiddenPlayers.Set(Raid.FindSlot(actor.InstanceID));
    }
}

class P2DiamondDustSafespots(BossModule module) : BossComponent(module)
{
    private readonly FRUConfig _config = Service.Config.Get<FRUConfig>();
    private bool? _out;
    private bool? _supportsBaitCones;
    private bool? _conesAtCardinals;
    private readonly WDir[] _safeOffs = new WDir[PartyState.MaxPartySize];

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_safeOffs[slot] != default)
        {
            hints.PathfindMapBounds = FRU.PathfindHugBorderBounds;
            hints.AddForbiddenZone(ShapeContains.PrecisePosition(Module.Center + _safeOffs[slot], new WDir(0, 1), Module.Bounds.MapResolution, actor.Position, 0.1f));
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_safeOffs[pcSlot] != default)
            Arena.AddCircle(Module.Center + _safeOffs[pcSlot], 1, ArenaColor.Safe);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.IcicleImpact:
                if (_conesAtCardinals == null)
                {
                    _conesAtCardinals = IsCardinal(caster.Position - Module.Center);
                    InitIfReady();
                }
                break;
            case AID.AxeKick:
                _out = true;
                InitIfReady();
                break;
            case AID.ScytheKick:
                _out = false;
                InitIfReady();
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AxeKick:
                // out done => cone baiters go in, ice baiters stay
                for (int i = 0; i < _safeOffs.Length; ++i)
                    if (_safeOffs[i] != default && Raid[i]?.Class.IsSupport() == _supportsBaitCones)
                        _safeOffs[i] = 4 * _safeOffs[i].Normalized();
                break;
            case AID.ScytheKick:
                // in done => cone baiters stay, ice baiters go out
                for (int i = 0; i < _safeOffs.Length; ++i)
                    if (_safeOffs[i] != default && Raid[i]?.Class.IsSupport() != _supportsBaitCones)
                        _safeOffs[i] = 8 * _safeOffs[i].Normalized();
                break;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.FrigidStone && _supportsBaitCones == null)
        {
            _supportsBaitCones = actor.Class.IsDD();
            InitIfReady();
        }
    }

    private void InitIfReady()
    {
        if (_out == null || _supportsBaitCones == null || _conesAtCardinals == null)
            return;
        var supportsAtCardinals = _supportsBaitCones == _conesAtCardinals;
        var offsetTH = supportsAtCardinals ? 0.Degrees() : _config.P2DiamondDustSupportsCCW ? 45.Degrees() : -45.Degrees();
        var offsetDD = !supportsAtCardinals ? 0.Degrees() : _config.P2DiamondDustDDCCW ? 45.Degrees() : -45.Degrees();
        foreach (var (slot, group) in _config.P2DiamondDustCardinals.Resolve(Raid))
        {
            var support = group < 4;
            var baitCone = _supportsBaitCones == support;
            var dir = 180.Degrees() - (group & 3) * 90.Degrees();
            dir += support ? offsetTH : offsetDD;
            var radius = (_out.Value ? 16 : 0) + (baitCone ? 1 : 3);
            _safeOffs[slot] = radius * dir.ToDirection();
        }
    }

    private bool IsCardinal(WDir off) => Math.Abs(off.X) < 1 || Math.Abs(off.Z) < 1;
}

class P2HeavenlyStrike(BossModule module) : Components.Knockback(module, AID.HeavenlyStrike)
{
    private readonly WDir[] _safeDirs = BuildSafeDirs(module);
    private readonly DateTime _activation = module.WorldState.FutureTime(3.9f);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        yield return new(Module.Center, 12, _activation);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_safeDirs[slot] != default)
            hints.AddForbiddenZone(ShapeContains.PrecisePosition(Module.Center + 6 * _safeDirs[slot], new(1, 0), Module.Bounds.MapResolution, actor.Position, 0.25f), _activation);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (_safeDirs[pcSlot] != default)
            Arena.AddCircle(Module.Center + 18 * _safeDirs[pcSlot], 1, ArenaColor.Safe);
    }

    private static WDir[] BuildSafeDirs(BossModule module)
    {
        var res = new WDir[PartyState.MaxPartySize];
        var icicle = module.FindComponent<P2IcicleImpact>();
        if (icicle?.AOEs.Count > 0)
        {
            var safeDir = (icicle.AOEs[0].Origin - module.Center).Normalized();
            if (safeDir.X > 0.5f || safeDir.Z > 0.8f)
                safeDir = -safeDir; // G1
            foreach (var (slot, group) in Service.Config.Get<FRUConfig>().P2DiamondDustKnockbacks.Resolve(module.Raid))
                res[slot] = group == 1 ? -safeDir : safeDir;
        }
        return res;
    }
}

class P2SinboundHoly(BossModule module) : Components.UniformStackSpread(module, 6, 0, 4, 4)
{
    public int NumCasts;
    private DateTime _nextExplosion;
    private readonly WDir _destinationDir = CalculateDestination(module);
    private readonly WPos[] _initialSpots = new WPos[PartyState.MaxPartySize];

    private static WDir CalculateDestination(BossModule module)
    {
        // if oracle jumps directly to one of the initial safespots, both groups run opposite in one (arbitrary, CW) direction, and the one that ends up behind boss slides across - in that case we return zero destination
        // note: we assume that when this is called oracle is already at position
        var icicles = module.FindComponent<P2IcicleImpact>();
        var oracle = module.Enemies(OID.OraclesReflection).FirstOrDefault();
        if (icicles == null || icicles.AOEs.Count == 0 || oracle == null)
            return default;

        var idealDir = (module.Center - oracle.Position).Normalized(); // ideally we wanna stay as close as possible to across the oracle
        var destDir = (icicles.AOEs[0].Origin - module.Center).Normalized().OrthoL(); // actual destination is one of the last icicles
        return destDir.Dot(idealDir) switch
        {
            > 0.5f => destDir,
            < -0.5f => -destDir,
            _ => default, // fast movement mode
        };
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var master = actor.Role != Role.Healer ? Stacks.MinBy(s => (s.Target.Position - actor.Position).LengthSq()).Target : null;
        if (master != null && ((master.Position - actor.Position).LengthSq() > 100 || (master.Position - Module.Center).LengthSq() < 196))
            master = null; // our closest healer is too far away or too close to center, something is wrong (maybe kb didn't finish yet, or healer fucked up)

        // determine movement speed and direction
        // baseline is towards safety (opposite boss), or CW (arbitrary) if there's no obvious safe direction
        // however, if we're non-healer, it is overridden by healer's decision (we can slide over later)
        var moveQuickly = _destinationDir == default;
        var preferredDir = !moveQuickly ? _destinationDir : (actor.Position - Module.Center).Normalized().OrthoR();
        moveQuickly &= NumCasts > 0; // don't start moving while waiting for first cast

        if (master != null)
        {
            if (NumCasts > 0 && Raid.TryFindSlot(master, out var masterSlot))
            {
                var masterMovement = preferredDir.Dot(master.Position - _initialSpots[masterSlot]);
                if (masterMovement < -2)
                    preferredDir = -preferredDir; // swap movement direction to follow healer
            }

            moveQuickly &= (actor.Position - master.Position).LengthSq() < 25; // don't move too quickly if healer can't catch up

            // non-healers should just stack with whatever closest healer is
            // before first cast, ignore master's movements
            var moveDir = NumCasts > 0 ? master.LastFrameMovement.Normalized() : default;
            var capsule = ShapeContains.Capsule(master.Position + 2 * moveDir, moveDir, 4, 1.5f);
            hints.AddForbiddenZone(p => !capsule(p), DateTime.MaxValue);
        }

        // note: other hints have to be 'later' than immediate (to make getting out of voidzones higher prio), but 'earlier' than stack-with-healer:
        // healer's position is often overlapped by new voidzones, if healer is moving slowly - in that case we still need to dodge in correct direction
        var hintTime = WorldState.FutureTime(50);

        // stay near border
        hints.AddForbiddenZone(ShapeContains.Circle(Module.Center, 16), hintTime);

        // prefer moving towards safety (CW is arbitrary)
        var planeOffset = moveQuickly ? 2 : -2; // if we're moving quickly, mark our current spot as forbidden
        hints.AddForbiddenZone(ShapeContains.HalfPlane(Module.Center + planeOffset * preferredDir, preferredDir), hintTime);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SinboundHoly)
        {
            AddStacks(Raid.WithoutSlot().Where(p => p.Role == Role.Healer), Module.CastFinishAt(spell, 0.9f));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SinboundHolyAOE && WorldState.CurrentTime > _nextExplosion)
        {
            if (NumCasts == 0)
                foreach (var (i, p) in Raid.WithSlot())
                    _initialSpots[i] = p.Position;

            ++NumCasts;
            _nextExplosion = WorldState.FutureTime(0.5f);
        }
    }
}

class P2SinboundHolyVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.SinboundHolyVoidzone).Where(z => z.EventState != 7))
{
    public bool AIHintsEnabled = true;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (AIHintsEnabled)
            base.AddAIHints(slot, actor, assignment, hints);
    }
}

class P2ShiningArmor(BossModule module) : Components.GenericGaze(module, AID.ShiningArmor)
{
    private Actor? _source;
    private DateTime _activation;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_source != null)
            yield return new(_source.Position, _activation);
    }

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        if ((OID)actor.OID == OID.BossP2 && id == 0x1E43)
        {
            _source = actor;
            _activation = WorldState.FutureTime(7.2f);
        }
    }
}

class P2TwinStillnessSilence(BossModule module) : Components.GenericAOEs(module)
{
    public readonly List<AOEInstance> AOEs = [];
    private readonly Actor? _source = module.Enemies(OID.OraclesReflection).FirstOrDefault();
    private BitMask _thinIce;
    private readonly WPos[] _slideBackPos = new WPos[PartyState.MaxPartySize]; // used for hints only
    private P2SinboundHolyVoidzone? _voidzones; // used for hints only

    private const float SlideDistance = 32;
    private readonly AOEShapeCone _shapeFront = new(30, 135.Degrees());
    private readonly AOEShapeCone _shapeBack = new(30, 45.Degrees());

    public void EnableAIHints()
    {
        _voidzones = Module.FindComponent<P2SinboundHolyVoidzone>();
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs.Take(1);

    public override void Update()
    {
        if (AOEs.Count != 2)
            return;
        foreach (var (i, p) in Raid.WithSlot().IncludedInMask(_thinIce))
            if (_slideBackPos[i] == default && p.LastFrameMovement != default)
                _slideBackPos[i] = p.PrevPosition;
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_voidzones == null || _source == null)
            return;

        if (!_thinIce[slot])
        {
            // preposition
            // this is a bit hacky - we need to stay either far away from boss, or close (and slide over at the beginning of the ice)
            // the actual shape is quite complicated ('primary' shape is a set of points at distance X from a cone behind boss, 'secondary' is a set of points at distance X from primary), so we use a rough approximation

            // first, find a set of allowed angles along the border
            var zoneList = new ArcList(Module.Center, 17);
            foreach (var z in _voidzones.Sources(Module))
                zoneList.ForbidCircle(z.Position, _voidzones.Shape.Radius);

            // now find closest allowed zone
            var actorDir = Angle.FromDirection(actor.Position - Module.Center);
            var closest = zoneList.Allowed(5.Degrees()).MinBy(z => actorDir.DistanceToRange(z.min, z.max).Abs().Rad);
            if (closest != default)
            {
                var desiredDir = (closest.min + closest.max) * 0.5f;
                var halfWidth = (closest.max - closest.min) * 0.5f;
                if (halfWidth.Deg > 5)
                {
                    // destination is very wide, narrow it down a bit to be in line with the boss
                    halfWidth = 5.Degrees();
                    var sourceDir = Angle.FromDirection(_source.Position - Module.Center);
                    var sourceDist = sourceDir.DistanceToRange(closest.min + halfWidth, closest.max - halfWidth);
                    var oppositeDist = (sourceDir + 180.Degrees()).DistanceToRange(closest.min + halfWidth, closest.max - halfWidth);
                    desiredDir = oppositeDist.Abs().Rad < sourceDist.Abs().Rad ? (sourceDir + 180.Degrees() + oppositeDist) : (sourceDir + sourceDist);
                }
                hints.AddForbiddenZone(ShapeContains.Circle(Module.Center, 16), WorldState.FutureTime(50));
                hints.AddForbiddenZone(ShapeContains.InvertedCone(Module.Center, 100, desiredDir, halfWidth), DateTime.MaxValue);
            }
        }
        else if (actor.LastFrameMovement == default)
        {
            // at this point, we have thin ice, so we can either stay or move fixed distance
            var sourceOffset = _source.Position - Module.Center;
            var needToMove = AOEs.Count > 0 ? AOEs[0].Check(actor.Position) : NumCasts == 0 && sourceOffset.Dot(actor.Position - Module.Center) > 0;
            if (!needToMove)
                return;

            var zoneList = new ArcList(actor.Position, SlideDistance);
            zoneList.ForbidInverseCircle(Module.Center, Module.Bounds.Radius);

            foreach (var z in _voidzones.Sources(Module))
            {
                var offset = z.Position - actor.Position;
                var dist = offset.Length();
                if (dist >= SlideDistance)
                {
                    // voidzone center is outside slide distance => forbid voidzone itself
                    zoneList.ForbidCircle(z.Position, _voidzones.Shape.Radius);
                }
                else if (dist >= _voidzones.Shape.Radius)
                {
                    // forbid the voidzone's shadow
                    zoneList.ForbidArcByLength(Angle.FromDirection(offset), Angle.Asin(_voidzones.Shape.Radius / dist));
                }
                // else: we're already in voidzone, oh well
            }

            if (AOEs.Count == 0)
            {
                // if we're behind boss, slide over to the safe point as opposite to the boss as possible
                var farthestDir = Angle.FromDirection(-sourceOffset);
                var bestRange = zoneList.Allowed(5.Degrees()).MinBy(r => farthestDir.DistanceToRange(r.min, r.max).Abs().Rad);
                var dir = farthestDir.ClosestInRange(bestRange.min, bestRange.max);
                hints.AddForbiddenZone(ShapeContains.InvertedCircle(actor.Position + SlideDistance * dir.ToDirection(), 1), DateTime.MaxValue);
            }
            else
            {
                // dodge next aoe
                ref var nextAOE = ref AOEs.Ref(0);
                zoneList.ForbidInfiniteCone(nextAOE.Origin, nextAOE.Rotation, ((AOEShapeCone)nextAOE.Shape).HalfAngle);

                // prefer to return to the starting spot, for more natural preposition for next mechanic
                if (AOEs.Count == 1 && _slideBackPos[slot] != default && !zoneList.Forbidden.Contains(Angle.FromDirection(_slideBackPos[slot] - actor.Position).Rad))
                {
                    hints.AddForbiddenZone(ShapeContains.InvertedCircle(_slideBackPos[slot], 1), DateTime.MaxValue);
                }
                else if (zoneList.Allowed(1.Degrees()).MaxBy(r => (r.max - r.min).Rad) is var best && best.max.Rad > best.min.Rad)
                {
                    var dir = 0.5f * (best.min + best.max);
                    hints.AddForbiddenZone(ShapeContains.InvertedCircle(actor.Position + SlideDistance * dir.ToDirection(), 1), DateTime.MaxValue);
                }
                // else: no good direction can be found, wait for a bit, maybe voidzone will disappear
            }
        }
        // else: we are already sliding, nothing to do...
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        Arena.Actor(_source, ArenaColor.Object, true);
        if (_thinIce[pcSlot])
        {
            Arena.AddCircle(pc.Position, SlideDistance, ArenaColor.Vulnerable);
            Arena.AddLine(pc.Position, pc.Position - SlideDistance * WorldState.Client.CameraAzimuth.ToDirection(), ArenaColor.Vulnerable);
        }
    }

    public override void OnStatusGain(Actor actor, ActorStatus status)
    {
        if ((SID)status.ID == SID.ThinIce)
            _thinIce.Set(Raid.FindSlot(actor.InstanceID));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var (shape1, off1, shape2, off2) = (AID)spell.Action.ID switch
        {
            AID.TwinStillnessFirst => (_shapeFront, 0.Degrees(), _shapeBack, 180.Degrees()),
            AID.TwinSilenceFirst => (_shapeBack, 0.Degrees(), _shapeFront, 180.Degrees()),
            _ => (null, default, null, default)
        };
        if (shape1 != null && shape2 != null)
        {
            AOEs.Add(new(shape1, caster.Position, spell.Rotation + off1, Module.CastFinishAt(spell)));
            AOEs.Add(new(shape2, caster.Position, spell.Rotation + off2, Module.CastFinishAt(spell, 2.1f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.TwinStillnessFirst or AID.TwinStillnessSecond or AID.TwinSilenceFirst or AID.TwinSilenceSecond)
        {
            ++NumCasts;
            if (AOEs.Count > 0)
                AOEs.RemoveAt(0);
        }
    }
}

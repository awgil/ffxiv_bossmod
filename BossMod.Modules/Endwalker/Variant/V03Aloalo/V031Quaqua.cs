namespace BossMod.Endwalker.Variant.V03Aloalo.V031Quaqua;

public enum OID : uint
{
    Boss = 0x40BA, // R5.250, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    AethericCharge = 0x40BB, // R1.500-2.200, x?, sphere for Arcane Armaments
    SpearMarker = 0x40BD, // R0.500, x5, middle-path water spear tether source
    HammerBeacon = 0x40E0, // R1.000, x?, Hammer Landing jump markers
    DrakeFamiliarLarge = 0x40BF, // R2.700, x?, Cloud to Ground visual (stormy)
    DrakeFamiliar = 0x4135, // R1.000, x?, Cloud to Ground exaflares (stormy)
    AnalaFamiliar = 0x4134, // R1.000, x?, Scalding Waves (fair skies)
    PoisonPuddle = 0x1EB94A, // R0.500, EventObj type, growing poison pools (4+ flowers path)
    PursuitCharge = 0x40BC, // R1.500, x?, chasing orb for Arcane Pursuit (both statues repaired)
    AnalaFamiliarLarge = 0x40BE, // R1.500, x0 (spawn during fight), initial Scalding Waves casters (fair skies)
}

public enum AID : uint
{
    AutoAttack = 6497, // Boss->player, no cast, single-target
    Teleport = 35727, // Boss->location, no cast, single-target

    MadeMagic = 35732, // Boss->self, 5.0s cast, range 50 circle, raidwide + spawn spheres
    ArcaneArmamentsAxeQuoit = 35720, // Boss->self, 8.0s cast, single-target, tether spheres (axe/chakram)
    ArcaneArmamentsResolve = 35721, // Helper->self, no cast, single-target, visual when icon reaches sphere
    RavagingAxe = 35722, // AethericCharge->self, 2.0s cast, range 14 circle
    RingingQuoits = 35723, // AethericCharge->self, 2.0s cast, range 5-18 donut

    ArcaneArmamentsSpearHammer = 35728, // Boss->self, 5.0s cast, single-target, left-path spears (Rout) / hammers
    Rout = 35729, // Boss->location, 5.0s cast, range 45 width 16 rect
    RoutRepeat = 35730, // Boss->location, no cast, range 45 width 16 rect

    ArcaneArmamentsHammers = 35724, // Boss->self, 5.0s cast, single-target, spawn hammer beacons
    HammerLanding = 35725, // Boss->location, 8.0s cast, range 40 circle, knockback
    HammerLandingRepeat = 35726, // Boss->location, no cast, range 40 circle, knockback

    // Middle path
    ArcaneArmamentsWaterSpears = 35743, // Boss->self, 8.0s cast, single-target, tether spear markers
    ElementalImpact = 35744, // Helper->self, 6.5s cast, range 14 circle
    FlowingLanceCW = 35745, // Helper->self, 8.0s cast, range 24 width 12 cross (turns right / CW)
    FlowingLanceCCW = 36049, // Helper->self, 8.0s cast, range 24 width 12 cross (turns left / CCW)
    FlowingLanceRest = 35746, // Helper->self, 1.0s cast, range 24 width 12 cross

    VioletStorm = 35733, // Boss->self, 5.5s cast, range 32 120-degree cone
    Howl = 35734, // Boss->self, 4.0s cast, single-target, summon familiars (left path)

    // Fair skies - analas
    ScaldingWaves = 35731, // Helper->self, 2.7-3.0s cast, range 40 width 10 rect
    ScaldingWavesFairSkies = 35735, // AnalaFamiliarLarge->self, 5.0s cast, range 50 width 8 rect
    ScaldingWavesFairSkiesRepeat = 35736, // AnalaFamiliar->self, no cast, range 50 width 4 rect

    // Stormy - drakes
    CloudToGround = 35739, // DrakeFamiliarLarge->self, 4.0s cast, single-target, visual
    CloudToGroundFirst = 35740, // DrakeFamiliar->self, 5.0s cast, range 6 circle
    CloudToGroundRest = 35741, // DrakeFamiliar->self, 1.0s cast, range 6 circle

    ArcaneArmamentsPoisonSpears = 35747, // Boss->self, 8.0s cast, single-target

    ArcaneIntervention = 35758, // Helper->self, 5.0s cast, range 50 circle, gaze

    ArcanePursuit = 35748, // Boss->self, 5.0s cast, single-target, spawn chasing orbs
    FellForces = 35749, // PursuitCharge->player, no cast, single-target, contact damage (range 1)
    ArcanePursuitLaser = 35750, // Helper->self, 5.0s cast, range 50 width 6 rect (destroys orbs, harmless to players)
    ArcanePursuitLaserSecond = 35757, // Helper->self, 5.0s cast, range 50 width 6 rect
}

public enum TetherID : uint
{
    Axe = 256, // Ravaging Axe (circle)
    Quoit = 257, // Ringing Quoits (donut)
    Hammer = 249, // HammerBeacon->Boss
    WaterSpear = 258, // SpearMarker->Boss
}

public enum IconID : uint
{
    TurnRight = 235, // Helper->self, Flowing Lance CW (-15 degrees)
    TurnLeft = 236, // Helper->self, Flowing Lance CCW (+15 degrees)
}

class MadeMagic(BossModule module) : Components.RaidwideCast(module, AID.MadeMagic);
class VioletStorm(BossModule module) : Components.StandardAOEs(module, AID.VioletStorm, new AOEShapeCone(32, 60.Degrees()));
class ElementalImpact(BossModule module) : Components.StandardAOEs(module, AID.ElementalImpact, 14);

class ArcaneIntervention(BossModule module) : Components.CastGaze(module, AID.ArcaneIntervention);

class ArcanePursuit(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _lasers = [];
    private static readonly AOEShapeRect _laser = new(50, 3);
    private const float KeepAway = 3.5f;

    private IEnumerable<Actor> Charges => Module.Enemies(OID.PursuitCharge).Where(c => !c.IsDead);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in Charges)
            yield return new(new AOEShapeCircle(KeepAway), c.Position);
        // safe zones
        foreach (var l in _lasers)
            if (l.CastInfo != null)
                yield return new(_laser, l.Position, l.CastInfo.Rotation, Module.CastFinishAt(l.CastInfo), ArenaColor.SafeFromAOE, Risky: false, Inverted: true);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var near = Charges.FirstOrDefault(c => actor.Position.InCircle(c.Position, KeepAway));
        if (near != null)
            hints.Add("Away from orb!");
        else if (_lasers.Count > 0 && Charges.Any())
            hints.Add("Lead orb into statue laser!", false);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var c in Charges)
        {
            hints.AddForbiddenZone(ShapeDistance.Circle(c.Position, KeepAway));
            hints.AddForbiddenZone(ShapeDistance.Capsule(c.Position, c.Rotation.ToDirection(), 8, KeepAway), WorldState.FutureTime(2));
        }

        foreach (var l in _lasers)
        {
            if (l.CastInfo == null)
                continue;
            var rot = l.CastInfo.Rotation;
            var origin = l.Position;
            var finish = Module.CastFinishAt(l.CastInfo);
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(origin, rot, 50, 0, 3), finish);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var c in Charges)
        {
            var target = WorldState.Actors.Find(c.TargetID);
            if (target != null)
            {
                if (Arena.Config.ShowOutlinesAndShadows)
                    Arena.AddLine(c.Position, target.Position, 0xFF000000, 2);
                Arena.AddLine(c.Position, target.Position, target == pc ? ArenaColor.Danger : ArenaColor.PlayerGeneric);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcanePursuitLaser or AID.ArcanePursuitLaserSecond)
            _lasers.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcanePursuitLaser or AID.ArcanePursuitLaserSecond)
            _lasers.Remove(caster);
    }
}

class ArcaneArmamentsPoison(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Actor, DateTime Spawn)> _puddles = [];
    private bool _armamentsActive;

    private const float PuddleStartRadius = 6;
    private const float PuddleMaxRadius = 16;
    private const float GrowSeconds = 14;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (puddle, spawn) in _puddles)
        {
            var t = (float)(WorldState.CurrentTime - spawn).TotalSeconds;
            var radius = Math.Clamp(PuddleStartRadius + (PuddleMaxRadius - PuddleStartRadius) * (t / GrowSeconds), PuddleStartRadius, PuddleMaxRadius);
            yield return new(new AOEShapeCircle(radius), puddle.Position, default, spawn);
            if (radius < PuddleMaxRadius - 0.5f)
                yield return new(new AOEShapeCircle(PuddleMaxRadius), puddle.Position, default, spawn.AddSeconds(GrowSeconds), ArenaColor.AOE, Risky: false);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArcaneArmamentsPoisonSpears)
        {
            _armamentsActive = true;
            _puddles.Clear();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ArcaneArmamentsPoisonSpears)
            _armamentsActive = true; // puddles still inbound briefly after cast
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.PoisonPuddle)
        {
            _puddles.Add((actor, WorldState.CurrentTime));
            _armamentsActive = false;
        }
    }

    public override void OnActorDestroyed(Actor actor)
    {
        _puddles.RemoveAll(p => p.Actor == actor);
    }

    public override void OnActorEState(Actor actor, ushort state)
    {
        if ((OID)actor.OID == OID.PoisonPuddle)
            _puddles.RemoveAll(p => p.Actor == actor);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);

        var origins = _puddles.Count > 0
            ? [.. _puddles.Select(p => p.Actor.Position)]
            : _armamentsActive ? ActiveSpears().Select(s => s.Position).ToList() : [];
        if (origins.Count < 2)
            return;

        // midpoint between furthest spears
        WPos a = default, b = default;
        var best = 0f;
        for (var i = 0; i < origins.Count; ++i)
        {
            for (var j = i + 1; j < origins.Count; ++j)
            {
                var d = (origins[i] - origins[j]).LengthSq();
                if (d > best)
                {
                    best = d;
                    a = origins[i];
                    b = origins[j];
                }
            }
        }

        var mid = a + (b - a) * 0.5f;
        var toWall = mid - Module.Center;
        if (toWall.LengthSq() < 0.01f)
            return;
        var safe = Module.Center + toWall.Normalized() * (Module.Bounds.Radius - 1.5f);
        hints.GoalZones.Add(AIHints.GoalProximity(safe, 3, 40));
    }

    private IEnumerable<Actor> ActiveSpears()
        => Module.Enemies(OID.SpearMarker).Where(s => !s.IsDead && s.Tether.ID == (uint)TetherID.WaterSpear);
}

class ScaldingWaves(BossModule module) : Components.StandardAOEs(module, AID.ScaldingWaves, new AOEShapeRect(40, 5))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => Module.FindComponent<Rout>()?.Active == true ? [] : base.ActiveAOEs(slot, actor);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (Module.FindComponent<Rout>()?.Active != true)
            base.OnCastStarted(caster, spell);
    }
}

class ScaldingWavesFairSkies(BossModule module) : Components.Exaflare(module, new AOEShapeRect(25, 2, 25))
{
    private readonly List<Actor> _initialCasters = [];
    private static readonly AOEShapeRect _initialShape = new(50, 4);
    private const float FirstOffset = 6;
    private const float Advance = 4;
    private const float FirstDelay = 2.8f;
    private const float Step = 2.1f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var caster in _initialCasters)
            if (caster.CastInfo is { } cast)
                yield return new(_initialShape, cast.LocXZ, cast.Rotation, Module.CastFinishAt(cast));
        foreach (var aoe in base.ActiveAOEs(slot, actor))
            yield return aoe;
    }

    public override void OnActorDestroyed(Actor actor) => _initialCasters.Remove(actor);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ScaldingWavesFairSkies)
        {
            if (_initialCasters.Count == 0)
            {
                Lines.Clear();
                NumCasts = 0;
            }
            _initialCasters.Add(caster);
            var side = spell.Rotation.ToDirection().OrthoL();
            AddLine(caster.Position + FirstOffset * side, Advance * side, spell.Rotation, Module.CastFinishAt(spell, FirstDelay));
            AddLine(caster.Position - FirstOffset * side, -Advance * side, spell.Rotation, Module.CastFinishAt(spell, FirstDelay));
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.ScaldingWavesFairSkies)
            _initialCasters.Remove(caster);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ScaldingWavesFairSkiesRepeat)
        {
            ++NumCasts;
            var line = Lines.MinBy(l => (l.Next - caster.Position).LengthSq());
            if (line != null && line.Next.AlmostEqual(caster.Position, 1))
            {
                AdvanceLine(line, caster.Position);
                if (line.ExplosionsLeft == 0)
                    Lines.Remove(line);
            }
        }
    }

    private void AddLine(WPos next, WDir advance, Angle rotation, DateTime activation)
    {
        var count = 0;
        for (var p = next; (p - Module.Center).Length() <= Module.Bounds.Radius + 2.5f; p += advance)
            ++count;
        Lines.Add(new()
        {
            Next = next,
            Advance = advance,
            Rotation = rotation,
            NextExplosion = activation,
            TimeToMove = Step,
            ExplosionsLeft = count,
            MaxShownExplosions = 2
        });
    }
}

class ArcaneArmaments(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(ulong InstanceID, AOEInstance AOE)> _aoes = [];
    private DateTime _activation;
    private bool _active;

    private static readonly AOEShapeCircle _axe = new(14);
    private static readonly AOEShapeDonut _quoit = new(5, 18);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Select(a => a.AOE);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ArcaneArmamentsAxeQuoit:
                _aoes.Clear();
                _active = true;
                _activation = Module.CastFinishAt(spell, 5);
                foreach (var orb in Module.Enemies(OID.AethericCharge).Where(o => !o.IsDead && o.Tether.ID != 0))
                    TryAdd(orb, orb.Tether.ID, _activation);
                break;
            case AID.RavagingAxe:
                Upsert(caster.InstanceID, caster.Position, _axe, Module.CastFinishAt(spell));
                break;
            case AID.RingingQuoits:
                Upsert(caster.InstanceID, caster.Position, _quoit, Module.CastFinishAt(spell));
                break;
        }
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        if (!_active || tether.ID == 0)
            return;

        if ((OID)source.OID == OID.AethericCharge)
            TryAdd(source, tether.ID, _activation);
        else if (source == Module.PrimaryActor && WorldState.Actors.Find(tether.Target) is { } target && (OID)target.OID == OID.AethericCharge)
            TryAdd(target, tether.ID, _activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.RavagingAxe or AID.RingingQuoits))
            return;

        _aoes.RemoveAll(a => a.InstanceID == caster.InstanceID);
        ++NumCasts;
        if (_aoes.Count == 0)
        {
            _activation = default;
            _active = false;
        }
    }

    private void TryAdd(Actor orb, uint tetherId, DateTime activation)
    {
        var shape = ShapeForTether(tetherId);
        if (shape != null)
            Upsert(orb.InstanceID, orb.Position, shape, activation);
    }

    private static AOEShape? ShapeForTether(uint tetherId) => tetherId switch
    {
        (uint)TetherID.Axe => _axe,
        (uint)TetherID.Quoit => _quoit,
        _ => null
    };

    private void Upsert(ulong instanceId, WPos position, AOEShape shape, DateTime activation)
    {
        var index = _aoes.FindIndex(a => a.InstanceID == instanceId);
        var entry = (instanceId, new AOEInstance(shape, position, default, activation));
        if (index >= 0)
            _aoes[index] = entry;
        else
            _aoes.Add(entry);
    }
}

class Rout(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(WPos From, WPos To, DateTime Activation)> _charges = [];
    private bool _armed;

    public bool Active => _armed || _charges.Count > 0;

    private const float HalfWidth = 8;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var c in _charges)
            yield return new(new AOEShapeRect((c.To - c.From).Length(), HalfWidth), c.From, (c.To - c.From).ToAngle(), c.Activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ArcaneArmamentsSpearHammer:
                _armed = true;
                _charges.Clear();
                break;
            case AID.ScaldingWaves:
                if (!_armed)
                    break;
                var from = spell.LocXZ;
                var len = _charges.Count == 0 ? 45f : 40f;
                var to = from + len * spell.Rotation.ToDirection();
                _charges.Add((from, to, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.Rout or AID.RoutRepeat))
            return;

        ++NumCasts;
        if (_charges.Count > 0)
            _charges.RemoveAt(0);
        if (_charges.Count == 0)
            _armed = false;
    }
}

class HammerLanding(BossModule module) : Components.Knockback(module, default, maxCasts: 3)
{
    private readonly List<WPos> _landings = [];
    private DateTime _firstActivation;

    private const float Distance = 20;
    private const float BeaconMinRadius = 13;
    private static readonly AOEShapeCircle _shape = new(40);

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        for (var i = NumCasts; i < _landings.Count; ++i)
            yield return new(_landings[i], Distance, _firstActivation.AddSeconds(2.1f * i), _shape);
    }

    public override void Update()
    {
        if (_firstActivation == default || _landings.Count >= 3)
            return;
        TryAddBeacons();
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.HammerLanding)
            return;

        _landings.Clear();
        NumCasts = 0;
        _firstActivation = Module.CastFinishAt(spell);
        _landings.Add(spell.LocXZ);
        TryAddBeacons();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.HammerLanding or AID.HammerLandingRepeat))
            return;

        ++NumCasts;
        if (NumCasts >= _landings.Count)
        {
            _landings.Clear();
            _firstActivation = default;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_firstActivation == default || NumCasts >= _landings.Count)
            return;

        var origin = _landings[NumCasts];
        var activation = _firstActivation.AddSeconds(2.1f * NumCasts);
        if (IsImmune(slot, activation))
            return;

        WDir dir;
        if (NumCasts + 1 < _landings.Count)
        {
            dir = _landings[NumCasts + 1] - origin;
        }
        else if (_landings.Count >= 3)
        {
            dir = Module.Center - origin;
        }
        else
        {
            // stay close to origin until next beacon is known
            hints.AddForbiddenZone(ShapeDistance.InvertedCircle(origin, 0.5f), activation);
            return;
        }

        if (dir.LengthSq() < 0.01f)
            return;

        var aim = dir.Normalized();
        hints.AddForbiddenZone(ShapeDistance.InvertedRect(origin, aim, 4, 0, 0.4f), activation);
        hints.GoalZones.Add(AIHints.GoalProximity(origin + aim * 2, 1.5f, 80));
    }

    private void TryAddBeacons()
    {
        foreach (var b in Module.Enemies(OID.HammerBeacon))
        {
            if ((b.Position - Module.Center).LengthSq() < BeaconMinRadius * BeaconMinRadius)
                continue;
            if (_landings.Any(l => l.AlmostEqual(b.Position, 1)))
                continue;
            _landings.Add(b.Position);
        }
    }
}

class FlowingLance(BossModule module) : Components.GenericAOEs(module)
{
    private class Lance(WPos origin, Angle baseRot, Angle increment, DateTime firstActivation)
    {
        public WPos Origin = origin;
        public Angle BaseRot = baseRot;
        public Angle Increment = increment;
        public DateTime FirstActivation = firstActivation;
        public int NumFinished;
        public DateTime? ImminentOverride;
    }

    private readonly List<Lance> _lances = [];
    private static readonly AOEShapeCross _shape = new(24, 6);
    private const int TotalCasts = 7;
    private const int MaxShown = 2;
    private const float Step = 2.1f;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var l in _lances)
        {
            var remaining = TotalCasts - l.NumFinished;
            if (remaining <= 0)
                continue;

            var show = Math.Min(MaxShown, remaining);
            for (var i = 0; i < show; ++i)
            {
                var idx = l.NumFinished + i;
                var rot = l.BaseRot + idx * l.Increment;
                var time = i == 0 && l.ImminentOverride is { } im ? im : l.FirstActivation.AddSeconds(Step * idx);
                yield return new(_shape, l.Origin, rot, time, i == 0 ? ArenaColor.Danger : ArenaColor.AOE);
            }
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.FlowingLanceCW:
                _lances.Add(new(caster.Position, spell.Rotation, -15.Degrees(), Module.CastFinishAt(spell)));
                break;
            case AID.FlowingLanceCCW:
                _lances.Add(new(caster.Position, spell.Rotation, 15.Degrees(), Module.CastFinishAt(spell)));
                break;
            case AID.FlowingLanceRest:
                if (FindLance(caster.Position, spell.LocXZ) is { } l)
                {
                    l.BaseRot = spell.Rotation - l.NumFinished * l.Increment;
                    l.ImminentOverride = Module.CastFinishAt(spell);
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.FlowingLanceCW or AID.FlowingLanceCCW or AID.FlowingLanceRest))
            return;

        if (FindLance(caster.Position, spell.TargetXZ) is not { } l)
        {
            ReportError($"Failed to find FlowingLance near {caster.Position}");
            return;
        }

        ++l.NumFinished;
        ++NumCasts;
        l.ImminentOverride = null;
        if (l.NumFinished >= TotalCasts)
            _lances.Remove(l);
    }

    private Lance? FindLance(WPos casterPos, WPos locXZ) => _lances.FirstOrDefault(l => l.Origin.AlmostEqual(casterPos, 1)) ?? _lances.FirstOrDefault(l => l.Origin.AlmostEqual(locXZ, 1));
}

class CloudToGround(BossModule module) : Components.Exaflare(module, 6, AID.CloudToGroundFirst)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            var advance = 6 * spell.Rotation.ToDirection();
            var max = Math.Clamp((int)(Module.Bounds.Radius * 2 / 6) + 1, 1, 8);
            Lines.Add(new()
            {
                Next = caster.Position,
                Advance = advance,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 1,
                ExplosionsLeft = max,
                MaxShownExplosions = 4
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.CloudToGroundFirst or AID.CloudToGroundRest))
            return;

        ++NumCasts;
        var index = FindLine(caster.Position);
        if (index == -1)
        {
            ReportError($"Failed to find CloudToGround entry near {caster.Position}");
            return;
        }

        AdvanceLine(Lines[index], caster.Position);
        if (Lines[index].ExplosionsLeft == 0 || !Module.InBounds(Lines[index].Next))
            Lines.RemoveAt(index);
    }

    private int FindLine(WPos pos)
    {
        var index = Lines.FindIndex(l => l.Next.AlmostEqual(pos, 1));
        if (index >= 0)
            return index;

        var best = -1;
        var bestDist = 9f;
        for (var i = 0; i < Lines.Count; ++i)
        {
            var d = (Lines[i].Next - pos).LengthSq();
            if (d < bestDist)
            {
                bestDist = d;
                best = i;
            }
        }
        return best;
    }
}

class V031QuaquaStates : StateMachineBuilder
{
    public V031QuaquaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<MadeMagic>()
            .ActivateOnEnter<ArcaneArmaments>()
            .ActivateOnEnter<Rout>()
            .ActivateOnEnter<HammerLanding>()
            .ActivateOnEnter<ElementalImpact>()
            .ActivateOnEnter<ArcaneArmamentsPoison>()
            .ActivateOnEnter<FlowingLance>()
            .ActivateOnEnter<VioletStorm>()
            .ActivateOnEnter<ScaldingWaves>()
            .ActivateOnEnter<ScaldingWavesFairSkies>()
            .ActivateOnEnter<CloudToGround>()
            .ActivateOnEnter<ArcaneIntervention>()
            .ActivateOnEnter<ArcanePursuit>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 961, NameID = 12527, Contributors = "croizat")]
public class V031Quaqua(WorldState ws, Actor primary) : BossModule(ws, primary, new(primary.Position.X, primary.Position.Z), new ArenaBoundsCircle(20f));

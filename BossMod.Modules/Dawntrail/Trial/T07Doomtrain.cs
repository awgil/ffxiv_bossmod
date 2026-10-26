using System.Runtime.InteropServices;

namespace BossMod.Dawntrail.Trial.T07Doomtrain;

public enum OID : uint
{
    Boss = 0x4A30, // R19.040, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    HeadlightTrain = 0x4A33, // R1.000, x1, Helper type
    LevinSignal = 0x4A31, // R1.000, x0 (spawn during fight), plasma beam portals
    KinematicTurret = 0x4A32, // R1.200, x0 (spawn during fight), side lasers
    AetherIntermission = 0x4A34, // R1.500, x0 (spawn during fight)
    IntermissionTrain = 0x4B7E, // R19.040, x0 (spawn during fight)
    GhostTrain = 0x4B80, // R2.720, x0 (spawn during fight)
    ArcaneRevelation = 0x4A36, // R1.000, x0 (spawn during fight), hail of thunder indicator
}

public enum AID : uint
{
    AutoAttack = 45662, // Helper->player, no cast, single-target
    LightningBurstVisual = 45660, // Boss->self, 5.0s cast, single-target
    LightningBurst = 45661, // Helper->player, 0.5s cast, range 5 circle, tankbuster
    LightningExpress = 45618, // Boss->self, 6.0s cast, range 70 width 70 rect, knockback to the back of the car
    PlasmaBeamOverhead = 45620, // LevinSignal->self, 1.0s cast, range 30 width 5 rect
    PlasmaBeamGround = 45619, // LevinSignal->self, 1.0s cast, range 30 width 5 rect
    PlasmaBeamMedium = 45621, // LevinSignal->self, 1.0s cast, range 20 width 5 rect
    PlasmaBeamShort = 45622, // LevinSignal->self, 1.0s cast, range 10 width 5 rect
    WindpipeVisual = 45625, // Boss->self, 6.0+1.0s cast, single-target
    Windpipe = 45667, // Helper->self, 7.0s cast, range 30 width 20 rect, draw-in towards the boss
    BlastpipeVisual = 45626, // Boss->self, no cast, single-target
    Blastpipe = 45627, // Helper->self, 3.0s cast, range 10 width 20 rect
    UnlimitedExpressVisual = 45623, // Boss->self, 5.0s cast, single-target
    UnlimitedExpress = 45624, // Helper->self, 5.9s cast, range 70 width 70 rect, raidwide + next car
    Zoom = 45641, // Boss->location, no cast, single-target
    TurretCrossing = 45628, // Boss->self, 3.0s cast, single-target
    ElectrayLong = 45629, // KinematicTurret->self, 5.0s cast, range 25 width 5 rect
    ElectrayUpper = 45630, // KinematicTurret->self, 5.0s cast, range 10 width 5 rect, platform
    Electray20 = 45631, // KinematicTurret->self, 5.0s cast, range 20 width 5 rect
    ElectrayMedium = 45632, // KinematicTurret->self, 5.0s cast, range 15 width 5 rect
    ElectrayShort = 45633, // KinematicTurret->self, 5.0s cast, range 10 width 5 rect
    HeadOnEmissionGround = 45634, // Boss->self, 6.0+1.0s cast, single-target
    ThunderousBreath = 45635, // Helper->self, 7.0s cast, range 70 width 70 rect, lower deck
    HeadOnEmissionAir = 45636, // Boss->self, 6.0+1.0s cast, single-target
    Headlight = 45637, // HeadlightTrain->self, 7.0s cast, range 30 width 20 rect, platforms
    RunawayTrain = 45638, // Boss->self, 5.0s cast, single-target, intermission start
    Overdraught = 45639, // AetherIntermission->self, no cast, single-target
    AetherialRay = 45640, // Helper->self, no cast, range 50 35-degree cone, tankbuster
    AetherSurgeVisual = 45642, // AetherIntermission->self, 6.0s cast, single-target
    AetherSurge = 45643, // Helper->self, 6.0s cast, range 30 45-degree cone
    RunawayTrainVisual = 45644, // IntermissionTrain->self, no cast, single-target
    RunawayTrainRaidwide = 45645, // Helper->self, no cast, range 20 circle
    ShockwaveVisual = 45646, // Boss->self, no cast, single-target
    Shockwave = 45647, // Helper->self, no cast, range 50 circle
    DerailmentSiegeVisual = 45648, // Boss->self, 6.0+1.0s cast, single-target
    DerailmentSiegeFinal = 45649, // Helper->self, 10.0s cast, range 5 circle, tower
    DerailmentSiegeHit = 45650, // Helper->self, no cast, range 5 circle
    DerailmentSiegeHit2 = 45651, // Helper->self, 0.5s cast, range 5 circle
    DerailVisual = 45653, // Boss->self, 10.1s cast, single-target
    Derail = 45654, // Helper->self, 10.0s cast, range 30 width 20 rect
    DerailMove = 45655, // Boss->location, no cast, single-target
    HailOfThunderShort = 45656, // Boss->self, no cast, single-target, indicator moves 2 (or not at all)
    HailOfThunderMedium = 45657, // Boss->self, no cast, single-target, indicator moves 3
    HailOfThunder = 45659, // Helper->location, 3.2s cast, range 16 circle
    ArcaneRevelation = 47527, // Boss->self, 2.0+1.0s cast, single-target
    BatteringArmsVisual = 47529, // Boss->self, 6.0+1.0s cast, single-target
    BatteringArmsHit = 47236, // Helper->self, no cast, range 5 circle
    BatteringArmsHit2 = 47237, // Helper->self, 0.5s cast, range 5 circle
}

public enum SID : uint
{
    Distance = 4541, // none->GhostTrain, extra=0x578
    Stop = 4176, // none->GhostTrain, extra=0x0
}

public enum IconID : uint
{
    LightningBurst = 343, // player->self
    Horn = 642, // player->self, aetherial ray target
}

static class Shapes
{
    public static Sdf RectOutline(WPos center, float halfLength, float halfWidth) => Sdf.Discrete(p => p.InRect(center, default(Angle), halfLength + 0.25f, halfLength + 0.25f, halfWidth + 0.25f) && !p.InRect(center, default(Angle), halfLength - 0.25f, halfLength - 0.25f, halfWidth - 0.25f));
}

class CarGeometry : BossComponent
{
    public int Car
    {
        get;
        set
        {
            field = value;
            switch (value)
            {
                case 1:
                    SetPlain(new(100, 100));
                    break;
                case 2:
                    Car2();
                    break;
                case 3:
                    Car3();
                    break;
                case 4:
                    SetPlain(new(100, 250));
                    break;
                case 5:
                    Car5();
                    break;
            }
        }
    } = 1;

    public const float CarHeight = 14.6f;

    public AOEShape GroundShape { get; private set; } = new AOEShapeRect(CarHeight, 10, CarHeight);
    public AOEShape AirShape { get; private set; } = new AOEShapeRect(0, 0, 0);
    private List<(WPos from, WPos to)> _portals = [];
    private List<(WPos center, float halfWidth, float halfLength)> _platforms = [];
    public IEnumerable<WPos> Portals => _portals.Select(p => p.from);

    private PolygonClipper Clipper => Arena.Bounds.Clipper;

    public CarGeometry(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_platforms.Count > 0)
            AirShape.Outline(Arena, Arena.Center, default, ArenaColor.Border);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var p in Portals)
            Arena.AddCircle(p, 1, ArenaColor.Safe);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.Portals.AddRange(_portals.Select(p => (p.from, 1f, p.to)));
        if (actor.PosRot.Y < 4)
            foreach (var (c, hw, hl) in _platforms)
                hints.TemporaryObstacles.Add(Shapes.RectOutline(c, hl, hw));
    }

    private void SetPlain(WPos center)
    {
        Arena.Bounds = new ArenaBoundsRect(10, CarHeight);
        Arena.Center = center;
        GroundShape = new AOEShapeRect(CarHeight, 10, CarHeight);
        AirShape = new AOEShapeRect(0, 0, 0);
        _portals = [];
        _platforms = [];
    }

    private void Car2()
    {
        var rect = CurveApprox.Rect(new WDir(10, 0), new WDir(0, CarHeight));
        var crate1 = CurveApprox.Rect(new WDir(2.5f, -2.5f), new WDir(2.5f, 0), new WDir(0, 2.5f));
        var crate2 = CurveApprox.Rect(new WDir(-2.5f, 7.5f), new WDir(2.5f, 0), new WDir(0, 2.5f));
        var poly = Clipper.Difference(new(rect), new(crate1));
        poly = Clipper.Difference(new(poly), new(crate2));
        Arena.Bounds = new ArenaBoundsCustom(CarHeight, poly);
        Arena.Center = new(100, 150);
        GroundShape = new AOEShapeRect(CarHeight, 10, CarHeight);
        AirShape = new AOEShapeRect(0, 0, 0);
        _portals = [];
        _platforms = [];
    }

    private void Car3() => SetPlatforms(new(100, 200), [(new(-7.5f, 5), 2.5f, 10.1f), (new(7.5f, 5), 2.5f, 10.1f)], [(new(-3.9f, 4.9f), new(-7.6f, 4.9f)), (new(4.1f, 4.9f), new(7.6f, 4.9f))]);

    private void Car5() => SetPlatforms(new(100, 300), [(new(-7.5f, 10), 2.5f, 5), (new(7.5f, 0), 2.5f, 5)], [(new(-3.9f, 10), new(-7.6f, 10)), (new(4.1f, 0), new(7.6f, 0))]);

    private void SetPlatforms(WPos center, (WDir offset, float halfWidth, float halfLength)[] platforms, (WDir from, WDir to)[] portals)
    {
        var rect = CurveApprox.Rect(new WDir(10, 0), new WDir(0, CarHeight));
        RelSimplifiedComplexPolygon? air = null;
        foreach (var (o, hw, hl) in platforms)
        {
            var p = CurveApprox.Rect(o, new WDir(hw, 0), new WDir(0, hl));
            air = air == null ? new(p) : Clipper.Union(new(air), new(p));
        }
        Arena.Bounds = new ArenaBoundsRect(10, CarHeight);
        Arena.Center = center;
        AirShape = new AOEShapeCustom(air!);
        GroundShape = new AOEShapeCustom(Clipper.Difference(new(rect), new(air!)));
        _platforms = [.. platforms.Select(p => (center + p.offset, p.halfWidth, p.halfLength))];
        _portals = [.. portals.Select(p => (center + p.from, center + p.to))];
    }

    public void MultiCar4()
    {
        var car4 = CurveApprox.Rect(new WDir(0, -25), new WDir(10, 0), new WDir(0, CarHeight));
        var car5 = CurveApprox.Rect(new WDir(0, 25), new WDir(10, 0), new WDir(0, CarHeight));
        Arena.Bounds = new ArenaBoundsCustom(39.5f, Clipper.Union(new(car4), new(car5)));
        Arena.Center = new(100, 275);
    }
}

class LevinSignal(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, DateTime Activation, float Length, float Offset)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casters.Select(c => new AOEInstance(new AOEShapeRect(c.Length, 2.5f), c.Caster.Position + new WDir(0, c.Offset), default, c.Activation));

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID != OID.LevinSignal)
            return;

        var ground = actor.PosRot.Y < 2;
        var car = Module.FindComponent<CarGeometry>()?.Car ?? 0;
        var x = actor.Position.X;
        float length = ground ? 30 : 0;
        float offset = 0;
        switch (car)
        {
            case 2:
                if (x is > 95 and < 100)
                    length = 20;
                else if (x is > 100 and < 105)
                    length = 10;
                break;
            case 3:
                if (x is < 95 or > 105)
                    (length, offset) = ground ? (10, 0) : (20, 10);
                break;
            case 5:
                if (x < 95)
                    (length, offset) = ground ? (20, 0) : (10, 20);
                else if (x > 105)
                    (length, offset) = ground ? (10, 0) : (10, 10);
                break;
        }
        if (length > 0)
            _casters.Add((actor, WorldState.FutureTime(6.4f), length, offset));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PlasmaBeamGround or AID.PlasmaBeamOverhead or AID.PlasmaBeamMedium or AID.PlasmaBeamShort)
        {
            _casters.RemoveAll(c => c.Caster == caster);
            NumCasts++;
        }
    }
}

class UnlimitedExpress(BossModule module) : Components.RaidwideCast(module, AID.UnlimitedExpress);

class Electray(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<(Actor Caster, float Length)> _casters = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, l) in _casters)
            if (c.CastInfo != null)
                yield return new AOEInstance(new AOEShapeRect(l, 2.5f), c.CastInfo.LocXZ, c.CastInfo.Rotation, Module.CastFinishAt(c.CastInfo));
    }

    private static float Length(AID aid) => aid switch
    {
        AID.ElectrayLong => 25,
        AID.Electray20 => 20,
        AID.ElectrayMedium => 15,
        AID.ElectrayShort or AID.ElectrayUpper => 10,
        _ => 0
    };

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var len = Length((AID)spell.Action.ID);
        if (len > 0)
            _casters.Add((caster, len));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (Length((AID)spell.Action.ID) > 0)
        {
            NumCasts++;
            _casters.RemoveAll(c => c.Caster == caster);
        }
    }
}

class LightningBurst(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(5), (uint)IconID.LightningBurst, AID.LightningBurst, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class LightningExpress(BossModule module) : Components.KnockbackFromCastTarget(module, AID.LightningExpress, 30, kind: Kind.DirForward, stopAtWall: true);

class Windpipe(BossModule module) : Components.KnockbackFromCastTarget(module, AID.WindpipeVisual, 20, kind: Kind.DirForward, stopAtWall: true)
{
    public override IEnumerable<Source> Sources(int slot, Actor actor) => base.Sources(slot, actor).Select(src => src with { Direction = src.Direction + 180.Degrees() });
    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => pos.InRect(Arena.Center - new WDir(0, 15), new WDir(0, 10), 10);
}

class Blastpipe(BossModule module) : Components.GenericAOEs(module, AID.Blastpipe)
{
    private DateTime _activation;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeRect(10, 10), Arena.Center - new WDir(0, 15), default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Windpipe)
            _activation = Module.CastFinishAt(spell, 2);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
        }
    }
}

class HeadOnEmission(BossModule module) : Components.GenericAOEs(module)
{
    private readonly CarGeometry _car = module.FindComponent<CarGeometry>()!;
    private readonly List<(bool Air, DateTime Activation)> _casts = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _casts.Take(1).Select(c => new AOEInstance(c.Air ? _car.AirShape : _car.GroundShape, Arena.Center, default, c.Activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ThunderousBreath:
                _casts.Add((false, Module.CastFinishAt(spell)));
                break;
            case AID.Headlight:
                _casts.Add((true, Module.CastFinishAt(spell)));
                break;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_casts.Count > 0)
            hints.Add(_casts[0].Air ? "Stay on the ground!" : "Go up on a platform!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_casts.Count > 0 && _casts[0].Air)
            foreach (var p in _car.Portals)
                hints.AddForbiddenZone(ShapeDistance.Circle(p, 1.5f), _casts[0].Activation);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.ThunderousBreath or AID.Headlight)
        {
            NumCasts++;
            if (_casts.Count > 0)
                _casts.RemoveAt(0);
        }
    }
}

class AetherSurge(BossModule module) : Components.StandardAOEs(module, AID.AetherSurge, new AOEShapeCone(30, 22.5f.Degrees()));

class AetherialRay(BossModule module) : Components.GenericBaitAway(module, AID.AetherialRay, damageType: AIHints.PredictedDamageType.Tankbuster)
{
    private Angle _travel;
    private float _direction = -1;

    public override void Update()
    {
        if (Module.Enemies(OID.GhostTrain).FirstOrDefault() is { } train && train.LastFrameMovement != default)
        {
            var sign = MathF.Sign((train.Position - Arena.Center).OrthoL().Dot(train.LastFrameMovement));
            if (sign != 0)
                _direction = sign;
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Distance)
        {
            _travel = status.Extra switch
            {
                0x578 => 139.Degrees(),
                _ => default
            };
            if (_travel == default)
                ReportError($"Unrecognized status {status.Extra:X} on Ghost Train, don't know where to predict");
        }

        if ((SID)status.ID == SID.Stop)
            foreach (ref var bait in CollectionsMarshal.AsSpan(CurrentBaits))
                bait.Source = actor;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if ((IconID)iconID != IconID.Horn || Module.Enemies(OID.GhostTrain).FirstOrDefault() is not { } train || WorldState.Actors.Find(targetID) is not { } target)
            return;
        var dir = (train.Position - Arena.Center).ToAngle() + _direction * _travel;
        var pos = Arena.Center + dir.ToDirection() * 25;
        var source = new Actor(1, 0, 819, 0, "fake actor", 0, ActorType.Enemy, Class.ACN, 1, new Vector4(pos.X, train.PosRot.Y, pos.Z, 0));
        CurrentBaits.Add(new(source, target, new AOEShapeCone(50, 17.5f.Degrees()), WorldState.FutureTime(7.6f)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            CurrentBaits.Clear();
        }
    }
}

class RunawayTrain(BossModule module) : Components.RaidwideInstant(module, AID.RunawayTrainRaidwide, 15.2f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.RunawayTrainVisual)
            Activation = WorldState.FutureTime(Delay);
    }
}

class Shockwave(BossModule module) : Components.RaidwideInstant(module, AID.Shockwave, 5.8f)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if ((AID)spell.Action.ID == AID.ShockwaveVisual)
            Activation = WorldState.FutureTime(Delay);
    }
}

class HailOfThunder(BossModule module) : Components.GenericAOEs(module, AID.HailOfThunder)
{
    private static readonly WDir[] Spots = [new(0, -10), new(-4.8f, 0), new(0, 10), new(4.8f, 0)];
    private int _advance;
    private DateTime _signal;
    private DateTime _activation;
    private WPos? _next;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_next != null)
            yield return new(new AOEShapeCircle(16), _next.Value, default, _activation);
    }

    private int NearestSpot(WPos pos)
    {
        var best = 0;
        for (var i = 1; i < Spots.Length; ++i)
            if ((Arena.Center + Spots[i] - pos).LengthSq() < (Arena.Center + Spots[best] - pos).LengthSq())
                best = i;
        return best;
    }

    public override void Update()
    {
        if (_advance == 0 || Module.Enemies(OID.ArcaneRevelation).LastOrDefault() is not { } indicator)
            return;

        if (indicator.LastFrameMovement != default)
        {
            var from = NearestSpot(indicator.Position - indicator.LastFrameMovement);
            var sign = MathF.Sign(indicator.LastFrameMovement.OrthoL().Dot(indicator.DirectionTo(Arena.Center)));
            var index = ((from + _advance * sign) % 4 + 4) % 4;
            _next = Arena.Center + Spots[index];
            _advance = 0;
        }
        else if (WorldState.CurrentTime > _signal.AddSeconds(1.5f))
        {
            _next = Arena.Center + Spots[NearestSpot(indicator.Position)];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
        {
            _advance = 0;
            _next = spell.LocXZ;
            _activation = Module.CastFinishAt(spell);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.HailOfThunderShort:
                Signal(2, 7.7f);
                break;
            case AID.HailOfThunderMedium:
                Signal(3, 10.8f);
                break;
            case AID.HailOfThunder:
                NumCasts++;
                _next = null;
                _advance = 0;
                break;
        }
    }

    private void Signal(int advance, float delay)
    {
        _advance = advance;
        _signal = WorldState.CurrentTime;
        _activation = WorldState.FutureTime(delay);
        _next = null;
    }
}

class DerailmentSiege(BossModule module) : Components.GenericTowers(module)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.DerailmentSiegeFinal)
            Towers.Add(new(caster.Position, 5, maxSoakers: 8, activation: Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.DerailmentSiegeFinal:
                NumCasts++;
                Towers.Clear();
                break;
            case AID.DerailmentSiegeHit:
            case AID.DerailmentSiegeHit2:
            case AID.BatteringArmsHit:
            case AID.BatteringArmsHit2:
                NumCasts++;
                break;
        }
    }
}

class Derail(BossModule module) : Components.CastCounter(module, AID.Derail);

class Launchpad(BossModule module) : BossComponent(module)
{
    private WPos? _pad;
    private const float Radius = 2;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x0A && state == 0x00020001)
            _pad = new(100, 262.5f);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_pad != null && pc.Position.Z < _pad.Value.Z + Radius)
            Arena.ZoneCircle(_pad.Value, Radius, ArenaColor.SafeFromAOE);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_pad != null && actor.Position.Z < _pad.Value.Z + Radius)
            hints.Add("Get to launchpad!", !actor.Position.InCircle(_pad.Value, Radius));
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_pad != null && actor.Position.Z < _pad.Value.Z + Radius)
            hints.AddForbiddenZone(ShapeDistance.Donut(_pad.Value, Radius, 100));
    }
}

class T07DoomtrainStates : StateMachineBuilder
{
    public T07DoomtrainStates(BossModule module) : base(module)
    {
        DeathPhase(0, SinglePhase)
            .ActivateOnEnter<CarGeometry>();
    }

    private void SinglePhase(uint id)
    {
        Car1(id, 10);
        Car2(id + 0x10000, 5.7f);
        Car3(id + 0x20000, 5.8f);
        Car4(id + 0x30000, 5.8f);
        Intermission(id + 0x40000, 3.9f);
        Car4Return(id + 0x50000, 1.4f);
        Car5(id + 0x60000, 11.3f);
    }

    private void Car1(uint id, float delay)
    {
        Tankbusters(id, delay)
            .ActivateOnEnter<LevinSignal>()
            .ActivateOnEnter<LightningExpress>()
            .ActivateOnEnter<Windpipe>()
            .ActivateOnEnter<Blastpipe>()
            .ActivateOnEnter<UnlimitedExpress>();
        ExpressOrWindpipe(id + 0x10, 2.6f);
        ExpressOrWindpipe(id + 0x20, 5.4f);
        NextCar(id + 0x30, 7.2f, 2);
    }

    private void Car2(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss reappears");
        Cast(id + 0x10, AID.TurretCrossing, 4.3f, 2.7f)
            .ActivateOnEnter<Electray>();
        ComponentCondition<Electray>(id + 0x20, 11.2f, e => e.NumCasts >= 6, "Lasers");
        ExpressOrWindpipe(id + 0x30, 3.1f);
        ComponentCondition<Electray>(id + 0x40, 5.2f, e => e.NumCasts >= 12, "Lasers");
        ExpressOrWindpipe(id + 0x50, 3.2f);
        ComponentCondition<Electray>(id + 0x60, 5.2f, e => e.NumCasts >= 18, "Lasers")
            .DeactivateOnExit<Electray>();
        Tankbusters(id + 0x70, 2.1f);
        NextCar(id + 0x80, 3.9f, 3);
    }

    private void Car3(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss reappears")
            .ActivateOnEnter<HeadOnEmission>();
        HeadOn(id + 0x10, 6.2f, 1);
        HeadOn(id + 0x20, 3.4f, 2);
        ExpressOrWindpipe(id + 0x30, 3.3f);
        Tankbusters(id + 0x40, 5.3f);
        NextCar(id + 0x50, 3.9f, 4);
    }

    private void Car4(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss reappears");
        Cast(id + 0x10, AID.RunawayTrain, 4.2f, 4.7f);
    }

    private void Intermission(uint id, float delay)
    {
        Targetable(id, false, delay, "Intermission")
            .ActivateOnEnter<AetherSurge>()
            .ActivateOnEnter<AetherialRay>()
            .ActivateOnEnter<RunawayTrain>()
            .SetHint(StateMachine.StateHint.DowntimeStart);
        ActorTargetable(id + 0x10, () => Module.Enemies(OID.AetherIntermission).FirstOrDefault(), true, 7, "Orb appears")
            .OnEnter(() =>
            {
                Module.Arena.Bounds = new ArenaBoundsCircle(14.5f);
                Module.Arena.Center = new(-400, -400);
            })
            .SetHint(StateMachine.StateHint.DowntimeEnd);
        ComponentCondition<AetherSurge>(id + 0x20, 10.7f, a => a.NumCasts >= 4, "Cones 1", 2);
        ComponentCondition<AetherialRay>(id + 0x21, 1.8f, a => a.NumCasts >= 1, "Tank cone 1")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AetherSurge>(id + 0x30, 8.7f, a => a.NumCasts >= 8, "Cones 2");
        ComponentCondition<AetherialRay>(id + 0x31, 1.8f, a => a.NumCasts >= 2, "Tank cone 2")
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<AetherSurge>(id + 0x40, 7.3f, a => a.NumCasts >= 12, "Cones 3")
            .DeactivateOnExit<AetherSurge>();
        ComponentCondition<AetherialRay>(id + 0x41, 1.8f, a => a.NumCasts >= 3, "Tank cone 3")
            .DeactivateOnExit<AetherialRay>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        ComponentCondition<RunawayTrain>(id + 0x50, 23.9f, r => r.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<RunawayTrain>()
            .ExecOnExit<CarGeometry>(c => c.Car = 4)
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void Car4Return(uint id, float delay)
    {
        Targetable(id, true, delay, "Boss reappears")
            .ActivateOnEnter<Shockwave>();
        ComponentCondition<Shockwave>(id + 0x10, 17.9f, s => s.NumCasts > 0, "Raidwide")
            .DeactivateOnExit<Shockwave>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Hails(id + 0x20, 5.4f, 14.1f, 14.1f);
        Cast(id + 0x30, AID.DerailmentSiegeVisual, 3.3f, 5.7f)
            .ActivateOnEnter<DerailmentSiege>()
            .ActivateOnEnter<Launchpad>()
            .ActivateOnEnter<Derail>();
        ComponentCondition<DerailmentSiege>(id + 0x40, 4.3f, d => d.NumCasts >= 4, "Tower")
            .DeactivateOnExit<DerailmentSiege>()
            .ExecOnExit<CarGeometry>(c => c.MultiCar4())
            .SetHint(StateMachine.StateHint.Raidwide);
        Cast(id + 0x50, AID.DerailVisual, 0.1f, 10.1f, "Derail")
            .DeactivateOnExit<Derail>()
            .DeactivateOnExit<Launchpad>()
            .ExecOnExit<CarGeometry>(c => c.Car = 5);
    }

    private void Car5(uint id, float delay)
    {
        Hails(id, delay, 17.1f, 11.1f);
        Cast(id + 0x10, AID.TurretCrossing, 4.3f, 2.7f)
            .ActivateOnEnter<Electray>();
        HeadOn(id + 0x20, 2.3f, 3);
        ComponentCondition<Electray>(id + 0x30, 3.3f, e => e.NumCasts >= 6, "Lasers");
        HeadOn(id + 0x40, 3.1f, 4);
        ComponentCondition<Electray>(id + 0x50, 3.2f, e => e.NumCasts >= 12, "Lasers");
        ExpressOrWindpipe(id + 0x60, 3.1f);
        ComponentCondition<Electray>(id + 0x68, 5.2f, e => e.NumCasts >= 18, "Lasers")
            .DeactivateOnExit<Electray>();
        Cast(id + 0x70, AID.BatteringArmsVisual, 5.2f, 5.7f)
            .ActivateOnEnter<DerailmentSiege>();
        ComponentCondition<DerailmentSiege>(id + 0x80, 4.2f, d => d.NumCasts >= 4, "Tower")
            .DeactivateOnExit<DerailmentSiege>()
            .SetHint(StateMachine.StateHint.Raidwide);
        Tankbusters(id + 0x90, 2.2f);
        Hails(id + 0xA0, 7.8f, 14.1f, 14.1f);
        // TODO: timeline after the last Arcane Revelation + enrage unseen (both kills ended before it)
        SimpleState(id + 0xFF0000, 10000, "???");
    }

    private State Tankbusters(uint id, float delay)
    {
        var start = Cast(id, AID.LightningBurstVisual, delay, 4.7f)
            .ActivateOnEnter<LightningBurst>();
        ComponentCondition<LightningBurst>(id + 2, 0.6f, l => l.NumCasts > 0, "Tankbusters")
            .DeactivateOnExit<LightningBurst>()
            .SetHint(StateMachine.StateHint.Tankbuster);
        return start;
    }

    private void ExpressOrWindpipe(uint id, float delay)
    {
        CastStartMulti(id, [AID.LightningExpress, AID.WindpipeVisual], delay);
        CastEnd(id + 1, 5.7f, "Knockback / pull");
    }

    private void HeadOn(uint id, float delay, int count)
    {
        CastStartMulti(id, [AID.HeadOnEmissionGround, AID.HeadOnEmissionAir], delay);
        ComponentCondition<HeadOnEmission>(id + 1, 6.7f, h => h.NumCasts >= count, "Ground / platforms");
    }

    private void Hails(uint id, float delay, float first, float second)
    {
        Cast(id, AID.ArcaneRevelation, delay, 1.7f)
            .ActivateOnEnter<HailOfThunder>();
        ComponentCondition<HailOfThunder>(id + 2, first, h => h.NumCasts >= 1, "Hail 1", 4);
        ComponentCondition<HailOfThunder>(id + 3, second, h => h.NumCasts >= 2, "Hail 2", 4)
            .DeactivateOnExit<HailOfThunder>();
    }

    private void NextCar(uint id, float delay, int car)
    {
        Cast(id, AID.UnlimitedExpressVisual, delay, 4.7f);
        ComponentCondition<UnlimitedExpress>(id + 2, 0.9f, u => u.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide);
        Targetable(id + 3, false, 1.1f, $"Car {car}")
            .ExecOnExit<CarGeometry>(c => c.Car = car);
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1076, NameID = 14284)]
public class T07Doomtrain(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsRect(10, CarGeometry.CarHeight))
{
    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        if (PrimaryActor.IsTargetable)
        {
            var height = Arena.Bounds.Radius;
            if (PrimaryActor.Position.InRect(Arena.Center, default(Angle), height + 12, height + 12, 10))
                Arena.ActorInsideBounds(Arena.Center - new WDir(0, height), PrimaryActor.Rotation, ArenaColor.Enemy);
            else
                Arena.ActorOutsideBounds(Arena.Center - new WDir(0, height), PrimaryActor.Rotation, ArenaColor.Enemy);
        }

        Arena.Actors(Enemies(OID.AetherIntermission), ArenaColor.Enemy);
        Arena.Actors(Enemies(OID.GhostTrain), ArenaColor.Object, true);
        foreach (var obj in Enemies(OID.ArcaneRevelation))
            Arena.ActorInsideBounds(obj.Position, obj.Rotation, ArenaColor.Object);
    }
}

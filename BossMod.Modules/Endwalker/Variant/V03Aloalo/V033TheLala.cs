namespace BossMod.Endwalker.Variant.V03Aloalo.V033TheLala;

public enum OID : uint
{
    Boss = 0x4039, // R4.500, x1
    Helper = 0x233C, // R0.500, x?, Helper type
    Rodiaki = 0x403C, // R2.000, x?, Faunal Figure armadillos
    ArrowBright = 0x1EB941, // R0.500, EventObj type, Arcane Plot arrows
    ArrowDim = 0x1EB942, // R0.500, EventObj type, Arcane Plot redirect arrows
    Kapokapo = 0x403B, // R2.040, x?, route 6 Floral Figure seedlings
    AloaloGolem = 0x403D, // R1.900, x?, route 7 Constructive Figure golems
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Teleport = 34932, // Boss->location, no cast, single-target

    InfernoTheorem = 34943, // Boss->self, 5.0s cast, range 80 circle, raidwide
    StrategicStrike = 34942, // Boss->players, 5.0s cast, single-target, tankbuster

    ArcaneBlightVisual1 = 34927, // Boss->self, 6.0s cast, single-target
    ArcaneBlightVisual2 = 34928, // Boss->self, 6.0s cast, single-target
    ArcaneBlightVisual3 = 34929, // Boss->self, 6.0s cast, single-target
    ArcaneBlightVisual4 = 34930, // Boss->self, 6.0s cast, single-target
    ArcaneBlight = 34931, // Helper->self, 6.0s cast, range 60 270-degree cone

    ArcanePlot = 34933, // Boss->self, 5.0s cast, single-target
    ArcanePlotAlt = 34934, // Boss->self, 5.0s cast, single-target
    BrightPulseFirst = 34936, // Helper->self, 12.0s cast, range 8 width 8 rect
    BrightPulseRest = 34937, // Helper->self, no cast, range 8 width 8 rect

    Analysis = 34939, // Boss->self, 3.0s cast, single-target, apply Unseen
    TargetedLight = 36060, // Boss->self, 9.5+0.5s cast, single-target
    TargetedLightAOE = 36061, // Helper->player, 10.0s cast, single-target, inverted gaze

    CalculatedTrajectory = 34941, // Boss->self, 3.0s cast, single-target, Forced March setup

    FaunalFigure = 34946, // Boss->self, 3.0s cast, single-target, spawn Rodiaki
    FlailSmash = 34947, // Rodiaki->location, 7.5s cast, range 80 circle, proximity
    FlailSmashLong = 35436, // Rodiaki->location, 20.0s cast, range 80 circle, proximity (with Arcane Plot)

    FloralFigure = 34944, // Boss->self, 3.0s cast, single-target, spawn Kapokapo
    RollingSpout = 34945, // Kapokapo->self, 5.0s cast, range 4-12 donut

    ConstructiveFigure = 34948, // Boss->self, 3.0s cast, single-target, spawn AloaloGolem
    AeroII = 34949, // AloaloGolem->self, 9.3s cast, range 50 width 8 rect

    VolcanicCoordinates = 36140, // Boss->self, 3.0s cast, single-target, visual
    VolcanicCoordinatesAOE = 36141, // Helper->location, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    TimesThreePlayer = 3721, // Boss->player
    TimesFivePlayer = 3790, // Boss->player
    FrontUnseen = 3726, // Boss->player
    BackUnseen = 3727, // Boss->player
    RightUnseen = 3728, // Boss->player
    LeftUnseen = 3729, // Boss->player
    ForwardMarch = 3715, // Boss->player
    ForcedMarch = 3719, // Boss->player
}

public enum IconID : uint
{
    BossRotateCW = 484, // Boss
    BossRotateCCW = 485, // Boss
    PlayerRotateCW = 493, // player
    PlayerRotateCCW = 494, // player
}

public enum TetherID : uint
{
    TargetedLight = 17, // player->Boss
}

class InfernoTheorem(BossModule module) : Components.RaidwideCast(module, AID.InfernoTheorem);
class StrategicStrike(BossModule module) : Components.SingleTargetCast(module, AID.StrategicStrike);
class ArcaneBlight(BossModule module) : Components.StandardAOEs(module, AID.ArcaneBlight, new AOEShapeCone(60, 135.Degrees()));

class ArcanePlot(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];
    public readonly List<WPos> SafeZoneCenters = [];
    public static readonly AOEShapeRect Shape = new(4, 4, 4);
    private DateTime _expire;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public bool IsTileUnsafe(WPos pos) => _aoes.Any(a => a.Check(pos));

    public IEnumerable<WPos> ActiveTileOrigins => _aoes.Select(a => a.Origin);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_aoes.Count == 0)
            return;

        // only forbid imminent/active
        var horizon = WorldState.FutureTime(1.5f);
        foreach (var c in _aoes)
        {
            if (c.Risky && c.Activation <= horizon)
                hints.AddForbiddenZone(c.Distance, c.Activation);
        }

        if (SafeZoneCenters.Count > 0)
        {
            var safe = SafeZoneCenters.MinBy(s => (s - actor.Position).LengthSq());
            hints.GoalZones.Add(AIHints.GoalProximity(safe, 2, 40));
        }
    }

    public override void Update()
    {
        if (_expire != default && _aoes.Count > 0 && WorldState.CurrentTime >= _expire)
        {
            _aoes.Clear();
            _expire = default;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.ArcanePlot or AID.ArcanePlotAlt)
        {
            _aoes.Clear();
            _expire = default;
            ResetSafeZones();
        }
    }

    private void ResetSafeZones()
    {
        SafeZoneCenters.Clear();
        for (var z = -16; z <= 16; z += 8)
            for (var x = -16; x <= 16; x += 8)
                SafeZoneCenters.Add(Module.Center + new WDir(x, z));
    }

    public override void OnActorCreated(Actor actor)
    {
        switch ((OID)actor.OID)
        {
            case OID.ArrowBright:
                AddLine(actor, FirstPulseActivation(actor.Position) ?? WorldState.FutureTime(11.5f), false);
                break;
            case OID.ArrowDim:
                AddLine(actor, WorldState.FutureTime(8.2f), true);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BrightPulseFirst:
            case AID.BrightPulseRest:
                ++NumCasts;
                RefreshExpiry();
                break;
            case AID.InfernoTheorem:
                _expire = WorldState.CurrentTime.AddSeconds(2f) > _expire ? WorldState.CurrentTime.AddSeconds(2f) : _expire;
                break;
        }
    }

    private void RefreshExpiry()
    {
        if (_aoes.Count == 0)
            return;
        _expire = _aoes.Max(a => a.Activation).AddSeconds(5f);
    }

    private DateTime? FirstPulseActivation(WPos origin)
    {
        foreach (var a in WorldState.Actors)
            if (a.CastInfo != null && (AID)a.CastInfo.Action.ID == AID.BrightPulseFirst && a.Position.AlmostEqual(origin, 1))
                return Module.CastFinishAt(a.CastInfo);
        return null;
    }

    private void AddLine(Actor actor, DateTime activation, bool preAdvance)
    {
        var pos = actor.Position;
        var offset = 8 * actor.Rotation.ToDirection();
        if (preAdvance)
            pos += offset;

        for (var i = 0; i < 5; ++i)
        {
            _aoes.Add(new(Shape, pos, default, activation));
            SafeZoneCenters.RemoveAll(c => Shape.Check(c, pos, default));
            activation = activation.AddSeconds(1.2f);
            pos += offset;
        }
        _aoes.SortBy(aoe => aoe.Activation);
        RefreshExpiry();
    }
}

class Analysis(BossModule module) : BossComponent(module)
{
    public Angle[] SafeDir = new Angle[PartyState.MaxPartySize];

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        Angle? offset = (SID)status.ID switch
        {
            SID.FrontUnseen => 0.Degrees(),
            SID.BackUnseen => 180.Degrees(),
            SID.LeftUnseen => 90.Degrees(),
            SID.RightUnseen => -90.Degrees(),
            _ => null
        };
        if (offset != null && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < SafeDir.Length)
            SafeDir[slot] = offset.Value;
    }
}

// I bastardised this from criterion
class TargetedLight(BossModule module) : Components.GenericGaze(module, default, true)
{
    public bool Active;
    private bool _showEye;
    private readonly Analysis? _analysis = module.FindComponent<Analysis>();
    private readonly Angle[] _rotation = new Angle[PartyState.MaxPartySize];
    private readonly Angle[] _safeDir = new Angle[PartyState.MaxPartySize];
    private readonly int[] _rotationCount = new int[PartyState.MaxPartySize];
    private DateTime _activation;

    public override IEnumerable<Eye> ActiveEyes(int slot, Actor actor)
    {
        if (_showEye || Active)
            yield return new(Module.PrimaryActor.Position, _activation, _safeDir[slot]);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!Active)
            return;
        base.AddAIHints(slot, actor, assignment, hints);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_rotation[slot] != default)
            hints.Add($"Rotation: {(_rotation[slot].Rad < 0 ? "CW" : "CCW")}", false);
        if (!Active)
            return;
        base.AddHints(slot, actor, hints);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        var count = (SID)status.ID switch
        {
            SID.TimesThreePlayer => -1,
            SID.TimesFivePlayer => 1,
            _ => 0
        };
        if (count != 0 && Raid.TryFindSlot(actor.InstanceID, out var slot) && slot < _rotationCount.Length)
            _rotationCount[slot] = count;
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        var rot = (IconID)iconID switch
        {
            IconID.PlayerRotateCW => -90.Degrees(),
            IconID.PlayerRotateCCW => 90.Degrees(),
            _ => default
        };
        if (rot == default || !Raid.TryFindSlot(actor.InstanceID, out var slot) || slot >= _rotation.Length)
            return;

        var count = _rotationCount[slot] == 0 ? 1 : _rotationCount[slot];
        _rotation[slot] = rot * count;
        if (_analysis != null)
            _safeDir[slot] = _analysis.SafeDir[slot] + _rotation[slot];
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.TargetedLightAOE)
            return;

        _showEye = true;
        _activation = Module.CastFinishAt(spell);
        if (_analysis != null)
            for (var i = 0; i < _safeDir.Length; ++i)
                if (_rotation[i] == default)
                    _safeDir[i] = _analysis.SafeDir[i];
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.TargetedLight:
                Active = true;
                _activation = WorldState.Actors.FirstOrDefault(a => a.CastInfo != null && (AID)a.CastInfo.Action.ID == AID.TargetedLightAOE) is Actor helper
                    ? Module.CastFinishAt(helper.CastInfo)
                    : WorldState.FutureTime(0.5f);
                if (_analysis != null)
                    for (var i = 0; i < _safeDir.Length; ++i)
                        if (_rotation[i] == default)
                            _safeDir[i] = _analysis.SafeDir[i];
                break;
            case AID.TargetedLightAOE:
                ++NumCasts;
                Active = false;
                _showEye = false;
                Array.Clear(_rotation);
                Array.Clear(_rotationCount);
                break;
        }
    }
}

class CalculatedTrajectory : Components.GenericForcedMarch
{
    private readonly int[] _rotationCount = new int[PartyState.MaxPartySize];
    private readonly Angle[] _rotation = new Angle[PartyState.MaxPartySize];
    private DateTime _activation;
    private bool _armed;

    public CalculatedTrajectory(BossModule module) : base(module) => MovementSpeed = 4;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_armed)
            return;
        if (_rotation[slot] != default)
            hints.Add($"Rotation: {(_rotation[slot].Rad < 0 ? "CW" : "CCW")}", false);
        base.AddHints(slot, actor, hints);
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_armed)
            return;
        base.DrawArenaForeground(pcSlot, pc);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos) => !Module.InBounds(pos) || (Module.FindComponent<ArcanePlot>()?.IsTileUnsafe(pos) ?? false);

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_armed)
            return;

        var state = State.GetValueOrDefault(actor.InstanceID);
        if (state == null || state.PendingMoves.Count == 0 && state.ForcedEnd <= WorldState.CurrentTime)
            return;

        var forcedEnd = state.ForcedEnd;
        var pending = state.PendingMoves.ToArray();
        var startRot = actor.Rotation;
        var speed = MovementSpeed;
        var now = WorldState.CurrentTime;
        var center = Module.Center;
        var bound = Module.Bounds.Radius;
        var tileOrigins = Module.FindComponent<ArcanePlot>()?.ActiveTileOrigins ?? [];

        hints.AddForbiddenZone(pos =>
        {
            var p = pos;
            var dir = startRot;
            if (forcedEnd > now)
            {
                var dist = speed * (float)(forcedEnd - now).TotalSeconds;
                p += dist * dir.ToDirection();
            }
            foreach (var move in pending)
            {
                dir += move.dir;
                p += speed * move.duration * dir.ToDirection();
            }
            if (Math.Abs(p.X - center.X) > bound || Math.Abs(p.Z - center.Z) > bound)
                return -1f;
            foreach (var origin in tileOrigins)
            {
                if (ArcanePlot.Shape.Check(p, origin, default))
                    return -1f;
            }
            return 1f;
        });
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CalculatedTrajectory)
            _armed = true;
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if (!_armed)
            return;

        switch ((SID)status.ID)
        {
            case SID.TimesThreePlayer:
                _activation = status.ExpireAt;
                if (Raid.TryFindSlot(actor.InstanceID, out var slot1) && slot1 < _rotationCount.Length)
                    _rotationCount[slot1] = -1;
                break;
            case SID.TimesFivePlayer:
                _activation = status.ExpireAt;
                if (Raid.TryFindSlot(actor.InstanceID, out var slot2) && slot2 < _rotationCount.Length)
                    _rotationCount[slot2] = 1;
                break;
            case SID.ForcedMarch:
                State.GetOrAdd(actor.InstanceID).PendingMoves.Clear();
                ActivateForcedMovement(actor, status.ExpireAt);
                break;
        }
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.ForcedMarch)
        {
            DeactivateForcedMovement(actor);
            if (NumActiveForcedMarches == 0)
                _armed = false;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (!_armed)
            return;

        var rot = (IconID)iconID switch
        {
            IconID.PlayerRotateCW => -90.Degrees(),
            IconID.PlayerRotateCCW => 90.Degrees(),
            _ => default
        };
        if (rot == default || !Raid.TryFindSlot(actor.InstanceID, out var slot) || slot >= _rotationCount.Length)
            return;

        var count = _rotationCount[slot] == 0 ? 1 : _rotationCount[slot];
        _rotation[slot] = rot * count;
        AddForcedMovement(actor, _rotation[slot], 6, _activation);
    }
}

class FlailSmash(BossModule module) : Components.ProximityAOEs(module, AID.FlailSmash, 25);
class FlailSmashLong(BossModule module) : Components.ProximityAOEs(module, AID.FlailSmashLong, 25);
class RollingSpout(BossModule module) : Components.StandardAOEs(module, AID.RollingSpout, new AOEShapeDonut(4, 12));
class AeroII(BossModule module) : Components.StandardAOEs(module, AID.AeroII, new AOEShapeRect(50, 4));
class VolcanicCoordinates(BossModule module) : Components.StandardAOEs(module, AID.VolcanicCoordinatesAOE, 6);

class V033TheLalaStates : StateMachineBuilder
{
    public V033TheLalaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<InfernoTheorem>()
            .ActivateOnEnter<StrategicStrike>()
            .ActivateOnEnter<ArcaneBlight>()
            .ActivateOnEnter<ArcanePlot>()
            .ActivateOnEnter<Analysis>()
            .ActivateOnEnter<TargetedLight>()
            .ActivateOnEnter<CalculatedTrajectory>()
            .ActivateOnEnter<FlailSmash>()
            .ActivateOnEnter<FlailSmashLong>()
            .ActivateOnEnter<RollingSpout>()
            .ActivateOnEnter<AeroII>()
            .ActivateOnEnter<VolcanicCoordinates>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 961, NameID = 12639, Contributors = "croizat")]
public class V033TheLala(WorldState ws, Actor primary) : BossModule(ws, primary, new(135, -870), new ArenaBoundsSquare(20));

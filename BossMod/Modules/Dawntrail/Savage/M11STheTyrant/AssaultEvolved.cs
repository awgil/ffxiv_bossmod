#pragma warning disable IDE0028
using BossMod.Components;
namespace BossMod.Dawntrail.Savage.M11STheTyrant;

abstract class SequencedPreviewAOEs<TEntry>(BossModule module)
    : GenericAOEs(module)
    where TEntry : struct
{
    // Reused buffer (max 2 AOEs: danger + preview)
    protected readonly List<AOEInstance> _aoes = new(2);

    private int _cursor = -1;           // currently executing index
    private int _previewIndex = -1;     // next index to preview
    private bool _active;
    private DateTime _dangerActivation; // real resolve time of current execution

    // --- REQUIRED FROM CHILD --------------------------------------------------

    // The authoritative ordered sequence of mechanic entries
    protected abstract ReadOnlySpan<TEntry> Sequence { get; }

    // Returns true if this cast AID represents execution of the next sequence step
    protected abstract bool IsExecutionCast(uint aid);

    // Builds an AOE instance for a sequence entry
    protected abstract AOEInstance BuildInstance(
        TEntry entry,
        bool danger,
        DateTime activation);

    // --------------------------------------------------------------------------

    /// Call when the scripted sequence begins (AssaultEvolved / AssaultApex).
    protected void StartSequence()
    {
        _cursor = -1;
        _previewIndex = 0;
        _active = true;
        _dangerActivation = default;
    }

    /// Stops and clears all state.
    protected void StopSequence()
    {
        _active = false;
        _cursor = -1;
        _previewIndex = -1;
        _dangerActivation = default;
        _aoes.Clear();
    }

    /// Cast start drives sequence execution.
    /// Preview becomes danger immediately.
    /// AI timing uses the real cast finish time.
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (!_active)
            return;

        if (IsExecutionCast(spell.Action.ID))
        {
            ++_cursor;
            _previewIndex = _cursor + 1;

            // real timing — no guessing
            _dangerActivation = Module.CastFinishAt(spell);
        }
    }

    /// Rebuilds only the active and preview AOEs each frame.
    public override void Update()
    {
        _aoes.Clear();

        if (!_active)
            return;

        var seq = Sequence;
        if (seq.Length == 0)
            return;

        if (_cursor >= seq.Length)
        {
            StopSequence();
            return;
        }

        var span = seq;

        // ---- CURRENT DANGER STEP ----
        if (_cursor >= 0 && _cursor < span.Length)
        {
            var entry = span[_cursor];
            _aoes.Add(BuildInstance(entry, danger: true, activation: _dangerActivation));
        }

        // ---- NEXT PREVIEW STEP ----
        if (_previewIndex >= 0 && _previewIndex < span.Length)
        {
            var entry = span[_previewIndex];
            _aoes.Add(BuildInstance(entry, danger: false, activation: DateTime.MaxValue));
        }
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);
}
sealed class AssaultWeaponTimeline(BossModule module) : BossComponent(module)
{
    internal enum WeaponType { Sword, Axe, Scythe }

    internal readonly struct WeaponEntry
    {
        internal readonly Actor Actor;
        internal readonly WeaponType Type;

        internal WeaponEntry(Actor actor, WeaponType type)
        {
            Actor = actor;
            Type = type;
        }
    }

    private readonly List<WeaponEntry> _discovered = new(3);
    private readonly List<WeaponEntry> _sequence = new(3);

    internal ReadOnlySpan<WeaponEntry> Sequence => CollectionsMarshal.AsSpan(_sequence);
    internal bool Executing { get; private set; }
    internal int CurrentIndex { get; private set; } = -1;
    internal DateTime CurrentActivation { get; private set; }

    private enum SequenceMode { None, Rotation, Timeline }
    private SequenceMode _mode;

    private const float RotationTolerance = 0.05f;

    // ---------------- PHASE CONTROL ----------------

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            // ---- STAGING START ----
            case AID.TrophyWeapons:
                Reset();
                _mode = SequenceMode.Rotation;
                break;

            case AID.UltimateTrophyWeapons:
                Reset();
                _mode = SequenceMode.Timeline;
                break;

            // ---- EXECUTION START ----
            case AID.AssaultEvolved:
                BuildRotationOrder(spell.Rotation);
                Executing = true;
                CurrentIndex = -1;
                break;

            case AID.AssaultApex:
                Executing = true;
                CurrentIndex = -1;
                break;

            // ---- STEP EXECUTION ----
            case AID.AssaultEvolved_SwordCross:
            case AID.AssaultEvolved_AxeAOE:
            case AID.AssaultEvolved_ScytheDonut:
                if (Executing)
                {
                    ++CurrentIndex;
                    CurrentActivation = Module.CastFinishAt(spell);
                }
                break;
        }
    }

    // ---------------- TIMELINE DISCOVERY ----------------

    public override void OnActorPlayActionTimelineEvent(Actor actor, ushort id)
    {
        var type = id switch
        {
            0x11D1 => WeaponType.Sword,
            0x11D2 => WeaponType.Axe,
            0x11D3 => WeaponType.Scythe,
            _ => (WeaponType?)null
        };

        if (type != null)
        {
            var entry = new WeaponEntry(actor, type.Value);
            _discovered.Add(entry);

            // Apex builds sequence live in order
            if (_mode == SequenceMode.Timeline)
                _sequence.Add(entry);
        }
    }

    // ---------------- ROTATION ORDER (EVOLVED) ----------------

    private void BuildRotationOrder(Angle bossRot)
    {
        _sequence.Clear();
        if (_discovered.Count == 0)
            return;

        var firstIndex = 0;
        var bestDiff = float.MaxValue;

        for (var i = 0; i < _discovered.Count; ++i)
        {
            var diff = MathF.Abs((_discovered[i].Actor.Rotation - bossRot).Normalized().Rad);
            if (diff < bestDiff)
            {
                bestDiff = diff;
                firstIndex = i;
            }
        }

        if (bestDiff > RotationTolerance)
            firstIndex = 0;

        var first = _discovered[firstIndex];
        _sequence.Add(first);

        var secondIndex = -1;
        var bestCW = float.MaxValue;

        for (var i = 0; i < _discovered.Count; ++i)
        {
            if (i == firstIndex)
                continue;

            var cw = ClockwiseDelta(first.Actor.Rotation, _discovered[i].Actor.Rotation);
            if (cw > 0 && cw < bestCW)
            {
                bestCW = cw;
                secondIndex = i;
            }
        }

        if (secondIndex >= 0)
            _sequence.Add(_discovered[secondIndex]);

        for (var i = 0; i < _discovered.Count; ++i)
            if (i != firstIndex && i != secondIndex)
                _sequence.Add(_discovered[i]);
    }

    private static float ClockwiseDelta(Angle from, Angle to)
    {
        var diff = from.Rad - to.Rad;
        var twoPi = 2 * MathF.PI;
        diff %= twoPi;
        if (diff < 0)
            diff += twoPi;
        return diff;
    }

    internal void Reset()
    {
        _discovered.Clear();
        _sequence.Clear();
        _mode = SequenceMode.None;
        Executing = false;
        CurrentIndex = -1;
        CurrentActivation = default;
    }
}
sealed class AssaultFloorPredictor(BossModule module) : GenericAOEs(module)
{
    private readonly AssaultWeaponTimeline _timeline =
        module.FindComponent<AssaultWeaponTimeline>()!;

    private readonly List<AOEInstance> _aoes = new(2);

    private static readonly AOEShapeCross SwordCross = new(40f, 5f);
    private static readonly AOEShapeCircle AxeCircle = new(8f);
    private static readonly AOEShapeDonut ScytheDonut = new(5f, 60f);

    public override void Update()
    {
        _aoes.Clear();

        if (!_timeline.Executing)
            return;

        var seq = _timeline.Sequence;
        var index = _timeline.CurrentIndex;
        var now = WorldState.CurrentTime;
        // ---- CURRENT (danger) ----
        if (index >= 0 && index < seq.Length && now < _timeline.CurrentActivation)
        {
            var entry = seq[index];
            _aoes.Add(BuildInstance(entry, true));
        }

        // ---- NEXT (preview) ----
        var next = index + 1;
        if (next >= 0 && next < seq.Length)
        {
            var entry = seq[next];
            _aoes.Add(BuildInstance(entry, false));
        }
    }

    private AOEInstance BuildInstance(
        AssaultWeaponTimeline.WeaponEntry entry,
        bool danger)
    {
        AOEShape shape = entry.Type switch
        {
            AssaultWeaponTimeline.WeaponType.Sword => SwordCross,
            AssaultWeaponTimeline.WeaponType.Axe => AxeCircle,
            AssaultWeaponTimeline.WeaponType.Scythe => ScytheDonut,
            _ => SwordCross
        };

        return new AOEInstance(
            shape,
            entry.Actor.Position,
            entry.Actor.Rotation,
            activation: danger ? _timeline.CurrentActivation : DateTime.MaxValue,
            color: danger ? Colors.Danger : Colors.AOE,
            risky: danger
        );
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
        => CollectionsMarshal.AsSpan(_aoes);
}

sealed class AssaultEvolvedSword(BossModule module) : GenericAOEs(module)
{
    private bool _active;
    internal bool Active => _active;
    private WPos _origin;
    internal WPos Origin => _origin;
    private readonly List<Actor> _healers = [];
    private readonly List<AOEInstance> _aoes = [];

    // Reborn-native shapes
    private static readonly AOEShapeRect Line = new(60f, 2f);

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AssaultEvolved_SwordDash)
            Arm(spell.TargetXZ);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo cast)
    {
        // Helper resolves the mechanic
        if (_active && (AID)cast.Action.ID == AID.AssaultEvolved_SwordCross)
        {
            Clear();
            NumCasts++;
        }
    }

    private void Arm(WPos origin)
    {
        _active = true;
        _origin = origin;
        _aoes.Clear();
        _healers.Clear();

        // Cache healers for stacks
        foreach (var p in WorldState.Party.WithoutSlot())
        {
            if (!p.IsDead && p.Role == Role.Healer)
                _healers.Add(p);
        }
    }

    private void Clear()
    {
        _active = false;
        _aoes.Clear();
        _healers.Clear();
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active)
            return default;

        var origin = _origin;
        _aoes.Clear();

        foreach (var h in _healers)
        {
            var dir = Angle.FromDirection(h.Position - origin);
            _aoes.Add(new AOEInstance(Line, origin, dir));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active)
            return;

        hints.Add("Stack in light parties");
    }
}

sealed class AssaultEvolvedAxeStack(BossModule module) : BossComponent(module)
{
    private Actor? _target;
    internal Actor? Target => _target;
    private WPos _origin;
    internal WPos Origin => _origin;
    private const float Radius = 6f;
    public int NumCasts;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.AssaultEvolved_AxeDash:
                SelectHealer();
                _origin = spell.TargetXZ;
                break;

            case AID.AssaultEvolved_HeavyWeight:
                NumCasts++;
                _target = null; // clear on resolution
                break;
        }
    }

    private void SelectHealer()
    {
        _target = null;

        foreach (var p in WorldState.Party.WithoutSlot())
        {
            if (!p.IsDead && p.Role == Role.Healer)
            {
                _target = p;
                break;
            }
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_target == null)
            return;

        if (actor == _target)
            hints.Add("Party stack");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (_target != null)
            Arena.ZoneCircleOutline(_target.Position, Radius, Colors.Safe);
    }
}

sealed class AssaultEvolvedScythe(BossModule module) : GenericAOEs(module)
{
    private bool _active;
    internal bool Active => _active;
    private WPos _origin;
    internal WPos Origin => _origin;
    private readonly List<Actor> _players = [];
    private readonly List<AOEInstance> _aoes = [];

    // Reborn-native shapes
    private static readonly AOEShapeCone PlayerCone =
        new(radius: 60f, halfAngle: 10.Degrees()); // very narrow cones

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.AssaultEvolved_ScytheDash)
            Arm(spell.TargetXZ);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo cast)
    {
        // Helper resolves the donut → clear everything
        if (_active && (AID)cast.Action.ID == AID.AssaultEvolved_ScytheDonut)
        {
            Clear();
            NumCasts++;
        }
    }

    private void Arm(WPos origin)
    {
        _active = true;
        _origin = origin;
        _aoes.Clear();
        _players.Clear();

        // Cache alive party members for cones
        foreach (var p in WorldState.Party.WithoutSlot())
        {
            if (!p.IsDead)
                _players.Add(p);
        }
    }

    private void Clear()
    {
        _active = false;
        _aoes.Clear();
        _players.Clear();
    }

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (!_active)
            return default;

        var origin = _origin;
        _aoes.Clear();

        // Rebuild cones dynamically so they track player movement
        foreach (var p in _players)
        {
            var dir = Angle.FromDirection(p.Position - origin);
            _aoes.Add(new AOEInstance(PlayerCone, origin, dir));
        }

        return CollectionsMarshal.AsSpan(_aoes);
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_active)
            return;

        hints.Add("Spread cones, stay in donut");
    }
}

sealed class AssaultWeaponSafeSpots(BossModule module) : BossComponent(module)
{
    private AssaultWeaponTimeline? _timeline;
    private AssaultEvolvedSword? _sword;
    private AssaultEvolvedAxeStack? _axe;
    private AssaultEvolvedScythe? _scythe;

    private readonly PartyRolesConfig _roles = Service.Config.Get<PartyRolesConfig>();

    private int _activeIndex = -1;
    private bool _waitingForResolve;
    private bool _initialized;

    private const float Radius = 1f;
    private const float SwordDistance = 8f;
    private const float ScytheDistance = 3.5f;
    private const float AxeDistance = 9f;

    public override void Update()
    {
        _timeline ??= Module.FindComponent<AssaultWeaponTimeline>();
        _sword ??= Module.FindComponent<AssaultEvolvedSword>();
        _axe ??= Module.FindComponent<AssaultEvolvedAxeStack>();
        _scythe ??= Module.FindComponent<AssaultEvolvedScythe>();

        if (_timeline == null || _sword == null || _axe == null || _scythe == null)
            return;

        if (!_timeline.Executing)
        {
            _activeIndex = -1;
            _waitingForResolve = false;
            _initialized = false;
            return;
        }

        var seq = _timeline.Sequence;
        var len = seq.Length;

        if (!_initialized && len > 0)
        {
            _activeIndex = 0;
            _initialized = true;
        }

        if (!_initialized || (uint)_activeIndex >= (uint)len)
            return;

        var entry = seq[_activeIndex];

        var mechanicActive = entry.Type switch
        {
            AssaultWeaponTimeline.WeaponType.Sword => _sword.Active,
            AssaultWeaponTimeline.WeaponType.Axe => _axe.Target != null,
            AssaultWeaponTimeline.WeaponType.Scythe => _scythe.Active,
            _ => false
        };

        if (mechanicActive)
            _waitingForResolve = true;

        // advance once mechanic finishes so next preview appears instantly
        if (_waitingForResolve && !mechanicActive)
        {
            _activeIndex++;
            _waitingForResolve = false;
        }
    }

    public override void DrawArenaForeground(int slot, Actor pc)
    {
        if (!_initialized || pc == null || _timeline == null)
            return;

        var seq = _timeline.Sequence;
        if ((uint)_activeIndex >= (uint)seq.Length)
            return;

        var assignments = _roles.AssignmentsPerSlot(WorldState.Party);
        if (assignments.Length == 0 || (uint)slot >= (uint)assignments.Length)
            return;

        var entry = seq[_activeIndex];
        var actor = entry.Actor;
        if (actor == null)
            return;

        var role = assignments[slot];

        var pos = actor.Position;
        var rot = actor.Rotation;

        var spot = entry.Type switch
        {
            AssaultWeaponTimeline.WeaponType.Sword => SwordSpot(role, pos, rot),
            AssaultWeaponTimeline.WeaponType.Axe => AxeSpot(pos),
            AssaultWeaponTimeline.WeaponType.Scythe => ScytheSpot(role, pos, rot),
            _ => default
        };

        if (spot == default)
            return;

        Arena.ZoneCircleOutline(spot, Radius, Colors.Safe);
        Arena.AddLine(pc.Position, spot, Colors.Safe);
    }

    private static WPos SwordSpot(PartyRolesConfig.Assignment role, WPos pos, Angle rot)
    {
        var offset = role switch
        {
            PartyRolesConfig.Assignment.MT or
            PartyRolesConfig.Assignment.H1 or
            PartyRolesConfig.Assignment.M1 or
            PartyRolesConfig.Assignment.R1 => 135.Degrees(),
            PartyRolesConfig.Assignment.OT or
            PartyRolesConfig.Assignment.H2 or
            PartyRolesConfig.Assignment.M2 or
            PartyRolesConfig.Assignment.R2 => (-135).Degrees(),
            _ => default
        };

        return pos + SwordDistance * (rot + offset).ToDirection();
    }

    private WPos AxeSpot(WPos pos)
    {
        var dir = Angle.FromDirection(Module.Center - pos);
        return pos + AxeDistance * dir.ToDirection();
    }

    private static WPos ScytheSpot(PartyRolesConfig.Assignment role, WPos pos, Angle rot)
    {
        var offset = role switch
        {
            PartyRolesConfig.Assignment.MT => 0.Degrees(),
            PartyRolesConfig.Assignment.OT => 180.Degrees(),
            PartyRolesConfig.Assignment.H2 => (-90).Degrees(),
            PartyRolesConfig.Assignment.H1 => 90.Degrees(),
            PartyRolesConfig.Assignment.R2 => (-45).Degrees(),
            PartyRolesConfig.Assignment.R1 => 45.Degrees(),
            PartyRolesConfig.Assignment.M2 => (-135).Degrees(),
            PartyRolesConfig.Assignment.M1 => 135.Degrees(),
            _ => default
        };

        return pos + ScytheDistance * (rot + offset).ToDirection();
    }
}

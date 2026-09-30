namespace BossMod.Dawntrail.Trial.T05Necron;

public enum OID : uint
{
    Boss = 0x4870, // R20.000, Necron
    Helper = 0x233C, // R0.500
    AutoAttacker = 0x49B1, // R0.000, Part
    NecronHelper = 0x4945, // R1.000
    IcyHands1 = 0x4903, // R3.575
    IcyHandsJail = 0x4904, // R3.575, intermission adds
    IcyHands5 = 0x4905, // R3.575
    IcyHands6 = 0x4906, // R3.575
    LoomingSpecter1 = 0x4907, // R15.750
    IcyHands3 = 0x4908, // R4.400
    IcyHandsTower = 0x4909, // R3.575, SpreadingFear
    AzureAether1 = 0x490A, // R1.000
    AzureAether2 = 0x4948, // R1.000
    LoomingSpecter2 = 0x49DD, // R3.000
}

public enum AID : uint
{
    AutoAttack = 44614, // AutoAttacker->player, no cast, single-target
    FearOfDeath = 44521, // Boss->self, 5.0s cast, range 100 circle
    FearOfDeathPuddle = 44522, // Helper->location, 3.0s cast, range 3 circle
    ChokingGrasp = 44523, // IcyHands->self, 3.0s cast, range 24 width 6 rect
    ColdGripLeftSafe = 44524, // Boss->self, 5.0+1.0s cast, single-target visual
    ColdGripRightSafe = 44525, // Boss->self, 5.0+1.0s cast, single-target visual
    ExistentialDread = 44526, // Helper->self, 1.0s cast, range 100 width 24 rect
    ColdGripAOE = 44611, // Helper->self, 6.0s cast, range 100 width 12 rect
    SoulReaping = 44527, // Boss->self, 6.0s cast, single-target visual
    Aetherblight = 44528, // Boss->self, 5.0s cast, single-target visual
    AetherblightRepeat1 = 44529, // Boss->self, no cast, single-target
    AetherblightRepeat2 = 44530, // Boss->self, no cast, single-target
    RelentlessReaping = 44531, // Boss->self, 15.0s cast, single-target visual
    CropRotation = 44609, // Boss->self, 3.0s cast, single-target visual
    SeasonsOfBlight = 45166, // Boss->self, 10.0s cast, single-target visual
    AetherblightCircle = 45181, // Helper->self, 1.0s cast, range 20 circle
    AetherblightDonut = 45182, // Helper->self, 1.0s cast, range 16-60 donut
    MementoMori = 44532, // Boss->self, 5.0s cast, range 37 width 12 rect
    GrandCross = 44533, // Boss->location, 7.0s cast, range 50 circle
    GrandCrossLaser = 44534, // Helper->self, 0.3s cast, range 100 width 4 rect
    GrandCrossProximity = 44535, // Helper->self, 5.0s cast, range 100 width 100 rect
    GrandCrossPuddle = 44536, // Helper->location, 3.0s cast, range 3 circle
    GrandCrossArena = 44603, // Helper->location, 7.0s cast, range 9-60 donut
    NeutronRingCast = 44538, // Boss->location, 7.0s cast, single-target visual
    NeutronRing = 44539, // Helper->self, no cast, range 50 circle
    FearOfDeathPuddleAdds = 44540, // Helper->location, 3.0s cast, range 3 circle
    DarknessOfEternityCast = 44541, // Boss->self, 10.0s cast, single-target visual
    DarknessOfEternity = 44542, // Helper->self, no cast, range 50 circle
    Inevitability = 44544, // Helper->self, no cast, range 50 circle, jail teleport
    SpecterOfDeath = 44605, // Boss->self, 5.0s cast, single-target visual
    InvitationVisual = 44545, // LoomingSpecter1->self, 4.3+0.7s cast, range 36 width 10 rect
    Invitation = 44817, // Helper->self, 5.0s cast, range 36 width 10 rect
    BlueShockwaveCast = 44546, // Boss->self, 6.0+1.0s cast, single-target visual
    BlueShockwave = 44547, // Helper->self, no cast, range 100 100-degree cone
    MassMacabre = 44548, // Boss->self, 4.0s cast, single-target visual
    SpreadingFear = 44549, // IcyHandsTower->self, 8.0s cast, range 50 circle
}

public enum IconID : uint
{
    StoreCircle = 604, // Boss, Out
    StoreDonut = 605, // Boss, In
    BlueShockwave = 615, // Boss
    StoreCircle2 = 621, // Boss, Out
    StoreDonut2 = 622, // Boss, In
}

class FearOfDeath(BossModule module) : Components.RaidwideCast(module, AID.FearOfDeath);
class FearOfDeathPuddle(BossModule module) : Components.GroupedAOEs(module, [AID.FearOfDeathPuddle, AID.FearOfDeathPuddleAdds], new AOEShapeCircle(3));
class ChokingGrasp(BossModule module) : Components.StandardAOEs(module, AID.ChokingGrasp, new AOEShapeRect(24, 3));
class ColdGripAOE(BossModule module) : Components.StandardAOEs(module, AID.ColdGripAOE, new AOEShapeRect(100, 6));
class ExistentialDread(BossModule module) : Components.GenericAOEs(module, AID.ExistentialDread)
{
    enum Side { Unknown, Left, Right }

    private Side _safeSide;
    private WPos _origin;
    private Angle _rotation;
    private DateTime _activation;
    private bool _risky;
    private static readonly AOEShapeRect _shape = new(100, 12);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_safeSide != Side.Unknown && _activation != default)
            yield return new(_shape, _origin, _rotation, _activation, _risky ? ArenaColor.Danger : ArenaColor.AOE, _risky);
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_safeSide != Side.Unknown && !_risky && _activation != default)
            hints.AddForbiddenZone(ShapeDistance.InvertedRect(PreposOrigin(), new WDir(0, 1), 30, 0, 2), _activation);
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        base.DrawArenaBackground(pcSlot, pc);
        if (_safeSide != Side.Unknown && !_risky)
            Arena.ZoneRect(PreposOrigin(), default(Angle), 30, 0, 1, ArenaColor.SafeFromAOE);
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_safeSide != Side.Unknown)
            hints.Add($"Safe side: {_safeSide}");
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.ColdGripLeftSafe:
                Predict(Side.Left);
                break;
            case AID.ColdGripRightSafe:
                Predict(Side.Right);
                break;
            case AID.ColdGripAOE:
                _activation = Module.CastFinishAt(spell, 1.6f);
                break;
            case AID.ExistentialDread:
                _origin = spell.LocXZ;
                _rotation = spell.Rotation;
                _activation = Module.CastFinishAt(spell);
                _risky = true;
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.ColdGripAOE)
        {
            _risky = true;
            if (_activation == default)
                _activation = WorldState.FutureTime(1.6f);
        }

        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _activation = default;
            _safeSide = Side.Unknown;
            _risky = false;
        }
    }

    private WPos PreposOrigin()
    {
        var x = _safeSide == Side.Left ? Arena.Center.X - 5 : Arena.Center.X + 5;
        return new(x, Arena.Center.Z - 15);
    }

    private void Predict(Side safe)
    {
        _safeSide = safe;
        var src = new WPos(Arena.Center.X, Arena.Center.Z - 15);
        src.X += safe == Side.Left ? 6 : -6;
        _origin = src;
        _rotation = default;
        _risky = false;
    }
}

class MementoMori(BossModule module) : Components.GenericAOEs(module, AID.MementoMori)
{
    private Actor? _caster;
    private bool _voidzone;
    private static readonly AOEShapeRect _line = new(37, 6);
    private static readonly AOEShapeRect _void = new(30, 6);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_caster?.CastInfo is { } cast)
        {
            yield return new(_line, cast.LocXZ, cast.Rotation, Module.CastFinishAt(cast), ArenaColor.Danger);
            yield return new(_void, new(Arena.Center.X, Arena.Center.Z - 15), default, Module.CastFinishAt(cast), ArenaColor.AOE, false);
        }
        else if (_voidzone)
            yield return new(_void, new(Arena.Center.X, Arena.Center.Z - 15));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction)
            _caster = caster;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            NumCasts++;
            _caster = null;
            _voidzone = true;
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x18 && state == 0x00080004)
            _voidzone = false;
    }
}

class GrandCross(BossModule module) : Components.RaidwideCast(module, AID.GrandCross);
class GrandCrossLaser(BossModule module) : Components.GenericAOEs(module, AID.GrandCrossLaser)
{
    private readonly List<(Angle Rot, DateTime Activation)> _predicted = [];
    private static readonly AOEShapeRect _shape = new(20, 2, 20);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (rot, t) in _predicted)
            yield return new(_shape, Arena.Center, rot, t);
    }

    public override void OnTethered(Actor source, in ActorTetherInfo tether)
    {
        var angle = (Arena.Center - source.Position).ToAngle();
        if (tether.ID == 344)
        {
            _predicted.Add((angle + 207.Degrees(), WorldState.FutureTime(5.6f)));
            _predicted.SortBy(l => l.Activation);
        }
        if (tether.ID == 343)
        {
            _predicted.Add((angle + 42.Degrees(), WorldState.FutureTime(7.6f)));
            _predicted.SortBy(l => l.Activation);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && _predicted.Count > 0)
            _predicted.RemoveAt(0);
    }
}

class GrandCrossLaserCast(BossModule module) : Components.StandardAOEs(module, AID.GrandCrossLaser, new AOEShapeRect(100, 2));
class GrandCrossProximity(BossModule module) : Components.StandardAOEs(module, AID.GrandCrossProximity, new AOEShapeRect(100, 4.5f));
class GrandCrossPuddle(BossModule module) : Components.StandardAOEs(module, AID.GrandCrossPuddle, 3);
class NeutronRing(BossModule module) : Components.RaidwideCastDelay(module, AID.NeutronRingCast, AID.NeutronRing, 2.6f);
class DarknessOfEternity(BossModule module) : Components.RaidwideCastDelay(module, AID.DarknessOfEternityCast, AID.DarknessOfEternity, 6.4f);
class Invitation(BossModule module) : Components.StandardAOEs(module, AID.Invitation, new AOEShapeRect(36, 5));
class GrandCrossArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private DateTime _activation;
    public int NumChanges;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(new AOEShapeDonut(9, 60), Arena.Center, default, _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.GrandCrossArena)
            _activation = Module.CastFinishAt(spell);
    }

    public override void OnEventDirectorUpdate(uint updateID, uint param1, uint param2, uint param3, uint param4)
    {
        if (updateID == 0x8000000D)
        {
            if (param1 == 2)
            {
                NumChanges++;
                _activation = default;
                Arena.Bounds = new ArenaBoundsCircle(9);
            }
            else if (param1 == 1)
            {
                NumChanges++;
                Arena.Bounds = new ArenaBoundsRect(18, 15);
            }
        }
    }
}

class MacabreMark(BossModule module) : Components.GenericTowers(module)
{
    private int _numAdded;
    private static readonly WPos FrontNE = new(106, 90);
    private static readonly WPos FrontNW = new(94, 90);
    private static readonly WPos CenterSE = new(106, 100);
    private static readonly WPos CenterSW = new(94, 100);

    public override void OnMapEffect(byte index, uint state)
    {
        var pos = index switch
        {
            0x1A => FrontNW,
            0x1B => FrontNE,
            0x1C => CenterSW,
            0x1D => CenterSE,
            _ => default
        };
        if (pos == default)
            return;

        if (state == 0x00020001 && _numAdded < 4)
        {
            ++_numAdded;
            Towers.Add(new(pos, 3, 4, 4, activation: WorldState.FutureTime(15)));
        }
        else if (state is 0x00080004 or 0x08000400)
        {
            Towers.RemoveAll(t => t.Position.AlmostEqual(pos, 1));
            if (Towers.Count == 0)
                _numAdded = 0;
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (Towers.Count == 0)
            return;

        var i = PreferredIndex();
        if (i < 0)
            return;

        var t = Towers[i];
        hints.AddForbiddenZone(ShapeDistance.InvertedCircle(t.Position, t.Radius), t.Activation);
        hints.GoalZones.Add(AIHints.GoalSingleTarget(t.Position, 1, 5));
    }

    // Duty Support: player takes east towers (NE then SE); west left for NPCs
    private int PreferredIndex()
    {
        var ne = -1;
        var se = -1;
        for (var i = 0; i < Towers.Count; ++i)
        {
            var t = Towers[i];
            if (t.NumInside(Module) >= t.MinSoakers)
                continue;
            if (t.Position.AlmostEqual(FrontNE, 1))
                ne = i;
            else if (t.Position.AlmostEqual(CenterSE, 1))
                se = i;
        }
        return ne >= 0 ? ne : se;
    }
}

class Aetherblight(BossModule module) : Components.GenericAOEs(module)
{
    public enum Blight { Circle, Donut }

    private readonly List<Blight> _order = [];
    private DateTime _nextActivation;
    private bool _rotated;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_nextActivation == default)
            yield break;
        var danger = true;
        foreach (var aoes in RenderAOEs().Take(2))
        {
            foreach (var aoe in aoes)
                yield return aoe with { Color = danger ? ArenaColor.Danger : ArenaColor.AOE, Risky = danger };
            danger = false;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (_order.Count > 0)
            hints.Add($"Aetherblight: {string.Join(" -> ", _order.Select(b => b == Blight.Circle ? "Out" : "In"))}");
    }

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (actor != Module.PrimaryActor)
            return;
        Blight? blight = (IconID)iconID switch
        {
            IconID.StoreCircle or IconID.StoreCircle2 => Blight.Circle,
            IconID.StoreDonut or IconID.StoreDonut2 => Blight.Donut,
            _ => null
        };
        if (blight != null)
        {
            if (_order.Count == 0)
                _rotated = false;
            _order.Add(blight.Value);
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.Aetherblight:
                _nextActivation = Module.CastFinishAt(spell, 1.2f);
                break;
            case AID.SeasonsOfBlight:
                ApplyCropRotation();
                _nextActivation = Module.CastFinishAt(spell, 1.2f);
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.AetherblightCircle or AID.AetherblightDonut)
        {
            if (_order.Count > 0)
                _order.RemoveAt(0);
            NumCasts++;
            _nextActivation = _order.Count > 0 ? WorldState.FutureTime(2.8f) : default;
        }
    }

    public override void OnActorModelStateChange(Actor actor, byte modelState, byte animState1, byte animState2)
    {
        if (actor == Module.PrimaryActor && !_rotated && _order.Count >= 4 && modelState is 0x15 or 0x93 or 0x41 or 0x16)
            ApplyCropRotation(modelState);
    }

    private void ApplyCropRotation(byte? modelState = null)
    {
        if (_rotated)
            return;
        var n = (modelState ?? Module.PrimaryActor.ModelState.ModelState) switch
        {
            0x93 => 1,
            0x41 => 2,
            0x16 => 3,
            _ => 0
        };
        for (var i = 0; i < n && _order.Count > 0; i++)
        {
            _order.Add(_order[0]);
            _order.RemoveAt(0);
        }
        _rotated = true;
    }

    private IEnumerable<AOEInstance[]> RenderAOEs()
    {
        var next = _nextActivation;
        foreach (var aoe in _order)
        {
            var origin = Module.PrimaryActor.Position;
            yield return aoe switch
            {
                Blight.Circle => [new(new AOEShapeCircle(20), origin, default, next)],
                Blight.Donut => [new(new AOEShapeDonut(16, 60), origin, default, next)],
                _ => []
            };
            next = next.AddSeconds(2.8f);
        }
    }
}

class BlueShockwave(BossModule module) : Components.TankSwap(module, default(AID), default(AID), default(AID), 4.1f, new AOEShapeCone(100, 50.Degrees()), false)
{
    private DateTime _lastHit;

    public override void OnEventIcon(Actor actor, uint iconID, ulong targetID)
    {
        if (iconID == (uint)IconID.BlueShockwave)
        {
            NumCasts = 0;
            _source = actor;
            _prevTarget = targetID;
            _activation = WorldState.FutureTime(7.2f);
            _lastHit = default;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is not (AID.BlueShockwave or AID.BlueShockwaveCast))
            return;

        if (_lastHit != default && WorldState.CurrentTime < _lastHit.AddSeconds(0.5f))
            return;
        _lastHit = WorldState.CurrentTime;

        ++NumCasts;
        _prevTarget = spell.MainTargetID == caster.InstanceID && spell.Targets.Count != 0 ? spell.Targets[0].ID : spell.MainTargetID;
        _activation = WorldState.FutureTime(4.1f);

        if (NumCasts >= 2)
        {
            CurrentBaits.Clear();
            _source = null;
            _activation = default;
        }
    }
}

class Jail : BossComponent
{
    public const float AbyssY = -100;
    private static readonly WPos MainCenter = new(100, 100);
    private static readonly WDir[] WellOffsets = [
        new(0, -7.4f),
        new(-2.5f, -20f),
        new(15f, -11.5f),
        new(20f, 0),
    ];
    private static readonly WDir[] WellLandings = [
        new(-6f, -18f),
        new(10f, -15f),
        new(19f, -2f),
    ];
    public static readonly ArenaBoundsCustom JailBounds = BuildJailBounds();
    private bool _jailed;
    private WPos _pad;
    private BitMask _wells;

    public Jail(BossModule module) : base(module)
    {
        KeepOnPhaseChange = true;
    }

    public static bool InAbyss(Actor p) => p.PosRot.Y < AbyssY;

    public static bool NearJailPad(Actor p)
        => InAbyss(p) || T05Necron.JailArenas.Any(j => (j - p.Position).LengthSq() < 225);

    public override void Update()
    {
        if (Module.Raid.Player() is not { } p)
            return;

        if (NearJailPad(p))
        {
            _pad = IslandPad(p);
            _jailed = true;
            if ((Arena.Center - _pad).LengthSq() > 1 || Arena.Bounds != JailBounds)
            {
                Arena.Center = _pad;
                Arena.Bounds = JailBounds;
            }
            return;
        }

        if (_jailed && (p.Position - MainCenter).LengthSq() < 625 && !InAbyss(p))
        {
            _jailed = false;
            Arena.Center = MainCenter;
            Arena.Bounds = new ArenaBoundsRect(18, 15);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index > 0x07)
            return;
        if (state == 0x00020001)
            _wells.Set(index);
        else if (state == 0x00080004)
            _wells.Clear(index);
    }

    private WPos IslandPad(Actor p)
    {
        var known = T05Necron.JailArenas.MinBy(j => (j - p.Position).LengthSq());
        return (known - p.Position).LengthSq() < 625 ? known : p.Position;
    }

    private bool WellReady()
    {
        if (_wells.None())
            return true;
        for (var i = 0; i < T05Necron.JailArenas.Length; ++i)
            if ((T05Necron.JailArenas[i] - _pad).LengthSq() < 1)
                return _wells[i];
        return true;
    }

    private bool JailAddsAlive(Actor p)
    {
        foreach (var a in WorldState.Actors)
        {
            if (a.OID != (uint)OID.IcyHandsJail || !a.IsTargetable || a.IsDeadOrDestroyed)
                continue;
            if ((a.Position - p.Position).LengthSq() < 400)
                return true;
        }
        return false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!_jailed)
            return;
        if (JailAddsAlive(actor))
            hints.Add("Kill the adds!");
        else if (WellReady())
            hints.Add("Jump into the wells!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (!_jailed)
            return;
        hints.SetPriority(Module.PrimaryActor, AIHints.Enemy.PriorityPointless);
        if (JailAddsAlive(actor) || !WellReady())
            return;
        for (var i = 0; i < WellLandings.Length; ++i)
            hints.Portals.Add((_pad + WellOffsets[i], 2, _pad + WellLandings[i]));
        hints.GoalZones.Add(AIHints.GoalSingleTarget(_pad + WellOffsets[3], 1, 9));
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (!_jailed || !WellReady())
            return;
        Arena.ZoneCircle(_pad + WellOffsets[0], 2, ArenaColor.SafeFromAOE);
        Arena.ZoneCircle(_pad + WellOffsets[1], 2, ArenaColor.SafeFromAOE);
        Arena.ZoneCircle(_pad + WellOffsets[2], 2, ArenaColor.SafeFromAOE);
        Arena.ZoneCircle(_pad + WellOffsets[3], 1.5f, ArenaColor.SafeFromAOE);
    }

    private static ArenaBoundsCustom BuildJailBounds()
    {
        (WDir Off, float R)[] islands = [
            (default, 9.5f),
            (new(-5f, -21f), 4.5f),
            (new(14f, -14f), 4.5f),
            (new(20f, 0), 3.25f),
        ];
        return new(28, new(islands.Select(i => new RelPolygonWithHoles([.. CurveApprox.Circle(i.R, 0.05f).Select(c => c + i.Off)])).ToList()));
    }
}

class SpreadingFear(BossModule module) : Components.CastHint(module, AID.SpreadingFear, "Hand enrage!", true);

class IcyHandsAdds(BossModule module) : Components.AddsMulti(module, [OID.IcyHands1, OID.IcyHandsJail, OID.IcyHands3, OID.IcyHandsTower, OID.IcyHands5, OID.IcyHands6], 1)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var a in ActiveActors)
        {
            var e = hints.FindEnemy(a);
            e?.Priority = 1;
            e?.ForbidDOTs = true;
        }
    }
}

class T05NecronStates : StateMachineBuilder
{
    public T05NecronStates(BossModule module) : base(module)
    {
        SimplePhase(0, P1, "P1")
            .Raw.Update = () => Module.Raid.Player() is { } p && Jail.NearJailPad(p);
        SimplePhase(1, PJail, "Intermission")
            .OnEnter(() =>
            {
                if (Module.Raid.Player() is { } player)
                {
                    var center = T05Necron.JailArenas.MinBy(j => (j - player.Position).LengthSq());
                    if ((center - player.Position).LengthSq() > 225)
                        center = player.Position;
                    Module.Arena.Center = center;
                    Module.Arena.Bounds = Jail.JailBounds;
                }
            })
            .Raw.Update = () => Module.Raid.Player() is { } p && !Jail.InAbyss(p) && (p.Position - new WPos(100, 100)).LengthSq() < 625;
        SimplePhase(2, P2, "P2")
            .OnEnter(() =>
            {
                Module.Arena.Center = new(100, 100);
                Module.Arena.Bounds = new ArenaBoundsRect(18, 15);
            })
            .Raw.Update = () => Module.PrimaryActor.IsDead;
    }

    private void P1(uint id) => ActivateCommon(id);

    private void PJail(uint id)
    {
        Timeout(id, 50, "Doom")
            .ActivateOnEnter<IcyHandsAdds>()
            .ActivateOnEnter<ChokingGrasp>()
            .ActivateOnEnter<SpreadingFear>();
    }

    private void P2(uint id) => ActivateCommon(id);

    private void ActivateCommon(uint id)
    {
        SimpleState(id, 10000, "Enrage")
            .ActivateOnEnter<FearOfDeath>()
            .ActivateOnEnter<FearOfDeathPuddle>()
            .ActivateOnEnter<ChokingGrasp>()
            .ActivateOnEnter<MacabreMark>()
            .ActivateOnEnter<ExistentialDread>()
            .ActivateOnEnter<ColdGripAOE>()
            .ActivateOnEnter<MementoMori>()
            .ActivateOnEnter<GrandCross>()
            .ActivateOnEnter<GrandCrossLaser>()
            .ActivateOnEnter<GrandCrossLaserCast>()
            .ActivateOnEnter<GrandCrossProximity>()
            .ActivateOnEnter<GrandCrossPuddle>()
            .ActivateOnEnter<GrandCrossArenaChange>()
            .ActivateOnEnter<NeutronRing>()
            .ActivateOnEnter<DarknessOfEternity>()
            .ActivateOnEnter<Invitation>()
            .ActivateOnEnter<Aetherblight>()
            .ActivateOnEnter<BlueShockwave>()
            .ActivateOnEnter<IcyHandsAdds>()
            .ActivateOnEnter<SpreadingFear>()
            .ActivateOnEnter<Jail>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1061, NameID = 14093)]
public class T05Necron(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsRect(18, 15))
{
    public static readonly WPos[] JailArenas = [
        new(100, -100),
        new(300, -100),
        new(300, 100),
        new(300, 300),
        new(100, 300),
        new(-100, 300),
        new(-100, 100),
        new(-100, -100),
    ];
}

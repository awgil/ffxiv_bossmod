namespace BossMod.Dawntrail.Raid.RM12NTheLindwurm;

public enum OID : uint
{
    Boss = 0x4AF7, // R13.800, x1
    Helper = 0x233C, // R0.500, x40, Helper type
    SnakeHead = 0x4AF8, // R4.000, x2
    FloorSnake = 0x4AF9, // R1.000, x8
    BossPart = 0x4AFA, // R0.000, x2, Part type
    Unknown = 0x4AE4, // R1.000, x1
    BurstBlob = 0x1EBF29, // R0.500, EventObj type, Burst spot 6.6s before
}

public enum AID : uint
{
    AutoAttack = 46225, // BossPart->player, no cast, single-target

    TheFixer = 46228, // Boss->self, 5.0s cast, range 60 circle

    BloodshedEast = 46182, // Boss->self, 0.2s cast, single-target, east half Serpentine Scourge next
    BloodshedWest = 46181, // Boss->self, 0.2s cast, single-target, west half Serpentine Scourge next
    SerpentineScourgeEastBoss = 46184, // Boss->self, 1.0s cast, single-target
    SerpentineScourgeWestBoss = 46183, // Boss->self, 1.0s cast, single-target
    SerpentineScourge = 47547, // Helper->self, 2.0s cast, range 30 width 20 rect

    RavenousReachBoss = 46185, // Boss->self, 1.0s cast, single-target
    RavenousReach = 46189, // Helper->self, 10.6s cast, range 35 120-degree cone
    RavenousReachEnd = 46190, // Boss->self, no cast, single-target
    RavenousReachHead1 = 46186, // SnakeHead->self, no cast, single-target
    RavenousReachHead2 = 46187, // SnakeHead->self, no cast, single-target
    RavenousReachHead3 = 46390, // SnakeHead->self, no cast, single-target
    RavenousReachHead4 = 46952, // SnakeHead->self, no cast, single-target

    Burst = 46191, // Helper->location, 2.5s cast, range 12 circle

    CruelCoil1 = 45339, // Boss->location, 3.0s cast, single-target
    CruelCoil2 = 45340, // Boss->location, 3.0s cast, single-target
    CruelCoilBind = 46194, // Helper->self, no cast, range 60 circle, pulls everyone to the center
    CruelCoilVisual = 46198, // Helper->self, 40.0s cast, range 13 circle, ends early with Constrictor
    SkinsplitterBoss = 46195, // Boss->self, no cast, single-target
    Skinsplitter = 46396, // Helper->self, no cast, range 9?-13 donut
    ConstrictorBoss1 = 46397, // Boss->location, no cast, single-target
    ConstrictorBoss2 = 48628, // Boss->location, no cast, single-target
    Constrictor = 46199, // Helper->self, 1.0s cast, range 13 circle

    VisceralBurstBoss = 46226, // Boss->self, 4.0s cast, single-target
    VisceralBurst = 46227, // Helper->players, no cast, range 6 circle, tankbuster

    Grotesquerie = 46209, // Boss->self, 3.0s cast, single-target
    DramaticLysis = 46211, // Helper->location, no cast, range 6 circle, Bursting Grotesquerie spread
    FourthWallFusion = 46212, // Helper->location, no cast, range 6 circle, Shared Grotesquerie stack
    HemorrhagicProjection = 46213, // Helper->self, no cast, range 60 30-degree cone, Directed Grotesquerie
    Shockwave = 46210, // Helper->player, no cast, single-target, Flesh Forward/Back knockback

    SplattershedBoss1 = 47552, // Boss->self, 3.0s cast, single-target
    SplattershedBoss2 = 47549, // Boss->self, 3.0s cast, single-target
    Splattershed = 47557, // Helper->self, no cast, range 60 circle
    SplattershedBoss3 = 48096, // Boss->self, 5.0s cast, single-target
    SplattershedLong = 48097, // Helper->self, 2.3s cast, range 60 circle

    FeralFission = 46200, // Boss->self, 3.0s cast, single-target
    GrandEntranceAppear = 46202, // FloorSnake->self, 3.0s cast, range 2 circle
    GrandEntranceDisappear = 46203, // Helper->location, 3.5s cast, range 2 circle
    GrandEntranceVisual = 46206, // FloorSnake->self, no cast, single-target
    BringDownTheHouseBoss = 46204, // Boss->self, 3.0s cast, single-target
    BringDownTheHouse = 46205, // Helper->self, 4.0s cast, range 15 width 10 rect
    SplitScourge = 46207, // SnakeHead->self, 5.0s cast, range 30 width 10 rect
    VenomousScourge = 46208, // Helper->players, 5.0s cast, range 5 circle

    MindlessFleshBoss1 = 48088, // Boss->self, 3.0s cast, single-target
    MindlessFleshBoss2 = 48089, // Boss->self, 3.0s cast, single-target
    MindlessFlesh1 = 48090, // Helper->self, 4.0s cast, range 30 width 8 rect
    MindlessFlesh2 = 48091, // Helper->self, 5.5s cast, range 30 width 8 rect
    MindlessFlesh3 = 48092, // Helper->self, 7.5s cast, range 30 width 8 rect
    MindlessFlesh4 = 48093, // Helper->self, 8.5s cast, range 30 width 8 rect
    MindlessFlesh5 = 48094, // Helper->self, 10.0s cast, range 30 width 8 rect
    MindlessFleshWide = 48095, // Helper->self, 16.1s cast, range 30 width 35 rect
    MindlessFleshDramaticLysis = 48371, // Helper->players, 5.0s cast, range 6 circle

    Unknown1 = 46201, // Boss->self, no cast, single-target
    Unknown2 = 46224, // Boss->self, no cast, single-target
    Unknown3 = 47046, // Boss->self, no cast, single-target
    PhaseChange1 = 48085, // Boss->self, no cast, single-target, phase 2 starts ~25s later
    PhaseChange2 = 48332, // Boss->self, no cast, single-target, phase 2 starts ~25s later
}

public enum SID : uint
{
    FleshForward = 4747, // none->player, extra=0x0, knockback forward on expiry
    FleshBack = 4748, // none->player, extra=0x0, knockback backward on expiry
    BurstingGrotesquerie = 4749, // none->player, extra=0x0, spread on expiry
    SharedGrotesquerie = 4750, // none->player, extra=0x0, stack on expiry
    DirectedGrotesquerie = 4751, // none->player, extra=0x0, cone on expiry
    FateOfTheWurm = 4772, // none->player, extra=0x0
    GrotesquerieVisual = 4956, // none->player, extra=0x441
    Direction = 2056, // none->player, extra=0x408 front, 0x409 right, 0x40A back, 0x40B left (Directed Grotesquerie cone)
}

public enum IconID : uint
{
    VisceralBurst = 344, // player->self
    DramaticLysis = 139, // player->self
    VenomousScourge = 376, // player->self
    Flesh = 654, // player->self
    SharedGrotesquerie = 93, // player->self
    DirectedGrotesquerie = 354, // player->self
}

class TheFixer(BossModule module) : Components.RaidwideCast(module, AID.TheFixer);

class SerpentineScourge(BossModule module) : Components.GenericAOEs(module, AID.SerpentineScourge)
{
    private static readonly AOEShapeRect _shape = new(30, 10);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_aoe is { } aoe)
            yield return aoe;
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.BloodshedEast:
                _aoe = new(_shape, new(110, 85), default, WorldState.FutureTime(8.1f));
                break;
            case AID.BloodshedWest:
                _aoe = new(_shape, new(90, 85), default, WorldState.FutureTime(8.1f));
                break;
            case AID.SerpentineScourge:
                ++NumCasts;
                _aoe = null;
                break;
        }
    }
}

class RavenousReach(BossModule module) : Components.StandardAOEs(module, AID.RavenousReach, new AOEShapeCone(35, 60.Degrees()));
class Burst(BossModule module) : Components.GenericAOEs(module, AID.Burst)
{
    private static readonly AOEShapeCircle _shape = new(12);
    public readonly List<AOEInstance> AOEs = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => AOEs;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (actor.OID == (uint)OID.BurstBlob && state == 0x00100020)
            AOEs.Add(new(_shape, actor.Position, default, WorldState.FutureTime(6.6f)));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action == WatchedAction && !AOEs.Any(a => a.Origin.AlmostEqual(spell.LocXZ, 1)))
            AOEs.Add(new(_shape, spell.LocXZ, default, Module.CastFinishAt(spell)));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            AOEs.Clear();
        }
    }
}
class VisceralBurst(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.VisceralBurst, AID.VisceralBurst, 5.1f, centerAtTarget: true, damageType: AIHints.PredictedDamageType.Tankbuster);

class CruelCoil(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShape SkinsplitterShape = new AOEShapeDonutSector(9, 14, 157.5f.Degrees());
    private static readonly AOEShape CoilShape = new AOEShapeDonutSector(9.5f, 13.5f, 157.5f.Degrees());
    private bool _active;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_active)
            yield return new(SkinsplitterShape, Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees());
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (_active)
            hints.TemporaryObstacles.Add(CoilShape.Distance(Module.PrimaryActor.Position, Module.PrimaryActor.Rotation + 180.Degrees()));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SkinsplitterBoss)
            _active = true;
        else if ((AID)spell.Action.ID == AID.Constrictor)
            _active = false;
    }
}

class Constrictor(BossModule module) : Components.GenericAOEs(module, AID.Constrictor)
{
    private static readonly AOEShapeCircle _shape = new(13);
    private DateTime _activation;
    private DateTime _gapCloses;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_activation != default)
            yield return new(_shape, Arena.Center, default, _gapCloses != default && _gapCloses < _activation ? _gapCloses : _activation);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CruelCoilVisual)
            _activation = WorldState.FutureTime(11);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SkinsplitterBoss)
        {
            _gapCloses = WorldState.FutureTime(5);
        }
        else if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            _activation = default;
            _gapCloses = default;
        }
    }
}

class BurstingGrotesquerie(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Spread> _pending = [];

    public bool Done => _pending.Count == 0 && Spreads.Count == 0;

    public override void Update()
    {
        var show = WorldState.FutureTime(6);
        Spreads.AddRange(_pending.Where(s => s.Activation <= show));
        _pending.RemoveAll(s => s.Activation <= show);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.BurstingGrotesquerie)
            _pending.Add(new(actor, 6, status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.BurstingGrotesquerie)
        {
            _pending.RemoveAll(s => s.Target == actor);
            Spreads.RemoveAll(s => s.Target == actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.DramaticLysis)
            Spreads.RemoveAll(s => s.Target.InstanceID == spell.MainTargetID);
    }
}

class SharedGrotesquerie(BossModule module) : Components.GenericStackSpread(module)
{
    private readonly List<Stack> _pending = [];

    public bool Done => _pending.Count == 0 && Stacks.Count == 0;

    public override void Update()
    {
        var show = WorldState.FutureTime(6);
        Stacks.AddRange(_pending.Where(s => s.Activation <= show));
        _pending.RemoveAll(s => s.Activation <= show);
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.SharedGrotesquerie)
            _pending.Add(new(actor, 6, activation: status.ExpireAt));
    }

    public override void OnStatusLose(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.SharedGrotesquerie)
        {
            _pending.RemoveAll(s => s.Target == actor);
            Stacks.RemoveAll(s => s.Target == actor);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.FourthWallFusion)
        {
            _pending.Clear();
            Stacks.Clear();
        }
    }
}

class DirectedGrotesquerie(BossModule module) : Components.GenericBaitAway(module, AID.HemorrhagicProjection, centerAtTarget: true)
{
    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID == SID.Direction && status.Extra is >= 0x408 and <= 0x40B)
            CurrentBaits.Add(new(actor, actor, new AOEShapeCone(60, 15.Degrees(), ((status.Extra - 0x408) * -90f).Degrees()), status.ExpireAt));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        foreach (var b in ActiveBaits)
        {
            if (b.Target != actor)
            {
                hints.AddForbiddenZone(b.Shape, b.Target.Position, b.Rotation, b.Activation);
            }
            else if (b.Shape is AOEShapeCone cone)
            {
                foreach (var p in Raid.WithoutSlot().Exclude(actor))
                    hints.ForbiddenDirections.Add((Angle.FromDirection(p.Position - actor.Position) - cone.DirectionOffset, cone.HalfAngle, b.Activation));
            }
        }
    }
}

class FleshKnockback(BossModule module) : Components.Knockback(module, AID.Shockwave)
{
    private const float Distance = 15;
    private readonly DateTime[] _activation = new DateTime[PartyState.MaxPartySize];
    private readonly bool[] _back = new bool[PartyState.MaxPartySize];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        if (slot < _activation.Length && _activation[slot] != default)
            yield return new(actor.Position, Distance, _activation[slot], null, actor.Rotation + (_back[slot] ? 180.Degrees() : default), Kind.DirForward);
    }

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
        => base.DestinationUnsafe(slot, actor, pos) || Module.FindComponent<Burst>()?.AOEs.Any(a => a.Check(pos)) == true;

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (slot >= _activation.Length || _activation[slot] == default || IsImmune(slot, _activation[slot]))
            return;
        for (var deg = 0; deg < 360; deg += 10)
        {
            var facing = deg.Degrees();
            var push = (facing + (_back[slot] ? 180.Degrees() : default)).ToDirection();
            if (DestinationUnsafe(slot, actor, actor.Position + push * Distance))
                hints.ForbiddenDirections.Add((facing, 5.Degrees(), _activation[slot]));
        }
    }

    public override void OnStatusGain(Actor actor, in ActorStatus status)
    {
        if ((SID)status.ID is SID.FleshForward or SID.FleshBack && Raid.FindSlot(actor.InstanceID) is var slot && slot >= 0 && slot < _activation.Length)
        {
            _activation[slot] = status.ExpireAt;
            _back[slot] = (SID)status.ID == SID.FleshBack;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction && Raid.FindSlot(spell.MainTargetID) is var slot && slot >= 0 && slot < _activation.Length)
        {
            ++NumCasts;
            _activation[slot] = default;
        }
    }
}

class Splattershed1(BossModule module) : Components.RaidwideCastDelay(module, AID.SplattershedBoss1, AID.Splattershed, 2.5f);
class Splattershed2(BossModule module) : Components.RaidwideCastDelay(module, AID.SplattershedBoss2, AID.Splattershed, 2.5f);
class SplattershedLong(BossModule module) : Components.RaidwideCastDelay(module, AID.SplattershedBoss3, AID.SplattershedLong, 2.3f);

class GrandEntrance(BossModule module) : Components.GroupedAOEs(module, [AID.GrandEntranceAppear, AID.GrandEntranceDisappear], new AOEShapeCircle(2));
class BringDownTheHouse(BossModule module) : Components.StandardAOEs(module, AID.BringDownTheHouse, new AOEShapeRect(15, 5));

class FloorBreak(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeRect _tile = new(7.5f, 5, 7.5f);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != AID.GrandEntranceAppear)
            return;
        var column = Math.Clamp((int)((caster.Position.X - Module.Center.X + 20) / 10), 0, 3);
        var tile = new WPos(Module.Center.X - 15 + 10 * column, caster.Position.Z < Module.Center.Z ? Module.Center.Z - 7.5f : Module.Center.Z + 7.5f);
        if (!_aoes.Any(a => a.Origin.AlmostEqual(tile, 1)))
            _aoes.Add(new(_tile, tile, default, WorldState.FutureTime(10)));
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state is 0x00020001 or 0x02000100)
            _aoes.Clear();
    }
}
class SplitScourge(BossModule module) : Components.StandardAOEs(module, AID.SplitScourge, new AOEShapeRect(30, 5));
class VenomousScourge(BossModule module) : Components.SpreadFromCastTargets(module, AID.VenomousScourge, 5);

class MindlessFlesh(BossModule module) : Components.GroupedAOEs(module, [AID.MindlessFlesh1, AID.MindlessFlesh2, AID.MindlessFlesh3, AID.MindlessFlesh4, AID.MindlessFlesh5], new AOEShapeRect(30, 4), 2, true);
class MindlessFleshWide(BossModule module) : Components.StandardAOEs(module, AID.MindlessFleshWide, new AOEShapeRect(30, 17.5f))
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        foreach (var c in Casters)
        {
            var inside = ShapeDistance.Rect(c.CastInfo!.LocXZ, c.CastInfo.Rotation, 30, 0, 17.5f);
            hints.GoalZones.Add(p => inside(p) > 0 ? 1 : 0);
        }
    }
}
class MindlessFleshDramaticLysis(BossModule module) : Components.SpreadFromCastTargets(module, AID.MindlessFleshDramaticLysis, 6);

class ArenaChanges(BossModule module) : BossComponent(module)
{
    private const float Margin = 0.5f;
    private static readonly WPos[] TilesA = [new(85, 107.5f), new(95, 92.5f), new(105, 107.5f), new(115, 92.5f)];
    private static readonly WPos[] TilesB = [new(85, 92.5f), new(95, 107.5f), new(105, 92.5f), new(115, 107.5f)];
    private WPos[] _tiles = [];
    private bool _middle;
    private readonly List<WPos> _holes = [];

    public override void OnMapEffect(byte index, uint state)
    {
        if (index != 0x00)
            return;
        switch (state)
        {
            case 0x00020001:
                _tiles = TilesA;
                _middle = false;
                _holes.Clear();
                break;
            case 0x02000100:
                _tiles = TilesB;
                _middle = false;
                _holes.Clear();
                break;
            case 0x00200010:
            case 0x08000400:
                _middle = true;
                break;
            case 0x10000004:
            case 0x00080004:
            case 0x00800004:
            case 0x80000004:
                _tiles = [];
                _middle = false;
                _holes.Clear();
                break;
            default:
                return;
        }
        UpdateBounds();
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.GrandEntranceAppear or AID.GrandEntranceDisappear)
        {
            _holes.Add((AID)spell.Action.ID == AID.GrandEntranceAppear ? caster.Position : spell.TargetXZ);
            UpdateBounds();
        }
    }

    private void UpdateBounds()
    {
        if (_tiles.Length == 0 && _holes.Count == 0)
        {
            Arena.Bounds = new ArenaBoundsRect(20, 15);
            return;
        }

        var clipper = new PolygonClipper();
        List<PolygonClipper.Operand> floor = [];
        if (_tiles.Length == 0)
            floor.Add(new(CurveApprox.Rect(new WDir(0, 1), 20, 15)));
        foreach (var t in _tiles)
            floor.Add(new(CurveApprox.Rect(new WDir(0, 1), 5 - Margin, 7.5f - Margin).Select(c => c + (t - Arena.Center))));
        if (_middle)
            floor.Add(new(CurveApprox.Rect(new WDir(0, 1), 20, 5 - Margin)));
        var poly = clipper.UnionAll(floor[0], [.. floor.Skip(1)]);
        if (_holes.Count > 0)
        {
            PolygonClipper.Operand holes = new();
            foreach (var h in _holes)
                holes.AddContour(CurveApprox.Circle(2 + Margin, 0.1f).Select(c => c + (h - Arena.Center)));
            poly = clipper.Difference(new(poly), holes);
        }
        Arena.Bounds = new ArenaBoundsCustom(20, poly);
    }
}

class PhaseChange(BossModule module) : BossComponent(module)
{
    public bool Done;

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.PhaseChange1 or AID.PhaseChange2)
            Done = true;
    }
}

class RM12NTheLindwurmStates : StateMachineBuilder
{
    public RM12NTheLindwurmStates(BossModule module) : base(module)
    {
        SimplePhase(0, Phase1, "P1")
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<DirectedGrotesquerie>()
            .ActivateOnEnter<VisceralBurst>()
            .ActivateOnEnter<PhaseChange>()
            .Raw.Update = () => Module.PrimaryActor.IsDeadOrDestroyed || Module.FindComponent<PhaseChange>()?.Done == true;
        DeathPhase(1, Phase2)
            .ActivateOnEnter<ArenaChanges>();
    }

    private void Phase1(uint id)
    {
        Fixer(id, 6.1f);
        SerpentineScourge(id + 0x10000, 7.2f);
        RavenousReach(id + 0x20000, 2.2f);
        Burst(id + 0x30000, 9.2f);
        CruelCoil(id + 0x40000, 5.6f, 22.4f);
        VisceralBurst(id + 0x50000, 6.1f, 1);
        SerpentineScourge(id + 0x60000, 4.1f);
        GrotesquerieAct1(id + 0x70000, 2.1f);
        SnakeFloor(id + 0x80000, 11.8f);
        GrotesquerieAct2(id + 0x90000, 8.0f);
        SerpentineScourge(id + 0xA0000, 6.7f);
        Splattershed(id + 0xB0000, 2.2f);
        VisceralBurst(id + 0xC0000, 11.3f, 2);
        CruelCoil(id + 0xD0000, 7.1f, 17.2f);
        GrotesquerieAct3(id + 0xE0000, 9.2f);

        SimpleState(id + 0xF0000, 120, "Until phase change")
            .ActivateOnEnter<TheFixer>()
            .ActivateOnEnter<FleshKnockback>()
            .ActivateOnEnter<BurstingGrotesquerie>()
            .ActivateOnEnter<SharedGrotesquerie>()
            .ActivateOnEnter<CruelCoil>()
            .ActivateOnEnter<Constrictor>()
            .ActivateOnEnter<SerpentineScourge>()
            .ActivateOnEnter<RavenousReach>()
            .ActivateOnEnter<Burst>()
            .ActivateOnEnter<Splattershed1>()
            .ActivateOnEnter<Splattershed2>()
            .ActivateOnEnter<GrandEntrance>()
            .ActivateOnEnter<BringDownTheHouse>()
            .ActivateOnEnter<FloorBreak>()
            .ActivateOnEnter<SplitScourge>()
            .ActivateOnEnter<VenomousScourge>();
    }

    private void Phase2(uint id)
    {
        MindlessFlesh(id, 25.1f);
        SplattershedLong(id + 0x10000, 3.1f);
        MindlessFlesh(id + 0x20000, 8.0f);
        SplattershedLong(id + 0x30000, 3.1f);
        MindlessFlesh(id + 0x40000, 8.0f);
        SplattershedLong(id + 0x50000, 3.1f);
        MindlessFlesh(id + 0x60000, 8.0f);
        SplattershedLong(id + 0x70000, 3.1f);
        SimpleState(id + 0x80000, 30, "???");
    }

    private void Fixer(uint id, float delay)
    {
        Cast(id, AID.TheFixer, delay, 5, "Raidwide")
            .ActivateOnEnter<TheFixer>()
            .DeactivateOnExit<TheFixer>()
            .SetHint(StateMachine.StateHint.Raidwide);
    }

    private void SerpentineScourge(uint id, float delay)
    {
        CastMulti(id, [AID.BloodshedEast, AID.BloodshedWest], delay, 0.5f)
            .ActivateOnEnter<SerpentineScourge>();
        CastMulti(id + 0x10, [AID.SerpentineScourgeEastBoss, AID.SerpentineScourgeWestBoss], 6.2f, 1);
        ComponentCondition<SerpentineScourge>(id + 0x20, 1, c => c.NumCasts > 0, "Half-room cleave")
            .DeactivateOnExit<SerpentineScourge>();
    }

    private void RavenousReach(uint id, float delay)
    {
        Cast(id, AID.RavenousReachBoss, delay, 1)
            .ActivateOnEnter<RavenousReach>();
        ComponentCondition<RavenousReach>(id + 0x10, 10.6f, c => c.NumCasts > 0, "Cone")
            .DeactivateOnExit<RavenousReach>();
    }

    private void Burst(uint id, float delay)
    {
        ComponentCondition<Burst>(id, delay, c => c.NumCasts > 0, "Circles")
            .ActivateOnEnter<Burst>()
            .DeactivateOnExit<Burst>();
    }

    private void CruelCoil(uint id, float delay, float constrictorDelay)
    {
        CastMulti(id, [AID.CruelCoil1, AID.CruelCoil2], delay, 3)
            .ActivateOnEnter<CruelCoil>()
            .ActivateOnEnter<Constrictor>();
        ComponentCondition<Constrictor>(id + 0x10, constrictorDelay, c => c.NumCasts > 0, "Constrictor")
            .DeactivateOnExit<CruelCoil>()
            .DeactivateOnExit<Constrictor>();
    }

    private void VisceralBurst(uint id, float delay, int count)
    {
        Cast(id, AID.VisceralBurstBoss, delay, 4);
        ComponentCondition<VisceralBurst>(id + 0x10, 1, c => c.NumCasts >= count, "Tankbusters")
            .SetHint(StateMachine.StateHint.Tankbuster);
    }

    private void Splattershed(uint id, float delay)
    {
        Cast(id, AID.SplattershedBoss2, delay, 3)
            .ActivateOnEnter<Splattershed2>();
        ComponentCondition<Splattershed2>(id + 0x10, 2.5f, c => c.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<Splattershed2>();
    }

    private void GrotesquerieAct1(uint id, float delay)
    {
        Cast(id, AID.Grotesquerie, delay, 3)
            .ActivateOnEnter<BurstingGrotesquerie>();
        RavenousReach(id + 0x100, 3.2f);
        Burst(id + 0x200, 9.2f);
        ComponentCondition<BurstingGrotesquerie>(id + 0x300, 5.3f, c => c.Done, "Spreads")
            .DeactivateOnExit<BurstingGrotesquerie>();
        Cast(id + 0x400, AID.SplattershedBoss1, 2.3f, 3)
            .ActivateOnEnter<Splattershed1>();
        ComponentCondition<Splattershed1>(id + 0x410, 2.5f, c => c.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<Splattershed1>();
    }

    private void SnakeFloor(uint id, float delay)
    {
        Cast(id, AID.FeralFission, delay, 3)
            .ActivateOnEnter<GrandEntrance>()
            .ActivateOnEnter<BringDownTheHouse>()
            .ActivateOnEnter<FloorBreak>()
            .ActivateOnEnter<SplitScourge>();
        Cast(id + 0x10, AID.BringDownTheHouseBoss, 4.2f, 3)
            .DeactivateOnExit<GrandEntrance>();
        ComponentCondition<BringDownTheHouse>(id + 0x20, 1, c => c.NumCasts > 0, "Rects")
            .DeactivateOnExit<BringDownTheHouse>()
            .DeactivateOnExit<FloorBreak>();
        ComponentCondition<SplitScourge>(id + 0x30, 13.2f, c => c.NumCasts > 0, "Lines")
            .ActivateOnEnter<VenomousScourge>()
            .DeactivateOnExit<SplitScourge>();
    }

    private void GrotesquerieAct2(uint id, float delay)
    {
        CastStart(id, AID.Grotesquerie, delay)
            .ActivateOnEnter<SharedGrotesquerie>()
            .ActivateOnEnter<FleshKnockback>();
        ComponentCondition<VenomousScourge>(id + 0x10, 0.1f, c => c.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<VenomousScourge>();
        CastEnd(id + 0x20, 2.9f);
        Cast(id + 0x30, AID.RavenousReachBoss, 5.2f, 1)
            .ActivateOnEnter<RavenousReach>();
        ComponentCondition<FleshKnockback>(id + 0x40, 2.1f, c => c.NumCasts > 0, "Knockbacks")
            .DeactivateOnExit<FleshKnockback>();
        ComponentCondition<RavenousReach>(id + 0x50, 8.6f, c => c.NumCasts > 0, "Cone")
            .DeactivateOnExit<RavenousReach>();
        ComponentCondition<SharedGrotesquerie>(id + 0x60, 0.4f, c => c.Done, "Stack")
            .DeactivateOnExit<SharedGrotesquerie>();
        Burst(id + 0x70, 8.5f);
    }

    private void GrotesquerieAct3(uint id, float delay)
    {
        Cast(id, AID.Grotesquerie, delay, 3)
            .ActivateOnEnter<BurstingGrotesquerie>()
            .ActivateOnEnter<FleshKnockback>()
            .ActivateOnEnter<Burst>();
        ComponentCondition<FleshKnockback>(id + 0x10, 8.3f, c => c.NumCasts > 0, "Knockbacks")
            .DeactivateOnExit<FleshKnockback>();
        ComponentCondition<Burst>(id + 0x20, 1.3f, c => c.NumCasts > 0, "Circles")
            .DeactivateOnExit<Burst>();
        ComponentCondition<BurstingGrotesquerie>(id + 0x30, 5.7f, c => c.Done, "Spreads")
            .DeactivateOnExit<BurstingGrotesquerie>();
    }

    private void MindlessFlesh(uint id, float delay)
    {
        CastMulti(id, [AID.MindlessFleshBoss1, AID.MindlessFleshBoss2], delay, 3)
            .ActivateOnEnter<MindlessFlesh>()
            .ActivateOnEnter<MindlessFleshWide>()
            .ActivateOnEnter<MindlessFleshDramaticLysis>();
        ComponentCondition<MindlessFlesh>(id + 0x10, 1, c => c.NumCasts > 0, "Lanes start");
        ComponentCondition<MindlessFleshDramaticLysis>(id + 0x20, 5, c => c.NumFinishedSpreads > 0, "Spreads")
            .DeactivateOnExit<MindlessFleshDramaticLysis>();
        ComponentCondition<MindlessFleshWide>(id + 0x30, 7.1f, c => c.NumCasts > 0, "Wide line")
            .DeactivateOnExit<MindlessFlesh>()
            .DeactivateOnExit<MindlessFleshWide>();
    }

    private void SplattershedLong(uint id, float delay)
    {
        Cast(id, AID.SplattershedBoss3, delay, 5)
            .ActivateOnEnter<SplattershedLong>();
        ComponentCondition<SplattershedLong>(id + 0x10, 2.3f, c => c.NumCasts > 0, "Raidwide")
            .SetHint(StateMachine.StateHint.Raidwide)
            .DeactivateOnExit<SplattershedLong>();
    }
}

[ModuleInfo(Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1074, NameID = 14378)]
public class RM12NTheLindwurm(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsRect(20, 15));

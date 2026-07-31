namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

public enum OID : uint
{
    Boss = 0x4A94,
    Helper = 0x233C,
    Horse = 0x4A95, // R2.200, x0 (spawn during fight)
    Stalwart = 0x4A96, // R2.000, x0 (spawn during fight)
    Crab = 0x4A98, // R2.200, x0 (spawn during fight)
    Turtle = 0x4A97, // R2.200, x0 (spawn during fight)
    AquaSpearTile = 0x1EBF1E,
    IceDefamations = 0x1EBF1B,
    IceDonut = 0x1EBF1D,
    IceSphere = 0x1EBF1C,
}
public enum AID : uint
{
    AutoAttack = 45769, // Boss->player, no cast, single-target
    PiercingPlunge = 45837, // Boss->self, 5.0s cast, range 70 circle
    Hydrocannon = 45836, // Helper->self/player, 5.0s cast, range 70 width 6 rect
    SwimmingInTheAir = 45809, // Boss->self, 4.0s cast, single-target
    Tidalspout = 47089, // Helper->players, 6.0s cast, range 6 circle
    Hydrofall = 45810, // Helper->location, 1.0s cast, range 12 circle
    SurgingCurrent = 45827, // Helper->self, 8.0s cast, range 60 45-degree cone
    AquaBall = 45835, // Helper->location, 3.0s cast, range 5 circle
    WatersongSteed = 45805, // Horse->self, 1.0s cast, range 40 width 8 rect
    WatersongStalwart = 45806, // 4A96->self, 1.0s cast, range 40 width 8 rect
    WatersongSteward = 45807, // 4A97->self, 1.0s cast, range 40 width 8 rect
    WatersongSoldier = 45808, // 4A98->self, 1.0s cast, range 40 width 8 rect
    AquaSpear = 45817, // Boss->self, 4.0s cast, single-target
    AquaSpearTileCast = 45818, // Helper->self, 3.0s cast, range 8 width 8 rect
    TidalWaveCast = 45819, // Boss->self, 4.0+1.0s cast, single-target
    TidalWave = 45820, // Helper->self, 6.0s cast, range 60 width 60 rect
    SphereShatterCircle = 45813, // Helper->self, no cast, range 18 circle
    SphereShatterDonut = 45814, // Helper->self, no cast, range ?-20 donut
    RecedingTwinTides = 45828, // Boss->self, 3.0+1.0s cast, single-target
    EncroachingTwinTides = 45831, // Boss->self, 3.0+1.0s cast, single-target
    NearTideFirst = 45829, // Helper->location, 4.0s cast, range 10 circle
    NearTideSecond = 45830, // Helper->location, no cast, range 10 circle
    FarTideFirst = 45833, // Helper->location, no cast, range ?-40 donut
    FarTideSecond = 45832, // Helper->location, 4.0s cast, range 10-40 donut
    HydrobulletCast = 45815, // Boss->self, 3.0+1.0s cast, single-target
    HydrobulletDefamation = 45816, // Helper->players, 5.0s cast, range 15 circle
    HydrobulletDefamation2 = 45811, // Helper->player, 6.0s cast, range 15 circle
    CeaselessCurrentCast = 45823, // Boss->self, 4.0+1.0s cast, single-target
    CeaselessCurrentFirst = 45824, // Helper->self, 5.0s cast, range 8 width 40 rect
    CeaselessCurrentRest = 45825, // Helper->self, no cast, range 8 width 40 rect
}
public enum SID : uint
{
    RightFace = 2164, // Boss->player, extra=0x0
    LeftFace = 2163, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x8/0x4
    ForwardMarch = 2161, // none->player, extra=0x0
    AboutFace = 2162, // none->player, extra=0x0
}
public enum VfxID : uint
{
    Horse = 2741,
    Stalwart = 2742,
    Turtle = 2743,
    Crab = 2744,
}
public enum IconID : uint
{
    HydrofallStack = 318, // player->self
}
public enum TetherID : uint
{
    Bad = 376, // player->player
    Good = 377, // player->player
}

class PiercingPlunge(BossModule module) : Components.RaidwideCast(module, AID.PiercingPlunge);
class Hydrobullet(BossModule module) : Components.SpreadFromCastTargets(module, AID.HydrobulletDefamation, 7.5f);
class Hydrobullet2(BossModule module) : Components.SpreadFromCastTargets(module, AID.HydrobulletDefamation2, 7.5f);
class AquaSpear(BossModule module) : Components.StandardAOEs(module, AID.AquaSpearTileCast, new AOEShapeRect(8, 4));

// using BaitAway because not sure of any other TB rect components atm
class Hydrocannon(BossModule module) : Components.GenericBaitAway(module, AID.Hydrocannon)
{
    private static readonly AOEShapeRect _shape = new(40, 3);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        var target = WorldState.Actors.Find(spell.TargetID)!;
        if (spell.Action == WatchedAction && target != null)
            CurrentBaits.Add(new(caster, target, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (spell.Action == WatchedAction)
        {
            CurrentBaits.Clear();
        }
    }
}

class Hydrofall(BossModule module) : Components.StackWithIcon(module, (uint)IconID.HydrofallStack, AID.Hydrofall, 3, 0)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.Hydrofall)
            Stacks.Clear();
    }
}

class FamiliarCall(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShapeRect Rect = new(40, 4);

    public uint ColorImminent = ArenaColor.Danger;
    public int NumShown = 2;

    protected record struct Caster(Actor Source, AOEShape Shape, DateTime Activation);

    protected readonly List<List<Caster>> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var i = 0;
        foreach (var srcs in _sources.Take(NumShown))
        {
            foreach (var src in srcs)
                yield return new(src.Shape, src.Source.Position, src.Source.Rotation, src.Activation, Risky: i == 0, Color: i == 0 ? ColorImminent : ArenaColor.AOE);
            i++;
        }
    }

    public override void OnEventVFX(Actor actor, uint vfxID, ulong targetID)
    {
        if ((OID)actor.OID == OID.Boss)
        {
            var (caster, shape) = (VfxID)vfxID switch
            {
                VfxID.Stalwart => (OID.Stalwart, Rect),
                VfxID.Turtle => (OID.Turtle, Rect),
                VfxID.Horse => (OID.Horse, Rect),
                VfxID.Crab => (OID.Crab, Rect),
                _ => (0, null)
            };
            if (shape != null)
            {
                var activation = _sources.Count > 0 ? _sources[^1][0].Activation.AddSeconds(3) : WorldState.FutureTime(11.6f);
                _sources.Add([.. Module.Enemies(caster).Select(c => new Caster(c, shape, activation))]);
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.WatersongSoldier or AID.WatersongStalwart or AID.WatersongSteed or AID.WatersongSteward)
        {
            if (_sources.Count > 0)
            {
                _sources[0].RemoveAll(c => c.Source == caster);
                if (_sources[0].Count == 0)
                {
                    NumCasts++;
                    _sources.RemoveAt(0);
                }
            }
        }
    }
}

class SurgingCurrent(BossModule module) : Components.StandardAOEs(module, AID.SurgingCurrent, new AOEShapeCone(60, 45.Degrees()), maxCasts: 2)
{
    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
            Casters.SortBy(c => Module.CastFinishAt(c.CastInfo));
    }
}

class AquaSpearTiles(BossModule module) : Components.GenericAOEs(module)
{
    public bool Active => _tiles.Count > 0;

    readonly List<(Actor Actor, DateTime Spawn)> _tiles = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _tiles.Where(t => t.Actor.EventState != 7).Select(t => new AOEInstance(new AOEShapeRect(4, 4, 4), t.Actor.Position, default, t.Spawn.AddSeconds(2)));

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == 0x1EBF1E)
            _tiles.Add((actor, WorldState.CurrentTime));
    }

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00040008)
            _tiles.RemoveAll(t => t.Actor == actor);
    }
}

class TidalWave(BossModule module) : Components.Knockback(module, AID.TidalWave)
{
    private readonly List<Actor> _casters = [];

    public override IEnumerable<Source> Sources(int slot, Actor actor)
    {
        foreach (var c in _casters)
        {
            yield return new(
                c.Position,
                25f,
                Module.CastFinishAt(c.CastInfo!),
                Direction: c.CastInfo!.Rotation,
                Kind: Kind.DirForward
            );
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TidalWave)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.TidalWave)
            _casters.Remove(caster);
    }
}

class SphereShatter(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly AOEShape Donut = new AOEShapeDonut(4, 20);
    public static readonly AOEShape Circle = new AOEShapeCircle(18);

    readonly List<AOEInstance> _predicted = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.TakeSpan(TimeSpan.FromSeconds(2));
    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00100020)
        {
            switch ((OID)actor.OID)
            {
                case OID.IceSphere:
                    _predicted.Add(new(Circle, actor.Position, default, WorldState.FutureTime(10.2f)));
                    break;
                case OID.IceDonut:
                    _predicted.Add(new(Donut, actor.Position, default, WorldState.FutureTime(10.2f)));
                    break;
            }
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SphereShatterDonut or AID.SphereShatterCircle)
        {
            NumCasts++;
            _predicted.RemoveAll(p => p.Origin.AlmostEqual(caster.Position, 1));
        }
    }
}

class CeaselessCurrent(BossModule module) : Components.Exaflare(module, new AOEShapeRect(8, 20))
{
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        foreach (var (c, t, r) in FutureAOEs())
            yield return new(Shape, c, r, t, FutureColor, Risky: false);
        foreach (var (c, t, r) in ImminentAOEs())
            yield return new(Shape, c, r, t, ImminentColor);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.CeaselessCurrentFirst)
        {
            Lines.Add(new()
            {
                Next = spell.LocXZ,
                Advance = spell.Rotation.ToDirection() * 8,
                Rotation = spell.Rotation,
                NextExplosion = Module.CastFinishAt(spell),
                TimeToMove = 2.1f,
                ExplosionsLeft = 5,
                MaxShownExplosions = 2
            });
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.CeaselessCurrentFirst or AID.CeaselessCurrentRest)
        {
            NumCasts++;
            var ix = Lines.FindIndex(l => l.Rotation.AlmostEqual(caster.Rotation, 0.1f));
            if (ix >= 0)
            {
                AdvanceLine(Lines[ix], Lines[ix].Next);
                if (Lines[ix].ExplosionsLeft <= 0)
                    Lines.RemoveAt(ix);
            }
        }
    }
}

class SeaShackles(BossModule module) : BossComponent(module)
{
    enum Assignment
    {
        None,
        Good,
        Bad
    }

    record struct PlayerState(Assignment Assignment, int Partner);

    readonly PlayerState[] _assignments = Utils.MakeArray<PlayerState>(4, new(Assignment.None, -1));

    public bool Active => _assignments.Any(s => s.Assignment != Assignment.None);

    public override void OnTethered(Actor source, ActorTetherInfo tether)
    {
        var assignment = (TetherID)tether.ID switch
        {
            TetherID.Good => Assignment.Good,
            TetherID.Bad => Assignment.Bad,
            _ => Assignment.None
        };

        if (assignment == Assignment.None)
            return;

        if (Raid.TryFindSlot(source, out var fromSlot) && Raid.TryFindSlot(tether.Target, out var toSlot))
        {
            _assignments[fromSlot] = new(assignment, toSlot);
            _assignments[toSlot] = new(assignment, fromSlot);
        }
    }

    public override void OnUntethered(Actor source, ActorTetherInfo tether)
    {
        if ((TetherID)tether.ID is TetherID.Good or TetherID.Bad)
            Array.Fill(_assignments, new(Assignment.None, -1));
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        var state = _assignments[pcSlot];

        if (state.Partner < 0)
            return;

        var color = state.Assignment switch
        {
            Assignment.Good => ArenaColor.Safe,
            Assignment.Bad => ArenaColor.Danger,
            _ => ArenaColor.Object
        };

        Arena.AddLine(pc.Position, Raid[state.Partner]!.Position, color);
    }

    public override PlayerPriority CalcPriority(int pcSlot, Actor pc, int playerSlot, Actor player, ref uint customColor)
        => _assignments[pcSlot].Partner == playerSlot ? PlayerPriority.Danger : PlayerPriority.Normal;
}

abstract class TwinTides(BossModule module) : Components.GenericAOEs(module)
{
    readonly List<AOEInstance> _predicted = [];

    protected abstract AID Trigger { get; }
    protected abstract bool CircleFirst { get; }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _predicted.Take(1);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID != Trigger)
            return;

        _predicted.Clear();

        var activation = Module.CastFinishAt(spell);
        var bossPos = Module.PrimaryActor.Position;
        var circle = new AOEInstance(new AOEShapeCircle(10f), bossPos, Activation: activation);
        var donut = new AOEInstance(new AOEShapeDonut(10f, 40f), bossPos, Activation: activation.AddSeconds(2));

        _predicted.AddRange(CircleFirst ? [circle, donut] : [donut, circle]);
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NearTideFirst:
            case AID.FarTideFirst:
            case AID.NearTideSecond:
            case AID.FarTideSecond:
                ++NumCasts;
                if (_predicted.Count > 0)
                    _predicted.RemoveAt(0);
                break;
        }
    }
}
class RecedingTwinTides(BossModule module) : TwinTides(module)
{
    protected override AID Trigger => AID.RecedingTwinTides;
    protected override bool CircleFirst => true;
}
class EncroachingTwinTides(BossModule module) : TwinTides(module)
{
    protected override AID Trigger => AID.EncroachingTwinTides;
    protected override bool CircleFirst => false;
}

class AquaBall(BossModule module) : Components.StandardAOEs(module, AID.AquaBall, 5)
{
    public int NumBaits;

    DateTime _lastStarted;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        base.OnCastStarted(caster, spell);

        if (spell.Action == WatchedAction)
        {
            if (_lastStarted.AddSeconds(1) < WorldState.CurrentTime)
                NumBaits++;
            _lastStarted = WorldState.CurrentTime;
        }
    }
}

class SwimmingInTheAir(BossModule module) : Components.GenericAOEs(module, AID.Hydrofall)
{
    DateTime _activation;

    readonly List<WPos> _sources = [];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _sources.Select(s => new AOEInstance(new AOEShapeCircle(12), s, default, _activation));

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SwimmingInTheAir)
            _activation = WorldState.FutureTime(17.6f);
    }
    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Tidalspout)
            _sources.Clear();
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID == OID.IceDefamations && _activation != default)
            _sources.Add(actor.Position);
    }
}

class ForcedMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace)
{
    public bool EnableHints = true;

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (EnableHints)
            base.AddHints(slot, actor, hints);
    }
}

class V03DaryaTheSeaMaidStates : StateMachineBuilder
{
    public V03DaryaTheSeaMaidStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AquaBall>()
            .ActivateOnEnter<TidalWave>()
            .ActivateOnEnter<AquaSpear>()
            .ActivateOnEnter<Hydrofall>()
            .ActivateOnEnter<ForcedMarch>()
            .ActivateOnEnter<SeaShackles>()
            .ActivateOnEnter<Hydrocannon>()
            .ActivateOnEnter<Hydrobullet>()
            .ActivateOnEnter<Hydrobullet2>()
            .ActivateOnEnter<FamiliarCall>()
            .ActivateOnEnter<SphereShatter>()
            .ActivateOnEnter<PiercingPlunge>()
            .ActivateOnEnter<SurgingCurrent>()
            .ActivateOnEnter<AquaSpearTiles>()
            .ActivateOnEnter<CeaselessCurrent>()
            .ActivateOnEnter<SwimmingInTheAir>()
            .ActivateOnEnter<RecedingTwinTides>()
            .ActivateOnEnter<EncroachingTwinTides>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1084, NameID = 14291)]
public class V03DaryaTheSeaMaid(WorldState ws, Actor primary) : BossModule(ws, primary, new(375, 530), new ArenaBoundsSquare(20));

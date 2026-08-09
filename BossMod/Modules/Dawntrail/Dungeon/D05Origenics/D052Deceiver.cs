namespace BossMod.Dawntrail.Dungeon.D05Origenics.D052Deceiver;

public enum OID : uint
{
    Boss = 0x4170, // R5.0
    Cahciua = 0x418F, // R0.96
    OrigenicsSentryG91 = 0x4172, // R0.9
    OrigenicsSentryG92 = 0x4171, // R0.9
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 870, // Boss->player, no cast, single-target
    AutoAttack2 = 873, // OrigenicsSentryG92->player, no cast, single-target
    Teleport = 36362, // Boss->location, no cast, single-target

    Electrowave = 36371, // Boss->self, 5.0s cast, range 72 circle, raidwide

    BionicThrashVisual1 = 36369, // Boss->self, 7.0s cast, single-target
    BionicThrashVisual2 = 36368, // Boss->self, 7.0s cast, single-target
    BionicThrash = 36370, // Helper->self, 8.0s cast, range 30 90-degree cone

    InitializeAndroids = 36363, // Boss->self, 4.0s cast, single-target, spawns OrigenicsSentryG91 and OrigenicsSentryG92

    SynchroshotFake = 36373, // OrigenicsSentryG91->self, 5.0s cast, range 40 width 4 rect
    SynchroshotReal = 36372, // OrigenicsSentryG92->self, 5.0s cast, range 40 width 4 rect

    InitializeTurretsVisual = 36364, // Boss->self, 4.0s cast, single-target
    InitializeTurretsFake = 36426, // Helper->self, 4.7s cast, range 4 width 10 rect
    InitializeTurretsReal = 36365, // Helper->self, 4.7s cast, range 4 width 10 rect

    LaserLashReal = 36366, // Helper->self, 5.0s cast, range 40 width 10 rect
    LaserLashFake = 38807, // Helper->self, 5.0s cast, range 40 width 10 rect

    SurgeNPCs = 39736, // Helper->self, 8.5s cast, range 40 width 40 rect, knockback 15 dir left/right, only seems to apply to NPCs
    Surge = 36367, // Boss->location, 8.0s cast, range 40 width 40 rect, knockback 30 dir left/right

    Electray = 38320 // Helper->player, 8.0s cast, range 5 circle
}

sealed class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    private const float HalfWidth = 5.5f; // adjusted for 0.5 player hitbox
    public byte CurIndex;

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Electrowave && Arena.Bounds.Radius > 21f)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [new Square(center, 25f)], [new Square(center, 20f)]);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell, 0.7d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00020001u)
        {
            if (index is >= 0x1B and <= 0x2D)
            {
                var defaultSquare = new Square[] { new(Arena.Center, 20f) };
                RectangleSE CreateRow(float x1, float x2, int row) => row switch
                {
                    0 => new(new(x1, -147f), new(x2, -157f), HalfWidth),
                    1 => new(new(x1, -147f), new(x2, -147f), HalfWidth),
                    2 => new(new(x1, -137f), new(x2, -137f), HalfWidth),
                    3 => new(new(x1, -127f), new(x2, -127f), HalfWidth),
                    _ => new(default, default, default),
                };

                RectangleSE West(int row) => CreateRow(-192f, -187.5f, row);
                RectangleSE East(int row) => CreateRow(-152f, -156.5f, row);

                var arena = index switch
                {
                    0x2A => new ArenaBoundsCustom(defaultSquare, [West(1), West(3)]),
                    0x1B => new ArenaBoundsCustom(defaultSquare, [West(1), West(3), East(0), East(2)]),
                    0x2C => new ArenaBoundsCustom(defaultSquare, [West(1), West(2)]),
                    0x1E => new ArenaBoundsCustom(defaultSquare, [West(1), West(2), East(0), East(3)]),
                    0x2D => new ArenaBoundsCustom(defaultSquare, [West(0), West(3)]),
                    0x1D => new ArenaBoundsCustom(defaultSquare, [West(0), West(3), East(1), East(2)]),
                    0x2B => new ArenaBoundsCustom(defaultSquare, [West(0), West(2)]),
                    0x1C => new ArenaBoundsCustom(defaultSquare, [West(0), West(2), East(1), East(3)]),
                    _ => null,
                };
                if (arena != null)
                {
                    CurIndex = index;
                    Arena.Bounds = arena;
                }
            }
            else if (index == 0x12)
            {
                Arena.Bounds = new ArenaBoundsSquare(20f);
                _aoe = [];
            }
        }
        else if (state == 0x00080004u)
        {
            Arena.Bounds = new ArenaBoundsSquare(20f);
        }
    }
}

sealed class Electrowave(BossModule module) : Components.RaidwideCast(module, (uint)AID.Electrowave);
sealed class BionicThrash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BionicThrash, new AOEShapeCone(30f, 45f.Degrees()));
sealed class Synchroshot(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SynchroshotReal, new AOEShapeRect(40f, 2f));
sealed class InitializeTurrets(BossModule module) : Components.SimpleAOEs(module, (uint)AID.InitializeTurretsReal, new AOEShapeRect(4f, 5f));
sealed class LaserLash(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LaserLashReal, new AOEShapeRect(40f, 5f));
sealed class Electray(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.Electray, 5f);

sealed class Surge(BossModule module) : Components.GenericKnockback(module)
{
    public readonly List<Knockback> KBs = [with(2)];
    private readonly ArenaChanges arena = module.FindComponent<ArenaChanges>()!;

    private readonly WDir offset = new(4f, default);
    private readonly AOEShapeCone _shape = new(60f, 90f.Degrees());

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => CollectionsMarshal.AsSpan(KBs);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        void AddSource(Angle offset, SafeWall[] safeWalls)
            => KBs.Add(new(spell.LocXZ, 30f, Module.CastFinishAt(spell), _shape, spell.Rotation + offset, Kind.DirForward, default, safeWalls));
        if (spell.Action.ID == (uint)AID.Surge)
        {
            var safewalls = GetActiveSafeWalls();
            AddSource(90f.Degrees(), safewalls);
            AddSource(-90f.Degrees(), safewalls);
        }
    }

    private SafeWall[] GetActiveSafeWalls()
    {
        static SafeWall West(float z1, float z2) => new(new(-187.5f, z1), new(-187.5f, z2));
        static SafeWall East(float z1, float z2) => new(new(-156.5f, z1), new(-156.5f, z2));

        return arena.CurIndex switch
        {
            0x1B => [West(-142f, -152f), West(-122f, -132f), East(-152f, -162f), East(-132f, -142f)],
            0x1E => [West(-142f, -152f), West(-132f, -142f), East(-152f, -162f), East(-122f, -132f)],
            0x1D => [West(-152f, -162f), West(-122f, -132f), East(-142f, -152f), East(-132f, -142f)],
            0x1C => [West(-152f, -162f), West(-132f, -142f), East(-142f, -152f), East(-122f, -132f)],
            _ => [],
        };
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Surge)
        {
            KBs.Clear();
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (KBs.Count != 0)
        {
            ref readonly var kb = ref KBs.Ref(0);
            var forbidden = new ShapeDistance[4];
            var safewalls = kb.SafeWalls;
            var centerX = Arena.Center.X;
            for (var i = 0; i < 4; ++i)
            {
                ref readonly var safeWall = ref safewalls[i];
                var v1 = safeWall.Vertex1;
                forbidden[i] = new SDInvertedRect(new(centerX, v1.Z - 5f), v1.X == -187.5f ? -offset : offset, 10f, default, 20f);
            }
            hints.AddForbiddenZone(new SDIntersection(forbidden), kb.Activation);
        }
    }
}

sealed class SurgeHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly AOEShapeRect rect = new(15.5f, 5);
    private readonly List<AOEInstance> _hints = [with(4)];
    private readonly Surge _kb = module.FindComponent<Surge>()!;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => CollectionsMarshal.AsSpan(_hints);

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Surge)
        {
            ref readonly var kb = ref _kb.KBs.Ref(0);
            var safewalls = kb.SafeWalls;
            var centerX = Arena.Center.X;
            for (var i = 0; i < 4; ++i)
            {
                ref readonly var safewall = ref safewalls[i];
                var v1 = safewall.Vertex1;
                _hints.Add(new(rect, new(centerX, v1.Z - 5f), (v1.X == -187.5f ? -1f : 1f) * 90f.Degrees(), default, Colors.SafeFromAOE, false));
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Surge)
            _hints.Clear();
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        var count = _hints.Count;
        if (count != 0)
        {
            var isPositionSafe = false;
            var hintz = CollectionsMarshal.AsSpan(_hints);
            for (var i = 0; i < count; ++i)
            {
                ref var hint = ref hintz[i];
                if (hint.Check(actor.Position))
                {
                    isPositionSafe = true;
                    break;
                }
            }
            hints.Add("Wait inside safespot for knockback!", !isPositionSafe);
        }
    }
}

sealed class D052DeceiverStates : StateMachineBuilder
{
    public D052DeceiverStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<Electrowave>()
            .ActivateOnEnter<BionicThrash>()
            .ActivateOnEnter<Synchroshot>()
            .ActivateOnEnter<InitializeTurrets>()
            .ActivateOnEnter<LaserLash>()
            .ActivateOnEnter<Electray>()
            .ActivateOnEnter<Surge>()
            .ActivateOnEnter<SurgeHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 825u, NameID = 12693u, SortOrder = 3)]
public sealed class D052Deceiver(WorldState ws, Actor primary) : BossModule(ws, primary, new(-172f, -142f), new ArenaBoundsSquare(25f))
{
    private static readonly uint[] adds = [(uint)OID.OrigenicsSentryG92, (uint)OID.OrigenicsSentryG91];

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(this, adds);
    }
}

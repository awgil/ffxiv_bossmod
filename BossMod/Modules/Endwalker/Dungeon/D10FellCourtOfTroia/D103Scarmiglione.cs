namespace BossMod.Endwalker.Dungeon.D10FellCourtOfTroia.D103Scarmiglione;

public enum OID : uint
{
    Boss = 0x39C5, // R=7.7
    Necroserf1 = 0x39C7, // R1.4
    Necroserf2 = 0x39C6, // R1.4
    SafewallHitbox = 0x39C8, // R1.0
    Helper = 0x233C
}

public enum AID : uint
{
    AutoAttack1 = 30258, // Boss->player, no cast, single-target
    AutoAttack2 = 872, // Necroserf1->player, no cast, single-target
    Teleport = 30237, // Boss->location, no cast, single-target

    BlightedBedevilment = 30235, // Boss->self, 4.9s cast, range 9 circle
    BlightedBladeworkVisual = 30259, // Boss->location, 10.0s cast, single-target
    BlightedBladework = 30260, // Helper->self, 11.0s cast, range 25 circle
    BlightedSweep = 30261, // Boss->self, 7.0s cast, range 40 180-degree cone

    CorruptorsPitchVisual = 30245, // Boss->self, no cast, single-target
    CorruptorsPitch1 = 30247, // Helper->self, no cast, range 60 circle
    CorruptorsPitch2 = 30248, // Helper->self, no cast, range 60 circle
    CorruptorsPitch3 = 30249, // Helper->self, no cast, range 60 circle
    CorruptorsPitchEnrage = 30250, // Helper->self, no cast, range 60 circle

    CreepingDecay = 30240, // Boss->self, 4.0s cast, single-target
    CursedEcho = 30257, // Boss->self, 4.0s cast, range 40 circle, raidwide

    Nox = 30241, // Helper->self, 5.0s cast, range 10 circle

    RottenRampageVisual = 30231, // Boss->self, 8.0s cast, single-target
    RottenRampage = 30232, // Helper->location, 10.0s cast, range 6 circle
    RottenRampageSpread = 30233, // Helper->players, 10.0s cast, range 6 circle
    RottenRampageEnd = 30234, // Boss->self, no cast, single-target

    LimitBreakStart = 30244, // Boss->self, no cast, single-target

    VacuumWave = 30236, // Helper->self, 5.4s cast, range 40 circle, knockback 30, away from source

    VoidVortexVisual = 30253, // Boss->self, no cast, single-target
    VoidVortex1 = 30243, // Helper->players, 5.0s cast, range 6 circle, stack
    VoidVortex2 = 30254, // Helper->players, 5.0s cast, range 6 circle, stack
    VoidGravity = 30242, // Helper->player, 5.0s cast, range 6 circle, spread

    FiredampVisual = 30262, // Boss->self, 5.0s cast, single-target
    Firedamp = 30263 // Helper->player, 5.4s cast, range 5 circle, tankbuster
}

sealed class ArenaChanges : Components.GenericAOEs
{
    public ArenaChanges(BossModule module) : base(module)
    {
        var walls = safeWalls = GetSafeWalls();
        SafeWalls = [with(12), .. walls];
        _kb = module.FindComponent<VacuumWave>()!;
    }

    private readonly VacuumWave _kb;
    public readonly Rectangle[] safeWalls;
    public readonly List<Rectangle> SafeWalls;
    public readonly List<Rectangle> PendingSafeWalls = [with(4)];
    private readonly AOEShapeDonut donut = new(20f, 25f);
    private static Polygon[] GetDefaultCircle() => [new Polygon(new(-35f, -298f), 20f, 64)];

    private AOEInstance[] _aoe = [];
    private bool outOfBounds;
    private bool defaultArena;
    private const float HalfWidth = 5f;
    private const float HalfHeight = 0.95f;
    public static Rectangle[] GetSafeWalls()
    => [
        new(new(-41.511f, -279.09f), HalfWidth, HalfHeight, -19f.Degrees()), // ENVC 0B
        new(new(-49.142f, -283.858f), HalfWidth, HalfHeight, -45f.Degrees()), // ENVC 0C
        new(new(-53.91f, -291.489f), HalfWidth, HalfHeight, -71f.Degrees()), // ENVC 0D
        new(new(-53.91f, -304.511f), HalfWidth, HalfHeight, 71f.Degrees()), // ENVC 0E
        new(new(-49.142f, -312.142f), HalfWidth, HalfHeight, 45f.Degrees()), // ENVC 0F
        new(new(-41.511f, -316.91f), HalfWidth, HalfHeight, 19f.Degrees()), // ENVC 10
        new(new(-28.489f, -316.911f), HalfWidth, HalfHeight, -19f.Degrees()), // ENVC 05
        new(new(-20.858f, -312.142f), HalfWidth, HalfHeight, -45f.Degrees()), // ENVC 06
        new(new(-16.09f, -304.511f), HalfWidth, HalfHeight, -71f.Degrees()), // ENVC 07
        new(new(-16.09f, -291.489f), HalfWidth, HalfHeight, 71f.Degrees()), // ENVC 08
        new(new(-20.858f, -283.858f), HalfWidth, HalfHeight, 45f.Degrees()), // ENVC 09
        new(new(-28.489f, -279.09f), HalfWidth, HalfHeight, 19f.Degrees()), // ENVC 0A
    ];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.RottenRampageSpread && Arena.Bounds.Radius > 21f)
        {
            var center = Arena.Center;
            _aoe = [new(donut, center, default, Module.CastFinishAt(spell, -1d), shapeDistance: donut.Distance(center, default))];
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        if (state == 0x00200010u)
        {
            void RemoveSafeWall(int index)
            {
                PendingSafeWalls.Remove(safeWalls[index]);
                SafeWalls.Remove(safeWalls[index]);
                ArenaBoundsCustom arena = new(GetDefaultCircle(), [.. SafeWalls]);
                Arena.Bounds = arena;
                Arena.Center = arena.Center;
            }
            switch (index)
            {
                case 0x05:
                    RemoveSafeWall(6);
                    break;
                case 0x06:
                    RemoveSafeWall(7);
                    break;
                case 0x07:
                    RemoveSafeWall(8);
                    break;
                case 0x08:
                    RemoveSafeWall(9);
                    break;
                case 0x09:
                    RemoveSafeWall(10);
                    break;
                case 0x0A:
                    RemoveSafeWall(11);
                    break;
                case 0x0B:
                    RemoveSafeWall(0);
                    break;
                case 0x0C:
                    RemoveSafeWall(1);
                    break;
                case 0x0D:
                    RemoveSafeWall(2);
                    break;
                case 0x0E:
                    RemoveSafeWall(3);
                    break;
                case 0x0F:
                    RemoveSafeWall(4);
                    break;
                case 0x10:
                    RemoveSafeWall(5);
                    break;
            }
        }
        else if (state == 0x00020001u)
        {
            void AddPendingSafeWall(int index, float pos)
            {
                PendingSafeWalls.Add(safeWalls[index]);
                var swalls = _kb.safeWalls;
                var count = swalls.Count;
                var safewalls = CollectionsMarshal.AsSpan(swalls);
                for (var i = 0; i < count; ++i)
                {
                    if (safewalls[i].Vertex1.X == pos)
                    {
                        swalls.RemoveAt(i);
                        return;
                    }
                }
            }
            switch (index)
            {
                case 0x04:
                    _aoe = [];
                    defaultArena = true;
                    break;
                case 0x05:
                    AddPendingSafeWall(6, -33.053f);
                    break;
                case 0x06:
                    AddPendingSafeWall(7, -24.712f);
                    break;
                case 0x07:
                    AddPendingSafeWall(8, -18.453f);
                    break;
                case 0x08:
                    AddPendingSafeWall(9, -18.348f);
                    break;
                case 0x09:
                    AddPendingSafeWall(10, -36.947f);
                    break;
                case 0x0A:
                    AddPendingSafeWall(11, -24.543f);
                    break;
                case 0x0B:
                    AddPendingSafeWall(0, -36.947f);
                    break;
                case 0x0C:
                    AddPendingSafeWall(1, -45.288f);
                    break;
                case 0x0D:
                    AddPendingSafeWall(2, -51.547f);
                    break;
                case 0x0E:
                    AddPendingSafeWall(3, -54.477f);
                    break;
                case 0x0F:
                    AddPendingSafeWall(4, -51.652f);
                    break;
                case 0x10:
                    AddPendingSafeWall(5, -45.457f);
                    break;
            }
        }
    }

    public override void Update()
    {
        // safety precaution so AI can properly pathfind back into arena if it somehow got knocked out of it...
        var player = Raid.Player()!.Position;
        if (_aoe.Length == 0 && defaultArena && !Arena.InBounds(player))
        {
            _aoe = [new(donut, Arena.Center)];
            ArenaBoundsCustom arena = new(D103Scarmiglione.GetStartingCircle(), [.. SafeWalls]);
            Arena.Bounds = arena;
            outOfBounds = true;
        }
        else if (outOfBounds && (player - Arena.Center).LengthSq() < 399f)
        {
            _aoe = [];
            ArenaBoundsCustom arena = new(GetDefaultCircle(), [.. SafeWalls]);
            Arena.Bounds = arena;
            outOfBounds = false;
        }
    }
}

sealed class VacuumWave(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];
    public readonly List<SafeWall> safeWalls =
    [
        new(new(-36.947f, -278.523f), new(-45.457f, -281.453f)), // ENVC 0B
        new(new(-45.288f, -281.348f), new(-51.652f, -287.712f)), // ENVC 0C
        new(new(-51.547f, -287.543f), new(-54.477f, -296.053f)), // ENVC 0D
        new(new(-54.477f, -299.947f), new(-51.547f, -308.457f)), // ENVC 0E
        new(new(-51.652f, -308.288f), new(-45.288f, -314.652f)), // ENVC 0F
        new(new(-45.457f, -314.547f), new(-36.947f, -317.477f)), // ENVC 10
        new(new(-33.053f, -317.478f), new(-24.543f, -314.548f)), // ENVC 05
        new(new(-24.712f, -314.652f), new(-18.348f, -308.288f)), // ENVC 06
        new(new(-18.453f, -308.457f), new(-15.523f, -299.947f)), // ENVC 07
        new(new(-15.523f, -296.053f), new(-18.453f, -287.543f)), // ENVC 08
        new(new(-18.348f, -287.712f), new(-24.712f, -281.348f)), // ENVC 09
        new(new(-24.543f, -281.453f), new(-33.053f, -278.523f)), // ENVC 0A
    ];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VacuumWave)
        {
            _kb = [new(spell.LocXZ, 30f, Module.CastFinishAt(spell), safeWalls: safeWalls, ignoreImmunes: true)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VacuumWave)
        {
            _kb = [];
        }
    }
}

sealed class VacuumWaveHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly ArenaChanges _arena = module.FindComponent<ArenaChanges>()!;
    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VacuumWave)
        {
            var count = _arena.SafeWalls.Count;
            if (count == 0)
            {
                return;
            }
            var cones = new ConeHA[count - _arena.PendingSafeWalls.Count];
            var index = 0;
            var angle = 7f.Degrees(); // safe wall half angle is 13°, but due to deceleration you slide along the wall, so its not actually safe for the full length of the wall - reducing half angle by a conservative 6°
            var truecenter = new WPos(-35f, -298f);
            for (var i = 0; i < count; ++i)
            {
                var wall = _arena.SafeWalls[i];
                if (!_arena.PendingSafeWalls.Contains(wall))
                {
                    cones[index++] = new(truecenter, 20f, Angle.FromDirection(wall.Center - truecenter), angle);
                }
            }
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, cones, invertForbiddenZone: true);
            _aoe = [new(shape, center, default, Module.CastFinishAt(spell), Colors.SafeFromAOE, shapeDistance: shape.InvertedDistance(center, default))];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.VacuumWave)
        {
            _aoe = [];
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length != 0)
        {
            ref var aoe = ref _aoe[0];
            hints.Add("Use safewalls for knockback!", !aoe.Check(actor.Position));
        }
    }
}

sealed class VoidVortex1(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.VoidVortex1, 6f, 4, 4);
sealed class VoidVortex2(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.VoidVortex2, 6f, 4, 4);
sealed class BlightedBedevilment(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlightedBedevilment, 9f);
sealed class BlightedBladework(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlightedBladework, 25f, riskyWithSecondsLeft: 8d);
sealed class Nox(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Nox, 10f);
sealed class RottenRampageAOE(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RottenRampage, 6f);
sealed class RottenRampageSpread(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.RottenRampageSpread, 6f)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        base.AddAIHints(slot, actor, assignment, hints);
        if (IsSpreadTarget(actor))
        {
            var walls = Module.Enemies((uint)OID.SafewallHitbox);
            var count = walls.Count;
            if (count == 0)
            {
                return;
            }
            var forbidden = new ShapeDistance[count];
            for (var i = 0; i < count; ++i)
            {
                forbidden[i] = new SDCircle(walls[i].Position, 7f);
            }
            if (forbidden.Length != 0)
            {
                hints.AddForbiddenZone(new SDUnion(forbidden), Spreads.Ref(0).Activation);
            }
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        base.DrawArenaForeground(pcSlot, pc);
        if (!IsSpreadTarget(pc))
        {
            return;
        }
        var walls = Module.Enemies((uint)OID.SafewallHitbox);
        var count = walls.Count;
        for (var i = 0; i < count; ++i)
        {
            var a = walls[i];
            Arena.ZoneCircleOutline(a.Position, a.HitboxRadius);
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (IsSpreadTarget(actor))
        {
            hints.Add("Spread, avoid intersecting wall hitboxes!");
        }
    }
}

sealed class BlightedSweep(BossModule module) : Components.SimpleAOEs(module, (uint)AID.BlightedSweep, new AOEShapeCone(40f, 90f.Degrees()));
sealed class CursedEcho(BossModule module) : Components.RaidwideCast(module, (uint)AID.CursedEcho);
sealed class VoidGravity(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.VoidGravity, 6f);
sealed class Firedamp(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Firedamp, 5f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);

sealed class CorruptorsPitch(BossModule module) : Components.RaidwideCastDelay(module, (uint)AID.CorruptorsPitchVisual, (uint)AID.CorruptorsPitch3, 8.1d)
{
    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        base.OnEventCast(caster, spell);
        if (spell.Action.ID == ActionVisual)
        {
            Activation = WorldState.FutureTime(Delay);
        }
    }
}

sealed class D103ScarmiglioneStates : StateMachineBuilder
{
    public D103ScarmiglioneStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<VacuumWave>()
            .ActivateOnEnter<ArenaChanges>()
            .ActivateOnEnter<VacuumWaveHint>()
            .ActivateOnEnter<VoidVortex1>()
            .ActivateOnEnter<VoidVortex2>()
            .ActivateOnEnter<BlightedBedevilment>()
            .ActivateOnEnter<BlightedBladework>()
            .ActivateOnEnter<Nox>()
            .ActivateOnEnter<RottenRampageAOE>()
            .ActivateOnEnter<RottenRampageSpread>()
            .ActivateOnEnter<BlightedSweep>()
            .ActivateOnEnter<CursedEcho>()
            .ActivateOnEnter<CorruptorsPitch>()
            .ActivateOnEnter<Firedamp>()
            .ActivateOnEnter<VoidGravity>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 869u, NameID = 11372u, SortOrder = 7)]
public sealed class D103Scarmiglione : BossModule
{
    public D103Scarmiglione(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D103Scarmiglione(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static Polygon[] GetStartingCircle() => [new Polygon(new(-35f, -298f), 24.5f * CosPI.Pi64th, 64)];

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom(GetStartingCircle(), ArenaChanges.GetSafeWalls());
        return (arena.Center, arena);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actor(PrimaryActor);
        Arena.Actors(Enemies((uint)OID.Necroserf1));
    }
}

namespace BossMod.Heavensward.Dungeon.D11Antitower.D152DotoliCiloc;

public enum OID : uint
{
    Boss = 0x179F, // R1.98
    ArenaVoidzone = 0x1EA187, // R2.0
    Whirlwind = 0x17A0 // R1.0
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target

    OnLow = 6606, // Boss->self, 4.0s cast, range 9+R 120-degree cone
    OnHigh = 6607, // Boss->self, 3.0s cast, range 50+R circle, knockback 30, away from source
    DarkWings = 32556, // Boss->player, no cast, range 6 circle, spread
    Swiftfeather = 6609, // Boss->self, 3.0s cast, single-target, applies Haste to boss
    Stormcoming = 32557, // Boss->location, 4.0s cast, range 6 circle
    TerribleFlurry = 6610 // Whirlwind->self, no cast, range 6 circle
}

public enum IconID : uint
{
    DarkWings = 139 // player
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private AOEInstance[] _aoe = [];
    private bool begin;
    private static Polygon[] GetDefaultBoundsPoly() => [new Polygon(new(245.28799f, 13.62114f), 20.3436f, 16, 11.25f.Degrees())];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorEAnim(Actor actor, uint state)
    {
        if (state == 0x00010002u && actor.OID == (uint)OID.ArenaVoidzone)
        {
            var bounds = new ArenaBoundsCustom(GetDefaultBoundsPoly(), D152DotoliCiloc.GetDifferenceShapes(), AdjustForHitboxInwards: true);
            Arena.Bounds = bounds;
            Arena.Center = bounds.Center;
            _aoe = [];
            begin = true;
        }
    }

    public override void Update()
    {
        if (!begin && _aoe.Length == 0)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, D152DotoliCiloc.GetStartingBoundsPoly(), GetDefaultBoundsPoly());
            _aoe = [new(shape, center, default, WorldState.FutureTime(4d), shapeDistance: shape.Distance(center, default))];
        }
    }
}

sealed class DarkWings(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.DarkWings, (uint)AID.DarkWings, 6f, 5.1d);
sealed class Whirlwind(BossModule module) : Components.Voidzone(module, 6f, GetWhirlwinds)
{
    private static List<Actor> GetWhirlwinds(BossModule module) => module.Enemies((uint)OID.Whirlwind);
}
sealed class Stormcoming(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Stormcoming, 6f);
sealed class OnLow(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OnLow, new AOEShapeCone(10.98f, 60f.Degrees()));

sealed class OnLowHaste(BossModule module) : Components.Cleave(module, (uint)AID.Swiftfeather, new AOEShapeCone(10.98f, 60f.Degrees()))
{
    private bool active;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.Swiftfeather)
        {
            active = true;
        }
        else if (spell.Action.ID == (uint)AID.OnLow)
        {
            active = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (active)
        {
            base.AddHints(slot, actor, hints);
        }
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (active)
        {
            base.AddAIHints(slot, actor, assignment, hints);
        }
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        if (active)
        {
            base.DrawArenaForeground(pcSlot, pc);
        }
    }
}

sealed class OnHigh(BossModule module) : Components.GenericKnockback(module)
{
    private Knockback[] _kb = [];

    public override ReadOnlySpan<Knockback> ActiveKnockbacks(int slot, Actor actor) => _kb;

    public override bool DestinationUnsafe(int slot, Actor actor, WPos pos)
    {
        var whirlwinds = Module.Enemies((uint)OID.Whirlwind);
        var count = whirlwinds.Count;
        for (var i = 0; i < count; ++i)
        {
            if (pos.InCircle(whirlwinds[i].Position, 6f))
            {
                return true;
            }
        }
        return !Arena.InBounds(pos);
    }

    private static SafeWall[] GenerateRotatedSafeWalls(ref SafeWall[] baseWalls, float angle)
    {
        var len = baseWalls.Length;
        var rotatedWalls = new SafeWall[len];
        for (var i = 0; i < len; ++i)
        {
            ref var bw = ref baseWalls[i];
            var rotatedVertex1 = GenerateRotatedVertice(bw.Vertex1, angle);
            var rotatedVertex2 = GenerateRotatedVertice(bw.Vertex2, angle);
            rotatedWalls[i] = new(rotatedVertex1, rotatedVertex2);
        }
        return rotatedWalls;
        static WPos GenerateRotatedVertice(WPos vertex, float rotationAngle) => WPos.RotateAroundOrigin(rotationAngle, new(245.28799f, 13.62114f), vertex);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OnHigh)
        {
            SafeWall[] safeWallsW = [new(new(227.487f, 16.825f), new(226.567f, 13.39f)), new(new(226.567f, 13.39f), new(227.392f, 10.301f))];
            var safeWallsN = GenerateRotatedSafeWalls(ref safeWallsW, 90f);
            var safeWallsE = GenerateRotatedSafeWalls(ref safeWallsW, 180f);
            var safeWallsS = GenerateRotatedSafeWalls(ref safeWallsW, 270f);
            SafeWall[] allSafeWalls = [.. safeWallsW, .. safeWallsN, .. safeWallsE, .. safeWallsS];
            _kb = [new(spell.LocXZ, 30f, Module.CastFinishAt(spell), safeWalls: allSafeWalls)];
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OnHigh)
        {
            _kb = [];
        }
    }
}

sealed class OnHighHint(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<ConeHA> cones = [with(4)];
    private AOEInstance[] _aoe = [];
    private DateTime activation;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OnHigh)
        {
            activation = Module.CastFinishAt(spell);
            GenerateHints();
        }
    }

    private void GenerateHints()
    {
        var whirlwinds = Module.Enemies((uint)OID.Whirlwind);
        var count = whirlwinds.Count;
        WPos trueArenaCenter = new(245.28799f, 13.62114f);
        var a11 = 11.25f.Degrees();
        for (var i = 0; i < 4; ++i)
        {
            var deg = (i * 90f).Degrees();
            var enemyInCone = false;
            for (var j = 0; j < count; ++j)
            {
                if (whirlwinds[j].Position.InCone(trueArenaCenter, deg, a11))
                {
                    enemyInCone = true;
                    break;
                }
            }
            if (!enemyInCone)
            {
                cones.Add(new(trueArenaCenter, 20f, deg, a11));
            }
        }
        var center = Arena.Center;
        var shape = new AOEShapeCustom(center, cones, invertForbiddenZone: true);
        _aoe = [new(shape, center, default, activation, Colors.SafeFromAOE, shapeDistance: shape.Distance(center, default))];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (cones.Count != 0 && actor.OID == (uint)OID.Whirlwind) // sometimes the creation of whirlwinds is delayed
        {
            cones.Clear();
            GenerateHints();
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.OnHigh)
        {
            cones.Clear();
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

sealed class D152DotoliCilocStates : StateMachineBuilder
{
    public D152DotoliCilocStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<DarkWings>()
            .ActivateOnEnter<Whirlwind>()
            .ActivateOnEnter<Stormcoming>()
            .ActivateOnEnter<OnLow>()
            .ActivateOnEnter<OnLowHaste>()
            .ActivateOnEnter<OnHigh>()
            .ActivateOnEnter<OnHighHint>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182u, NameID = 5269u, SortOrder = 6)]
public sealed class D152DotoliCiloc : BossModule
{
    public D152DotoliCiloc(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D152DotoliCiloc(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    public static Polygon[] GetStartingBoundsPoly() => [new Polygon(new(245.28799f, 13.62114f), 30.414f, 16, 11.25f.Degrees())];
    public static Rectangle[] GetDifferenceShapes()
    {
        return [new(new(243.52251f, -6.1198f), 2.17224f, 1f, 15f.Degrees()),
        new(new(247.06107f, -6.19115f), 2.05614f, 0.95054f, -15f.Degrees()),
        new(new(265.02832f, 11.84884f), 2.17224f, 1f, -74.98f.Degrees()),
        new(new(265.10089f, 15.38737f), 2.05614f, 0.95054f, -104.98f.Degrees()),
        new(new(247.05348f, 33.36208f), 2.17224f, 1f, -165f.Degrees()),
        new(new(243.51492f, 33.43343f), 2.05614f, 0.95054f, 165f.Degrees()),
        new(new(225.54645f, 15.37981f), 2.17224f, 1f, 104.98f.Degrees()),
        new(new(225.47632f, 11.84123f), 2.05614f, 0.95054f, 74.98f.Degrees())];
    }
    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        var arena = new ArenaBoundsCustom(GetStartingBoundsPoly(), GetDifferenceShapes(), AdjustForHitboxInwards: true);
        return (arena.Center, arena);
    }
}

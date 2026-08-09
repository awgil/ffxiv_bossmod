namespace BossMod.Heavensward.Dungeon.D15Xelphatol.D150SiegeGobbue;

public enum OID : uint
{
    Boss = 0x17B5, // R2.85
    XelphatolBravewing = 0x17B2, // R1.08
    XelphatolWhirltalon = 0x17B1, // R1.08
    XelphatolStrongbeak = 0x17B3, // R1.08
}

public enum AID : uint
{
    AutoAttack1 = 870, // XelphatolWhirltalon/XelphatolBravewing/Boss->player, no cast, single-target
    AutoAttack2 = 871, // XelphatolStrongbeak->player, no cast, single-target

    Overpower = 720, // XelphatolBravewing->self, 2.5s cast, range 6+R 90-degree cone
    FastBlade = 717, // XelphatolWhirltalon->player, no cast, single-target
    ComingStorm = 423, // XelphatolWhirltalon/XelphatolStrongbeak->self, 2.5s cast, single-target
    TrueThrust = 722, // XelphatolStrongbeak->player, no cast, single-target
    Sneeze = 6625, // Boss->self, 4.0s cast, range 20+R 90-degree cone
}

sealed class ArenaChange(BossModule module) : Components.GenericAOEs(module)
{
    private const int Vertices = 8;
    private const float InnerRadius = 11f;
    private const float OuterRadius = 16f; // 15.5 if adjusted for hitbox radius, but not needed here

    private AOEInstance[] _aoe = [];

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoe;

    public override void OnActorNpcYell(Actor actor, ushort id)
    {
        if (_aoe.Length == 0 && actor.PosRot.Z < -10f)
        {
            var center = Arena.Center;
            var a22 = 22.5f.Degrees();
            var c1 = new WPos(70.5f, -56f);
            var shape = new AOEShapeCustom(center, [new Polygon(c1, OuterRadius, Vertices, a22)],
            [new Polygon(c1, InnerRadius, Vertices, a22),
            new PolygonCustom([new(62.562f, -68.197f), new(58.33f, -63.967f), new(59.195f, -63.239f),
            new(63.214f, -63.273f), new(63.267f, -67.123f)]), new Rectangle(new(59f, -67.5f), 10f, 2.1f, -135f.Degrees())]);
            _aoe = [new(shape, center, default, WorldState.FutureTime(4d), shapeDistance: shape.Distance(center, default))];
        }
    }

    public override void Update()
    {
        if (_aoe.Length == 0 && Module.PrimaryActor.PosRot.Z > -10f) // for some reason NPC yells that are exactly at the start of a module do not get recognized despite appearing in replay?
        {
            var center = Arena.Center;
            var c2 = new WPos(178.083f, -4.225f);
            var a22 = 22.5f.Degrees();
            var shape = new AOEShapeCustom(center, [new Polygon(c2, OuterRadius, Vertices, a22)],
            [new Polygon(c2, InnerRadius, Vertices, a22),
            new PolygonCustom([new(181.034f, -18.5f), new(175.119f, -18.5f), new(175.522f, -17.112f),
            new(178.095f, -14.489f), new(180.889f, -17.318f)])]);
            _aoe = [new(shape, center, default, WorldState.FutureTime(4d), shapeDistance: shape.Distance(center, default))];
        }
    }
}

sealed class Overpower(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Overpower, new AOEShapeCone(7.08f, 45f.Degrees()));
sealed class Sneeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Sneeze, new AOEShapeCone(20.85f, 45f.Degrees()));
sealed class SneezeHint(BossModule module) : Components.CastInterruptHint(module, (uint)AID.Sneeze, true, true, showNameInHint: true);

sealed class D150SiegeGobbueStates : StateMachineBuilder
{
    public D150SiegeGobbueStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<ArenaChange>()
            .ActivateOnEnter<Sneeze>()
            .ActivateOnEnter<SneezeHint>()
            .ActivateOnEnter<Overpower>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D150SiegeGobbue.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 182u, NameID = 5254u, SortOrder = 3)]
public sealed class D150SiegeGobbue : BossModule
{
    public D150SiegeGobbue(WorldState ws, Actor primary) : this(ws, primary, primary.PosRot.Z < -10f ? BuildArena1() : BuildArena2()) { }

    private D150SiegeGobbue(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena1()
    {
        WPos[] vertices = [new(37.14f, -107.01f), new(38.84f, -105.21f), new(39.33f, -104.79f), new(41.89f, -104.55f), new(42.35f, -104.03f),
        new(42.82f, -103.73f), new(43.26f, -103.23f), new(43.91f, -102.16f), new(44.17f, -101.58f), new(44.63f, -100.24f),
        new(45.31f, -97.65f), new(45.54f, -97.07f), new(46.05f, -96.92f), new(47.88f, -96.82f), new(48.35f, -96.39f),
        new(48.66f, -95.81f), new(48.79f, -95.3f), new(48.72f, -94.66f), new(48.16f, -92.22f), new(48.24f, -91.62f),
        new(48.84f, -89.08f), new(48.49f, -88.71f), new(47.85f, -88.36f), new(47.34f, -88.17f), new(47.02f, -87.7f),
        new(47.19f, -87.16f), new(47.45f, -86.57f), new(47.81f, -86.12f), new(49.42f, -85.05f), new(49.44f, -84.36f),
        new(48.2f, -84.03f), new(47.76f, -83.71f), new(47.47f, -83.15f), new(47.32f, -82.62f), new(47.52f, -82.04f),
        new(47.65f, -81.41f), new(47.81f, -80.84f), new(48.2f, -80.48f), new(48.82f, -80.19f), new(49.3f, -79.79f),
        new(49.9f, -79.42f), new(61.27f, -68.05f), new(61.74f, -67.71f), new(62.2f, -67.96f), new(64.14f, -69.9f),
        new(64.7f, -70.33f), new(76.31f, -70.33f), new(76.9f, -69.96f), new(84.9f, -61.8f), new(84.9f, -50.09f),
        new(84.46f, -49.58f), new(83.91f, -49.16f), new(79.33f, -49.16f), new(78.81f, -49.34f), new(77.57f, -49.08f),
        new(77.38f, -48.41f), new(77.38f, -42.62f), new(76.96f, -42.07f), new(76.45f, -41.61f), new(64.79f, -41.61f),
        new(64.33f, -41.84f), new(56.16f, -50.07f), new(56.18f, -61.74f), new(56.54f, -62.32f), new(57.5f, -63.28f),
        new(57.99f, -63.58f), new(58.39f, -64.04f), new(58.73f, -64.52f), new(58.58f, -65.14f), new(47.32f, -76.44f),
        new(47.05f, -77.03f), new(46.71f, -77.57f), new(45.45f, -79.03f), new(44.96f, -79.45f), new(44.42f, -79.8f),
        new(43.86f, -80.05f), new(43.4f, -79.71f), new(43, -79.2f), new(42.91f, -78.69f), new(43.15f, -78.09f),
        new(43.58f, -77.64f), new(43.54f, -76.99f), new(44.29f, -76.24f), new(44.29f, -75.59f), new(43.87f, -75.28f),
        new(42.67f, -76.6f), new(39.76f, -78.38f), new(39.2f, -78.59f), new(36.64f, -76.64f), new(34.84f, -75.89f),
        new(34.19f, -76.08f), new(32.64f, -77.1f), new(32.1f, -77.23f), new(30.5f, -76.22f), new(29.83f, -76.31f),
        new(28.55f, -77f), new(27.99f, -76.89f), new(25.5f, -76.06f), new(23.94f, -78.65f), new(19.71f, -79.74f),
        new(19.54f, -80.21f), new(18.74f, -82.81f), new(18.98f, -83.42f), new(19.92f, -84.32f), new(20.43f, -84.7f),
        new(20.45f, -85.26f), new(20.21f, -87.24f), new(20.04f, -87.78f), new(17.86f, -89.17f), new(17.34f, -89.27f),
        new(12.21f, -88.09f), new(10.2f, -88.22f), new(9.78f, -87.89f), new(9.16f, -86.75f), new(8.49f, -86.6f),
        new(7.82f, -86.55f), new(5.17f, -85.69f), new(4.63f, -85.33f), new(4.36f, -84.83f), new(4.1f, -83.49f),
        new(3.57f, -82.98f), new(3.19f, -83.3f), new(2.72f, -83.78f), new(2.29f, -84.14f), new(1.64f, -84.22f),
        new(0.99f, -84.36f), new(0.37f, -84.62f), new(-0.2f, -84.97f), new(-0.73f, -85.4f), new(-1.12f, -85.96f),
        new(-2.05f, -87.77f), new(-2.21f, -88.45f), new(-2.24f, -89.12f), new(-2.12f, -89.77f), new(-2.1f, -90.39f),
        new(-1.27f, -93.59f), new(-1.03f, -94.25f), new(-0.15f, -95.9f), new(0.36f, -96.31f), new(2.13f, -97.31f),
        new(2.64f, -97.29f), new(4.03f, -96.92f), new(9.22f, -98.31f), new(9.78f, -98.53f), new(11.98f, -99.71f),
        new(14.62f, -99.89f), new(18.29f, -98.54f), new(18.85f, -98.54f), new(19.4f, -98.86f), new(20.05f, -99.16f),
        new(20.67f, -99.18f), new(21.35f, -99.35f), new(24.6f, -99.51f), new(25.11f, -99.81f), new(26.76f, -101.71f),
        new(27.14f, -102.23f), new(29.46f, -106.03f), new(30.16f, -106.21f), new(33.5f, -106.35f), new(36.64f, -107f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena2()
    {
        WPos[] vertices = [new(177.66f, -18.69f), new(183.93f, -18.6f), new(184.47f, -18.16f), new(192.47f, -10.16f), new(192.43f, 1.63f),
        new(192.03f, 2.15f), new(184.37f, 9.81f), new(182.97f, 10.14f), new(182.41f, 9.79f), new(178.41f, 5.79f),
        new(177.9f, 5.66f), new(173.72f, 9.83f), new(173.06f, 10.15f), new(172.22f, 10.15f), new(171.69f, 9.74f),
        new(163.7f, 1.69f), new(163.72f, -10.19f), new(171.84f, -18.26f), new(174.94f, -18.6f), new(177.66f, -18.69f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.XelphatolWhirltalon, (uint)OID.XelphatolStrongbeak, (uint)OID.XelphatolBravewing];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
        {
            hints.PotentialTargets[i].Priority = 0;
        }
    }
}

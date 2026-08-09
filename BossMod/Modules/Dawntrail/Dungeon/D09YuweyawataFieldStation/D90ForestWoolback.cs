namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90ForestWoolback;

public enum OID : uint
{
    Boss = 0x46A4, // R3.9
    ForestAxeBeak = 0x4611, // R3.0
    ForestWoolback = 0x4613, // R3.9
    Electrogolem = 0x46A3 // R1.9
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/ForestAxeBeak/Electrogolem/ForestWoolback->player, no cast, single-target

    SweepingGouge = 40668, // ForestWoolback->self, 4.0s cast, range 9 90-degree cone
    Thunderball = 40666 // ForestAxeBeak->location, 4.0s cast, range 8 circle
}

sealed class SweepingGouge(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SweepingGouge, new AOEShapeCone(9f, 45f.Degrees()));
sealed class Thunderball(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Thunderball, 8f);

sealed class D90ForestWoolbackStates : StateMachineBuilder
{
    public D90ForestWoolbackStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Thunderball>()
            .ActivateOnEnter<SweepingGouge>()
            .Raw.Update = () => AllDeadOrDestroyed(D90ForestWoolback.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008u, NameID = 13573u, SortOrder = 2)]
public sealed class D90ForestWoolback : BossModule
{
    public D90ForestWoolback(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D90ForestWoolback(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(54.16f, 358.33f), new(54.83f, 358.64f), new(61.41f, 363.35f), new(61.86f, 363.88f), new(60.68f, 366.6f),
            new(60.55f, 367.22f), new(61.05f, 370.44f), new(60.92f, 371.09f), new(57.05f, 372.87f), new(56.02f, 373.6f),
            new(53.39f, 374.66f), new(52.86f, 374.6f), new(51.6f, 374.6f), new(50.96f, 374.71f), new(49.87f, 375.38f),
            new(48.67f, 375.9f), new(48.16f, 376.32f), new(47.62f, 376.23f), new(47.11f, 376.19f), new(46.45f, 376.3f),
            new(45.81f, 376.55f), new(45.31f, 376.87f), new(44.85f, 377.42f), new(41.43f, 377.76f), new(40.86f, 377.99f),
            new(39.08f, 378.93f), new(38.03f, 378.94f), new(37.48f, 379.19f), new(36.27f, 380.01f), new(34.32f, 380.27f),
            new(30.82f, 379.37f), new(30.15f, 379.33f), new(28.84f, 379.59f), new(28.11f, 380.68f), new(27.61f, 380.78f),
            new(26.42f, 380.45f), new(25.74f, 380.36f), new(23.66f, 380.42f), new(20.02f, 382.33f), new(19.34f, 382.65f),
            new(18.85f, 383.1f), new(18.8f, 383.73f), new(18.89f, 384.48f), new(18.51f, 384.85f), new(17.33f, 385.58f),
            new(16.79f, 385.6f), new(16.27f, 385.55f), new(15.76f, 385.44f), new(14.3f, 385.06f), new(13.69f, 385.12f),
            new(12.98f, 385.35f), new(10.88f, 384.37f), new(9.89f, 384.05f), new(9.17f, 383.93f), new(7.64f, 383.93f),
            new(7, 384.04f), new(6.63f, 384.59f), new(6.12f, 384.77f), new(3.96f, 385.36f), new(3.35f, 385.63f),
            new(1.11f, 387.2f), new(-0.84f, 389.29f), new(-1.15f, 389.76f), new(-1.03f, 390.39f), new(-0.51f, 392.3f),
            new(-0.58f, 393.01f), new(-1.02f, 393.49f), new(-1.66f, 393.65f), new(-2.05f, 394.06f), new(-2.38f, 394.65f),
            new(-2.73f, 395.04f), new(-3.31f, 395.3f), new(-3.83f, 395.78f), new(-4.34f, 395.9f), new(-6.25f, 396.19f),
            new(-6.85f, 396.53f), new(-7.66f, 397.17f), new(-8.13f, 397.61f), new(-9.72f, 400.95f), new(-10.19f, 401.4f),
            new(-10.29f, 402.65f), new(-10.99f, 403.66f), new(-11.01f, 404.36f), new(-11.48f, 404.84f), new(-11.7f, 405.3f),
            new(-11.82f, 405.89f), new(-11.52f, 406.4f), new(-11.35f, 407.03f), new(-11.66f, 407.45f), new(-12.57f, 408.09f),
            new(-12.72f, 408.79f), new(-12.72f, 409.9f), new(-12.49f, 410.52f), new(-12.48f, 411.64f), new(-12.21f, 412.26f),
            new(-11.69f, 413.08f), new(-20.19f, 409.66f), new(-18.54f, 408.65f), new(-18.15f, 408.33f), new(-17.78f, 407.78f),
            new(-17.86f, 406.47f), new(-18.1f, 405.93f), new(-18.7f, 405.63f), new(-18.82f, 404.97f), new(-18.51f, 404.35f),
            new(-18.13f, 403.83f), new(-17.67f, 403.4f), new(-17.34f, 402.93f), new(-17.5f, 402.25f), new(-17.73f, 401.6f),
            new(-18.74f, 400.93f), new(-18.74f, 400.27f), new(-16.92f, 396.29f), new(-16.52f, 395.78f), new(-15.57f, 395.3f),
            new(-15.12f, 394.82f), new(-14.39f, 393.66f), new(-14.18f, 393.09f), new(-13.88f, 391.81f), new(-13.54f, 391.24f),
            new(-13.2f, 390.87f), new(-12.81f, 390.54f), new(-12.33f, 390.28f), new(-10.04f, 389.31f), new(-7.87f, 388.01f),
            new(-6.39f, 386.62f), new(-5.98f, 386.09f), new(-4.88f, 384.42f), new(-4.86f, 383.85f), new(-4.39f, 383.45f),
            new(-1.42f, 382.66f), new(-0.85f, 382.4f), new(0.92f, 380.16f), new(1.17f, 379.64f), new(1.52f, 378.41f),
            new(1.95f, 377.9f), new(3.03f, 377.55f), new(3.63f, 377.29f), new(4, 376.7f), new(6.8f, 375.28f),
            new(7.96f, 374.89f), new(8.5f, 374.58f), new(8.94f, 374.02f), new(9.52f, 373.64f), new(10.22f, 373.47f),
            new(14.17f, 375.4f), new(14.78f, 375.26f), new(15.38f, 374.89f), new(16.06f, 374.91f), new(16.65f, 375.13f),
            new(17.12f, 374.85f), new(17.39f, 374.18f), new(18.07f, 374.15f), new(18.64f, 374.01f), new(19.16f, 373.73f),
            new(19.39f, 373.22f), new(19.38f, 372.55f), new(19.46f, 372.05f), new(19.82f, 371.53f), new(19.98f, 371.04f),
            new(20, 370.33f), new(22.5f, 369.86f), new(23.47f, 368.93f), new(25.79f, 368.14f), new(26.35f, 367.81f),
            new(26.73f, 367.47f), new(27.99f, 367.41f), new(28.48f, 367.03f), new(28.8f, 366.63f), new(30.08f, 366.56f),
            new(30.59f, 366.17f), new(31.83f, 365.9f), new(32.32f, 365.46f), new(33.58f, 365.19f), new(34.04f, 364.75f),
            new(34.63f, 364.45f), new(40.01f, 362.44f), new(41.31f, 362.27f), new(41.88f, 362.11f), new(42.4f, 361.7f),
            new(42.98f, 361.41f), new(43.55f, 361.2f), new(44.23f, 361.17f), new(44.88f, 361.2f), new(46.74f, 360.92f),
            new(51.35f, 360.61f), new(51.7f, 360.07f), new(51.99f, 359.46f), new(52.32f, 359.07f), new(52.99f, 358.92f),
            new(53.49f, 358.63f), new(53.9f, 358.33f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.ForestAxeBeak, (uint)OID.ForestWoolback, (uint)OID.Electrogolem];

    protected override bool CheckPull() => IsAnyActorInBoundsInCombat(Trash);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.ActorsInBounds(this, Trash);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}

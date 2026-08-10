namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90ForestBat;

public enum OID : uint
{
    Boss = 0x4612, // R3.9
    ForestWoolback = 0x4613, // R2.0
    Electrogolem1 = 0x46A3, // R1.9
    Electrogolem2 = 0x4610 // R1.9
}

public enum AID : uint
{
    AutoAttack = 872, // Electrogolem1/Electrogolem2/Boss/ForestWoolback->player, no cast, single-target

    SweepingGouge = 40668, // ForestWoolback->self, 4.0s cast, range 9 90-degree cone
    LineVoltage = 40665 // Electrogolem2->self, 4.0s cast, range 14 width 4 rect
}

sealed class FlashFlood(BossModule module) : Components.SimpleAOEs(module, (uint)AID.SweepingGouge, new AOEShapeCone(9f, 45f.Degrees()));
sealed class LineVoltage(BossModule module) : Components.SimpleAOEs(module, (uint)AID.LineVoltage, new AOEShapeRect(14f, 2f));

sealed class D90ForestBatStates : StateMachineBuilder
{
    public D90ForestBatStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<FlashFlood>()
            .ActivateOnEnter<LineVoltage>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D90ForestBat.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008u, NameID = 13572u, SortOrder = 1)]
public sealed class D90ForestBat : BossModule
{
    public D90ForestBat(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D90ForestBat(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(-16.78f, 468.11f), new(-15.92f, 468.13f), new(-17.46f, 472.46f), new(-17.41f, 473.1f), new(-16.79f, 476),
        new(-16.78f, 476.7f), new(-16.99f, 480.51f), new(-16.28f, 481.71f), new(-15.96f, 482.38f), new(-15.41f, 484.5f),
        new(-14.62f, 485.59f), new(-15.45f, 489.94f), new(-15.64f, 491.93f), new(-15.5f, 492.57f), new(-14.27f, 495.23f),
        new(-15.25f, 499.73f), new(-15.07f, 500.34f), new(-14.03f, 502.83f), new(-13.72f, 503.46f), new(-13.59f, 504.16f),
        new(-13.56f, 504.76f), new(-13.13f, 505.23f), new(-12.93f, 505.9f), new(-12.98f, 506.55f), new(-13.1f, 507.21f),
        new(-13.1f, 507.86f), new(-12.72f, 508.35f), new(-12.23f, 508.79f), new(-11.83f, 509.39f), new(-11.74f, 510.76f),
        new(-11.57f, 511.3f), new(-9.78f, 512.11f), new(-9.29f, 512.61f), new(-8.97f, 513.94f), new(-8.73f, 514.4f),
        new(-8.1f, 514.62f), new(-7.5f, 514.98f), new(-2.5f, 523.65f), new(-0.46f, 526.68f), new(-0.4f, 527.36f),
        new(-0.55f, 529.27f), new(-0.11f, 529.71f), new(0.44f, 530.08f), new(0.92f, 530.59f), new(0.77f, 531.83f),
        new(0.8f, 532.42f), new(1.24f, 532.75f), new(1.57f, 533.15f), new(1.41f, 533.73f), new(1.72f, 534.24f),
        new(2.47f, 535.31f), new(2.96f, 535.5f), new(3.3f, 536.05f), new(3.81f, 537.22f), new(3.92f, 537.73f),
        new(4.13f, 538.24f), new(4.38f, 540.04f), new(4.57f, 540.5f), new(4.99f, 540.88f), new(5.58f, 541.04f),
        new(6.15f, 541.37f), new(6.5f, 541.95f), new(6.7f, 542.61f), new(6.69f, 543.97f), new(6.53f, 544.46f),
        new(6.24f, 544.88f), new(5.94f, 545.46f), new(6.08f, 546.13f), new(-5.58f, 549.09f), new(-5.95f, 548.52f),
        new(-6.09f, 548.04f), new(-5.81f, 547.38f), new(-5.51f, 546.81f), new(-5.9f, 544.73f), new(-6.12f, 544.2f),
        new(-6.6f, 543.74f), new(-7.01f, 543.23f), new(-9.39f, 535.91f), new(-9.67f, 535.37f), new(-11.09f, 533.73f),
        new(-12.19f, 532.98f), new(-12.5f, 532.57f), new(-13.03f, 531.69f), new(-13.82f, 529.99f), new(-14.15f, 529.48f),
        new(-14.76f, 529.21f), new(-15.34f, 528.8f), new(-15.44f, 528.12f), new(-15.65f, 527.53f), new(-17.44f, 525.26f),
        new(-19.31f, 524.56f), new(-19.82f, 524.1f), new(-20.06f, 523.64f), new(-20.27f, 523.17f), new(-20.81f, 522.13f),
        new(-20.39f, 520.89f), new(-20.23f, 520.27f), new(-20.33f, 519.61f), new(-20.73f, 518.48f), new(-21.08f, 518.06f),
        new(-23.41f, 516.94f), new(-24.69f, 516.68f), new(-26.66f, 514.18f), new(-27.84f, 513.33f), new(-28.71f, 510.84f),
        new(-30.02f, 508.71f), new(-31.64f, 506.54f), new(-32.65f, 504.3f), new(-32.95f, 503.7f), new(-33.03f, 501.75f),
        new(-33.23f, 501.14f), new(-33.55f, 500.48f), new(-33.77f, 499.15f), new(-33.97f, 498.58f), new(-34.59f, 498.22f),
        new(-34.41f, 497.09f), new(-34.39f, 496.42f), new(-34.5f, 495.92f), new(-34.47f, 495.25f), new(-34.4f, 494.64f),
        new(-34.61f, 493.95f), new(-34.71f, 493.27f), new(-34.7f, 492.6f), new(-34.85f, 491.29f), new(-35.6f, 489.36f),
        new(-35.43f, 488.71f), new(-35.31f, 488.11f), new(-35.47f, 486.77f), new(-35.28f, 483.61f), new(-35.49f, 483.01f),
        new(-35.64f, 482.3f), new(-35.43f, 481.68f), new(-35.42f, 479.49f), new(-34.93f, 471.44f), new(-34.95f, 470.74f),
        new(-34.79f, 470.06f), new(-34.9f, 469.45f), new(-35.73f, 468.5f), new(-16.78f, 468.11f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.Electrogolem1, (uint)OID.Electrogolem1, (uint)OID.ForestWoolback];

    protected override bool CheckPull() => IsAnyActorInCombat(Trash);

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

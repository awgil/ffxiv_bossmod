namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90RottenResearcher;

public enum OID : uint
{
    Boss = 0x4652, // R0.75
    RottenResearcher1 = 0x4654, // R0.75
    RottenResearcher2 = 0x4615, // R0.75
    RottenResearcher3 = 0x4653, // R0.75
    RottenHound1 = 0x46D4, // R2.2
    RottenHound2 = 0x4614 // R2.2
}

public enum AID : uint
{
    AutoAttack1 = 870, // RottenHound1/RottenHound2->player, no cast, single-target
    AutoAttack2 = 872, // Boss/RottenResearcher2/RottenResearcher3/RottenResearcher4->player, no cast, single-target

    FoulBite = 40670 // RottenHound2->player, no cast, single-target
}

sealed class D90RottenResearcherStates : StateMachineBuilder
{
    public D90RottenResearcherStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D90RottenResearcher.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008u, NameID = 13575u, SortOrder = 4)]
public sealed class D90RottenResearcher : BossModule
{
    public D90RottenResearcher(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D90RottenResearcher(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(54.67f, 59.58f), new(58.29f, 60.28f), new(58.94f, 60.14f), new(59.88f, 60.06f), new(60.69f, 60.1f),
            new(60.69f, 60.67f), new(61.09f, 61.18f), new(61.68f, 61.43f), new(61.95f, 61.86f), new(62.09f, 62.48f),
            new(62.62f, 63.64f), new(62.91f, 64.18f), new(63.79f, 65.18f), new(64.18f, 65.77f), new(63.86f, 67),
            new(63.56f, 70.22f), new(63.9f, 71.52f), new(63.71f, 74.85f), new(63.49f, 75.34f), new(63.37f, 75.89f),
            new(63.75f, 79.21f), new(63.98f, 79.69f), new(64.53f, 80.03f), new(64.66f, 80.69f), new(64.57f, 81.31f),
            new(64.39f, 81.97f), new(62.67f, 85.47f), new(62.53f, 86.05f), new(62.96f, 86.43f), new(64.18f, 87.17f),
            new(64.78f, 87.64f), new(63.86f, 90.02f), new(63.7f, 90.63f), new(64.02f, 91.08f), new(64.65f, 91.37f),
            new(65.85f, 94.43f), new(66.14f, 95.03f), new(66.12f, 95.68f), new(65.83f, 96.11f), new(65.77f, 96.66f),
            new(66.1f, 97.31f), new(65.69f, 97.72f), new(64.02f, 98.8f), new(63.55f, 99.33f), new(62.32f, 101.19f),
            new(62.38f, 100.62f), new(62.71f, 100), new(63.14f, 99.46f), new(63.37f, 98.21f), new(62.89f, 98.42f),
            new(62.43f, 99.52f), new(60.85f, 104), new(60.69f, 104.48f), new(59.24f, 104.56f), new(58.57f, 104.39f),
            new(57.94f, 104.42f), new(57.26f, 104.26f), new(56.72f, 104.29f), new(56.27f, 104.81f), new(55.97f, 105.39f),
            new(55.97f, 106.05f), new(56.2f, 106.55f), new(56.89f, 106.81f), new(58.33f, 107.04f), new(59, 106.9f),
            new(63.96f, 106.83f), new(64.77f, 106.92f), new(64.8f, 108.17f), new(65.49f, 109.29f), new(65.33f, 109.98f),
            new(64.91f, 110.53f), new(64.54f, 111.07f), new(64.53f, 111.7f), new(64.45f, 112.21f), new(63.78f, 112.36f),
            new(62.89f, 113.36f), new(62.88f, 113.88f), new(63.85f, 114.74f), new(64.52f, 114.68f), new(65.01f, 114.51f),
            new(66.23f, 115.89f), new(66.05f, 116.5f), new(65.59f, 116.95f), new(64.74f, 120.08f), new(64.26f, 120.57f),
            new(63.72f, 122.13f), new(64.19f, 126.77f), new(63.78f, 127.08f), new(63.16f, 127.2f), new(62.9f, 127.73f),
            new(61.68f, 130.7f), new(61.44f, 131.16f), new(61.25f, 131.78f), new(60.98f, 132.45f), new(60.75f, 133.55f),
            new(63.37f, 134.69f), new(62.81f, 134.68f), new(61.61f, 134.18f), new(61.01f, 134.02f), new(60.69f, 133.6f),
            new(60.57f, 133.08f), new(60.03f, 134.81f), new(59.48f, 135.41f), new(58.84f, 135.28f), new(56.73f, 135.9f),
            new(56.06f, 136.02f), new(55.85f, 136.55f), new(55.77f, 137.06f), new(55.96f, 137.63f), new(56.4f, 138.01f),
            new(57.59f, 138.6f), new(58.21f, 138.81f), new(59.22f, 139.61f), new(59.91f, 139.64f), new(60.63f, 139.59f),
            new(61.26f, 139.44f), new(61.4f, 138.92f), new(61.89f, 138.8f), new(70.16f, 138.6f), new(69.99f, 139.16f),
            new(71.03f, 140.08f), new(71.86f, 139.88f), new(71.74f, 140.58f), new(72.14f, 150.58f), new(72.26f, 151.22f),
            new(72.22f, 151.75f), new(71.12f, 152.3f), new(70.86f, 152.75f), new(70.86f, 153.4f), new(69.17f, 153.65f),
            new(68.04f, 153.62f), new(67.58f, 153.09f), new(67.3f, 152.48f), new(66.85f, 152.13f), new(66.23f, 151.86f),
            new(65.71f, 151.57f), new(65.06f, 151.38f), new(64.51f, 151.54f), new(64.22f, 152.03f), new(64.04f, 152.69f),
            new(64.18f, 153.2f), new(64.74f, 153.5f), new(47.04f, 153.61f), new(47.26f, 151.72f), new(47.28f, 151.12f),
            new(45.68f, 147.4f), new(45.31f, 147.04f), new(44.04f, 147.25f), new(43.36f, 147.04f), new(41.46f, 146.26f),
            new(40.87f, 146.22f), new(38.26f, 146.31f), new(37.75f, 145.85f), new(37.78f, 143.13f), new(38.33f, 143.16f),
            new(38.76f, 142.83f), new(39.1f, 142.3f), new(39.36f, 141.79f), new(39.09f, 141.36f), new(38.05f, 140.68f),
            new(38.39f, 140.24f), new(39.54f, 138.6f), new(47.99f, 138.6f), new(49.51f, 138.81f), new(50.19f, 138.62f),
            new(50.77f, 138.4f), new(50.85f, 136.79f), new(50.55f, 136.36f), new(49.93f, 136.2f), new(49.37f, 136.12f),
            new(48.76f, 136.27f), new(48.07f, 136.34f), new(47.18f, 136.29f), new(46.72f, 135.79f), new(46.3f, 135.44f),
            new(45.81f, 133.23f), new(45.81f, 127.42f), new(46.22f, 126.82f), new(46.46f, 126.3f), new(46.23f, 125.8f),
            new(45.8f, 125.29f), new(45.86f, 108.02f), new(46.29f, 107.74f), new(47.27f, 106.84f), new(48.06f, 106.84f),
            new(48.78f, 106.98f), new(49.3f, 107.03f), new(50.64f, 106.75f), new(50.85f, 105.12f), new(50.59f, 104.6f),
            new(49.9f, 104.42f), new(49.24f, 104.39f), new(48.56f, 104.56f), new(47.4f, 104.53f), new(46.46f, 103.73f),
            new(45.8f, 103.52f), new(45.85f, 90.96f), new(46.16f, 90.55f), new(46.47f, 89.99f), new(46.97f, 89.91f),
            new(47.53f, 89.65f), new(48.06f, 89.33f), new(48.09f, 88.82f), new(47.78f, 88.19f), new(47.5f, 87.76f),
            new(46.83f, 87.61f), new(46.15f, 87.77f), new(45.81f, 86.05f), new(45.81f, 61.62f), new(46.28f, 61.37f),
            new(46.76f, 60.97f), new(46.88f, 60.43f), new(47.34f, 60.15f), new(50.18f, 60.28f), new(50.73f, 59.9f),
            new(54.67f, 59.58f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.RottenResearcher1, (uint)OID.RottenResearcher2, (uint)OID.RottenResearcher3,
    (uint)OID.RottenHound1, (uint)OID.RottenHound2];

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

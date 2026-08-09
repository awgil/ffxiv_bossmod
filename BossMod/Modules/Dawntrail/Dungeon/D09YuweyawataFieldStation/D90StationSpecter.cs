namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90StationSpecter;

public enum OID : uint
{
    Boss = 0x4616, // R0.75
    RottenResearcher1 = 0x4653, // R0.75
    RottenResearcher2 = 0x4654, // R0.75
    RottenResearcher3 = 0x4615, // R0.75
    GiantCorse = 0x4617, // R2.28
    StationSpecter = 0x4655 // R0.75
}

public enum AID : uint
{
    AutoAttack1 = 41270, // StationSpecter/Boss->player, no cast, single-target
    AutoAttack2 = 872, // RottenResearcher1/RottenResearcher2/RottenResearcher3/GiantCorse->player, no cast, single-target

    GlassPunch = 40671, // GiantCorse->self, 4.0s cast, range 7 120-degree cone
    Catapult = 40672 // GiantCorse->location, 4.0s cast, range 6 circle
}

sealed class GlassPunch(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GlassPunch, new AOEShapeCone(7f, 60f.Degrees()));
sealed class Catapult(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Catapult, 6f);

sealed class D90StationSpecterStates : StateMachineBuilder
{
    public D90StationSpecterStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<GlassPunch>()
            .ActivateOnEnter<Catapult>()
            .Raw.Update = () => AllDeadOrDestroyed(D90StationSpecter.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008u, NameID = 13577u, SortOrder = 5)]
public sealed class D90StationSpecter : BossModule
{
    public D90StationSpecter(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D90StationSpecter(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(117.43f, -25.65f), new(119.51f, -25.64f), new(120.13f, -25.32f), new(120.8f, -25.13f), new(121.36f, -24.64f),
            new(121.94f, -24.26f), new(122.01f, -23.75f), new(121.85f, -23.07f), new(121.64f, -22.47f), new(121.53f, -21.84f),
            new(121.54f, -21.2f), new(121.96f, -20.73f), new(122, -16.37f), new(121.41f, -16.07f), new(121.11f, -15.5f),
            new(120.5f, -15.51f), new(119.96f, -15.72f), new(119.36f, -15.54f), new(118.83f, -15.11f), new(118.44f, -14.54f),
            new(118.43f, -14f), new(119.59f, -13.33f), new(120.1f, -12.84f), new(120.82f, -11.88f), new(121.4f, -11.63f),
            new(121.6f, -10.93f), new(121.59f, -10.26f), new(120.94f, -6.25f), new(120.95f, -5.57f), new(120.76f, -4.88f),
            new(120.68f, -4.18f), new(120.98f, -2.78f), new(121.06f, -1.37f), new(121.46f, -0.93f), new(122.03f, -0.88f),
            new(121.93f, -0.39f), new(121.73f, 1.01f), new(121.58f, 1.67f), new(120.84f, 2.74f), new(120.63f, 3.45f),
            new(120.6f, 4.12f), new(121.34f, 6.87f), new(121.39f, 7.6f), new(121.59f, 10.78f), new(121.59f, 13.26f),
            new(120.82f, 17.23f), new(120.86f, 17.85f), new(121.37f, 18.06f), new(121.6f, 18.77f), new(121.58f, 30.39f),
            new(121.4f, 31.06f), new(121.07f, 31.71f), new(120.69f, 32.32f), new(120.48f, 33), new(120.57f, 33.59f),
            new(121.06f, 34), new(121.59f, 34.14f), new(121.59f, 38.3f), new(121.23f, 41.5f), new(121.6f, 42.67f),
            new(121.57f, 43.6f), new(121.01f, 43.96f), new(120.54f, 44.35f), new(119.92f, 44.53f), new(119.36f, 44.8f),
            new(118.96f, 45.26f), new(119.01f, 45.9f), new(119.25f, 46.52f), new(119.84f, 46.68f), new(120.27f, 47.18f),
            new(121.07f, 47.23f), new(122.09f, 47.76f), new(121.47f, 47.88f), new(121.05f, 48.42f), new(120.92f, 48.93f),
            new(123.44f, 53.4f), new(123.28f, 54.06f), new(123.37f, 54.65f), new(123.63f, 55.25f), new(123.48f, 55.72f),
            new(123, 56.11f), new(122.66f, 56.62f), new(122.58f, 57.13f), new(122.01f, 57.38f), new(121.35f, 57.32f),
            new(116.1f, 57.64f), new(115.45f, 57.55f), new(114.91f, 57.54f), new(114.33f, 57.83f), new(113.87f, 58.36f),
            new(112.8f, 57.92f), new(85.21f, 57.78f), new(84.91f, 57.31f), new(83.34f, 46.6f), new(95.54f, 46.62f),
            new(95.6f, 47.15f), new(95.82f, 47.77f), new(96.83f, 48.44f), new(97.26f, 48.96f), new(97.95f, 49.14f),
            new(99.05f, 48.61f), new(99.06f, 47.93f), new(98.87f, 47.35f), new(98.85f, 46.84f), new(107.16f, 46.63f),
            new(109.04f, 47.87f), new(109.61f, 48.1f), new(110.56f, 47.46f), new(111.15f, 47.15f), new(111.72f, 47.24f),
            new(112.33f, 47.02f), new(112.8f, 46.45f), new(112.87f, 45.14f), new(112.52f, 44.72f), new(111.82f, 44.67f),
            new(111.45f, 44.33f), new(110.92f, 43.93f), new(110.4f, 43.78f), new(110.44f, 40.63f), new(110.69f, 39.2f),
            new(110.48f, 38.62f), new(110.4f, 37.7f), new(110.42f, 36.59f), new(110.74f, 35.94f), new(111.51f, 34.73f),
            new(112.17f, 33.46f), new(112.34f, 32.87f), new(110.85f, 30.98f), new(110.71f, 30.49f), new(110.87f, 28.77f),
            new(110.45f, 28.2f), new(110.42f, 27.66f), new(110.51f, 23.78f), new(111.06f, 19.86f), new(110.78f, 19.35f),
            new(110.53f, 18.71f), new(111.28f, 12.17f), new(110.68f, 11.14f), new(110.73f, 7.81f), new(110.79f, 7.28f),
            new(111.06f, 6.1f), new(111.16f, 5.51f), new(111.23f, 4.24f), new(111.17f, 3.66f), new(110.6f, 3.32f),
            new(110.41f, 2.81f), new(110.4f, 1.78f), new(110.29f, 1.07f), new(110.28f, 0.57f), new(110.34f, 0.05f),
            new(110.49f, -0.46f), new(110.72f, -0.94f), new(111.69f, -2.37f), new(111.92f, -2.93f), new(112.1f, -3.51f),
            new(112.41f, -5.31f), new(111.66f, -6.23f), new(110.46f, -9.35f), new(110.54f, -9.88f), new(110.67f, -10.37f),
            new(111.42f, -12.23f), new(111.52f, -13.21f), new(112.13f, -12.95f), new(112.79f, -13.15f), new(113.34f, -13.56f),
            new(113.66f, -14.09f), new(113.59f, -14.73f), new(113.04f, -14.94f), new(112.64f, -15.26f), new(112.05f, -15.61f),
            new(111.26f, -15.73f), new(110.8f, -15.94f), new(110.21f, -16.29f), new(109.99f, -16.97f), new(109.99f, -17.66f),
            new(110.65f, -18.55f), new(110.54f, -19.18f), new(110.07f, -19.71f), new(109.97f, -23.91f), new(110.22f, -24.35f),
            new(110.66f, -24.76f), new(111.26f, -25.17f), new(111.95f, -25.35f), new(117.43f, -25.65f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.RottenResearcher1, (uint)OID.RottenResearcher2, (uint)OID.RottenResearcher3,
    (uint)OID.GiantCorse, (uint)OID.StationSpecter];
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

namespace BossMod.Dawntrail.Dungeon.D09YuweyawataFieldStation.D90SprightlyPhoebad;

public enum OID : uint
{
    Boss = 0x4619, // R6.24
    SprightlyStone = 0x461D, // R3.0
    SprightlyMole = 0x461A, // R1.4
    SprightlyLoamkeep = 0x461C // R2.4
}

public enum AID : uint
{
    AutoAttack1 = 41119, // SprightlyStone->player, no cast, single-target
    AutoAttack2 = 872, // SprightlyMole/Boss->player, no cast, single-target
    AutoAttack3 = 40674, // SprightlyLoamkeep->player, no cast, single-target

    Plummet = 40676, // SprightlyStone1->self, 4.0s cast, range 10 circle
    Landslip = 41118 // Boss->self, 4.8s cast, range 12 120-degree cone
}

sealed class Landslip(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Landslip, new AOEShapeCone(12, 60.Degrees()));
sealed class Plummet(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Plummet, 10);

sealed class D90SprightlyPhoebadStates : StateMachineBuilder
{
    public D90SprightlyPhoebadStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Landslip>()
            .ActivateOnEnter<Plummet>()
            .Raw.Update = () => AllDeadOrDestroyedInBounds(D90SprightlyPhoebad.Trash);
    }
}

[ModuleInfo(BossModuleInfo.Maturity.AISupport, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1008, NameID = 13580, SortOrder = 7)]
public sealed class D90SprightlyPhoebad : BossModule
{
    public D90SprightlyPhoebad(WorldState ws, Actor primary) : this(ws, primary, BuildArena()) { }

    private D90SprightlyPhoebad(WorldState ws, Actor primary, (WPos center, ArenaBoundsCustom arena) a) : base(ws, primary, a.center, a.arena) { }

    private static (WPos center, ArenaBoundsCustom arena) BuildArena()
    {
        WPos[] vertices = [new(123.42f, -297.25f), new(123.17f, -290.97f), new(123.59f, -290.57f), new(125.1f, -289.46f), new(123.03f, -284.58f),
            new(121.39f, -281.1f), new(121.14f, -280.63f), new(120.2f, -279.18f), new(119.3f, -277.2f), new(118.52f, -276),
            new(117.28f, -273.65f), new(115.34f, -270.32f), new(114.3f, -268.72f), new(113.52f, -266.98f), new(112.45f, -265.22f),
            new(112.24f, -264.61f), new(111.34f, -262.75f), new(110.63f, -261.62f), new(110.14f, -259.81f), new(109.33f, -257.98f),
            new(109.01f, -256.68f), new(108.77f, -256.09f), new(107.1f, -241.1f), new(107.11f, -240.42f), new(107.06f, -239.72f),
            new(106.88f, -238.56f), new(111.77f, -217.52f), new(112.9f, -216.02f), new(112.92f, -215.36f), new(112.69f, -214.67f),
            new(112.21f, -214.17f), new(111.06f, -212.58f), new(110.95f, -212.07f), new(111.36f, -211.67f), new(116.83f, -207.22f),
            new(117.65f, -206.35f), new(118.55f, -205.57f), new(108.22f, -199.35f), new(107.64f, -199.13f), new(106.89f, -199.03f),
            new(105.18f, -199.19f), new(103.78f, -199.2f), new(103.51f, -199.81f), new(103.62f, -200.45f), new(103.52f, -201.17f),
            new(103.17f, -201.69f), new(100.93f, -201.86f), new(100.86f, -202.61f), new(100.84f, -204.15f), new(100.49f, -204.52f),
            new(99.91f, -204.6f), new(98.62f, -204.59f), new(98.32f, -205.2f), new(98.31f, -205.94f), new(98.09f, -206.47f),
            new(97.28f, -206.55f), new(91.86f, -206.55f), new(91.81f, -207.06f), new(91.81f, -214.15f), new(91.86f, -214.77f),
            new(91.86f, -215.41f), new(91.79f, -216.04f), new(91.31f, -216.49f), new(90.67f, -216.64f), new(89.98f, -216.67f),
            new(89.37f, -216.61f), new(88.65f, -216.63f), new(88.38f, -238.68f), new(88.73f, -240.01f), new(88.97f, -241.91f),
            new(89.57f, -243.77f), new(89.92f, -245.8f), new(90.55f, -247.78f), new(91.2f, -250.34f), new(91.7f, -251.59f),
            new(91.97f, -252.89f), new(92.8f, -255.29f), new(93.24f, -257.25f), new(93.95f, -259.21f), new(94.39f, -261.19f),
            new(95.86f, -265.54f), new(96.15f, -266.15f), new(96.98f, -268.72f), new(97.84f, -270.63f), new(104.98f, -283.07f),
            new(105.71f, -284.13f), new(106, -284.66f), new(106.49f, -285.86f), new(107.56f, -287.51f), new(108.05f, -288.7f),
            new(108.36f, -289.32f), new(108.72f, -289.86f), new(108.89f, -290.36f), new(110.23f, -292.79f), new(110.57f, -293.24f),
            new(110.83f, -293.82f), new(111.77f, -295.5f), new(112.26f, -296.71f), new(113.35f, -298.45f), new(113.98f, -298.78f)];
        var arena = new ArenaBoundsCustom([new PolygonCustom(vertices)]);
        return (arena.Center, arena);
    }

    public static readonly uint[] Trash = [(uint)OID.Boss, (uint)OID.SprightlyMole, (uint)OID.SprightlyStone, (uint)OID.SprightlyLoamkeep];

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

namespace BossMod.Heavensward.Dungeon.D08FractalContinuum.D080VioletControlUnit;

public enum OID : uint
{
    Boss = 0x1E990F, // R2.0
    ClockworkPredator = 0x102B // R0.75
}

public enum AID : uint
{
    AutoAttack1 = 872, // ImmortalizedClockworkSoldier/ManikinPugilist/ImmortalizedClockworkKnight->player, no cast, single-target
    AutoAttack2 = 870, // ManikinMarauder->player, no cast, single-target

    Rive = 1135, // ManikinMarauder->self, 2.5s cast, range 30+R width 2 rect
    Stone = 970, // Boss->player, 1.0s cast, single-target
    Overpower = 720 // ManikinMarauder->self, 2.5s cast, range 6+R 90-degree cone
}

class Interact(BossModule module) : BossComponent(module)
{
    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var primary = Module.PrimaryActor;
        if (Module.PrimaryActor.IsTargetable)
        {
            hints.GoalZones.Add(AIHints.GoalSingleTarget(primary, 1f, 5f));
            hints.InteractWithTarget = primary;
        }
    }

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (Module.PrimaryActor.IsTargetable)
            hints.Add("Interact with control unit to continue!");
    }

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        var primary = Module.PrimaryActor;
        if (primary.IsTargetable)
            Arena.ZoneCircleOutline(primary.Position, 3f, Colors.Safe);
    }
}

class D080VioletControlUnitStates : StateMachineBuilder
{
    public D080VioletControlUnitStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Interact>()
            .Raw.Update = () => module.PrimaryActor.EventState == 7;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 35, NameID = 6924, SortOrder = 6)]
public class D080VioletControlUnit(WorldState ws, Actor primary) : BossModule(ws, primary, arena1.Center, arena1)
{
    private static readonly WPos[] vertices = [new(-70.27f, -240.33f), new(-68.42f, -240.06f), new(-67.89f, -239.64f), new(-62.61f, -233.03f), new(-62.26f, -232.49f),
    new(-62.3f, -231.21f), new(-62.26f, -230.62f), new(-60.31f, -229.03f), new(-59.89f, -228.58f), new(-59.33f, -228.27f),
    new(-58.71f, -228.45f), new(-57.91f, -229.3f), new(-56.98f, -228.47f), new(-56.78f, -228.01f), new(-57.19f, -227.47f),
    new(-57.2f, -226.86f), new(-46.73f, -219.56f), new(-46.28f, -219.05f), new(-44.72f, -216.32f), new(-44.22f, -216f),
    new(-23.87f, -210.66f), new(-22.52f, -209.17f), new(-21.26f, -209.1f), new(-20.72f, -209.6f), new(-18.14f, -209.41f),
    new(-17.47f, -209.3f), new(-16.05f, -207.8f), new(-15.58f, -207.48f), new(15.33f, -207.51f), new(15.8f, -207.9f),
    new(17.13f, -209.31f), new(17.83f, -209.44f), new(19.93f, -209.59f), new(20.49f, -209.29f), new(20.96f, -209.1f),
    new(21.68f, -209.12f), new(22.25f, -209.27f), new(23.53f, -210.7f), new(44.19f, -216.12f), new(45.88f, -219f),
    new(46.31f, -219.54f), new(56.87f, -226.93f), new(56.85f, -227.44f), new(56.46f, -227.95f), new(56.68f, -228.55f),
    new(57.16f, -228.97f), new(57.63f, -229.2f), new(58.5f, -228.28f), new(59.01f, -228.28f), new(59.98f, -229.05f),
    new(60.68f, -229.05f), new(61.25f, -229.15f), new(61.81f, -230.47f), new(61.91f, -231.1f), new(61.89f, -232.4f),
    new(62.19f, -232.99f), new(67.67f, -239.82f), new(68.27f, -240.09f), new(70.2f, -240.35f), new(72.8f, -240.07f),
    new(68.28f, -231.4f), new(67.8f, -230.9f), new(67.26f, -230.49f), new(66.84f, -230.08f), new(67.31f, -220.45f),
    new(70.93f, -215.26f), new(70.98f, -214.58f), new(66.24f, -204.75f), new(61.78f, -199.48f), new(40.8f, -180.33f),
    new(40.1f, -180.22f), new(35.43f, -180.02f), new(33.7f, -179.11f), new(33.35f, -178.69f), new(32.01f, -175.2f),
    new(31.59f, -174.68f), new(24.52f, -171.64f), new(23.9f, -171.46f), new(23.41f, -171.12f), new(20.38f, -169.81f),
    new(19.79f, -170.24f), new(19.31f, -170.69f), new(18.71f, -170.51f), new(17.84f, -169.6f), new(17.96f, -169.11f),
    new(18.17f, -168.64f), new(17.85f, -168.16f), new(18.07f, -167.64f), new(19.05f, -166.79f), new(19.47f, -166.18f),
    new(15.31f, -164.48f), new(14.79f, -165.63f), new(14.2f, -165.78f), new(13.74f, -166.25f), new(13.26f, -166.52f),
    new(12.81f, -166.2f), new(12.36f, -165.79f), new(12.11f, -165.32f), new(10.61f, -152.44f), new(9.27f, -147.99f),
    new(-9.35f, -147.64f), new(-9.69f, -148.02f), new(-10.99f, -152.4f), new(-12.53f, -165.51f), new(-12.93f, -165.95f),
    new(-13.43f, -166.42f), new(-13.93f, -166.46f), new(-14.39f, -165.99f), new(-15.06f, -165.68f), new(-15.48f, -165.32f),
    new(-15.41f, -164.71f), new(-16.04f, -164.61f), new(-19.72f, -166.1f), new(-19.57f, -166.69f), new(-19.03f, -167.08f),
    new(-18.54f, -167.53f), new(-18.24f, -168.01f), new(-18.49f, -168.55f), new(-18.28f, -169.2f), new(-18.43f, -169.83f),
    new(-18.89f, -170.32f), new(-19.4f, -170.74f), new(-19.87f, -170.54f), new(-20.34f, -170.09f), new(-20.75f, -169.8f),
    new(-31.87f, -174.65f), new(-32.38f, -175.11f), new(-33.81f, -178.85f), new(-34.24f, -179.19f), new(-35.37f, -179.81f),
    new(-35.94f, -180.04f), new(-41.19f, -180.29f), new(-61.85f, -199.15f), new(-66.8f, -205.03f), new(-71.25f, -214.41f),
    new(-71.45f, -215.07f), new(-67.96f, -220.07f), new(-67.65f, -220.65f), new(-67.26f, -230.11f), new(-67.62f, -230.5f),
    new(-68.15f, -230.91f), new(-68.63f, -231.36f), new(-73.03f, -239.81f), new(-72.57f, -240.19f), new(-70.69f, -240.35f)];
    private static readonly ArenaBoundsCustom arena1 = new([new PolygonCustom(vertices)], [new Rectangle(new(-0.181f, -202.993f), 1.39f, 0.9f)]);

    protected override bool CheckPull() => IsActorInCombat((uint)OID.ClockworkPredator);

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Enemies((uint)OID.ClockworkPredator));
        Arena.Actor(PrimaryActor, Colors.Object);
    }

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        var count = hints.PotentialTargets.Count;
        for (var i = 0; i < count; ++i)
            hints.PotentialTargets[i].Priority = 0;
    }
}

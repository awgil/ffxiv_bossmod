namespace BossMod.Dawntrail.Foray.FATE.DaylightPottery;

public enum OID : uint
{
    CrimsonGremlin = 0x4D8B, // R3.000, x?
    CrescentGremlin = 0x4D8A, // R1.500, x?
    CrescentSapria = 0x4E15, // R1.920, x?
    CrescentDhruva = 0x4E8E, // R2.400, x?
    CrescentOpken = 0x4E13, // R1.690, x?
    CrescentSoblyn = 0x4E1A, // R2.200, x?
}

public enum AID : uint
{
    AutoAttack = 40542, // 4D8A/4D8B->player, no cast, single-target
    BadMouth = 50224, // 4D8A->player, no cast, single-target
    OffensiveRambling = 50226, // 4D8B->location, 3.0s cast, range 5 circle
    TouchySubject = 50225, // 4D8B->self, 3.0s cast, range 25 width 6 rect
}

sealed class OffensiveRambling(BossModule module) : Components.SimpleAOEs(module, (uint)AID.OffensiveRambling, 5f);
sealed class TouchySubject(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TouchySubject, new AOEShapeRect(25f, 3f));

[SkipLocalsInit]
sealed class DaylightPotteryStates : StateMachineBuilder
{
    public DaylightPotteryStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<OffensiveRambling>()
            .ActivateOnEnter<TouchySubject>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Dummy,
    StatesType = typeof(DaylightPotteryStates),
    ConfigType = null, // replace null with typeof(GreaterFanConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.CrimsonGremlin,
    Contributors = "",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2072u,
    SortOrder = 1,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class DaylightPottery(WorldState ws, Actor primary) : OpenWorldFate(ws, primary)
{
    // need to find something for OID.Boss to use as primary
    public static readonly uint[] Trash = [(uint)OID.CrimsonGremlin, (uint)OID.CrescentGremlin];
    public Actor? CrimsonGremlin;

    protected override void UpdateModule()
    {
        CrimsonGremlin ??= GetActor((uint)OID.CrimsonGremlin);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(this, Trash);
        Arena.Actor(CrimsonGremlin);
    }
}

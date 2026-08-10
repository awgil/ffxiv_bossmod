namespace BossMod.Dawntrail.Foray.FATE.TerritorialDispute;

public enum OID : uint {
    RuinHound = 0x4D5E,
    Helper = 0x233C,
    IcePillar = 0x4D5F, // R2.000, x0 (spawn during fight)
    RuinHound1 = 0x4DA0, // R1.000, x0 (spawn during fight)
    RuinHound2 = 0x4D60, // R1.000, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50536, // RuinHound->player, no cast, single-target
    IcePillarCast = 49758, // RuinHound->self, 3.0s cast, single-target
    IcePillar = 49770, // 4D5F->self, 3.0s cast, range 4 circle
    RoaringBlizzard = 49765, // RuinHound->self, 5.0s cast, range 50 60-degree cone
    Rush = 49759, // 4D5F->self, 4.0s cast, range 80 width 4 rect
    AgeOfEndlessFrostCast = 49760, // RuinHound->self, 3.0s cast, single-target
    AgeOfEndlessFrost = 49761, // 4DA0->self, 3.0s cast, range 40 60.000-degree cone
    TheStormWithin = 49756, // RuinHound->self, 5.0s cast, range 10 circle
    TheStormWithin1 = 49766, // 4D60->location, no cast, range 10 circle
    TheStormWithout = 49757, // RuinHound->self, 5.0s cast, range 10-40 donut
    TheStormWithout1 = 49767, // 4D60->location, no cast, range ?-40 donut
}

sealed class IcePillar(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IcePillar, new AOEShapeCircle(4.0f));
sealed class RoaringBlizzard(BossModule module) : Components.SimpleAOEs(module, (uint)AID.RoaringBlizzard, new AOEShapeCone(50.0f, 30.0f.Degrees()));
sealed class Rush(BossModule module) : Components.SimpleAOEs(module, (uint)AID.Rush, new AOEShapeRect(80.0f, 2.0f));
sealed class AgeOfEndlessFrost(BossModule module) : Components.SimpleAOEs(module, (uint)AID.AgeOfEndlessFrost, new AOEShapeCone(40.0f, 30.0f.Degrees()));
sealed class TheStormWithin(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheStormWithin, new AOEShapeCircle(10.0f));
sealed class TheStormWithout(BossModule module) : Components.SimpleAOEs(module, (uint)AID.TheStormWithout, new AOEShapeDonut(10.0f, 40.0f));

[SkipLocalsInit]
sealed class TerritorialDisputeStates : StateMachineBuilder {
    public TerritorialDisputeStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<RoaringBlizzard>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<AgeOfEndlessFrost>()
            .ActivateOnEnter<TheStormWithin>()
            .ActivateOnEnter<TheStormWithout>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed,
    StatesType = typeof(TerritorialDisputeStates),
    ConfigType = null, // replace null with typeof(RuinHoundConfig) if applicable
    ObjectIDType = typeof(OID),
    ActionIDType = typeof(AID), // replace null with typeof(AID) if applicable
    StatusIDType = null, // replace null with typeof(SID) if applicable
    TetherIDType = null, // replace null with typeof(TetherID) if applicable
    IconIDType = null, // replace null with typeof(IconID) if applicable
    PrimaryActorOID = (uint)OID.RuinHound,
    Contributors = "Equilius",
    Expansion = BossModuleInfo.Expansion.Dawntrail,
    Category = BossModuleInfo.Category.Foray,
    GroupType = BossModuleInfo.GroupType.ForayFATE,
    GroupID = 1093u,
    NameID = 2080u,
    SortOrder = 9,
    PlanLevel = 0)]
[SkipLocalsInit]
public sealed class TerritorialDispute(WorldState ws, Actor primary) : OpenWorldFate(ws, primary);

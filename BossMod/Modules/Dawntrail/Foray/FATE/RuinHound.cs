namespace BossMod.Dawntrail.Foray.FATE.RuinHound;

public enum OID : uint
{
    Boss = 0x4D5E,
    Helper = 0x233C,
    IcePillar = 0x4D5F, // R2.000, x0 (spawn during fight)
    RuinHound = 0x4DA0, // R1.000, x0 (spawn during fight)
    RuinHound1 = 0x4D60, // R1.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 50536, // Boss->player, no cast, single-target
    IcePillarCast = 49758, // Boss->self, 3.0s cast, single-target
    IcePillar = 49770, // 4D5F->self, 3.0s cast, range 4 circle
    RoaringBlizzard = 49765, // Boss->self, 5.0s cast, range 50 60-degree cone
    Rush = 49759, // 4D5F->self, 4.0s cast, range 80 width 4 rect
    AgeOfEndlessFrostCast = 49760, // Boss->self, 3.0s cast, single-target
    AgeOfEndlessFrost = 49761, // 4DA0->self, 3.0s cast, range 40 60-degree cone
    TheStormWithout = 49757, // Boss->self, 5.0s cast, range 10-40 donut
    TheStormWithout1 = 49767, // 4D60->location, no cast, range ?-40 donut
    TheStormWithin = 49756, // Boss->self, 5.0s cast, range 10 circle
    TheStormWithin1 = 49766, // 4D60->location, no cast, range 10 circle
}

class IcePillar(BossModule module) : Components.StandardAOEs(module, AID.IcePillar, 4.0f);
class RoaringBlizzard(BossModule module) : Components.StandardAOEs(module, AID.RoaringBlizzard, new AOEShapeCone(50.0f, 30.0f.Degrees()));
class Rush(BossModule module) : Components.StandardAOEs(module, AID.Rush, new AOEShapeRect(80.0f, 2.0f));
class AgeOfEndlessFrost(BossModule module) : Components.StandardAOEs(module, AID.AgeOfEndlessFrost, new AOEShapeCone(40.0f, 30.0f.Degrees()));
class TheStormWithout(BossModule module) : Components.StandardAOEs(module, AID.TheStormWithout, new AOEShapeDonut(10.0f, 40.0f));
class TheStormWithin(BossModule module) : Components.StandardAOEs(module, AID.TheStormWithin, 10.0f);

class RuinHoundStates : StateMachineBuilder
{
    public RuinHoundStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<IcePillar>()
            .ActivateOnEnter<RoaringBlizzard>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<AgeOfEndlessFrost>()
            .ActivateOnEnter<TheStormWithout>()
            .ActivateOnEnter<TheStormWithin>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14762)]
public class RuinHound(WorldState ws, Actor primary) : BossModule(ws, primary, new(-90.000f, 865.000f), new ArenaBoundsCircle(30));

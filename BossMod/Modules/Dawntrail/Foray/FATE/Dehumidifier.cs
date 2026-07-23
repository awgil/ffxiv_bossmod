namespace BossMod.Modules.Dawntrail.Foray.FATE.Dehumidifier;

public enum OID : uint {
    Boss = 0x46B8,
    Helper = 0x233C,
    HotAir = 0x46BA, // R3.200, x0 (spawn during fight)
    HandDryer = 0x46B9, // R2.000, x0 (spawn during fight)
    Dehumidifier = 0x46BB, // R0.500, x0 (spawn during fight)
    Dehumidifier1 = 0x46DB, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 39460, // Boss->player, no cast, single-target
    Teleport = 30352, // Boss->location, no cast, single-target
    Ability = 4731, // 46BB->self, no cast, single-target
    AmorphicFlail = 40354, // 45BF->player, 5.0s cast, single-target

    FluidSwing = 30351, // Boss->self, 7.0s cast, range 60 90-degree cone
    FluidSwing1 = 30353, // Boss->location, 7.0s cast, range 60 90-degree cone

    Reproduce = 30339, // Boss->self, 3.0s cast, single-target
    HeatVortex = 30340, // 46BA->self, 4.0s cast, range 10 circle
    FireBlast = 30341, // 46B9->self, 5.0s cast, range 25 width 6 rect
    EruptionCast = 30345, // Boss->self, 3.0s cast, single-target
    Eruption = 30346, // 46BB->location, 3.0s cast, range 8 circle

    DryCycleCast = 30342, // Boss->self, 3.0s cast, single-target
    DryCycle = 30344, // 46DB->self, 8.0s cast, range 5-40 donut
}

class FluidSwing(BossModule module) : Components.GroupedAOEs(module, [AID.FluidSwing, AID.FluidSwing1], new AOEShapeCone(60.0f, 45.0f.Degrees()));
class HeatVortex(BossModule module) : Components.StandardAOEs(module, AID.HeatVortex, 10.0f);
class FireBlast(BossModule module) : Components.StandardAOEs(module, AID.FireBlast, new AOEShapeRect(25.0f, 3.0f));
class Eruption(BossModule module) : Components.StandardAOEs(module, AID.Eruption, 8.0f);
class DryCycle(BossModule module) : Components.StandardAOEs(module, AID.DryCycle, new AOEShapeDonut(5, 40));

class DehumidifierStates : StateMachineBuilder {
    public DehumidifierStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<FluidSwing>()
            .ActivateOnEnter<HeatVortex>()
            .ActivateOnEnter<FireBlast>()
            .ActivateOnEnter<Eruption>()
            .ActivateOnEnter<DryCycle>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13819)]
public class Dehumidifier(WorldState ws, Actor primary) : BossModule(ws, primary, new(-372.0f, 644.0f), new ArenaBoundsCircle(40));

namespace BossMod.Modules.Dawntrail.Foray.FATE;

public enum OID : uint {
    Boss = 0x4735,
    Helper = 0x233C,
    Sisyphus = 0x4736, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 41994, // Boss->player, no cast, single-target
    Teleport = 41971, // Boss->location, no cast, single-target

    ThunderousMemoryCircleCast = 41973, // Boss->self, 3.3+0.7s cast, single-target
    ThunderousMemoryCircle = 41974, // 4736->location, 4.0s cast, range 10 circle
    ResoundingMemoryCircle = 41980, // 4736->location, 5.0s cast, range 10 circle

    ThunderousMemoryConeCast = 41977, // Boss->self, 3.3+0.7s cast, single-target
    ThunderousMemoryCone = 41978, // 4736->location, 4.0s cast, range 70 45-degree cone
    ResoundingMemoryCone = 41982, // 4736->location, 5.0s cast, range 70 ?-degree cone

    ResoundingMemoryCast = 41979, // Boss->self, 4.3+0.7s cast, single-target

    ThriceComeThunderCast = 41983, // Boss->self, 4.3+0.7s cast, single-target
    ThriceComeThunderInner = 41984, // 4736->location, 5.0s cast, range 10 circle
    ThriceComeThunderMiddle = 41985, // 4736->location, 7.0s cast, range 10-20 donut
    ThriceComeThunderOuter = 41986, // 4736->location, 9.0s cast, range 20-30 donut

    ThunderIICast = 41987, // Boss->self, 2.3+0.7s cast, single-target
    ThunderII = 41988, // 4736->location, 3.0s cast, range 6 circle
    ThunderIVCast = 41991, // Boss->self, 4.3+0.7s cast, single-target
    ThunderIV = 41992, // 4736->location, 5.0s cast, range 40 circle
}

public enum SID : uint {
    ThunderousMemory = 4382, // Boss->Boss, extra=0x1/0x2
}

class ThunderousMemoryCircle(BossModule module) : Components.GroupedAOEs(module, [AID.ThunderousMemoryCircle, AID.ResoundingMemoryCircle], new AOEShapeCircle(10.0f));
class ThunderousMemoryCone(BossModule module) : Components.GroupedAOEs(module, [AID.ThunderousMemoryCone, AID.ResoundingMemoryCone], new AOEShapeCone(70.0f, 22.5f.Degrees()));
class ThunderII(BossModule module) : Components.StandardAOEs(module, AID.ThunderII, new AOEShapeCircle(6.0f));
class ThunderIV(BossModule module) : Components.RaidwideCast(module, AID.ThunderIV);

class ThriceComeThunder(BossModule module) : Components.ConcentricAOEs(module, [new AOEShapeCircle(10.0f), new AOEShapeDonut(10.0f, 20.0f), new AOEShapeDonut(20.0f, 30.0f)]) {
    public override void OnCastStarted(Actor caster, ActorCastInfo spell) {
        if (spell.Action.ID == (uint)AID.ThriceComeThunderCast) {
            AddSequence(spell.LocXZ, Module.CastFinishAt(spell));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        var order = (AID)spell.Action.ID switch {
            AID.ThriceComeThunderInner => 0,
            AID.ThriceComeThunderMiddle => 1,
            AID.ThriceComeThunderOuter => 2,
            _ => -1
        };

        if (order >= 0) {
            AdvanceSequence(order, spell.TargetXZ, WorldState.FutureTime(2));
        }
    }
}

class SisyphusStates : StateMachineBuilder {
    public SisyphusStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<ThunderousMemoryCircle>()
            .ActivateOnEnter<ThunderousMemoryCone>()
            .ActivateOnEnter<ThunderII>()
            .ActivateOnEnter<ThunderIV>()
            .ActivateOnEnter<ThriceComeThunder>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13703)]
public class Sisyphus(WorldState ws, Actor primary) : BossModule(ws, primary, new(-227.0f, 37.0f), new ArenaBoundsCircle(30));

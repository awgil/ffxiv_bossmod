namespace BossMod.Modules.Dawntrail.Foray.FATE.MadMudarch;

public enum OID : uint {
    Boss = 0x46BD,
    Helper = 0x233C,
    MadMudarch = 0x46C0, // R0.500, x0 (spawn during fight)
    CommonMud = 0x46BE, // R2.500-8.125, x0 (spawn during fight)
    CommonCompost = 0x46BF, // R4.000, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 39461, // Boss->player, no cast, single-target
    FromMud = 30706, // Boss->self, 3.0s cast, single-target
    FromMud1 = 29809, // Boss->self, 3.0s cast, single-target

    FeculentFlood = 29825, // Boss->self, 3.0s cast, range 40 60-degree cone
    RoyalFlush = 29826, // Boss->self, 5.0s cast, range 8 circle
    BogBequest = 29827, // Boss->self, 5.0s cast, range 10-20 donut
    UnshowerCast = 29806, // Boss->self, 3.0s cast, single-target
    Unshower = 29807, // 46C0->location, 3.0s cast, range 6 circle

    RockyRoll = 29810, // 46BF->self, 5.0s cast, range 60 width 6 rect
    Rupture = 29808, // 46BE->self, 2.0s cast, range 16 circle
}

public enum SID : uint {
    Growth = 4221, // 46C0->46BE, extra=0x1/0x2/0x3
    Growth1 = 4222, // 46C0->Boss/46BE, extra=0x2/0x1
    Growth2 = 4223, // 46C0->Boss/46BE, extra=0x0
    Ball = 2056, // none->46BF, extra=0xE1
}

class Unshower(BossModule module) : Components.StandardAOEs(module, AID.Unshower, 6f);
class RockyRoll(BossModule module) : Components.StandardAOEs(module, AID.RockyRoll, new AOEShapeRect(60f, 3f));
class BogBequest(BossModule module) : Components.StandardAOEs(module, AID.BogBequest, new AOEShapeDonut(10, 20));
class FeculentFlood(BossModule module) : Components.StandardAOEs(module, AID.FeculentFlood, new AOEShapeCone(40f, 30.Degrees()));
class RoyalFlush(BossModule module) : Components.StandardAOEs(module, AID.RoyalFlush, 8f);

class Rupture(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnStatusGain(Actor actor, ActorStatus status) {
        if (status.ID == (uint)SID.Growth && status.Extra == 0x02) {
            aoes.Add(new(new AOEShapeCircle(16f), actor.Position, actor.Rotation, WorldState.FutureTime(7.3f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Rupture) {
            aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 0.1f));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;
}

class MadMudarchStates : StateMachineBuilder {
    public MadMudarchStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<Unshower>()
            .ActivateOnEnter<RockyRoll>()
            .ActivateOnEnter<BogBequest>()
            .ActivateOnEnter<FeculentFlood>()
            .ActivateOnEnter<Rupture>()
            .ActivateOnEnter<RoyalFlush>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13816)]
public class MadMudarch(WorldState ws, Actor primary) : BossModule(ws, primary, new(-585.0f, 323.0f), new ArenaBoundsCircle(40));

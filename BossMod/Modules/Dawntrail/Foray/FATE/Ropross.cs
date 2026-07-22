namespace BossMod.Modules.Dawntrail.Foray.FATE.Ropross;

public enum OID : uint {
    Boss = 0x46D7,
    Helper = 0x233C,
    GaleSphere = 0x46D8, // R1.000, x0 (spawn during fight)
    Ropross = 0x46D9, // R0.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 42899, // Boss->player, no cast, single-target
    Teleport = 41392, // Boss->location, no cast, single-target
    Teleport2 = 41384, // Boss->location, no cast, single-target

    GlidingSwoop = 41387, // Boss->location, 6.0s cast, range 50 circle
    BitingScratch = 41388, // Boss->self, 6.0s cast, range 40 90-degree cone

    WindSphere = 41385, // Boss->self, 3.0s cast, single-target
    Airburst = 41386, // 46D8->self, 1.0s cast, range 11 circle

    FeatherRainCast = 41389, // Boss->self, 4.0s cast, single-target
    FeatherRain = 41390, // 46D9->location, 4.0s cast, range 11 circle

    AeroIV = 41391, // Boss->self, 5.0s cast, range 60 circle
}

class GlidingSwoop(BossModule module) : Components.StandardAOEs(module, AID.GlidingSwoop, new AOEShapeCircle(25f));
class BitingScratch(BossModule module) : Components.StandardAOEs(module, AID.BitingScratch, new AOEShapeCone(40.0f, 45.0f.Degrees()));
class FeatherRain(BossModule module) : Components.StandardAOEs(module, AID.FeatherRain, new AOEShapeCircle(11.0f));
class AeroIV(BossModule module) : Components.RaidwideCast(module, AID.AeroIV);

class Airburst(BossModule module) : Components.GenericAOEs(module) {
    private List<AOEInstance> aoes = [];

    public override void OnActorCreated(Actor actor) {
        if (actor.OID == (uint)OID.GaleSphere) {
            aoes.Add(new(new AOEShapeCircle(11), actor.Position, actor.Rotation, WorldState.FutureTime(9.8f)));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell) {
        if (spell.Action.ID == (uint)AID.Airburst) {
            aoes.RemoveAll(a => a.Origin.AlmostEqual(caster.Position, 0.1f));
        }
    }

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => aoes;
}

class RoprossStates : StateMachineBuilder {
    public RoprossStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<GlidingSwoop>()
            .ActivateOnEnter<BitingScratch>()
            .ActivateOnEnter<Airburst>()
            .ActivateOnEnter<FeatherRain>()
            .ActivateOnEnter<AeroIV>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13740)]
public class Ropross(WorldState ws, Actor primary) : BossModule(ws, primary, new(-231.0f, 252.0f), new ArenaBoundsCircle(40));

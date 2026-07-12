namespace BossMod.Modules.Dawntrail.Foray.FATE.Observer;

public enum OID : uint {
    Boss = 0x47DC,
    Helper = 0x233C,
    ObserversEye = 0x47DD, // R0.600, x0 (spawn during fight)
    ObserversEye1 = 0x4818, // R0.600, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 43367, // Boss->player, no cast, single-target
    JumpScare = 43041, // Boss->player, no cast, single-target

    Stare = 43268, // Boss->self, 5.0s cast, range 60 width 8 rect
    Stare1 = 43044, // Boss->self, 5.0s cast, range 60 width 8 rect
    Oogle = 43043, // Boss->self, 4.0s cast, range 40 circle
    VoidThunderII = 43045, // 47DE->location, 3.0s cast, range 6 circle

    MarkofDeathCone = 42839, // 47DD/4818->self, no cast, single-target
    Search = 43038, // 47DD/4818->self, no cast, single-target
    MarkOfDeath = 43039, // 47DD/4818->self, no cast, range 6 ?-degree cone
}

public enum SID : uint {
    Gen = 2552, // 47DD/4818->47DD/4818, extra=0x372
    Prey = 4473, // 4818->player, extra=0x0
    Stun = 4374, // 4818->player, extra=0x0
}


class Stare(BossModule module) : Components.GroupedAOEs(module, [AID.Stare, AID.Stare1], new AOEShapeRect(60, 4));
class Oogle(BossModule module) : Components.CastGaze(module, AID.Oogle, false);
class VoidThunderII(BossModule module) : Components.StandardAOEs(module, AID.VoidThunderII, 6.0f);

class MarkOfDeath(BossModule module) : Components.GenericAOEs(module, AID.MarkOfDeath) {
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) {
        var observersEyes = WorldState.Actors.Where(a => a.OID is (uint)OID.ObserversEye or (uint)OID.ObserversEye1).ToList();

        foreach (var eye in observersEyes) {
            if (!eye.IsDead) {
                yield return new(new AOEShapeCone(7.5f, 60.0f.Degrees()), eye.Position, eye.Rotation); // Slightly bigger aoe as the target is constantly moving
            }
        }
    }
}

class ObserverStates : StateMachineBuilder {
    public ObserverStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<Stare>()
            .ActivateOnEnter<Oogle>()
            .ActivateOnEnter<VoidThunderII>()
            .ActivateOnEnter<MarkOfDeath>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13853)]
public class Observer(WorldState ws, Actor primary) : BossModule(ws, primary, new(-71.000f, 557.000f), new ArenaBoundsCircle(40));

namespace BossMod.Modules.Dawntrail.Foray.FATE.GaintBird;

public enum OID : uint {
    Boss = 0x46C1,
    Helper = 0x233C,
    Petrifog = 0x46C2, // R1.300, x0 (spawn during fight)
    Petrifog1 = 0x4821, // R1.300, x0 (spawn during fight)
    Petrifog2 = 0x4822, // R1.300, x0 (spawn during fight)
    Petrifog3 = 0x481F, // R1.300, x0 (spawn during fight)
    Petrifog4 = 0x481E, // R1.300, x0 (spawn during fight)
    Petrifog5 = 0x4820, // R1.300, x0 (spawn during fight)
    Petrifog6 = 0x481D, // R1.300, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 42900, // Boss->player, no cast, single-target
    Teleport = 44481, // Boss->location, no cast, single-target

    GaleCannon = 41274, // Boss->self, 5.0s cast, range 40 width 10 rect
    Petrisphere = 41272, // Boss->self, 4.0s cast, single-target
    SphereShatter = 41273, // 46C2/481F/481E/481D/4822/4821/4820->self, 2.0s cast, range 7 circle
}

class GaleCannon(BossModule module) : Components.StandardAOEs(module, AID.GaleCannon, new AOEShapeRect(40.0f, 5.0f));
class SphereShatter(BossModule module) : Components.StandardAOEs(module, AID.SphereShatter, new AOEShapeCircle(7.0f));

class GiantBirdStates : StateMachineBuilder {
    public GiantBirdStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<GaleCannon>()
            .ActivateOnEnter<SphereShatter>();
    }
}

[ModuleInfo(Incomplete = true, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1018, NameID = 13739)]
public class GiantBird(WorldState ws, Actor primary) : BossModule(ws, primary, new(-547.0f, -600.0f), new ArenaBoundsCircle(40));

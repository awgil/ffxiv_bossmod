namespace BossMod.Dawntrail.Foray.FATE.DemiMedusa;

public enum OID : uint {
    Boss = 0x4C6A,
    Helper = 0x233C,
    DefectiveLamia = 0x4DD8, // R2.500, x0 (spawn during fight)
    DemiMedusa1 = 0x4C6C, // R1.000, x0 (spawn during fight)
    DemiMedusa2 = 0x4EC1, // R0.500, x0 (spawn during fight)
    DemiMedusa3 = 0x4EC2, // R0.500, x0 (spawn during fight)
    DemiMedusa4 = 0x4CAE, // R0.500, x0 (spawn during fight)
    DefectiveLamia1 = 0x4DD7, // R2.500, x0 (spawn during fight)
    DefectiveLamia2 = 0x4DD6, // R2.500, x0 (spawn during fight)
    DefectiveLamia3 = 0x4DD5, // R2.500, x0 (spawn during fight)
    DefectiveLamia4 = 0x4D54, // R2.500, x0 (spawn during fight)
    DefectiveLamia5 = 0x4D52, // R2.500, x0 (spawn during fight)
    DefectiveLamia6 = 0x4D53, // R2.500, x0 (spawn during fight)
    DefectiveLamia7 = 0x4D51, // R2.500, x0 (spawn during fight)
}

public enum AID : uint {
    AutoAttack = 50538, // Boss->player, no cast, single-target
    Summon = 48300, // Boss->self, 3.0s cast, single-target
    CursedSight = 48252, // Boss->self, 5.0s cast, range 60 60-degree cone
    CursedSight1 = 48253, // 4D51/4D52/4D53/4D54->self, 5.0s cast, range 60 60-degree cone
    LamianLesion = 48254, // Boss->self, 5.0s cast, range 25 180-degree cone
    DarkCast = 48255, // Boss->self, 3.0s cast, single-target
    Dark = 48256, // 4C6C/4CAE/4EC1/4EC2->location, 3.0s cast, range 6 circle
}

public enum SID : uint {
    Gen = 2056, // none->4D53/4D52/4D54/4D51, extra=0xE1
    Petrification = 3007, // 4D52/4D51/4D54/4D53->player, extra=0x0
}

class CursedSight(BossModule module) : Components.GroupedAOEs(module, [AID.CursedSight, AID.CursedSight1], new AOEShapeCone(60.0f, 30.0f.Degrees()));
class LamianLesion(BossModule module) : Components.StandardAOEs(module, AID.LamianLesion, new AOEShapeCone(25.0f, 90.0f.Degrees()));
class Dark(BossModule module) : Components.StandardAOEs(module, AID.Dark, 6.0f);

class DemiMedusaStates : StateMachineBuilder {
    public DemiMedusaStates(BossModule module) : base(module) {
        TrivialPhase()
            .ActivateOnEnter<CursedSight>()
            .ActivateOnEnter<LamianLesion>()
            .ActivateOnEnter<Dark>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Equilius", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1093, NameID = 14736)]
public class DemiMedusa(WorldState ws, Actor primary) : BossModule(ws, primary, new(-661.000f, -54.000f), new ArenaBoundsCircle(40));

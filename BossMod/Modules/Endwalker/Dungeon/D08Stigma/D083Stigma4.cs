namespace BossMod.Endwalker.Dungeon.D08Stigma.D083Stigma4;

public enum OID : uint
{
    Boss = 0x3428, // R6.990, x1
    Helper = 0x233C, // R0.500, x2, 523 type
    OmegaFrame = 0x342A, // R8.995, x0 (spawn during fight)
    HybridDragon = 0x342B, // R5.000, x0 (spawn during fight)
    ProtoRocketPunch = 0x3429, // R2.500, x0 (spawn during fight)
}

public enum AID : uint
{
    AITakeover = 25641, // Boss->self, 5.0s cast, single-target
    AtomicRay = 25654, // Boss->self, 5.0s cast, range 50 circle //Raidwide

    ElectromagneticReleaseInVisual = 25649, // Boss->self, no cast, single-target
    ElectromagneticReleaseIn = 25650, // Helper->self, 10.0s cast, range 7-60 donut
    ElectromagneticReleaseOutVisual = 25651, // Boss->self, no cast, single-target
    ElectromagneticReleaseOut = 25652, // Helper->self, 10.0s cast, range 16 circle

    FireBreath = 25646, // HybridDragon->self, 7.0s cast, range 40 120-degree cone

    Mindhack = 25648, // Boss->self, 5.0s cast, range 50 circle //Raidwide and forced march
    MultiAITakeover = 27723, // Boss->self, 5.0s cast, single-target

    Plasmafodder = 25653, // Helper->player, no cast, single-target

    ProtoWaveCannon1 = 25642, // OmegaFrame->self, 7.0s cast, range 60 180-degree cone
    ProtoWaveCannon2 = 25643, // OmegaFrame->self, 7.0s cast, range 60 180-degree cone

    Rush = 25645, // ProtoRocketPunch->location, 5.0s cast, width 5 rect charge

    SelfDestructOmega = 25644, // OmegaFrame->self, 15.0s cast, range 60 circle
    SelfDestructDragon = 25647, // HybridDragon->self, 15.0s cast, range 60 circle

    Touchdown = 26873, // HybridDragon->self, 2.0s cast, range 7 circle
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // HybridDragon/Helper->player, extra=0x1/0x2/0x3
    AboutFace = 1959, // Boss->player, extra=0x0
    ForwardMarch = 1958, // Boss->player, extra=0x0
    RightFace = 1961, // Boss->player, extra=0x0
    LeftFace = 1960, // Boss->player, extra=0x0
    ForcedMarch = 1257, // Boss->player, extra=0x2/0x1/0x8
    Bleeding = 2088, // Boss->player, extra=0x0
}

class Mindhack(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);

class ElectromagneticReleaseIn(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElectromagneticReleaseIn), new AOEShapeDonut(7, 60));
class ElectromagneticReleaseOut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElectromagneticReleaseOut), new AOEShapeCircle(16));

class FireBreath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FireBreath), new AOEShapeCone(40, 60.Degrees()));
class Touchdown(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Touchdown), new AOEShapeCircle(7));

class ProtoWaveCannon1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ProtoWaveCannon1), new AOEShapeCone(60, 90.Degrees()));
class ProtoWaveCannon2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ProtoWaveCannon2), new AOEShapeCone(60, 90.Degrees()));

class Rush(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Rush), 2.5f);

class SelfDestructOmega(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SelfDestructOmega));
class SelfDestructDragon(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.SelfDestructDragon));
class AtomicRay(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AtomicRay));

class D083Stigma4States : StateMachineBuilder
{
    public D083Stigma4States(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Mindhack>()
            .ActivateOnEnter<ElectromagneticReleaseIn>()
            .ActivateOnEnter<ElectromagneticReleaseOut>()
            .ActivateOnEnter<FireBreath>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<ProtoWaveCannon1>()
            .ActivateOnEnter<ProtoWaveCannon2>()
            .ActivateOnEnter<Rush>()
            .ActivateOnEnter<SelfDestructOmega>()
            .ActivateOnEnter<SelfDestructDragon>()
            .ActivateOnEnter<AtomicRay>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10404)]
public class D083Stigma4(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 2.5f), new ArenaBoundsSquare(20));

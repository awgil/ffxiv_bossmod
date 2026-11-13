namespace BossMod.Endwalker.Dungeon.D08StigmaDreamscape.D083Stigma4;

public enum OID : uint
{
    Boss = 0x3428, // R6.990, x1
    Helper = 0x233C, // R0.500, Helper type
    ProtoRocketPunch = 0x3429, // R2.500, x0 (spawn during fight)
    OmegaFrame = 0x342A, // R8.995, x0 (spawn during fight)
    HybridDragon = 0x342B, // R5.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AITakeover = 25641, // Boss->self, 5.0s cast, single-target, spawns one add type
    MultiAITakeover = 27723, // Boss->self, 5.0s cast, single-target, spawns two add types
    ProtoWaveCannon1 = 25642, // OmegaFrame->self, 7.0s cast, range 60 180-degree cone
    ProtoWaveCannon2 = 25643, // OmegaFrame->self, 7.0s cast, range 60 180-degree cone
    SelfDestructOmega = 25644, // OmegaFrame->self, 15.0s cast, range 60 circle
    Rush = 25645, // ProtoRocketPunch->location, 5.0s cast, width 5 rect charge
    FireBreath = 25646, // HybridDragon->self, 7.0s cast, range 40 120-degree cone
    SelfDestructDragon = 25647, // HybridDragon->self, 15.0s cast, range 60 circle
    Touchdown = 26873, // HybridDragon->self, 2.0s cast, range 7 circle
    Mindhack = 25648, // Boss->self, 5.0s cast, range 50 circle, raidwide + forced march debuffs
    ElectromagneticReleaseInVisual = 25649, // Boss->self, no cast, single-target
    ElectromagneticReleaseIn = 25650, // Helper->self, 10.0s cast, range 7-60 donut
    ElectromagneticReleaseOutVisual = 25651, // Boss->self, no cast, single-target
    ElectromagneticReleaseOut = 25652, // Helper->self, 10.0s cast, range 16 circle
    Plasmafodder = 25653, // Helper->player, no cast, single-target
    AtomicRay = 25654, // Boss->self, 5.0s cast, range 50 circle, raidwide
}

public enum SID : uint
{
    ForwardMarch = 1958, // Boss->player, extra=0x0
    AboutFace = 1959, // Boss->player, extra=0x0
    LeftFace = 1960, // Boss->player, extra=0x0
    RightFace = 1961, // Boss->player, extra=0x0
}

class Mindhack(BossModule module) : Components.RaidwideCast(module, AID.Mindhack);
class MindhackMarch(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);
class ElectromagneticReleaseIn(BossModule module) : Components.StandardAOEs(module, AID.ElectromagneticReleaseIn, new AOEShapeDonut(7, 60));
class ElectromagneticReleaseOut(BossModule module) : Components.StandardAOEs(module, AID.ElectromagneticReleaseOut, new AOEShapeCircle(16));
class ProtoWaveCannon(BossModule module) : Components.GroupedAOEs(module, [AID.ProtoWaveCannon1, AID.ProtoWaveCannon2], new AOEShapeCone(60, 90.Degrees()));
class Rush(BossModule module) : Components.ChargeAOEs(module, AID.Rush, 2.5f);
class FireBreath(BossModule module) : Components.StandardAOEs(module, AID.FireBreath, new AOEShapeCone(40, 60.Degrees()));
class Touchdown(BossModule module) : Components.StandardAOEs(module, AID.Touchdown, new AOEShapeCircle(7));
class SelfDestructOmega(BossModule module) : Components.RaidwideCast(module, AID.SelfDestructOmega, "Kill Omega Frame or raidwide");
class SelfDestructDragon(BossModule module) : Components.RaidwideCast(module, AID.SelfDestructDragon, "Kill Hybrid Dragon or raidwide");
class AtomicRay(BossModule module) : Components.RaidwideCast(module, AID.AtomicRay);
class OmegaFrame(BossModule module) : Components.Adds(module, (uint)OID.OmegaFrame, 1);
class HybridDragon(BossModule module) : Components.Adds(module, (uint)OID.HybridDragon, 1);

class D083Stigma4States : StateMachineBuilder
{
    public D083Stigma4States(BossModule module) : base(module)
    {
        // TODO: forced march duration unverified (2s); edge bleed zone size unknown
        TrivialPhase()
            .ActivateOnEnter<AtomicRay>()
            .ActivateOnEnter<Mindhack>()
            .ActivateOnEnter<MindhackMarch>()
            .ActivateOnEnter<ElectromagneticReleaseOut>()
            .ActivateOnEnter<ElectromagneticReleaseIn>()
            .ActivateOnEnter<OmegaFrame>()
            .ActivateOnEnter<SelfDestructOmega>()
            .ActivateOnEnter<ProtoWaveCannon>()
            .ActivateOnEnter<HybridDragon>()
            .ActivateOnEnter<SelfDestructDragon>()
            .ActivateOnEnter<FireBreath>()
            .ActivateOnEnter<Touchdown>()
            .ActivateOnEnter<Rush>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10404)]
public class D083Stigma4(ModuleInit init) : BossModule(init, new(0, 2.5f), new ArenaBoundsSquare(20));

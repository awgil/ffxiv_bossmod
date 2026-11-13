namespace BossMod.Endwalker.Dungeon.D08StigmaDreamscape.D082ArchLambda;

public enum OID : uint
{
    Boss = 0x3416, // R6.000, x1
    Helper = 0x233C, // R0.500, Helper type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AutoMobileAssaultCannon = 25515, // Boss->self, 5.9s cast, single-target, visual (4 dashes)
    AutoMobileAssaultCannonStart = 25514, // Boss->location, no cast, single-target
    Tread1 = 25516, // Boss->location, no cast, width 8 rect charge
    Tread2 = 25517, // Boss->location, no cast, width 8 rect charge
    TreadTelegraph = 25518, // Helper->location, 1.5s cast, width 8 rect charge
    WaveCannon = 25519, // Boss->self, 2.5s cast, range 40 180-degree cone
    AutoMobileSniperCannon = 25520, // Boss->location, 7.0s cast, single-target, dash to the north wall
    Entrench = 25521, // Helper->self, 7.5s cast, range 41 width 8 rect
    SniperCannonBeam = 25522, // Helper->self, no cast, range 40 width 6 rect
    AtomicFlame = 25524, // Boss->self, 5.0s cast, range 40 circle, raidwide
    Wheel = 25525, // Boss->player, 5.0s cast, single-target, tankbuster
}

class Tread(BossModule module) : Components.ChargeAOEs(module, AID.TreadTelegraph, 4);
class WaveCannon(BossModule module) : Components.StandardAOEs(module, AID.WaveCannon, new AOEShapeCone(40, 90.Degrees()));
class Entrench(BossModule module) : Components.StandardAOEs(module, AID.Entrench, new AOEShapeRect(41, 4));
class AtomicFlame(BossModule module) : Components.RaidwideCast(module, AID.AtomicFlame);
class Wheel(BossModule module) : Components.SingleTargetCast(module, AID.Wheel);

class D082ArchLambdaStates : StateMachineBuilder
{
    public D082ArchLambdaStates(BossModule module) : base(module)
    {
        // TODO: Sniper Cannon beams (orb count per player) have no cast; check that the Tread telegraphs cover all 4 dashes
        TrivialPhase()
            .ActivateOnEnter<Tread>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnEnter<Entrench>()
            .ActivateOnEnter<AtomicFlame>()
            .ActivateOnEnter<Wheel>();
    }
}

[ModuleInfo(Incomplete = true, Contributors = "Kagekazu", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10403)]
public class D082ArchLambda(ModuleInit init) : BossModule(init, new(0, 0), new ArenaBoundsCircle(20));

namespace BossMod.Endwalker.Dungeon.D08Stigma.D082ArchLambda;

public enum OID : uint
{
    Boss = 0x3416, // R=6.0
    Helper = 0x233C,
}
public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    AtomicFlame = 25524, // Boss->self, 5.0s cast, range 40 circle //Raidwide
    AutoMobileAssaultCannon = 25515, // Boss->self, 5.9s cast, single-target
    AutoMobileSniperCannon1 = 25520, // Boss->location, 7.0s cast, single-target
    AutoMobileSniperCannon2 = 25522, // Helper->self, no cast, range 40 width 6 rect 
    Entrench = 25521, // Helper->self, 7.5s cast, range 41 width 8 rect
    Tread1 = 25516, // Boss->location, no cast, width 8 rect charge
    Tread2 = 25517, // Boss->location, no cast, width 8 rect charge
    Unknown1 = 25514, // Boss->location, no cast, single-target
    Unknown2 = 25518, // Helper->location, 1.5s cast, width 8 rect charge
    WaveCannon = 25519, // Boss->self, 2.5s cast, range 40 180-degree cone
    Wheel = 25525, // Boss->player, 5.0s cast, single-target //Tankbuster
}

public enum SID : uint
{
    VulnerabilityUp = 1789, // Boss->player, extra=0x1/0x2
}

public enum IconID : uint
{
    Icon218 = 218, // player
    Icon79 = 79, // player/Helper
    Icon81 = 81, // player/Helper
    Icon80 = 80, // player/Helper
}

class Tread1(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Tread1), 4);
class Tread2(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Tread2), 4);
class Unknown2(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Unknown2), 4);

class AutoMobileSniperCannon2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AutoMobileSniperCannon2), new AOEShapeRect(40, 3));
class Entrench(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Entrench), new AOEShapeRect(41, 4));

class WaveCannon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WaveCannon), new AOEShapeCone(40, 90.Degrees()));

class AtomicFlame(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.AtomicFlame));
class Wheel(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Wheel));

class D082ArchLambdaStates : StateMachineBuilder
{
    public D082ArchLambdaStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Tread1>()
            .ActivateOnEnter<Tread2>()
            .ActivateOnEnter<Unknown2>()
            .ActivateOnEnter<AutoMobileSniperCannon2>()
            .ActivateOnEnter<Entrench>()
            .ActivateOnEnter<WaveCannon>()
            .ActivateOnEnter<AtomicFlame>()
            .ActivateOnEnter<Wheel>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 784, NameID = 10403)]
public class D082ArchLambda(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(20));

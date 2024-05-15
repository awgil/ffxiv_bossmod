namespace BossMod.Endwalker.Dungeon.D04Ktisis.D042Ladon;

public enum OID : uint
{
    Boss = 0x3425, // R=3.99
    Helper = 0x233C,
    PyricSphere = 0x3426, // R0.700, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 872, // Boss->player, no cast, single-target
    Inhale1 = 25732, // Boss->self, 4.0s cast, single-target //Indicator
    Inhale2 = 25915, // Boss->self, no cast, single-target //Indicator
    IntimidationRaidwide = 25741, // Boss->self, 6.0s cast, range 40 circle //Raidwide
    PyricBlastStack = 25742, // Boss->players, 4.0s cast, range 6 circle //Stack
    PyricBreathFront = 25734, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreathLeft = 25735, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreathRight = 25736, // Boss->self, 7.0s cast, range 40 120-degree cone
    PyricBreath4 = 25737, // Boss->self, no cast, range 40 ?-degree cone
    PyricBreath5 = 25738, // Boss->self, no cast, range 40 ?-degree cone
    PyricBreath6 = 25739, // Boss->self, no cast, range 40 ?-degree cone
    PyricSphereVisual = 25744, // PyricSphere->self, 5.0s cast, single-target
    PyricSphereAOE = 25745, // Helper->self, 10.0s cast, range 50 width 4 cross //Cross
    ScratchTankbuster = 25743, // Boss->player, 5.0s cast, single-target //Tankbuster
    UnknownAbility = 25733, // Boss->location, no cast, single-target
    UnknownSpell = 25740, // Boss->self, no cast, ???
}

public enum SID : uint
{
    Unknown1 = 2195, // none->Boss, extra=0x144/0x145/0x149/0x146/0x148/0x147
    Unknown2 = 2812, // none->Boss, extra=0x9F6
    Unknown3 = 2813, // none->Boss, extra=0x177F
    Unknown4 = 2814, // none->Boss, extra=0x21A8
    VulnerabilityUp = 1789, // Helper->player, extra=0x2
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Stackmarker = 62, // player
}
class PyricSphereAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PyricSphereAOE), new AOEShapeCross(50, 2));

class PyricBreathFront(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PyricBreathFront), new AOEShapeCone(40, 60.Degrees()));
class PyricBreathLeft(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PyricBreathLeft), new AOEShapeCone(40, 60.Degrees()));
class PyricBreathRight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PyricBreathRight), new AOEShapeCone(40, 60.Degrees()));
class PyricBlastStack(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.PyricBlastStack), 6, 8);
class ScratchTankbuster(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ScratchTankbuster));
class IntimidationRaidwide(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.IntimidationRaidwide));

class D042LadonStates : StateMachineBuilder
{
    public D042LadonStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<PyricSphereAOE>()
            .ActivateOnEnter<PyricBreathFront>()
            .ActivateOnEnter<PyricBreathLeft>()
            .ActivateOnEnter<PyricBreathRight>()
            .ActivateOnEnter<PyricBlastStack>()
            .ActivateOnEnter<ScratchTankbuster>()
            .ActivateOnEnter<IntimidationRaidwide>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 787, NameID = 10398)]
public class D042Ladon(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 48), new ArenaBoundsSquare(20));

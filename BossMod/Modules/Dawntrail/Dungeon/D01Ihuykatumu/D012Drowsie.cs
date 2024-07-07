namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D012Drowsie;

public enum OID : uint
{
    Boss = 0x4195, // R5.000, x1
    Helper = 0x233C, // R0.500, x10, 523 type
    Apollyon = 0x41B9, // R7.000, x1
    IhuykatumuIvy = 0x419C, // R4.200-8.400, x0 (spawn during fight)
    BlueClot = 0x4197, // R2.000, x0 (spawn during fight)
    GreenClot = 0x4196, // R3.500, x0 (spawn during fight)
    RedClot = 0x4198, // R1.300, x0 (spawn during fight)
    Mimiclot1 = 0x419B, // R0.800, x0 (spawn during fight)
    Mimiclot2 = 0x41A0, // R1.200, x0 (spawn during fight)
    Mimiclot3 = 0x4199, // R1.750, x0 (spawn during fight)
    Mimiclot4 = 0x41A1, // R1.200, x0 (spawn during fight)
    Mimiclot5 = 0x419A, // R1.200, x0 (spawn during fight)
    Mimiclot6 = 0x419F, // R1.750, x0 (spawn during fight)
    Actor1e8fb8 = 0x1E8FB8, // R2.000, x2, EventObj type
    Actor1e8f2f = 0x1E8F2F, // R0.500, x1, EventObj type
}

public enum AID : uint
{
    AutoAttack = 872, // Boss/Mimiclot1/Mimiclot3/Mimiclot4/Mimiclot6/Mimiclot2/Mimiclot5->player, no cast, single-target

    Uppercut = 39132, // Boss->player, 5.0s cast, single-target

    Sow = 36476, // Boss->self, 3.0s cast, single-target // spawn seeds

    DrowsyDance = 36477, // Boss->self, 3.5s cast, single-target

    Arise = 36478, // IhuykatumuIvy->self, 3.0s cast, range 8 circle

    Wallop1 = 36479, // IhuykatumuIvy->self, 7.0s cast, range 40 width 10 rect
    Wallop2 = 36482, // IhuykatumuIvy->self, 7.0s cast, range 40 width 16 rect

    UnknownAbility1 = 36480, // Helper->IhuykatumuIvy, no cast, single-target
    UnknownAbility2 = 36481, // Boss->self, no cast, single-target
    UnknownAbility3 = 36484, // BlueClot/GreenClot/RedClot->location, no cast, single-target

    UnknownWeaponskill = 36762, // Boss->self, no cast, single-target

    Sneeze = 36475, // Boss->self, 5.0s cast, range 60 150.000-degree cone
    Spit = 36483, // Boss->self, 5.0s cast, single-target

    Metamorphosis1 = 36524, // BlueClot->self, 2.0s cast, single-target
    Metamorphosis2 = 36523, // GreenClot->self, 2.0s cast, single-target
    Metamorphosis3 = 36525, // RedClot->self, 2.0s cast, single-target

    FlagrantSpread1 = 36522, // Mimiclot5/Mimiclot2->player, 5.0s cast, range 6 circle
    FlagrantSpread2 = 36485, // Mimiclot3/Mimiclot6->self, 5.0s cast, range 6 circle
}

public enum SID : uint
{
    UnknownStatus1 = 2193, // Boss->Boss/IhuykatumuIvy, extra=0x2D2/0x2B9
    Vitalized = 3806, // none->IhuykatumuIvy, extra=0x1/0x2/0x3/0x4/0x5
    VulnerabilityUp = 1789, // IhuykatumuIvy/Mimiclot6->player, extra=0x1
    UnknownStatus2 = 2397, // none->Mimiclot1/Mimiclot2/Mimiclot3/Mimiclot4/Mimiclot5/Mimiclot6, extra=0x2C1/0x2C0/0x2BF
}

public enum IconID : uint
{
    Tankbuster = 218, // player
    Spreadmarker = 139, // player
}

class Uppercut(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.Uppercut));
class Arise(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Arise), new AOEShapeCircle(8));
class Wallop1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wallop1), new AOEShapeRect(40, 5));
class Wallop2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Wallop2), new AOEShapeRect(40, 8));
class SelfTargetSneezeedAOEs(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Sneeze), new AOEShapeCone(60, 75.Degrees()));
class FlagrantSpread1(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.FlagrantSpread1), 6);
class FlagrantSpread2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FlagrantSpread2), new AOEShapeCircle(6));

class D012DrowsieStates : StateMachineBuilder
{
    public D012DrowsieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Uppercut>()
            .ActivateOnEnter<Arise>()
            .ActivateOnEnter<Wallop1>()
            .ActivateOnEnter<Wallop2>()
            .ActivateOnEnter<SelfTargetSneezeedAOEs>()
            .ActivateOnEnter<FlagrantSpread1>()
            .ActivateOnEnter<FlagrantSpread2>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12716)]
public class D012Drowsie(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, 53), new ArenaBoundsCircle(19.5f));

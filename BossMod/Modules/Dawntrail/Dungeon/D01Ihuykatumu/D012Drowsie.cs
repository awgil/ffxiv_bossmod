namespace BossMod.Dawntrail.Dungeon.D01Ihuykatumu.D012Drowsie;

public enum OID : uint
{
    Boss = 0x4195, // R5.000, x1
    Helper = 0x233C, // R0.500, x10, Helper type
    GreenClot = 0x4196, // R3.500, x0 (spawn during fight)
    BlueClot = 0x4197, // R2.000, x0 (spawn during fight)
    RedClot = 0x4198, // R1.300, x0 (spawn during fight)
    MimiclotGreen1 = 0x4199, // R1.750, x0 (spawn during fight)
    MimiclotBlue1 = 0x419A, // R1.200, x0 (spawn during fight)
    MimiclotRed = 0x419B, // R0.800, x0 (spawn during fight)
    IhuykatumuIvy = 0x419C, // R4.200-8.400, x0 (spawn during fight) - seed add
    MimiclotGreen2 = 0x419F, // R1.750, x0 (spawn during fight)
    MimiclotBlue2 = 0x41A0, // R1.200, x0 (spawn during fight)
    MimiclotBlue3 = 0x41A1, // R1.200, x0 (spawn during fight)
}

public enum AID : uint
{
    Awaken = 36762, // Boss->self, no cast, single-target, visual (awaken on pull)
    AutoAttack = 872, // Boss/Mimiclot*->player, no cast, single-target
    Uppercut = 39132, // Boss->player, 5.0s cast, single-target, tankbuster
    Sow = 36476, // Boss->self, 3.0s cast, single-target, visual (spawn seeds)
    DrowsyDance = 36477, // Boss->self, 3.5s cast, single-target, visual (grow seeds)
    Arise = 36478, // IhuykatumuIvy->self, 3.0s cast, range 8 circle
    WallopNarrow = 36479, // IhuykatumuIvy->self, 7.0s cast, range 40 width 10 rect
    GrowIvy = 36480, // Helper->IhuykatumuIvy, no cast, single-target, visual (start growing)
    EndDance = 36481, // Boss->self, no cast, single-target, visual (stop dancing)
    WallopWide = 36482, // IhuykatumuIvy->self, 7.0s cast, range 40 width 16 rect
    Sneeze = 36475, // Boss->self, 5.0s cast, range 60 150-degree cone
    Spit = 36483, // Boss->self, 5.0s cast, single-target, visual (spawn clots)
    ClotAppear = 36484, // GreenClot/BlueClot/RedClot->location, no cast, single-target, visual (move from boss to location)
    MetamorphosisGreen = 36523, // GreenClot->self, 2.0s cast, single-target, die and turn into mimiclot
    MetamorphosisBlue = 36524, // BlueClot->self, 2.0s cast, single-target, die and turn into mimiclot
    MetamorphosisRed = 36525, // RedClot->self, 2.0s cast, single-target, die and turn into mimiclot
    FlagrantSpreadBlue = 36522, // MimiclotBlue*->players, 5.0s cast, range 6 circle
    FlagrantSpreadGreen = 36485, // MimiclotGreen*->self, 5.0s cast, range 6 circle
}

public enum IconID : uint
{
    Uppercut = 218, // player
    FlagrantSpread = 139, // player
}

class Uppercut(BossModule module) : Components.SingleTargetCast(module, AID.Uppercut);
class Arise(BossModule module) : Components.StandardAOEs(module, AID.Arise, new AOEShapeCircle(8));
class WallopNarrow(BossModule module) : Components.StandardAOEs(module, AID.WallopNarrow, new AOEShapeRect(40, 5));
class WallopWide(BossModule module) : Components.StandardAOEs(module, AID.WallopWide, new AOEShapeRect(40, 8));
class Sneeze(BossModule module) : Components.StandardAOEs(module, AID.Sneeze, new AOEShapeCone(60, 75.Degrees()));
class Mimiclots(BossModule module) : Components.AddsMulti(module, [OID.MimiclotGreen1, OID.MimiclotGreen2, OID.MimiclotBlue1, OID.MimiclotBlue2, OID.MimiclotBlue3, OID.MimiclotRed]);
class FlagrantSpreadBlue(BossModule module) : Components.SpreadFromCastTargets(module, AID.FlagrantSpreadBlue, 6);
class FlagrantSpreadGreen(BossModule module) : Components.StandardAOEs(module, AID.FlagrantSpreadGreen, 6);

class D012DrowsieStates : StateMachineBuilder
{
    public D012DrowsieStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<Uppercut>()
            .ActivateOnEnter<Arise>()
            .ActivateOnEnter<WallopNarrow>()
            .ActivateOnEnter<WallopWide>()
            .ActivateOnEnter<Sneeze>()
            .ActivateOnEnter<Mimiclots>()
            .ActivateOnEnter<FlagrantSpreadBlue>()
            .ActivateOnEnter<FlagrantSpreadGreen>();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 826, NameID = 12716)]
public class D012Drowsie(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, 53), new ArenaBoundsCircle(20));

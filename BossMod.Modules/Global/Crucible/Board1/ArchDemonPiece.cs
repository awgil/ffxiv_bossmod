namespace BossMod.Global.Crucible.ArchDemonPiece;

public enum OID : uint
{
    Boss = 0x4B88, // R3.000, x1
    AbyssalLance = 0x4B89, // R1.500, x0 (spawn during fight)
    Helper = 0x233C, // R0.500, x39, Helper type
}

public enum AID : uint
{
    AutoAttack = 49682, // Boss->player, no cast, single-target
    AbyssalChargeBoss = 46875, // Boss->self, 3.0s cast, single-target
    AbyssalCharge = 46876, // 4B89->self, 3.0s cast, range 40 width 4 rect
    DismemberBoss = 46877, // Boss->self, 3.0s cast, single-target
    DismemberUnk = 46878, // Helper->self, 4.0s cast, single-target
    Dismember = 46879, // Helper->self, 4.5s cast, range 35 width 8 rect
    AbyssalTransfixionBoss = 46880, // Boss->self, 3.0s cast, single-target
    SwordAppearLarge = 46881, // Helper->self, no cast, single-target
    AbyssalTransfixionLarge = 46882, // Helper->self, 3.7s cast, range 6 circle
    SwordAppearSmall = 46883, // Helper->self, no cast, single-target
    AbyssalTransfixionSmall = 46884, // Helper->self, 3.5s cast, range 3 circle
    AbyssalSwingBoss = 46885, // Boss->self, no cast, single-target
    AbyssalSwing = 46886, // Helper->self, 5.7s cast, range 40 180-degree cone
}

class AbyssalCharge(BossModule module) : Components.StandardAOEs(module, AID.AbyssalCharge, new AOEShapeRect(40, 2));
class Dismember(BossModule module) : Components.StandardAOEs(module, AID.Dismember, new AOEShapeRect(35, 4), 4);
class AbyssalTransfixionBig(BossModule module) : Components.StandardAOEs(module, AID.AbyssalTransfixionLarge, 6);
class AbyssalTransfixionSmall(BossModule module) : Components.StandardAOEs(module, AID.AbyssalTransfixionSmall, 3);
class AbyssalSwing(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSwing, new AOEShapeCone(40, 90.Degrees()));

class ArchDemonPieceStates : StateMachineBuilder
{
    public ArchDemonPieceStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbyssalCharge>()
            .ActivateOnEnter<Dismember>()
            .ActivateOnEnter<AbyssalTransfixionBig>()
            .ActivateOnEnter<AbyssalTransfixionSmall>()
            .ActivateOnEnter<AbyssalSwing>();
    }
}

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1088, NameID = 14533)]
public class ArchDemonPiece(ModuleInit init) : BossModule(init, new(520, 0), new ArenaBoundsRect(20, 15));


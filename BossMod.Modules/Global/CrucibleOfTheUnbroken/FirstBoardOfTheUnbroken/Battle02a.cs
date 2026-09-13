namespace BossMod.Global.CrucibleOfTheUnbroken.FirstBoardOfTheUnbroken.Battle02a;

public enum OID : uint
{
    Boss = 0x4B88,
    AbyssalLance = 0x4B89,
    Helper = 0x233C,
}
public enum AID : uint
{
    AutoAttack = 49682, // Boss->player, no cast, single-target
    AbyssalChargeVisual = 46875, // Boss->self, 3.0s cast, single-target
    AbyssalCharge = 46876, // AbyssalLance->self, 3.0s cast, range 40 width 4 rect
    DismemberVisual = 46877, // Boss->self, 3.0s cast, single-target
    Dismember = 46879, // Helper->self, 4.5s cast, range 35 width 8 rect
    AbyssalTransfixion = 46880, // Boss->self, 3.0s cast, single-target
    AbyssalTransfixionBig = 46882, // Helper->self, 3.7s cast, range 6 circle
    AbyssalTransfixionSmall = 46884, // Helper->self, 3.5s cast, range 3 circle
    AbyssalSwingVisual = 46885, // Boss->self, no cast, single-target
    AbyssalSwing = 46886, // Helper->self, 5.7s cast, range 40 180-degree cone
}
class AbyssalCharge(BossModule module) : Components.StandardAOEs(module, AID.AbyssalCharge, new AOEShapeRect(40f, 2f));
class Dismember(BossModule module) : Components.StandardAOEs(module, AID.Dismember, new AOEShapeRect(35f, 4f), 4, highlightImminent: true);
class AbyssalTransfixionBig(BossModule module) : Components.StandardAOEs(module, AID.AbyssalTransfixionBig, 6f);
class AbyssalTransfixionSmall(BossModule module) : Components.StandardAOEs(module, AID.AbyssalTransfixionSmall, 3f);
class AbyssalSwing(BossModule module) : Components.StandardAOEs(module, AID.AbyssalSwing, new AOEShapeCone(40f, 90.Degrees()));

class Battle02aStates : StateMachineBuilder
{
    public Battle02aStates(BossModule module) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<AbyssalCharge>()
            .ActivateOnEnter<Dismember>()
            .ActivateOnEnter<AbyssalTransfixionBig>()
            .ActivateOnEnter<AbyssalTransfixionSmall>()
            .ActivateOnEnter<AbyssalSwing>();
    }
}

[ModuleInfo(Contributors = "VeraNala", GroupType = BossModuleInfo.GroupType.CrucibleOfTheUnbroken, GroupID = 1088, NameID = 14533)]
public class Battle02a(WorldState ws, Actor primary) : BossModule(ws, primary, new(520, 0), new ArenaBoundsRect(20, 15));

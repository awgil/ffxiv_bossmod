namespace BossMod.Endwalker.Criterion.C02AMR.C020Trash1;

class RightSwipe(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(60, 90.Degrees()));
class NRightSwipe(BossModule module) : RightSwipe(module, AID.NRightSwipe);
class SRightSwipe(BossModule module) : RightSwipe(module, AID.SRightSwipe);

class LeftSwipe(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(60, 90.Degrees()));
class NLeftSwipe(BossModule module) : LeftSwipe(module, AID.NLeftSwipe);
class SLeftSwipe(BossModule module) : LeftSwipe(module, AID.SLeftSwipe);

class C020YukiStates : StateMachineBuilder
{
    public C020YukiStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NRightSwipe>(!savage)
            .ActivateOnEnter<NLeftSwipe>(!savage)
            .ActivateOnEnter<SRightSwipe>(savage)
            .ActivateOnEnter<SLeftSwipe>(savage);
    }
}
class C020NYukiStates(BossModule module) : C020YukiStates(module, false);
class C020SYukiStates(BossModule module) : C020YukiStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NYuki, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12425, SortOrder = 3)]
public class C020NYuki(WorldState ws, Actor primary) : C020Trash1(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SYuki, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12425, SortOrder = 3)]
public class C020SYuki(WorldState ws, Actor primary) : C020Trash1(ws, primary);

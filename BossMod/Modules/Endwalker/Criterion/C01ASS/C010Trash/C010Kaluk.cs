namespace BossMod.Endwalker.Criterion.C01ASS.C010Kaluk;

public enum OID : uint
{
    NBoss = 0x3AD6, // R2.800, x1
    SBoss = 0x3ADF, // R2.800, x1
}

public enum AID : uint
{
    AutoAttack = 31320, // NBoss/SBoss->player, no cast, single-target
    NRightSweep = 31075, // NBoss->self, 4.0s cast, range 30 210-degree cone aoe
    NLeftSweep = 31076, // NBoss->self, 4.0s cast, range 30 210-degree cone aoe
    NCreepingIvy = 31077, // NBoss->self, 3.0s cast, range 10 90-degree cone aoe
    SRightSweep = 31099, // SBoss->self, 4.0s cast, range 30 210-degree cone aoe
    SLeftSweep = 31100, // SBoss->self, 4.0s cast, range 30 210-degree cone aoe
    SCreepingIvy = 31101, // SBoss->self, 3.0s cast, range 10 90-degree cone aoe
}

class RightSweep(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(30, 105.Degrees()));
class NRightSweep(BossModule module) : RightSweep(module, AID.NRightSweep);
class SRightSweep(BossModule module) : RightSweep(module, AID.SRightSweep);

class LeftSweep(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(30, 105.Degrees()));
class NLeftSweep(BossModule module) : LeftSweep(module, AID.NLeftSweep);
class SLeftSweep(BossModule module) : LeftSweep(module, AID.SLeftSweep);

class CreepingIvy(BossModule module, AID aid) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(aid), new AOEShapeCone(10, 45.Degrees()));
class NCreepingIvy(BossModule module) : CreepingIvy(module, AID.NCreepingIvy);
class SCreepingIvy(BossModule module) : CreepingIvy(module, AID.SCreepingIvy);

class C010KalukStates : StateMachineBuilder
{
    public C010KalukStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NRightSweep>(!savage)
            .ActivateOnEnter<NLeftSweep>(!savage)
            .ActivateOnEnter<NCreepingIvy>(!savage)
            .ActivateOnEnter<SRightSweep>(savage)
            .ActivateOnEnter<SLeftSweep>(savage)
            .ActivateOnEnter<SCreepingIvy>(savage);
    }
}
class C010NKalukStates(BossModule module) : C010KalukStates(module, false);
class C010SKalukStates(BossModule module) : C010KalukStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11510, SortOrder = 2)]
public class C010NKaluk(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11510, SortOrder = 2)]
public class C010SKaluk(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

namespace BossMod.Endwalker.Criterion.C01ASS.C010Kaluk;

public enum OID : uint
{
    NBoss = 0x3AD6, // R2.800, x1
    SBoss = 0x3ADF, // R2.800, x1
};

public enum AID : uint
{
    AutoAttack = 31320, // NBoss/SBoss->player, no cast, single-target
    NRightSweep = 31075, // NBoss->self, 4.0s cast, range 30 210-degree cone aoe
    NLeftSweep = 31076, // NBoss->self, 4.0s cast, range 30 210-degree cone aoe
    NCreepingIvy = 31077, // NBoss->self, 3.0s cast, range 10 90-degree cone aoe
    SRightSweep = 31099, // SBoss->self, 4.0s cast, range 30 210-degree cone aoe
    SLeftSweep = 31100, // SBoss->self, 4.0s cast, range 30 210-degree cone aoe
    SCreepingIvy = 31101, // SBoss->self, 3.0s cast, range 10 90-degree cone aoe
};

class RightSweep : Components.SelfTargetedAOEs
{
    public RightSweep(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(30, 105.Degrees())) { }
}
class NRightSweep : RightSweep { public NRightSweep() : base(AID.NRightSweep) { } }
class SRightSweep : RightSweep { public SRightSweep() : base(AID.SRightSweep) { } }

class LeftSweep : Components.SelfTargetedAOEs
{
    public LeftSweep(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(30, 105.Degrees())) { }
}
class NLeftSweep : LeftSweep { public NLeftSweep() : base(AID.NLeftSweep) { } }
class SLeftSweep : LeftSweep { public SLeftSweep() : base(AID.SLeftSweep) { } }

class CreepingIvy : Components.SelfTargetedAOEs
{
    public CreepingIvy(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeCone(10, 45.Degrees())) { }
}
class NCreepingIvy : CreepingIvy { public NCreepingIvy() : base(AID.NCreepingIvy) { } }
class SCreepingIvy : CreepingIvy { public SCreepingIvy() : base(AID.SCreepingIvy) { } }

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
class C010NKalukStates : C010KalukStates { public C010NKalukStates(BossModule module) : base(module, false) { } }
class C010SKalukStates : C010KalukStates { public C010SKalukStates(BossModule module) : base(module, true) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11510, SortOrder = 2)]
public class C010NKaluk : SimpleBossModule { public C010NKaluk(WorldState ws, Actor primary) : base(ws, primary) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11510, SortOrder = 2)]
public class C010SKaluk : SimpleBossModule { public C010SKaluk(WorldState ws, Actor primary) : base(ws, primary) { } }

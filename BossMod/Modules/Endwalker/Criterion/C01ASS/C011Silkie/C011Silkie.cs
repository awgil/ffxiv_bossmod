namespace BossMod.Endwalker.Criterion.C01ASS.C011Silkie;

class FizzlingDuster(BossModule module, AID aid) : Components.StandardAOEs(module, aid, C011Silkie.ShapeYellow);
class NFizzlingDuster(BossModule module) : FizzlingDuster(module, AID.NFizzlingDusterAOE);
class SFizzlingDuster(BossModule module) : FizzlingDuster(module, AID.SFizzlingDusterAOE);

class DustBluster(BossModule module, AID aid) : Components.KnockbackFromCastTarget(module, aid, 16);
class NDustBluster(BossModule module) : DustBluster(module, AID.NDustBluster);
class SDustBluster(BossModule module) : DustBluster(module, AID.SDustBluster);

class SqueakyCleanE(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(60, 112.5f.Degrees()));
class NSqueakyCleanE(BossModule module) : SqueakyCleanE(module, AID.NSqueakyCleanAOE3E);
class SSqueakyCleanE(BossModule module) : SqueakyCleanE(module, AID.SSqueakyCleanAOE3E);

class SqueakyCleanW(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCone(60, 112.5f.Degrees()));
class NSqueakyCleanW(BossModule module) : SqueakyCleanW(module, AID.NSqueakyCleanAOE3W);
class SSqueakyCleanW(BossModule module) : SqueakyCleanW(module, AID.SSqueakyCleanAOE3W);

class ChillingDusterPuff(BossModule module, AID aid) : Components.StandardAOEs(module, aid, C011Silkie.ShapeBlue);
class NChillingDusterPuff(BossModule module) : ChillingDusterPuff(module, AID.NChillingDusterPuff);
class SChillingDusterPuff(BossModule module) : ChillingDusterPuff(module, AID.SChillingDusterPuff);

class BracingDusterPuff(BossModule module, AID aid) : Components.StandardAOEs(module, aid, C011Silkie.ShapeGreen);
class NBracingDusterPuff(BossModule module) : BracingDusterPuff(module, AID.NBracingDusterPuff);
class SBracingDusterPuff(BossModule module) : BracingDusterPuff(module, AID.SBracingDusterPuff);

class FizzlingDusterPuff(BossModule module, AID aid) : Components.StandardAOEs(module, aid, C011Silkie.ShapeYellow);
class NFizzlingDusterPuff(BossModule module) : FizzlingDusterPuff(module, AID.NFizzlingDusterPuff);
class SFizzlingDusterPuff(BossModule module) : FizzlingDusterPuff(module, AID.SFizzlingDusterPuff);

public abstract class C011Silkie(WorldState ws, Actor primary) : BossModule(ws, primary, new(-335, -155), new ArenaBoundsSquare(20))
{
    public static readonly AOEShapeCross ShapeBlue = new(60, 5);
    public static readonly AOEShapeDonut ShapeGreen = new(5, 60);
    public static readonly AOEShapeCone ShapeYellow = new(60, 22.5f.Degrees());
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11369, SortOrder = 5, PlanLevel = 90)]
public class C011NSilkie(WorldState ws, Actor primary) : C011Silkie(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11369, SortOrder = 5, PlanLevel = 90)]
public class C011SSilkie(WorldState ws, Actor primary) : C011Silkie(ws, primary);

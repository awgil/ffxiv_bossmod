namespace BossMod.Endwalker.Criterion.C02AMR.C022Gorai;

class Unenlightenment : Components.CastCounter
{
    public Unenlightenment(AID aid) : base(ActionID.MakeSpell(aid)) { }
}
class NUnenlightenment : Unenlightenment { public NUnenlightenment() : base(AID.NUnenlightenmentAOE) { } }
class SUnenlightenment : Unenlightenment { public SUnenlightenment() : base(AID.SUnenlightenmentAOE) { } }

public abstract class C022Gorai : BossModule
{
    public C022Gorai(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(300, -120), 20)) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12373, SortOrder = 7)]
public class C022NGorai : C022Gorai { public C022NGorai(WorldState ws, Actor primary) : base(ws, primary) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12373, SortOrder = 7)]
public class C022SGorai : C022Gorai { public C022SGorai(WorldState ws, Actor primary) : base(ws, primary) { } }

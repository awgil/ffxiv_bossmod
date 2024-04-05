namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class SplittingCry : Components.BaitAwayCast
{
    public SplittingCry(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeRect(60, 7)) { }
}
class NSplittingCry : SplittingCry { public NSplittingCry() : base(AID.NSplittingCry) { } }
class SSplittingCry : SplittingCry { public SSplittingCry() : base(AID.SSplittingCry) { } }

class ThunderVortex : Components.SelfTargetedAOEs
{
    public ThunderVortex(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeDonut(8, 30)) { }
}
class NThunderVortex : ThunderVortex { public NThunderVortex() : base(AID.NThunderVortex) { } }
class SThunderVortex : ThunderVortex { public SThunderVortex() : base(AID.SThunderVortex) { } }

public abstract class C021Shishio : BossModule
{
    public C021Shishio(WorldState ws, Actor primary) : base(ws, primary, new ArenaBoundsSquare(new(0, -100), 20)) { }
}

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12428, SortOrder = 4)]
public class C021NShishio : C021Shishio { public C021NShishio(WorldState ws, Actor primary) : base(ws, primary) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12428, SortOrder = 4)]
public class C021SShishio : C021Shishio { public C021SShishio(WorldState ws, Actor primary) : base(ws, primary) { } }

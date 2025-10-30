namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class SplittingCry(BossModule module, AID aid) : Components.BaitAwayCast(module, aid, new AOEShapeRect(60, 7));
class NSplittingCry(BossModule module) : SplittingCry(module, AID.NSplittingCry);
class SSplittingCry(BossModule module) : SplittingCry(module, AID.SSplittingCry);

class ThunderVortex(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeDonut(8, 30));
class NThunderVortex(BossModule module) : ThunderVortex(module, AID.NThunderVortex);
class SThunderVortex(BossModule module) : ThunderVortex(module, AID.SThunderVortex);

class CircleBounds(BossModule module) : BossComponent(module)
{
    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x34)
        {
            if (state == 0x00020001)
                Arena.Bounds = new ArenaBoundsCircle(20);
            if (state == 0x00080004)
                Arena.Bounds = new ArenaBoundsSquare(20);
        }
    }
}

public abstract class C021Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -100), new ArenaBoundsSquare(20));

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 946, NameID = 12428, SortOrder = 4, PlanLevel = 90)]
public class C021NShishio(WorldState ws, Actor primary) : C021Shishio(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 947, NameID = 12428, SortOrder = 4, PlanLevel = 90)]
public class C021SShishio(WorldState ws, Actor primary) : C021Shishio(ws, primary);

namespace BossMod.Endwalker.Criterion.C01ASS.C010Dullahan;

public enum OID : uint
{
    NBoss = 0x3AD7, // R2.500, x1
    SBoss = 0x3AE0, // R2.500, x1
}

public enum AID : uint
{
    AutoAttack = 31318, // Boss->player, no cast, single-target
    NBlightedGloom = 31078, // Boss->self, 4.0s cast, range 10 circle aoe
    NKingsWill = 31080, // Boss->self, 2.5s cast, single-target damage up
    NInfernalPain = 31081, // Boss->self, 5.0s cast, raidwide
    SBlightedGloom = 31102, // Boss->self, 4.0s cast, range 10 circle aoe
    SKingsWill = 31104, // Boss->self, 2.5s cast, single-target damage up
    SInfernalPain = 31105, // Boss->self, 5.0s cast, raidwide
}

class BlightedGloom(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeCircle(10));
class NBlightedGloom(BossModule module) : BlightedGloom(module, AID.NBlightedGloom);
class SBlightedGloom(BossModule module) : BlightedGloom(module, AID.SBlightedGloom);

class KingsWill(BossModule module, AID aid) : Components.CastHint(module, aid, "Damage increase buff");
class NKingsWill(BossModule module) : KingsWill(module, AID.NKingsWill);
class SKingsWill(BossModule module) : KingsWill(module, AID.SKingsWill);

class InfernalPain(BossModule module, AID aid) : Components.RaidwideCast(module, aid);
class NInfernalPain(BossModule module) : InfernalPain(module, AID.NInfernalPain);
class SInfernalPain(BossModule module) : InfernalPain(module, AID.SInfernalPain);

class C010DullahanStates : StateMachineBuilder
{
    public C010DullahanStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NBlightedGloom>(!savage)
            .ActivateOnEnter<NKingsWill>(!savage)
            .ActivateOnEnter<NInfernalPain>(!savage)
            .ActivateOnEnter<SBlightedGloom>(savage)
            .ActivateOnEnter<SKingsWill>(savage)
            .ActivateOnEnter<SInfernalPain>(savage);
    }
}
class C010NDullahanStates(BossModule module) : C010DullahanStates(module, false);
class C010SDullahanStates(BossModule module) : C010DullahanStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11506, SortOrder = 7)]
public class C010NDullahan(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11506, SortOrder = 7)]
public class C010SDullahan(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

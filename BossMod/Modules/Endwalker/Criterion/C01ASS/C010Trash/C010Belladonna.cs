namespace BossMod.Endwalker.Criterion.C01ASS.C010Belladonna;

public enum OID : uint
{
    NBoss = 0x3AD5, // R4.000, x1
    SBoss = 0x3ADE, // R4.000, x1
}

public enum AID : uint
{
    AutoAttack = 31320, // NBoss/SBoss->player, no cast, single-target
    NAtropineSpore = 31072, // NBoss->self, 4.0s cast, range 10-40 donut aoe
    NFrondAffront = 31073, // NBoss->self, 3.0s cast, gaze
    NDeracinator = 31074, // NBoss->player, 4.0s cast, single-target tankbuster
    SAtropineSpore = 31096, // SBoss->self, 4.0s cast, range 10-40 donut aoe
    SFrondAffront = 31097, // SBoss->self, 3.0s cast, gaze
    SDeracinator = 31098, // SBoss->player, 4.0s cast, single-target tankbuster
}

class AtropineSpore(BossModule module, AID aid) : Components.StandardAOEs(module, aid, new AOEShapeDonut(10, 40));
class NAtropineSpore(BossModule module) : AtropineSpore(module, AID.NAtropineSpore);
class SAtropineSpore(BossModule module) : AtropineSpore(module, AID.SAtropineSpore);

class FrondAffront(BossModule module, AID aid) : Components.CastGaze(module, aid);
class NFrondAffront(BossModule module) : FrondAffront(module, AID.NFrondAffront);
class SFrondAffront(BossModule module) : FrondAffront(module, AID.SFrondAffront);

class Deracinator(BossModule module, AID aid) : Components.SingleTargetCast(module, aid);
class NDeracinator(BossModule module) : Deracinator(module, AID.NDeracinator);
class SDeracinator(BossModule module) : Deracinator(module, AID.SDeracinator);

class C010BelladonnaStates : StateMachineBuilder
{
    public C010BelladonnaStates(BossModule module, bool savage) : base(module)
    {
        TrivialPhase()
            .ActivateOnEnter<NAtropineSpore>(!savage)
            .ActivateOnEnter<NFrondAffront>(!savage)
            .ActivateOnEnter<NDeracinator>(!savage)
            .ActivateOnEnter<SAtropineSpore>(savage)
            .ActivateOnEnter<SFrondAffront>(savage)
            .ActivateOnEnter<SDeracinator>(savage);
    }
}
class C010NBelladonnaStates(BossModule module) : C010BelladonnaStates(module, false);
class C010SBelladonnaStates(BossModule module) : C010BelladonnaStates(module, true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11514, SortOrder = 1)]
public class C010NBelladonna(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11514, SortOrder = 1)]
public class C010SBelladonna(WorldState ws, Actor primary) : SimpleBossModule(ws, primary);

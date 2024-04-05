namespace BossMod.Endwalker.Criterion.C01ASS.C010Belladonna;

public enum OID : uint
{
    NBoss = 0x3AD5, // R4.000, x1
    SBoss = 0x3ADE, // R4.000, x1
};

public enum AID : uint
{
    AutoAttack = 31320, // NBoss/SBoss->player, no cast, single-target
    NAtropineSpore = 31072, // NBoss->self, 4.0s cast, range 10-40 donut aoe
    NFrondAffront = 31073, // NBoss->self, 3.0s cast, gaze
    NDeracinator = 31074, // NBoss->player, 4.0s cast, single-target tankbuster
    SAtropineSpore = 31096, // SBoss->self, 4.0s cast, range 10-40 donut aoe
    SFrondAffront = 31097, // SBoss->self, 3.0s cast, gaze
    SDeracinator = 31098, // SBoss->player, 4.0s cast, single-target tankbuster
};

class AtropineSpore : Components.SelfTargetedAOEs
{
    public AtropineSpore(AID aid) : base(ActionID.MakeSpell(aid), new AOEShapeDonut(10, 40)) { }
}
class NAtropineSpore : AtropineSpore { public NAtropineSpore() : base(AID.NAtropineSpore) { } }
class SAtropineSpore : AtropineSpore { public SAtropineSpore() : base(AID.SAtropineSpore) { } }

class FrondAffront : Components.CastGaze
{
    public FrondAffront(AID aid) : base(ActionID.MakeSpell(aid)) { }
}
class NFrondAffront : FrondAffront { public NFrondAffront() : base(AID.NFrondAffront) { } }
class SFrondAffront : FrondAffront { public SFrondAffront() : base(AID.SFrondAffront) { } }

class Deracinator : Components.SingleTargetCast
{
    public Deracinator(AID aid) : base(ActionID.MakeSpell(aid)) { }
}
class NDeracinator : Deracinator { public NDeracinator() : base(AID.NDeracinator) { } }
class SDeracinator : Deracinator { public SDeracinator() : base(AID.SDeracinator) { } }

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
class C010NBelladonnaStates : C010BelladonnaStates { public C010NBelladonnaStates(BossModule module) : base(module, false) { } }
class C010SBelladonnaStates : C010BelladonnaStates { public C010SBelladonnaStates(BossModule module) : base(module, true) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.NBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 878, NameID = 11514, SortOrder = 1)]
public class C010NBelladonna : SimpleBossModule { public C010NBelladonna(WorldState ws, Actor primary) : base(ws, primary) { } }

[ModuleInfo(BossModuleInfo.Maturity.Verified, PrimaryActorOID = (uint)OID.SBoss, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 879, NameID = 11514, SortOrder = 1)]
public class C010SBelladonna : SimpleBossModule { public C010SBelladonna(WorldState ws, Actor primary) : base(ws, primary) { } }

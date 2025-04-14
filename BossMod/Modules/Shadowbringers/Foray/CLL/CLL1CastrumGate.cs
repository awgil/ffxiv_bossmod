#if DEBUG
namespace BossMod.Shadowbringers.Foray.CLL.CLL1CastrumGate;

public enum OID : uint
{
    Boss = 0x2ED9,
    Helper = 0x233C,
    _Gen_4ThLegionEques = 0x2EF0, // R0.500, x3
    _Gen_4ThLegionLaquearius = 0x2EEF, // R0.500, x3
    _Gen_4ThLegionDuplicarius = 0x2EDD, // R0.500, x4
    _Gen_4ThLegionHelldiver = 0x3005, // R2.800, x2
    _Gen_LacusLitoreDuplicarius = 0x300A, // R0.500, x2
    _Gen_4ThLegionHelldiver1 = 0x2ED4, // R2.800, x12
    _Gen_4ThLegionHelldiver2 = 0x2ED3, // R3.640, x1
    _Gen_ = 0x2ED2, // R0.500, x2
    _Gen_4ThLegionHelldiver3 = 0x2ED5, // R2.800, x0 (spawn during fight)
    _Gen_4ThLegionSkyArmor = 0x2F4E, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    _Weaponskill_ChainCannon = 20985, // _Gen_4ThLegionHelldiver2/_Gen_4ThLegionHelldiver1->self, 2.5s cast, range 60 width 5 rect
    _Weaponskill_ChainCannon1 = 20986, // Helper->self, no cast, range 60 width 5 rect
    _Weaponskill_MRVMissile = 20988, // _Gen_4ThLegionHelldiver2->self, 3.0s cast, single-target
    _Weaponskill_MRVMissile1 = 20989, // Helper->self, 5.0s cast, range 60 circle, raidwide
    _Weaponskill_CommandLinearDive = 20970, // _Gen_4ThLegionHelldiver2->self, 5.0s cast, single-target
    _Weaponskill_LinearDive = 20971, // _Gen_4ThLegionHelldiver1->2ED7, 7.0s cast, width 6 rect charge
    _Weaponskill_CommandSuppressiveFormation = 20981, // _Gen_4ThLegionHelldiver2->self, 3.0s cast, single-target
    _Weaponskill_CommandSuppressiveFormation1 = 20982, // _Gen_4ThLegionHelldiver1->location, 5.0s cast, width 6 rect charge
    _Weaponskill_MagitekMissiles = 20990, // _Gen_4ThLegionHelldiver2->self, 3.0s cast, single-target
    _Weaponskill_MagitekMissiles1 = 20991, // Helper->player, 5.0s cast, single-target, tankbuster
    _Weaponskill_CommandInfraredBlast = 20972, // _Gen_4ThLegionHelldiver2->self, 5.0s cast, single-target
    _Weaponskill_InfraredBlast = 20973, // _Gen_4ThLegionHelldiver1->self, 1.0s cast, single-target
    _Weaponskill_InfraredBlast1 = 20974, // Helper->player/2ED7, no cast, single-target, stacking fire vuln (from tethers)
    _Weaponskill_CommandChainCannon = 20987, // _Gen_4ThLegionHelldiver2->self, 4.0s cast, single-target
    _Weaponskill_CommandDiveFormation = 20975, // _Gen_4ThLegionHelldiver2->self, 5.0s cast, single-target
    _Weaponskill_DiveFormation = 20976, // _Gen_4ThLegionHelldiver1->2ED7, 11.0s cast, width 6 rect charge
    _Weaponskill_DiveFormation1 = 20977, // Helper->2ED7, 11.5s cast, width 10 rect charge
    _Weaponskill_CommandJointAttack = 20978, // _Gen_4ThLegionHelldiver2->self, 5.0s cast, single-target
    _Weaponskill_ = 20950, // Helper->self, no cast, ???
    _Weaponskill_1 = 20952, // Helper->self, no cast, ???
    _Weaponskill_SurfaceMissile = 20983, // _Gen_4ThLegionHelldiver2->self, 3.0s cast, single-target
    _Weaponskill_SurfaceMissile1 = 20984, // Helper->location, 3.0s cast, range 6 circle
    _Weaponskill_AntiMaterielMissile = 20979, // _Gen_4ThLegionHelldiver3->self, 5.0s cast, single-target
    _Weaponskill_AntiMaterielMissile1 = 20980, // Helper->2ED7, 2.0s cast, single-target
    _AutoAttack_Attack4 = 21263, // _Gen_4ThLegionSkyArmor->player, no cast, single-target
}

class CastrumGateStates : StateMachineBuilder
{
    public CastrumGateStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 735, NameID = 9441)]
public class CastrumGate(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, -177.3f), new ArenaBoundsRect(30, 26.7f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat && WorldState.Party.Player() is { } player && Bounds.Contains(player.Position - Arena.Center);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.SetPriority(PrimaryActor, AIHints.Enemy.PriorityInvincible);
    }
}
#endif

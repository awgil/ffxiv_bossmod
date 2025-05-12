namespace BossMod.Shadowbringers.Foray.CLL.CLL1CastrumGate;

public enum OID : uint
{
    Boss = 0x2ED9,
    Helper = 0x233C,
    O4ThLegionEques = 0x2EF0, // R0.500, x3
    O4ThLegionLaquearius = 0x2EEF, // R0.500, x3
    O4ThLegionDuplicarius = 0x2EDD, // R0.500, x4
    O4ThLegionHelldiver = 0x3005, // R2.800, x2
    OLacusLitoreDuplicarius = 0x300A, // R0.500, x2
    O4ThLegionHelldiver1 = 0x2ED4, // R2.800, x12
    O4ThLegionHelldiver2 = 0x2ED3, // R3.640, x1
    O = 0x2ED2, // R0.500, x2
    O4ThLegionHelldiver3 = 0x2ED5, // R2.800, x0 (spawn during fight)
    O4ThLegionSkyArmor = 0x2F4E, // R2.000, x0 (spawn during fight)
}

public enum AID : uint
{
    AutoAttack = 21263, // _Gen_4ThLegionSkyArmor->player, no cast, single-target
    ChainCannon = 20985, // _Gen_4ThLegionHelldiver2/_Gen_4ThLegionHelldiver1->self, 2.5s cast, range 60 width 5 rect
    ChainCannon1 = 20986, // Helper->self, no cast, range 60 width 5 rect
    MRVMissile = 20988, // _Gen_4ThLegionHelldiver2->self, 3.0s cast, single-target
    MRVMissile1 = 20989, // Helper->self, 5.0s cast, range 60 circle, raidwide
    CommandLinearDive = 20970, // _Gen_4ThLegionHelldiver2->self, 5.0s cast, single-target
    LinearDive = 20971, // _Gen_4ThLegionHelldiver1->2ED7, 7.0s cast, width 6 rect charge
    CommandSuppressiveFormation = 20981, // _Gen_4ThLegionHelldiver2->self, 3.0s cast, single-target
    CommandSuppressiveFormation1 = 20982, // _Gen_4ThLegionHelldiver1->location, 5.0s cast, width 6 rect charge
    MagitekMissiles = 20990, // _Gen_4ThLegionHelldiver2->self, 3.0s cast, single-target
    MagitekMissiles1 = 20991, // Helper->player, 5.0s cast, single-target, tankbuster
    CommandInfraredBlast = 20972, // _Gen_4ThLegionHelldiver2->self, 5.0s cast, single-target
    InfraredBlast = 20973, // _Gen_4ThLegionHelldiver1->self, 1.0s cast, single-target
    InfraredBlast1 = 20974, // Helper->player/2ED7, no cast, single-target, stacking fire vuln (from tethers)
    CommandChainCannon = 20987, // _Gen_4ThLegionHelldiver2->self, 4.0s cast, single-target
    CommandDiveFormation = 20975, // _Gen_4ThLegionHelldiver2->self, 5.0s cast, single-target
    DiveFormation = 20976, // _Gen_4ThLegionHelldiver1->2ED7, 11.0s cast, width 6 rect charge
    DiveFormation1 = 20977, // Helper->2ED7, 11.5s cast, width 10 rect charge
    CommandJointAttack = 20978, // _Gen_4ThLegionHelldiver2->self, 5.0s cast, single-target
    Unk1 = 20950, // Helper->self, no cast, ???
    Unk2 = 20952, // Helper->self, no cast, ???
    SurfaceMissile = 20983, // _Gen_4ThLegionHelldiver2->self, 3.0s cast, single-target
    SurfaceMissile1 = 20984, // Helper->location, 3.0s cast, range 6 circle
    AntiMaterielMissile = 20979, // _Gen_4ThLegionHelldiver3->self, 5.0s cast, single-target
    AntiMaterielMissile1 = 20980, // Helper->2ED7, 2.0s cast, single-target
}

class CastrumGateStates : StateMachineBuilder
{
    public CastrumGateStates(BossModule module) : base(module)
    {
        TrivialPhase();
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 735, NameID = 9441, DevOnly = true)]
public class CastrumGate(WorldState ws, Actor primary) : BossModule(ws, primary, new(80, -177.3f), new ArenaBoundsRect(30, 26.7f))
{
    protected override bool CheckPull() => PrimaryActor.InCombat && WorldState.Party.Player() is { } player && Bounds.Contains(player.Position - Arena.Center);

    protected override void CalculateModuleAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        hints.SetPriority(PrimaryActor, AIHints.Enemy.PriorityInvincible);
    }
}

namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class TheFixer(BossModule module) : Components.RaidwideCast(module, AID.TheFixer);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1075, NameID = 14378, PlanLevel = 100)]
public class RM12S1TheLindwurm(ModuleArgs init) : BossModule(init, new(100, 100), new ArenaBoundsRect(20, 15));


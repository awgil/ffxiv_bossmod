namespace BossMod.Dawntrail.Savage.RM12S1TheLindwurm;

class TheFixer(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_TheFixer);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1075, NameID = 14378)]
public class RM12S1TheLindwurm(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 15));


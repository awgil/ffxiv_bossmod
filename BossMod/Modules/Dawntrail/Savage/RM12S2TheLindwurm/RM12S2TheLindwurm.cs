namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class ArcadiaAflame(BossModule module) : Components.RaidwideCast(module, AID._Weaponskill_ArcadiaAflame);

[ModuleInfo(BossModuleInfo.Maturity.WIP, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1075, NameID = 14379, PlanLevel = 100)]
public class RM12S2TheLindwurm(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

namespace BossMod.Dawntrail.Savage.RM12S2TheLindwurm;

class ArcadiaAflame(BossModule module) : Components.RaidwideCast(module, AID.ArcadiaAflame);
class IdyllicDreamRaidwide(BossModule module) : Components.RaidwideCast(module, AID.IdyllicDream);
class LindwurmsMeteor(BossModule module) : Components.RaidwideCast(module, AID.LindwurmsMeteor);
class ArcadianHell5x(BossModule module) : Components.RaidwideCast(module, AID.ArcadianHell4x);
class ArcadianHell9x(BossModule module) : Components.RaidwideCast(module, AID.ArcadianHell8x);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1075, NameID = 14379, PlanLevel = 100)]
public class RM12S2TheLindwurm(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

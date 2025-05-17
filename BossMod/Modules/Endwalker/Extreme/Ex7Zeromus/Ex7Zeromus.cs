namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class AbyssalEchoes(BossModule module) : Components.StandardAOEs(module, AID.AbyssalEchoes, new AOEShapeCircle(12), 5);
class BigBangPuddle(BossModule module) : Components.StandardAOEs(module, AID.BigBangAOE, 5);
class BigBangSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.BigBangSpread, 5);
class BigCrunchPuddle(BossModule module) : Components.StandardAOEs(module, AID.BigCrunchAOE, 5);
class BigCrunchSpread(BossModule module) : Components.SpreadFromCastTargets(module, AID.BigCrunchSpread, 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 965, NameID = 12586, PlanLevel = 90)]
public class Ex7Zeromus(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));

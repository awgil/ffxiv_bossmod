namespace BossMod.Endwalker.Extreme.Ex7Zeromus;

class AbyssalEchoes(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AbyssalEchoes), new AOEShapeCircle(12), 5);
class BigBangPuddle(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BigBangAOE), 5);
class BigBangSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BigBangSpread), 5);
class BigCrunchPuddle(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BigCrunchAOE), 5);
class BigCrunchSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BigCrunchSpread), 5);

[ConfigDisplay(Order = 0x070, Parent = typeof(EndwalkerConfig))]
public class Ex7ZeromusConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 965, NameID = 12586)]
public class Ex7Zeromus(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));

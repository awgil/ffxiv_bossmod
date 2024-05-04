namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class ToxicCrunch(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.ToxicCrunchAOE)); // TODO: improve component?
class DoubleRush(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.DoubleRush), 50);
class DoubleRushReturn(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DoubleRushReturn)); // TODO: show knockback?
class SonicShatter(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.SonicShatterRest));
class DevourBait(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DevourBait));

[ConfigDisplay(Order = 0x150, Parent = typeof(EndwalkerConfig))]
public class P5SConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 873, NameID = 11440)]
public class P5S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(15));

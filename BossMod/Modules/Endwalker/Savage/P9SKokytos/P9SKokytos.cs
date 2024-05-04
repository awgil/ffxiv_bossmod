namespace BossMod.Endwalker.Savage.P9SKokytos;

class GluttonysAugur(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.GluttonysAugurAOE));
class SoulSurge(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.SoulSurge));
class BeastlyFury(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BeastlyFuryAOE));

[ConfigDisplay(Order = 0x190, Parent = typeof(EndwalkerConfig))]
public class P9SKokytosConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 937, NameID = 12369)]
public class P9SKokytos(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

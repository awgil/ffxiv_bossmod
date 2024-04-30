namespace BossMod.Endwalker.Savage.P7SAgdistis;

class HemitheosHoly(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.HemitheosHolyAOE), 6, 4);
class BoughOfAttisBack(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BoughOfAttisBackAOE), new AOEShapeCircle(25));
class BoughOfAttisFront(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BoughOfAttisFrontAOE), new AOEShapeCircle(19));
class BoughOfAttisSide(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BoughOfAttisSideAOE), new AOEShapeRect(50, 12.5f));
class HemitheosAeroKnockback1(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.HemitheosAeroKnockback1), 16); // TODO: verify distance...
class HemitheosAeroKnockback2(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.HemitheosAeroKnockback2), 16);
class HemitheosHolySpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HemitheosHolySpread), 6);
class HemitheosTornado(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HemitheosTornado), new AOEShapeCircle(25));
class HemitheosGlareMine(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HemitheosGlareMine), new AOEShapeDonut(5, 30)); // TODO: verify inner radius

[ConfigDisplay(Order = 0x170, Parent = typeof(EndwalkerConfig))]
public class P7SConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 877, NameID = 11374)]
public class P7S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(27));

namespace BossMod.Heavensward.Extreme.Ex3Thordan;

class AscalonsMight(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.AscalonsMight), new AOEShapeCone(8 + 3.8f, 45.Degrees()));
class Meteorain(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MeteorainAOE), 6);
class AscalonsMercy(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AscalonsMercy), new AOEShapeCone(34.8f, 10.Degrees()));
class AscalonsMercyHelper(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AscalonsMercyAOE), new AOEShapeCone(34.5f, 10.Degrees()));
class DragonsRage(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DragonsRage), 6, 6);
class Heavensflame(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HeavensflameAOE), 6);
class Conviction(BossModule module) : Components.CastTowers(module, ActionID.MakeSpell(AID.ConvictionAOE), 3);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, ActionID.MakeSpell(AID.HolyChain));
class SerZephirin(BossModule module) : Components.Adds(module, (uint)OID.Zephirin);
class LightOfAscalon(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.LightOfAscalon)); // TODO: show knockback 3 from [-0.8, -16.3]
class UltimateEnd(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.UltimateEndAOE));
class HeavenswardLeap(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.HeavenswardLeap));
class PureOfSoul(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.PureOfSoul));
class AbsoluteConviction(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.AbsoluteConviction));

[ModuleInfo(BossModuleInfo.Maturity.Contributed, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 91, NameID = 3632, PlanLevel = 60)]
public class Ex3Thordan(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(21));

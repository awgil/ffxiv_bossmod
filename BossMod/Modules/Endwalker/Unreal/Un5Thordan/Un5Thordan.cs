namespace BossMod.Endwalker.Unreal.Un5Thordan;

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

[ConfigDisplay(Order = 0x350, Parent = typeof(EndwalkerConfig))]
public class Un5ThordanConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 963, NameID = 3632)]
public class Un5Thordan(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(21));

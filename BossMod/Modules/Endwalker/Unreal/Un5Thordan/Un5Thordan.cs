namespace BossMod.Endwalker.Unreal.Un5Thordan;

class AscalonsMight(BossModule module) : Components.Cleave(module, AID.AscalonsMight, new AOEShapeCone(8 + 3.8f, 45.Degrees()));
class Meteorain(BossModule module) : Components.StandardAOEs(module, AID.MeteorainAOE, 6);
class AscalonsMercy(BossModule module) : Components.StandardAOEs(module, AID.AscalonsMercy, new AOEShapeCone(34.8f, 10.Degrees()));
class AscalonsMercyHelper(BossModule module) : Components.StandardAOEs(module, AID.AscalonsMercyAOE, new AOEShapeCone(34.5f, 10.Degrees()));
class DragonsRage(BossModule module) : Components.StackWithCastTargets(module, AID.DragonsRage, 6, 6);
class Heavensflame(BossModule module) : Components.StandardAOEs(module, AID.HeavensflameAOE, 6);
class Conviction(BossModule module) : Components.CastTowers(module, AID.ConvictionAOE, 3);
class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, AID.HolyChain);
class SerZephirin(BossModule module) : Components.Adds(module, (uint)OID.Zephirin);
class LightOfAscalon(BossModule module) : Components.CastCounter(module, AID.LightOfAscalon); // TODO: show knockback 3 from [-0.8, -16.3]
class UltimateEnd(BossModule module) : Components.CastCounter(module, AID.UltimateEndAOE);
class HeavenswardLeap(BossModule module) : Components.CastCounter(module, AID.HeavenswardLeap);
class PureOfSoul(BossModule module) : Components.CastCounter(module, AID.PureOfSoul);
class AbsoluteConviction(BossModule module) : Components.CastCounter(module, AID.AbsoluteConviction);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 963, NameID = 3632, PlanLevel = 90)]
public class Un5Thordan(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsCircle(21));

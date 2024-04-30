namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class Terrastorm(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TerrastormAOE), new AOEShapeCircle(16));
class LingeringSpark(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LingeringSparkAOE), 5);
class PhasesOfTheBladeFront(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhasesOfTheBlade), new AOEShapeCone(22, 90.Degrees()));
class PhasesOfTheBladeBack(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhasesOfTheBladeBack), new AOEShapeCone(22, 90.Degrees()));
class PhasesOfTheShadowFront(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhasesOfTheShadow), new AOEShapeCone(22, 90.Degrees()));
class PhasesOfTheShadowBack(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.PhasesOfTheShadowBack), new AOEShapeCone(22, 90.Degrees()));
class ArcticAssault(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ArcticAssaultAOE), new AOEShapeRect(15, 7.5f));
class RisingBeacon(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RisingBeaconAOE), new AOEShapeCircle(10));
class RisingRing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RisingRingAOE), new AOEShapeDonut(6, 22));
class BurningShade(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BurningShade), 5);
class ImmolatingShade(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ImmolatingShade), 6, 4);
class VoidBlizzard(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidBlizzard), 6, 4);
class VoidAero(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidAero), 3, 2);
class VoidTornado(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.VoidTornado), 6, 4);

[ConfigDisplay(Order = 0x060, Parent = typeof(EndwalkerConfig))]
public class Ex6GolbezConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 950, NameID = 12365)]
public class Ex6Golbez(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(15));

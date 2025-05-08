namespace BossMod.Endwalker.Extreme.Ex6Golbez;

class Terrastorm(BossModule module) : Components.StandardAOEs(module, AID.TerrastormAOE, new AOEShapeCircle(16));
class LingeringSpark(BossModule module) : Components.StandardAOEs(module, AID.LingeringSparkAOE, 5);
class PhasesOfTheBladeFront(BossModule module) : Components.StandardAOEs(module, AID.PhasesOfTheBlade, new AOEShapeCone(22, 90.Degrees()));
class PhasesOfTheBladeBack(BossModule module) : Components.StandardAOEs(module, AID.PhasesOfTheBladeBack, new AOEShapeCone(22, 90.Degrees()));
class PhasesOfTheShadowFront(BossModule module) : Components.StandardAOEs(module, AID.PhasesOfTheShadow, new AOEShapeCone(22, 90.Degrees()));
class PhasesOfTheShadowBack(BossModule module) : Components.StandardAOEs(module, AID.PhasesOfTheShadowBack, new AOEShapeCone(22, 90.Degrees()));
class ArcticAssault(BossModule module) : Components.StandardAOEs(module, AID.ArcticAssaultAOE, new AOEShapeRect(15, 7.5f));
class RisingBeacon(BossModule module) : Components.StandardAOEs(module, AID.RisingBeaconAOE, new AOEShapeCircle(10));
class RisingRing(BossModule module) : Components.StandardAOEs(module, AID.RisingRingAOE, new AOEShapeDonut(6, 22));
class BurningShade(BossModule module) : Components.SpreadFromCastTargets(module, AID.BurningShade, 5);
class ImmolatingShade(BossModule module) : Components.StackWithCastTargets(module, AID.ImmolatingShade, 6, 4);
class VoidBlizzard(BossModule module) : Components.StackWithCastTargets(module, AID.VoidBlizzard, 6, 4);
class VoidAero(BossModule module) : Components.StackWithCastTargets(module, AID.VoidAero, 3, 2);
class VoidTornado(BossModule module) : Components.StackWithCastTargets(module, AID.VoidTornado, 6, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 950, NameID = 12365, PlanLevel = 90)]
public class Ex6Golbez(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(15));

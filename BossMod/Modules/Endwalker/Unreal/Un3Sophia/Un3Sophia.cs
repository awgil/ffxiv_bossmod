namespace BossMod.Endwalker.Unreal.Un3Sophia;

class ThunderDonut(BossModule module) : Components.StandardAOEs(module, AID.ThunderDonut, new AOEShapeDonut(5, 20));
class ExecuteDonut(BossModule module) : Components.StandardAOEs(module, AID.ExecuteDonut, new AOEShapeDonut(5, 20));
class Aero(BossModule module) : Components.StandardAOEs(module, AID.Aero, new AOEShapeCircle(10));
class ExecuteAero(BossModule module) : Components.StandardAOEs(module, AID.ExecuteAero, new AOEShapeCircle(10));
class ThunderCone(BossModule module) : Components.StandardAOEs(module, AID.ThunderCone, new AOEShapeCone(20, 45.Degrees()));
class ExecuteCone(BossModule module) : Components.StandardAOEs(module, AID.ExecuteCone, new AOEShapeCone(20, 45.Degrees()));
class LightDewShort(BossModule module) : Components.StandardAOEs(module, AID.LightDewShort, new AOEShapeRect(55, 9, 5));
class LightDewLong(BossModule module) : Components.StandardAOEs(module, AID.LightDewLong, new AOEShapeRect(55, 9, 5));
class Onrush(BossModule module) : Components.StandardAOEs(module, AID.Onrush, new AOEShapeRect(55, 8, 5));
class Gnosis(BossModule module) : Components.KnockbackFromCastTarget(module, AID.Gnosis, 25);
class Cintamani(BossModule module) : Components.CastCounter(module, AID.Cintamani); // note: ~4.2s before first cast boss gets model state 5
class QuasarProximity1(BossModule module) : Components.StandardAOEs(module, AID.QuasarProximity1, 15);
class QuasarProximity2(BossModule module) : Components.StandardAOEs(module, AID.QuasarProximity2, 15); // TODO: reconsider distance

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 926, NameID = 5199, PlanLevel = 90)]
public class Un3Sophia(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsRect(20, 15));

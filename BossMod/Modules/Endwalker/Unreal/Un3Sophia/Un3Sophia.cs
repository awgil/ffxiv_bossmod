namespace BossMod.Endwalker.Unreal.Un3Sophia;

class ThunderDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderDonut), new AOEShapeDonut(5, 20));
class ExecuteDonut(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExecuteDonut), new AOEShapeDonut(5, 20));
class Aero(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Aero), new AOEShapeCircle(10));
class ExecuteAero(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExecuteAero), new AOEShapeCircle(10));
class ThunderCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderCone), new AOEShapeCone(20, 45.Degrees()));
class ExecuteCone(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExecuteCone), new AOEShapeCone(20, 45.Degrees()));
class LightDewShort(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightDewShort), new AOEShapeRect(55, 9, 5));
class LightDewLong(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LightDewLong), new AOEShapeRect(55, 9, 5));
class Onrush(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Onrush), new AOEShapeRect(55, 8, 5));
class Gnosis(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.Gnosis), 25);
class Cintamani(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Cintamani)); // note: ~4.2s before first cast boss gets model state 5
class QuasarProximity1(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.QuasarProximity1), 15);
class QuasarProximity2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.QuasarProximity2), 15); // TODO: reconsider distance

[ConfigDisplay(Order = 0x330, Parent = typeof(EndwalkerConfig))]
public class Un3SophiaConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.RemovedUnreal, GroupID = 926, NameID = 5199)]
public class Un3Sophia(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, 0), new ArenaBoundsRect(20, 15));

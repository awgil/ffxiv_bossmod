namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class SkyruinFire(BossModule module) : Components.CastCounter(module, AID.SkyruinFireAOE);
class SkyruinIce(BossModule module) : Components.CastCounter(module, AID.SkyruinIceAOE);
class SkyruinThunder(BossModule module) : Components.CastCounter(module, AID.SkyruinThunderAOE);
class DisasterZoneFire(BossModule module) : Components.CastCounter(module, AID.DisasterZoneFireAOE);
class DisasterZoneIce(BossModule module) : Components.CastCounter(module, AID.DisasterZoneIceAOE);
class DisasterZoneThunder(BossModule module) : Components.CastCounter(module, AID.DisasterZoneThunderAOE);
class Tulidisaster1(BossModule module) : Components.CastCounter(module, AID.TulidisasterAOE1);
class Tulidisaster2(BossModule module) : Components.CastCounter(module, AID.TulidisasterAOE2);
class Tulidisaster3(BossModule module) : Components.CastCounter(module, AID.TulidisasterAOE3);
class IceTalon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.IceTalon, AID.IceTalonAOE, 5.1f, true);
class WrathUnfurled(BossModule module) : Components.CastCounter(module, AID.WrathUnfurledAOE);
class TulidisasterEnrage1(BossModule module) : Components.CastCounter(module, AID.TulidisasterEnrageAOE1);
class TulidisasterEnrage2(BossModule module) : Components.CastCounter(module, AID.TulidisasterEnrageAOE2);
class TulidisasterEnrage3(BossModule module) : Components.CastCounter(module, AID.TulidisasterEnrageAOE3);

// TODO: investigate how exactly are omens drawn for northern cross & susurrant breath
[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 833, NameID = 12854, PlanLevel = 100)]
public class Ex1Valigarmanda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 15));

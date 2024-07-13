namespace BossMod.Dawntrail.Extreme.Ex1Valigarmanda;

class SkyruinFire(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.SkyruinFireAOE));
class SkyruinIce(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.SkyruinIceAOE));
class SkyruinThunder(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.SkyruinThunderAOE));
class DisasterZoneFire(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DisasterZoneFireAOE));
class DisasterZoneIce(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DisasterZoneIceAOE));
class DisasterZoneThunder(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DisasterZoneThunderAOE));
class Tulidisaster1(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TulidisasterAOE1));
class Tulidisaster2(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TulidisasterAOE2));
class Tulidisaster3(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TulidisasterAOE3));
class IceTalon(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(6), (uint)IconID.IceTalon, ActionID.MakeSpell(AID.IceTalonAOE), 5.1f, true);
class WrathUnfurled(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.WrathUnfurledAOE));
class TulidisasterEnrage1(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TulidisasterEnrageAOE1));
class TulidisasterEnrage2(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TulidisasterEnrageAOE2));
class TulidisasterEnrage3(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.TulidisasterEnrageAOE3));

// TODO: investigate how exactly are omens drawn for northern cross & susurrant breath
[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "veyn", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 833, NameID = 12854, PlanLevel = 100)]
public class Ex1Valigarmanda(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsRect(20, 15));

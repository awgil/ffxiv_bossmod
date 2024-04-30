namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class RagingStorm(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.RagingStorm));
class HairFlayUpbraid(BossModule module) : Components.CastStackSpread(module, ActionID.MakeSpell(AID.Upbraid), ActionID.MakeSpell(AID.HairFlay), 3, 10, maxStackSize: 2);
class CurlingIron(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.CurlingIronAOE));
class Catabasis(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.Catabasis));
class VoidAeroTankbuster(BossModule module) : Components.Cleave(module, ActionID.MakeSpell(AID.VoidAeroTankbuster), new AOEShapeCircle(5), originAtTarget: true);
class SecretBreezeCones(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SecretBreezeAOE), new AOEShapeCone(40, 22.5f.Degrees()));
class SecretBreezeProteans(BossModule module) : Components.SimpleProtean(module, ActionID.MakeSpell(AID.SecretBreezeProtean), new AOEShapeCone(40, 22.5f.Degrees()));
class WarningGale(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.WarningGale), new AOEShapeCircle(6));
class WindingGaleCharge(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.WindingGaleCharge), 2);
class BoulderBreak(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.BoulderBreak), 5);
class Boulder(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Boulder), 10);
class BrittleBoulder(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BrittleBoulder), 5);
class TornadoChainInner(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TornadoChainInner), new AOEShapeCircle(11));
class TornadoChainOuter(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TornadoChainOuter), new AOEShapeDonut(11, 20));
class KnuckleDrum(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.KnuckleDrum));
class KnuckleDrumLast(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.KnuckleDrumLast));
class BlowAwayRaidwide(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlowAwayRaidwide));
class BlowAwayPuddle(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlowAwayPuddle), new AOEShapeCircle(6));
class ImpactAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ImpactAOE), new AOEShapeCircle(6));
class ImpactKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ImpactKnockback), 6);
class BlusteryRuler(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlusteryRuler), new AOEShapeCircle(6));
class DryBlowsRaidwide(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.DryBlowsRaidwide));
class DryBlowsPuddle(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.DryBlowsPuddle), 3);
class IronOut(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.IronOutAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 871, NameID = 11398)]
public class Ex4Barbariccia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

namespace BossMod.Endwalker.Extreme.Ex4Barbariccia;

class RagingStorm(BossModule module) : Components.CastCounter(module, AID.RagingStorm);
class HairFlayUpbraid(BossModule module) : Components.CastStackSpread(module, AID.Upbraid, AID.HairFlay, 3, 10, maxStackSize: 2);
class CurlingIron(BossModule module) : Components.CastCounter(module, AID.CurlingIronAOE);
class Catabasis(BossModule module) : Components.CastCounter(module, AID.Catabasis);
class VoidAeroTankbuster(BossModule module) : Components.Cleave(module, AID.VoidAeroTankbuster, new AOEShapeCircle(5), originAtTarget: true);
class SecretBreezeCones(BossModule module) : Components.StandardAOEs(module, AID.SecretBreezeAOE, new AOEShapeCone(40, 22.5f.Degrees()));
class SecretBreezeProteans(BossModule module) : Components.SimpleProtean(module, AID.SecretBreezeProtean, new AOEShapeCone(40, 22.5f.Degrees()));
class WarningGale(BossModule module) : Components.StandardAOEs(module, AID.WarningGale, new AOEShapeCircle(6));
class WindingGaleCharge(BossModule module) : Components.ChargeAOEs(module, AID.WindingGaleCharge, 2);
class BoulderBreak(BossModule module) : Components.CastSharedTankbuster(module, AID.BoulderBreak, 5);
class Boulder(BossModule module) : Components.StandardAOEs(module, AID.Boulder, 10);
class BrittleBoulder(BossModule module) : Components.SpreadFromCastTargets(module, AID.BrittleBoulder, 5);
class TornadoChainInner(BossModule module) : Components.StandardAOEs(module, AID.TornadoChainInner, new AOEShapeCircle(11));
class TornadoChainOuter(BossModule module) : Components.StandardAOEs(module, AID.TornadoChainOuter, new AOEShapeDonut(11, 20));
class KnuckleDrum(BossModule module) : Components.CastCounter(module, AID.KnuckleDrum);
class KnuckleDrumLast(BossModule module) : Components.CastCounter(module, AID.KnuckleDrumLast);
class BlowAwayRaidwide(BossModule module) : Components.CastCounter(module, AID.BlowAwayRaidwide);
class BlowAwayPuddle(BossModule module) : Components.StandardAOEs(module, AID.BlowAwayPuddle, new AOEShapeCircle(6));
class ImpactAOE(BossModule module) : Components.StandardAOEs(module, AID.ImpactAOE, new AOEShapeCircle(6));
class ImpactKnockback(BossModule module) : Components.KnockbackFromCastTarget(module, AID.ImpactKnockback, 6);
class BlusteryRuler(BossModule module) : Components.StandardAOEs(module, AID.BlusteryRuler, new AOEShapeCircle(6));
class DryBlowsRaidwide(BossModule module) : Components.CastCounter(module, AID.DryBlowsRaidwide);
class DryBlowsPuddle(BossModule module) : Components.StandardAOEs(module, AID.DryBlowsPuddle, 3);
class IronOut(BossModule module) : Components.CastCounter(module, AID.IronOutAOE);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 871, NameID = 11398)]
public class Ex4Barbariccia(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

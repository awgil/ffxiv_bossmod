namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class GreatDivide(BossModule module) : Components.CastSharedTankbuster(module, AID.GreatDivide, new AOEShapeRect(60, 3));

class TerrestrialTitans(BossModule module) : Components.StandardAOEs(module, AID.TerrestrialTitans, new AOEShapeCircle(5));
class TitanicPursuit(BossModule module) : Components.RaidwideCast(module, AID.TitanicPursuit);

class Towerfall(BossModule module) : Components.StandardAOEs(module, AID.Towerfall, new AOEShapeRect(30, 5));

class FangedCrossing(BossModule module) : PlayActionAOEs(module, (uint)OID.GleamingFangCrossing, 0x11D1, new AOEShapeCross(21, 3.5f), AID.FangedCrossing, 6.1f);

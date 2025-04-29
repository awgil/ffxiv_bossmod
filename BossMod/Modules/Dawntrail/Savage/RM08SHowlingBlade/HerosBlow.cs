namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

class HerosBlow(BossModule module) : Components.GroupedAOEs(module, [AID.HerosBlow2, AID.HerosBlow1], new AOEShapeCone(40, 90.Degrees()));
class FangedMaw(BossModule module) : Components.StandardAOEs(module, AID.FangedMaw, new AOEShapeCircle(22));
class FangedPerimeter(BossModule module) : Components.StandardAOEs(module, AID.FangedPerimeter, new AOEShapeDonut(15, 30));
class HerosBlowInOut(BossModule module) : Components.CastCounterMulti(module, [AID.FangedMaw, AID.FangedPerimeter]);

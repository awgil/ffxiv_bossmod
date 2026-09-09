namespace BossMod.Dawntrail.Trial.T03Everkeep;

class SmitingCircuitDonut(BossModule module) : Components.StandardAOEs(module, AID.SmitingCircuitDonutAOE, new AOEShapeDonut(10, 30));
class SmitingCircuitCircle(BossModule module) : Components.StandardAOEs(module, AID.SmitingCircuitCircleAOE, new AOEShapeCircle(10));

namespace BossMod.Dawntrail.Trial.T03Everkeep;

class HalfCircuitRect(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitRect, new AOEShapeRect(60, 60));
class HalfCircuitDonut(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitDonut, new AOEShapeDonut(10, 30));
class HalfCircuitCircle(BossModule module) : Components.StandardAOEs(module, AID.HalfCircuitCircle, new AOEShapeCircle(10));

namespace BossMod.Endwalker.Savage.P3SPhoinix;

class HeatOfCondemnation(BossModule module) : Components.TankbusterTether(module, AID.HeatOfCondemnationAOE, (uint)TetherID.HeatOfCondemnation, 6);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 807, NameID = 10720, PlanLevel = 90)]
public class P3S(ModuleArgs init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));

namespace BossMod.Endwalker.Savage.P3SPhoinix;

class HeatOfCondemnation(BossModule module) : Components.TankbusterTether(module, AID.HeatOfCondemnationAOE, (uint)TetherID.HeatOfCondemnation, 6);

[ModuleInfo(GroupType = BossModuleInfo.GroupType.CFC, GroupID = 807, NameID = 10720, PlanLevel = 90)]
public class P3S(ModuleInit init) : BossModule(init, new(100, 100), new ArenaBoundsCircle(20));

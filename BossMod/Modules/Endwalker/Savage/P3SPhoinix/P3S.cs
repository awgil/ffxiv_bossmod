namespace BossMod.Endwalker.Savage.P3SPhoinix;

class HeatOfCondemnation(BossModule module) : Components.TankbusterTether(module, ActionID.MakeSpell(AID.HeatOfCondemnationAOE), (uint)TetherID.HeatOfCondemnation, 6);

[ConfigDisplay(Order = 0x130, Parent = typeof(EndwalkerConfig))]
public class P3SConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 807, NameID = 10720)]
public class P3S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

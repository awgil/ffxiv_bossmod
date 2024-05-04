namespace BossMod.Endwalker.Savage.P2SHippokampos;

class DoubledImpact(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.DoubledImpact), 6);
class SewageEruption(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.SewageEruptionAOE), 6);

[ConfigDisplay(Order = 0x120, Parent = typeof(EndwalkerConfig))]
public class P2SConfig() : CooldownPlanningConfigNode(90);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 811, NameID = 10348)]
public class P2S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));

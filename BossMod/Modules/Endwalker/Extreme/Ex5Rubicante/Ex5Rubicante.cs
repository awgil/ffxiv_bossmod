namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class ShatteringHeatBoss(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.ShatteringHeatBoss), 4);
class BlazingRapture(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.BlazingRaptureAOE));
class InfernoSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.InfernoSpreadAOE), 5);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 924, NameID = 12057, PlanLevel = 90)]
public class Ex5Rubicante(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

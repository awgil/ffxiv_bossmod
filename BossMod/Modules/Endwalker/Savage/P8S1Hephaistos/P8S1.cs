namespace BossMod.Endwalker.Savage.P8S1Hephaistos;

class VolcanicTorches(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TorchFlame), new AOEShapeRect(5, 5, 5));
class AbyssalFires(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.AbyssalFires), 15); // TODO: verify falloff

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 884, NameID = 11399, SortOrder = 1)]
public class P8S1(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));

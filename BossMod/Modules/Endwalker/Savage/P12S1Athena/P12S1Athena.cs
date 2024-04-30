namespace BossMod.Endwalker.Savage.P12S1Athena;

class RayOfLight(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RayOfLight), new AOEShapeRect(60, 5));
class UltimaBlade(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.UltimaBladeAOE));
class Parthenos(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Parthenos), new AOEShapeRect(60, 8, 60));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 943, NameID = 12377, SortOrder = 1)]
public class P12S1Athena(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));

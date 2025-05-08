namespace BossMod.Endwalker.Savage.P6SHegemone;

class UnholyDarkness(BossModule module) : Components.StackWithCastTargets(module, AID.UnholyDarknessAOE, 6);
class DarkDome(BossModule module) : Components.StandardAOEs(module, AID.DarkDomeAOE, 5);
class DarkAshes(BossModule module) : Components.SpreadFromCastTargets(module, AID.DarkAshesAOE, 6);
class DarkSphere(BossModule module) : Components.SpreadFromCastTargets(module, AID.DarkSphereAOE, 10);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 881, NameID = 11381, PlanLevel = 90)]
public class P6S(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsSquare(20));

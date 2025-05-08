namespace BossMod.Dawntrail.Alliance.A11Prishe;

class NullifyingDropkick(BossModule module) : Components.CastSharedTankbuster(module, AID.NullifyingDropkickAOE, 6);
class Holy(BossModule module) : Components.SpreadFromCastTargets(module, AID.HolyAOE, 6);
class Explosion(BossModule module) : Components.StandardAOEs(module, AID.Explosion, new AOEShapeCircle(8));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13351)]
public class A11Prishe(WorldState ws, Actor primary) : BossModule(ws, primary, new(800, 400), new ArenaBoundsSquare(35));

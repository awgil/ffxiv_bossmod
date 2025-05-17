namespace BossMod.Dawntrail.Alliance.A12Fafnir;

class ShudderingEarth(BossModule module) : Components.CastCounter(module, AID.ShudderingEarth);
class SharpSpike(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCircle(4), (uint)IconID.SharpSpike, AID.SharpSpikeAOE, 6.2f, true);
class Darter(BossModule module) : Components.Adds(module, (uint)OID.Darter);
class Venom(BossModule module) : Components.StandardAOEs(module, AID.Venom, new AOEShapeCone(30, 60.Degrees()));
class AbsoluteTerror(BossModule module) : Components.StandardAOEs(module, AID.AbsoluteTerrorAOE, new AOEShapeRect(70, 10));
class WingedTerror(BossModule module) : Components.StandardAOEs(module, AID.WingedTerrorAOE, new AOEShapeRect(70, 12.5f));

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1015, NameID = 13662)]
public class A12Fafnir(WorldState ws, Actor primary) : BossModule(ws, primary, new(-500, 600), new ArenaBoundsCircle(30));

namespace BossMod.Endwalker.Extreme.Ex5Rubicante;

class ArchInferno(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 5, AID.ArchInferno, m => Enumerable.Repeat(m.PrimaryActor, (m.PrimaryActor.CastInfo?.IsSpell(AID.ArchInferno) ?? false) ? 0 : 1), 0);
class InfernoDevilFirst(BossModule module) : Components.StandardAOEs(module, AID.InfernoDevilFirst, new AOEShapeCircle(10));
class InfernoDevilRest(BossModule module) : Components.StandardAOEs(module, AID.InfernoDevilRest, new AOEShapeCircle(10));
class Conflagration(BossModule module) : Components.StandardAOEs(module, AID.Conflagration, new AOEShapeRect(20, 5, 20));
class RadialFlagration(BossModule module) : Components.SimpleProtean(module, AID.RadialFlagrationAOE, new AOEShapeCone(21, 15.Degrees())); // TODO: verify angle
class SpikeOfFlame(BossModule module) : Components.SpreadFromCastTargets(module, AID.SpikeOfFlame, 5);
class FourfoldFlame(BossModule module) : Components.StackWithCastTargets(module, AID.FourfoldFlame, 6, 4, 4);
class TwinfoldFlame(BossModule module) : Components.StackWithCastTargets(module, AID.TwinfoldFlame, 4, 2, 2);

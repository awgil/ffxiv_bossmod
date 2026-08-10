namespace BossMod.Dawntrail.Chaotic.Ch01CloudOfDarkness;

sealed class Flare(BossModule module) : Components.SpreadFromIcon(module, (uint)IconID.Flare, (uint)AID.FlareAOE, 25f, 8.1d);
sealed class StygianShadow(BossModule module) : Components.Adds(module, (uint)OID.StygianShadow);
sealed class Atomos(BossModule module) : Components.Adds(module, (uint)OID.Atomos);
sealed class GhastlyGloomCross(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyGloomCrossAOE, new AOEShapeCross(40f, 15f));
sealed class GhastlyGloomDonut(BossModule module) : Components.SimpleAOEs(module, (uint)AID.GhastlyGloomDonutAOE, new AOEShapeDonut(21f, 40f));
sealed class FloodOfDarknessAdd(BossModule module) : Components.CastInterruptHint(module, (uint)AID.FloodOfDarknessAdd); // TODO: only if add is player's?..
sealed class Excruciate(BossModule module) : Components.BaitAwayCast(module, (uint)AID.Excruciate, 4f, tankbuster: true, damageType: AIHints.PredictedDamageType.Tankbuster);
sealed class LoomingChaos(BossModule module) : Components.CastCounter(module, (uint)AID.LoomingChaosAOE);
sealed class Phaser(BossModule module) : Components.SimpleAOEGroupsByTimewindow(module, [(uint)AID.Phaser], new AOEShapeCone(23f, 30f.Degrees()));
sealed class FeintParticleBeam(BossModule module) : Components.StandardChasingAOEs(module, 3f, (uint)AID.FeintParticleBeamAOEFirst, (uint)AID.FeintParticleBeamAOERest, 2.1f, 0.4d, 18, icon: (uint)IconID.FeintParticleBeam);

// TODO: tankswap hints component for phase1
// TODO: phase 2 teleport zones?
// TODO: grim embrace / curse of darkness prevent turning

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "The Combat Reborn Team (Malediktus, LTS)", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 1010u, NameID = 13624u, PlanLevel = 100)]
public sealed class Ch01CloudOfDarkness(WorldState ws, Actor primary) : BossModule(ws, primary, new(100f, 100f), new ArenaBoundsCircle(40f));

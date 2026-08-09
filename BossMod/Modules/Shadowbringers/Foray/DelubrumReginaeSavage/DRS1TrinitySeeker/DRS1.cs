namespace BossMod.Shadowbringers.Foray.DelubrumReginae.DRS1TrinitySeeker;

sealed class VerdantTempest(BossModule module) : Components.CastCounter(module, (uint)AID.VerdantTempestAOE);
sealed class MercifulBreeze(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MercifulBreeze, new AOEShapeRect(50f, 2.5f));
sealed class MercifulBlooms(BossModule module) : Components.SimpleAOEs(module, (uint)AID.MercifulBlooms, 20f);
sealed class MercifulArc(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(12f, 45f.Degrees()), (uint)IconID.MercifulArc, (uint)AID.MercifulArc);

// TODO: depending on phantom edge, it's either a shared tankbuster cleave or a weird cleave ignoring closest target (?)
abstract class BalefulOnslaught(BossModule module, uint aid) : Components.Cleave(module, aid, new AOEShapeCone(10f, 45f.Degrees())); // TODO: verify angle
sealed class BalefulOnslaught1(BossModule module) : BalefulOnslaught(module, (uint)AID.BalefulOnslaughtAOE1);
sealed class BalefulOnslaught2(BossModule module) : BalefulOnslaught(module, (uint)AID.BalefulOnslaughtAOE2);

sealed class BurningChains(BossModule module) : Components.Chains(module, (uint)TetherID.BurningChains, (uint)AID.ScorchingShackle);

// TODO: it's a line stack, but I don't think there's a way to determine cast target - so everyone should just stack?..
sealed class IronImpact(BossModule module) : Components.CastCounter(module, (uint)AID.IronImpact);
sealed class IronRose(BossModule module) : Components.SimpleAOEs(module, (uint)AID.IronRose, new AOEShapeRect(50f, 4f));

sealed class DeadIron(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50f, 15f.Degrees()), (uint)IconID.DeadIron, (uint)AID.DeadIronAOE, 4.6d);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 761u, NameID = 9834u, PlanLevel = 80)]
public sealed class DRS1TrinitySeeker(WorldState ws, Actor primary) : TrinitySeeker(ws, primary);

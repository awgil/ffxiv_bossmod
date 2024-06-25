namespace BossMod.Stormblood.Alliance.A31Mustadio;

class EnergyBurst(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.EnergyBurst));
class ArmShot(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.ArmShot));
class LegShot(BossModule module) : Components.PersistentVoidzone(module, 3, m => m.Enemies(OID.Actor1eaa60).Where(z => z.EventState != 7));
class LeftHandgonne(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftHandgonne), new AOEShapeCone(30, 105.Degrees()));
class RightHandgonne(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightHandgonne), new AOEShapeCone(30, 105.Degrees()));

class SatelliteBeam(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SatelliteBeam), new AOEShapeRect(30, 15, 15)); // Satellite Beam and Compress can both be shown earleir through tether
class Compress(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Compress), new AOEShapeRect(100, 7.5f));

class BallisticSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.BallisticImpact1), 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7915)] // 7919 
public class A31Mustadio(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, 290), new ArenaBoundsSquare(30, 45.Degrees()));

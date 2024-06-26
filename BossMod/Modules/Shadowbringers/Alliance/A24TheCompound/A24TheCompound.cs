namespace BossMod.Shadowbringers.Alliance.A24TheCompound;

class MechanicalLaceration1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MechanicalLaceration1));
class MechanicalDissection(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MechanicalDissection), new AOEShapeRect(85, 5.5f, 85));
class MechanicalDecapitation(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MechanicalDecapitation), new AOEShapeDonut(8, 43));
class MechanicalContusionGround(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.MechanicalContusionGround), 6);
class MechanicalContusionSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.MechanicalContusionSpread), 6);
class IncongruousSpinAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IncongruousSpinAOE), new AOEShapeRect(80, 75, -5));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 736, NameID = 9646)]
public class A24TheCompound(WorldState ws, Actor primary) : BossModule(ws, primary, new(200, -700), new ArenaBoundsSquare(29.5f));

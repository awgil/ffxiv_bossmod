namespace BossMod.Shadowbringers.Foray.DelubrumReginae.Normal.DRN5TrinityAvowed;

class WrathOfBozja(BossModule module) : Components.CastSharedTankbuster(module, ActionID.MakeSpell(AID.WrathOfBozja), new AOEShapeCone(60, 45.Degrees())); // TODO: verify angle

class ElementalImpact1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElementalImpact1), new AOEShapeCircle(20));
class ElementalImpact2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ElementalImpact2), new AOEShapeCircle(20));
class GleamingArrow(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GleamingArrow), new AOEShapeRect(60, 5));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 760, NameID = 9853)]
public class DRN5TrinityAvowed(WorldState ws, Actor primary) : BossModule(ws, primary, new(-272, -82), new ArenaBoundsSquare(25));

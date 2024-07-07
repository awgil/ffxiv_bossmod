namespace BossMod.RealmReborn.Alliance.A22Glasya;

class Aura(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Aura), 8);
class VileUtterance(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.VileUtterance), new AOEShapeCone(30, 22.5f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 102, NameID = 2815)]
public class A22Glasya(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -200), new ArenaBoundsCircle(35));

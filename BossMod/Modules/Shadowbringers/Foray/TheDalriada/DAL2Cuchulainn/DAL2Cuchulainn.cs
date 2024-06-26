namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL2Cuchulainn;

class PutrifiedSoul1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PutrifiedSoul1));
class PutrifiedSoul2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PutrifiedSoul2));
class MightOfMalice(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.MightOfMalice));
class BurgeoningDread(BossModule module) : Components.StatusDrivenForcedMarch(module, 3, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);
class NecroticBillowAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.NecroticBillowAOE), new AOEShapeCircle(8));
class AmbientPulsationAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AmbientPulsationAOE), new AOEShapeCircle(12));

class FellFlow1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FellFlow1), new AOEShapeCone(50, 60.Degrees()));
[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 32, SortOrder = 3)] //BossNameID = 10004
public class DAL2Cuchulainn(WorldState ws, Actor primary) : BossModule(ws, primary, new(650, -187.5f), new ArenaBoundsCircle(25));

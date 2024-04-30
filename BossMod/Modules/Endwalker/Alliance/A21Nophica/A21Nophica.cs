namespace BossMod.Endwalker.Alliance.A21Nophica;

class FloralHaze(BossModule module) : Components.StatusDrivenForcedMarch(module, 2, (uint)SID.ForwardMarch, (uint)SID.AboutFace, (uint)SID.LeftFace, (uint)SID.RightFace, activationLimit: 8);
class SummerShade(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SummerShade), new AOEShapeDonut(12, 40));
class SpringFlowers(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SpringFlowers), new AOEShapeCircle(12));
class ReapersGale(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ReapersGaleAOE), new AOEShapeRect(36, 4, 36), 9);
class Landwaker(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.LandwakerAOE), 10);
class Furrow(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Furrow), 6, 8);
class HeavensEarth(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.HeavensEarthAOE), new AOEShapeCircle(5), true);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 911, NameID = 12065)]
public class A21Nophica(WorldState ws, Actor primary) : BossModule(ws, primary, new(0, -238), new ArenaBoundsCircle(30));

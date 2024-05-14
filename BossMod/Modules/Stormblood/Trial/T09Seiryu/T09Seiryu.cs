namespace BossMod.Stormblood.Trial.T09Seiryu;

class HundredTonzeSwing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HundredTonzeSwing), new AOEShapeCircle(16));
class CoursingRiverCircleAOE(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.CoursingRiverCircleAOE), 25, kind: Kind.DirForward);
class CoursingRiverRectAOE(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.CoursingRiverRectAOE));
class DragonsWake2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.DragonsWake2));
class FifthElement(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FifthElement));
class FortuneBladeSigil(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.FortuneBladeSigil), new AOEShapeRect(50, 2, 50));

class GreatTyphoon28(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GreatTyphoon28), new AOEShapeDonut(20, 28));
class GreatTyphoon34(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GreatTyphoon34), new AOEShapeDonut(20, 34));
class GreatTyphoon40(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GreatTyphoon40), new AOEShapeDonut(20, 40));

class InfirmSoul(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.InfirmSoul));
class InfirmSoulSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.InfirmSoul), 4);

class Kanabo1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Kanabo1), new AOEShapeCone(45, 30.Degrees()));
class OnmyoSigil2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OnmyoSigil2), new AOEShapeCircle(12));

class SerpentDescending(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.SerpentDescending), 5);

class SerpentEyeSigil2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SerpentEyeSigil2), new AOEShapeDonut(6, 30));

class YamaKagura(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.YamaKagura), new AOEShapeRect(60, 3));

class Handprint3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Handprint3), new AOEShapeCone(20, 90.Degrees()));
class Handprint4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Handprint4), new AOEShapeCone(40, 90.Degrees()));

class ForceOfNature1(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ForceOfNature1), 10, stopAtWall: true, kind: Kind.AwayFromOrigin);
class ForceOfNature2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ForceOfNature2), new AOEShapeCircle(5));

class KanaboTether(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeCone(45, 30.Degrees()), (uint)TetherID.BaitAway, ActionID.MakeSpell(AID.Kanabo1));

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 637, NameID = 7922)]
public class T09Seiryu(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

﻿namespace BossMod.Dawntrail.Savage.RM02SHoneyBLovely;

class StingingSlash(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeCone(50, 45.Degrees()), (uint)IconID.StingingSlash, ActionID.MakeSpell(AID.StingingSlashAOE));
class KillerSting(BossModule module) : Components.IconSharedTankbuster(module, (uint)IconID.KillerSting, ActionID.MakeSpell(AID.KillerStingAOE), 6);
class BlindingLoveBait(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlindingLoveBaitAOE), new AOEShapeRect(50, 4));
class BlindingLoveCharge1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlindingLoveCharge1AOE), new AOEShapeRect(45, 5));
class BlindingLoveCharge2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.BlindingLoveCharge2AOE), new AOEShapeRect(45, 5));
class PoisonStingBait(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.PoisonStingAOE), new AOEShapeCircle(6), true);
class PoisonStingVoidzone(BossModule module) : Components.PersistentVoidzone(module, 6, m => m.Enemies(OID.PoisonStingVoidzone).Where(z => z.EventState != 7));
class BeeSting(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.BeeStingAOE), 6, 4);

[ModuleInfo(BossModuleInfo.Maturity.Verified, GroupType = BossModuleInfo.GroupType.CFC, GroupID = 988, NameID = 12685, PlanLevel = 100)]
public class RM02SHoneyBLovely(WorldState ws, Actor primary) : BossModule(ws, primary, new(100, 100), new ArenaBoundsCircle(20));

namespace BossMod.Endwalker.Alliance.A31Thaliak;

sealed class Katarraktes(BossModule module) : Components.CastCounter(module, (uint)AID.KatarraktesAOE);
sealed class Thlipsis(BossModule module) : Components.StackWithCastTargets(module, (uint)AID.ThlipsisAOE, 6f, 8);
sealed class Hydroptosis(BossModule module) : Components.SpreadFromCastTargets(module, (uint)AID.HydroptosisAOE, 6f);
sealed class Rhyton(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70f, 3f), (uint)IconID.Rhyton, (uint)AID.RhytonAOE, 6f);
sealed class Bank(BossModule module) : Components.SimpleAOEGroups(module, [(uint)AID.LeftBank, (uint)AID.RightBank, (uint)AID.HieroglyphikaLeftBank,
(uint)AID.HieroglyphikaRightBank], new AOEShapeCone(60f, 90f.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962u, NameID = 11298u, SortOrder = 2, PlanLevel = 90)]
public sealed class A31Thaliak(WorldState ws, Actor primary) : BossModule(ws, primary, new(-945f, 945f), new ArenaBoundsSquare(24f));

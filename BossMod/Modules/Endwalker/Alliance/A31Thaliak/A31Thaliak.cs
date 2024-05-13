namespace BossMod.Endwalker.Alliance.A31Thaliak;

class Katarraktes(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.KatarraktesAOE));
class Thlipsis(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ThlipsisAOE), 6, 8);
class Hydroptosis(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HydroptosisAOE), 6);
class Rhyton(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70, 3), (uint)IconID.Rhyton, ActionID.MakeSpell(AID.RhytonAOE), 6);
class LeftBank(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftBank), new AOEShapeCone(60, 90.Degrees()));
class RightBank(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightBank), new AOEShapeCone(60, 90.Degrees()));
class HieroglyphikaLeftBank(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HieroglyphikaLeftBank), new AOEShapeCone(60, 90.Degrees()));
class HieroglyphikaRightBank(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HieroglyphikaRightBank), new AOEShapeCone(60, 90.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11298, SortOrder = 2)]
public class A31Thaliak(WorldState ws, Actor primary) : BossModule(ws, primary, NormalCenter, NormalBounds)
{
    public static readonly WPos NormalCenter = new(-945, 945);
    public static readonly ArenaBoundsSquare NormalBounds = new(24);
    public static readonly WPos TriangleCenter = new(-945, 941.5f);
    public static readonly ArenaBoundsComplex TriangleBounds = new([new TriangleE(TriangleCenter, 24)]);

}

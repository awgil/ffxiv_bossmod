namespace BossMod.Endwalker.Alliance.A31Thaliak;

class Katarraktes(BossModule module) : Components.CastCounter(module, AID.KatarraktesAOE);
class Thlipsis(BossModule module) : Components.StackWithCastTargets(module, AID.ThlipsisAOE, 6, 8);
class Hydroptosis(BossModule module) : Components.SpreadFromCastTargets(module, AID.HydroptosisAOE, 6);
class Rhyton(BossModule module) : Components.BaitAwayIcon(module, new AOEShapeRect(70, 3), (uint)IconID.Rhyton, AID.RhytonAOE, 6);
class LeftBank(BossModule module) : Components.SelfTargetedAOEs(module, AID.LeftBank, new AOEShapeCone(60, 90.Degrees()));
class RightBank(BossModule module) : Components.SelfTargetedAOEs(module, AID.RightBank, new AOEShapeCone(60, 90.Degrees()));
class HieroglyphikaLeftBank(BossModule module) : Components.SelfTargetedAOEs(module, AID.HieroglyphikaLeftBank, new AOEShapeCone(60, 90.Degrees()));
class HieroglyphikaRightBank(BossModule module) : Components.SelfTargetedAOEs(module, AID.HieroglyphikaRightBank, new AOEShapeCone(60, 90.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11298, SortOrder = 2)]
public class A31Thaliak(WorldState ws, Actor primary) : BossModule(ws, primary, new(-945, 945), NormalBounds)
{
    public static readonly ArenaBoundsSquare NormalBounds = new(24);
    public static readonly ArenaBoundsCustom TriBounds = BuildTriBounds();

    private static ArenaBoundsCustom BuildTriBounds()
    {
        // equilateral triangle, apex at true north, base is equal to width => height = w * sqrt(3) / 2 => offset to base = w / 2 * (sqrt(3) - 1)
        var baseOffset = NormalBounds.Radius * 0.732050808f;
        List<WDir> verts = [new(0, -NormalBounds.Radius), new(NormalBounds.Radius, baseOffset), new(-NormalBounds.Radius, baseOffset)];
        return new(NormalBounds.Radius, new(verts));
    }
}

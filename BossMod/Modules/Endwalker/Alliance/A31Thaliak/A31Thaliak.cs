namespace BossMod.Endwalker.Alliance.A31Thaliak;

class Katarraktes(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.KatarraktesAOE), "Raidwide + Bleed");
class Thlipsis(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.ThlipsisStack), 6);
class Hydroptosis(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HydroptosisSpread), 6);

class Rhyton(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(70, 3);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.RhytonBuster)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RhytonHelper)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

class LeftBank(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftBank), new AOEShapeCone(60, 90.Degrees()));
class LeftBank2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftBank2), new AOEShapeCone(60, 90.Degrees()));
class RightBank(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightBank), new AOEShapeCone(60, 90.Degrees()));
class RightBank2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightBank2), new AOEShapeCone(60, 90.Degrees()));

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11298, SortOrder = 2)]
public class A31Thaliak(WorldState ws, Actor primary) : BossModule(ws, primary, NormalBounds)
{
    public static readonly ArenaBoundsSquare NormalBounds = new(new(-945, 945), 24);
    public static readonly ArenaBoundsCustom TriBounds = BuildTriBounds();

    private static ArenaBoundsCustom BuildTriBounds()
    {
        var center = new WPos(-945, 948.5f);
        var hw = NormalBounds.Radius * 0.86602540378443864676372317075294f; // sqrt(3) / 2;
        var hh = NormalBounds.Radius * 0.5f;
        ReadOnlySpan<WPos> verts = [
            center + new WDir(-hw, hh),
            center + new WDir(+hw, hh),
            center + new WDir(0, -NormalBounds.Radius)
        ];
        return new(center, NormalBounds.Radius, new([new(verts)]));
    }
}

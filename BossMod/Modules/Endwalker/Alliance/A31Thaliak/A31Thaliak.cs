namespace BossMod.Endwalker.Alliance.A31Thaliak;

class Katarraktes() : Components.RaidwideCast(ActionID.MakeSpell(AID.KatarraktesAOE), "Raidwide + Bleed");
class Thlipsis() : Components.StackWithCastTargets(ActionID.MakeSpell(AID.ThlipsisStack), 6);
class Hydroptosis() : Components.SpreadFromCastTargets(ActionID.MakeSpell(AID.HydroptosisSpread), 6);
class LeftBank() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.LeftBank), new AOEShapeCone(60, 90.Degrees()));
class LeftBank2() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.LeftBank2), new AOEShapeCone(60, 90.Degrees()));
class RightBank() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.RightBank), new AOEShapeCone(60, 90.Degrees()));
class RightBank2() : Components.SelfTargetedAOEs(ActionID.MakeSpell(AID.RightBank2), new AOEShapeCone(60, 90.Degrees()));

class Rhyton : Components.GenericBaitAway
{
    private static readonly AOEShapeRect _shape = new(70, 3);

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.RhytonBuster)
            CurrentBaits.Add(new(module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.RhytonHelper)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }

    public override void AddGlobalHints(BossModule module, GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "Malediktus, LTS", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11298)]
public class A31Thaliak(WorldState ws, Actor primary) : BossModule(ws, primary, new ArenaBoundsSquare(new(-945, 945), 24));
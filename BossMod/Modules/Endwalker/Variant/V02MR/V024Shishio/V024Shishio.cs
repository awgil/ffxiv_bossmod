namespace BossMod.Endwalker.Variant.V02MR.V024Shishio;

class ThunderOnefold2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderOnefold2), 6);
class NoblePursuit(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.NoblePursuit), 6);
class Enkyo(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.Enkyo));
class ThriceOnRokujo3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThriceOnRokujo3), new AOEShapeRect(60, 7, 60));
class TwiceOnRokujo3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.TwiceOnRokujo3), new AOEShapeRect(60, 7, 60));
class ThunderTwofold2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderTwofold2), 6);
class ThunderThreefold2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderThreefold2), 6);

class OnceOnRokujoAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.OnceOnRokujoAOE), new AOEShapeRect(60, 7, 60));
class LeapingLevin1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeapingLevin1), new AOEShapeCircle(8));
class LeapingLevin2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeapingLevin2), new AOEShapeCircle(12));
class LeapingLevin3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeapingLevin3), new AOEShapeCircle(23));

class SplittingCry(BossModule module) : Components.GenericBaitAway(module)
{
    private static readonly AOEShapeRect _shape = new(60, 7);

    public override void AddGlobalHints(GlobalHints hints)
    {
        if (CurrentBaits.Count > 0)
            hints.Add("Tankbuster cleave");
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        if (iconID == (uint)IconID.TankbusterCleave)
            CurrentBaits.Add(new(Module.PrimaryActor, actor, _shape));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID == AID.SplittingCry)
        {
            ++NumCasts;
            CurrentBaits.Clear();
        }
    }
}

//Route 8
class ThunderVortex(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThunderVortex), new AOEShapeDonut(8, 30));
class UnsagelySpin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.UnsagelySpin), new AOEShapeCircle(6));
class Rush(BossModule module) : Components.ChargeAOEs(module, ActionID.MakeSpell(AID.Rush), 4);
class Vasoconstrictor(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Vasoconstrictor), 5);

//Route 9
class Yoki2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Yoki2), new AOEShapeCircle(6));
class YokiUzu(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.YokiUzu), new AOEShapeCircle(23));
//Route 10
class HauntingThrall(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<Actor> _casters = [];

    private static readonly AOEShapeCone _shape = new(40, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _casters.Take(4).Select(c => new AOEInstance(_shape, c.Position, c.CastInfo!.Rotation, c.CastInfo.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RightSwipe or AID.LeftSwipe)
            _casters.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.RightSwipe or AID.LeftSwipe)
        {
            _casters.Remove(caster);
            ++NumCasts;
        }
    }
}
//Route 11
class ReishoAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ReishoAOE), new AOEShapeCircle(6));

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "CombatReborn Team", GroupType = BossModuleInfo.GroupType.CFC, Category = BossModuleInfo.Category.Criterion, GroupID = 945, NameID = 12428)]
public class V024Shishio(WorldState ws, Actor primary) : BossModule(ws, primary, new(-40, -300), new ArenaBoundsSquare(20));

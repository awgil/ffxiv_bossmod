namespace BossMod.Endwalker.Alliance.A34Eulogia;

class ArenaChanges(BossModule module) : Components.GenericAOEs(module)
{
    public static readonly WPos Center = new(945, -945);
    private static readonly ArenaBoundsSquare squareBounds = new(24);
    private static readonly ArenaBoundsCircle smallerBounds = new(30);
    public static readonly ArenaBoundsCircle BigBounds = new(35);
    private static readonly Circle circle = new(Center, 30);
    private static readonly Square square = new(Center, 24);
    private static readonly AOEShapeCustom transitionSquare = new([circle], [square]);
    private static readonly AOEShapeDonut transitionSmallerBounds = new(30, 35);
    private AOEInstance? _aoe;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => Utils.ZeroOrOne(_aoe);

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x1B)
        {
            if (state == 0x00080004)
                Module.Arena.Bounds = BigBounds;
            if (state == 0x00100001)
                Module.Arena.Bounds = smallerBounds;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hieroglyphika)
            _aoe = new(transitionSquare, Center, default, spell.NPCFinishAt);
        if ((AID)spell.Action.ID == AID.Whorl)
            _aoe = new(transitionSmallerBounds, Center, default, spell.NPCFinishAt);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.Hieroglyphika)
        {
            Module.Arena.Bounds = squareBounds;
            _aoe = null;
        }
        if ((AID)spell.Action.ID == AID.Whorl)
        {
            Module.Arena.Bounds = smallerBounds;
            _aoe = null;
        }
    }
}

class Sunbeam(BossModule module) : Components.BaitAwayCast(module, ActionID.MakeSpell(AID.SunbeamAOE), new AOEShapeCircle(6), true);
class DestructiveBolt(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.DestructiveBoltAOE), 6, 8);
class HandOfTheDestroyerWrath(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HandOfTheDestroyerWrathAOE), new AOEShapeRect(90, 20));
class HandOfTheDestroyerJudgment(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.HandOfTheDestroyerJudgmentAOE), new AOEShapeRect(90, 20));
class SoaringMinuet(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SoaringMinuet), new AOEShapeCone(40, 135.Degrees()));
class EudaimonEorzea(BossModule module) : Components.CastCounter(module, ActionID.MakeSpell(AID.EudaimonEorzeaAOE));

[ModuleInfo(BossModuleInfo.Maturity.Verified, Contributors = "Malediktus, LTS, veyn", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 962, NameID = 11301, SortOrder = 7)]
public class A34Eulogia(WorldState ws, Actor primary) : BossModule(ws, primary, ArenaChanges.Center, ArenaChanges.BigBounds);

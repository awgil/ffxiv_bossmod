namespace BossMod.Endwalker.Variant.V02MR.V025Enenra;

class PipeCleaner(BossModule module) : Components.BaitAwayTethers(module, new AOEShapeRect(60, 5), (uint)TetherID.PipeCleaner);
class Uplift(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Uplift), 6);
class Snuff(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Snuff), 6);
class Smoldering(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Smoldering), new AOEShapeCircle(8));
class IntoTheFireAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.IntoTheFireAOE), new AOEShapeRect(50, 25));
class FlagrantCombustion(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlagrantCombustion));
class SmokeRingsAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SmokeRingsAOE), new AOEShapeCircle(16));
class ClearingSmokeKB(BossModule module) : Components.KnockbackFromCastTarget(module, ActionID.MakeSpell(AID.ClearingSmokeKB), 16, stopAtWall: true);
class KiseruClamor(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.KiseruClamor), 6);

class StringRock(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.KiseruClamor))
{
    private readonly List<Actor> _castersRockFirst = [];
    private readonly List<Actor> _castersRockSecond = [];
    private readonly List<Actor> _castersRockThird = [];

    private static readonly AOEShape _shapeRockFirst = new AOEShapeDonut(6, 12);
    private static readonly AOEShape _shapeRockSecond = new AOEShapeDonut(12, 18);
    private static readonly AOEShape _shapeRockThird = new AOEShapeDonut(18, 24);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersRockFirst.Count > 0)
            return _castersRockFirst.Select(c => new AOEInstance(_shapeRockFirst, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
        else if (_castersRockSecond.Count > 0)
            return _castersRockSecond.Select(c => new AOEInstance(_shapeRockSecond, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
        else
            return _castersRockThird.Select(c => new AOEInstance(_shapeRockThird, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Add(caster);
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        CastersForSpell(spell.Action)?.Remove(caster);
    }

    private List<Actor>? CastersForSpell(ActionID spell) => (AID)spell.ID switch
    {
        AID.BedrockUplift1 => _castersRockFirst,
        AID.BedrockUplift2 => _castersRockSecond,
        AID.BedrockUplift3 => _castersRockThird,
        _ => null
    };
}

[ModuleInfo(BossModuleInfo.Maturity.Contributed, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, Category = BossModuleInfo.Category.Criterion, GroupID = 945, NameID = 12393)]
public class V025Enenra(WorldState ws, Actor primary) : BossModule(ws, primary, new(900, -900), new ArenaBoundsCircle(20));

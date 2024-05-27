namespace BossMod.Stormblood.Alliance.A34UltimaP1;

class HolyIVBait(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HolyIVBait), 6);
class HolyIVSpread(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.HolyIVSpread), 6);
class AuralightAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AuralightAOE), new AOEShapeCircle(20));
class AuralightRect(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.AuralightRect), new AOEShapeRect(70, 5));
class GrandCrossAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GrandCrossAOE), new AOEShapeCross(60, 7.5f));

class TimeEruption(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.TimeEruptionVisual))
{
    private readonly List<Actor> _castersEruptionAOEFirst = [];
    private readonly List<Actor> _castersEruptionAOESecond = [];

    private static readonly AOEShape _shapeEruptionAOEFirst = new AOEShapeRect(10, 10, 10);
    private static readonly AOEShape _shapeEruptionAOESecond = new AOEShapeRect(10, 10, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersEruptionAOEFirst.Count > 0)
            return _castersEruptionAOEFirst.Select(c => new AOEInstance(_shapeEruptionAOEFirst, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
        else
            return _castersEruptionAOESecond.Select(c => new AOEInstance(_shapeEruptionAOESecond, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
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
        AID.TimeEruptionAOEFirst => _castersEruptionAOEFirst,
        AID.TimeEruptionAOESecond => _castersEruptionAOESecond,
        _ => null
    };
}
class Eruption2(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Eruption2), 8);
class ControlTower2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ControlTower2), new AOEShapeCircle(6));
class ExtremeEdge1(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExtremeEdge1), new AOEShapeRect(60, 18));
class ExtremeEdge2(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ExtremeEdge2), new AOEShapeRect(60, 18));

class CrushWeapon(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.CrushWeapon), 6);
class Searchlight(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.Searchlight), 6);
class HallowedBolt(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.HallowedBolt), 6);

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.CFC, GroupID = 636, NameID = 7909)]
public class A34UltimaP1(WorldState ws, Actor primary) : BossModule(ws, primary, new(600, -600), new ArenaBoundsSquare(30));

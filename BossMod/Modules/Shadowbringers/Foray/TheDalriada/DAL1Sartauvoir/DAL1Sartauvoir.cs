namespace BossMod.Shadowbringers.Foray.TheDalriada.DAL1Sartauvoir;

class PyrokinesisAOE(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.PyrokinesisAOE));

class Flamedive(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Flamedive), new AOEShapeRect(55, 2.5f, 55));
class BurningBlade(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.BurningBlade));

class MannatheihwonFlame2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.MannatheihwonFlame2));
class MannatheihwonFlame3(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MannatheihwonFlame3), new AOEShapeRect(50, 4));
class MannatheihwonFlame4(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.MannatheihwonFlame4), new AOEShapeCircle(10));

class LeftBrand(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.LeftBrand), new AOEShapeCone(40, 90.Degrees()));
class RightBrand(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RightBrand), new AOEShapeCone(40, 90.Degrees()));

class Pyrocrisis(BossModule module) : Components.SpreadFromCastTargets(module, ActionID.MakeSpell(AID.Pyrocrisis), 6);
class Pyrodoxy(BossModule module) : Components.StackWithCastTargets(module, ActionID.MakeSpell(AID.Pyrodoxy), 6, 8);

class ThermalGustAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.ThermalGustAOE), new AOEShapeRect(44, 5));
class GrandCrossflameAOE(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GrandCrossflameAOE), new AOEShapeRect(40, 9));

class ReverseTimeEruption(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.ReverseTimeEruptionVisual))
{
    private readonly List<Actor> _castersEruptionAOEFirst = [];
    private readonly List<Actor> _castersEruptionAOESecond = [];

    private static readonly AOEShape _shapeEruptionAOEFirst = new AOEShapeRect(10, 10, 10);
    private static readonly AOEShape _shapeEruptionAOESecond = new AOEShapeRect(10, 10, 10);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersEruptionAOEFirst.Count > 0
            ? _castersEruptionAOEFirst.Select(c => new AOEInstance(_shapeEruptionAOEFirst, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersEruptionAOESecond.Select(c => new AOEInstance(_shapeEruptionAOESecond, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
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
        AID.ReverseTimeEruptionAOEFirst => _castersEruptionAOEFirst,
        AID.ReverseTimeEruptionAOESecond => _castersEruptionAOESecond,
        _ => null
    };
}

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
[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "The Combat Reborn Team", GroupType = BossModuleInfo.GroupType.BozjaCE, GroupID = 778, NameID = 32, SortOrder = 2)] //BossNameID = 9384
public class DAL1Sartauvoir : BossModule
{
    public readonly IReadOnlyList<Actor> Boss;
    public readonly IReadOnlyList<Actor> BossP2;

    public DAL1Sartauvoir(WorldState ws, Actor primary) : base(ws, primary, new(631, 157), new ArenaBoundsSquare(20))
    {
        Boss = Enemies(OID.Boss);
        BossP2 = Enemies(OID.BossP2);
    }

    protected override void DrawEnemies(int pcSlot, Actor pc)
    {
        Arena.Actors(Boss, ArenaColor.Enemy);
        Arena.Actors(BossP2, ArenaColor.Enemy);
        Arena.Actor(PrimaryActor, ArenaColor.Enemy);
    }
}

namespace BossMod.Endwalker.Variant.V01SS.V013Gladiator;

class SunderedRemains(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SunderedRemains), new AOEShapeCircle(10));
class Landing(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.Landing), new AOEShapeCircle(20));

class GoldenFlame(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.GoldenFlame), new AOEShapeRect(60, 5));
class SculptorsPassion(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.SculptorsPassion), new AOEShapeRect(60, 4));
class RackAndRuin(BossModule module) : Components.SelfTargetedAOEs(module, ActionID.MakeSpell(AID.RackAndRuin), new AOEShapeRect(40, 2.5f), 8);

class MightySmite(BossModule module) : Components.SingleTargetCast(module, ActionID.MakeSpell(AID.MightySmite));

class BitingWindBad(BossModule module) : Components.LocationTargetedAOEs(module, ActionID.MakeSpell(AID.BitingWindBad), 4);

class ShatteringSteel(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.ShatteringSteel), "Get in bigger Whirlwind to dodge");
class ViperPoisonPatterns(BossModule module) : Components.PersistentVoidzoneAtCastTarget(module, 6, ActionID.MakeSpell(AID.BitingWindBad), m => m.Enemies(OID.WhirlwindBad).Where(z => z.EventState != 7), 0);

class RingOfMight1(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.RingOfMightVisual))
{
    private readonly List<Actor> _castersRingOfMightOut = [];
    private readonly List<Actor> _castersRingOfMightIn = [];

    private static readonly AOEShape _shapeRingOfMightOut = new AOEShapeCircle(8);
    private static readonly AOEShape _shapeRingOfMightIn = new AOEShapeDonut(8, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersRingOfMightOut.Count > 0)
            return _castersRingOfMightOut.Select(c => new AOEInstance(_shapeRingOfMightOut, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
        else
            return _castersRingOfMightIn.Select(c => new AOEInstance(_shapeRingOfMightIn, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
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
        AID.RingOfMight1Out => _castersRingOfMightOut,
        AID.RingOfMight1In => _castersRingOfMightIn,
        _ => null
    };
}

class RingOfMight2(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.RingOfMightVisual))
{
    private readonly List<Actor> _castersRingOfMightOut = [];
    private readonly List<Actor> _castersRingOfMightIn = [];

    private static readonly AOEShape _shapeRingOfMightOut = new AOEShapeCircle(13);
    private static readonly AOEShape _shapeRingOfMightIn = new AOEShapeDonut(13, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_castersRingOfMightOut.Count > 0)
            return _castersRingOfMightOut.Select(c => new AOEInstance(_shapeRingOfMightOut, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
        else
            return _castersRingOfMightIn.Select(c => new AOEInstance(_shapeRingOfMightIn, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
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
        AID.RingOfMight2Out => _castersRingOfMightOut,
        AID.RingOfMight2In => _castersRingOfMightIn,
        _ => null
    };
}

class RingOfMight3(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.RingOfMightVisual))
{
    private readonly List<Actor> _castersRingOfMightOut = [];
    private readonly List<Actor> _castersRingOfMightIn = [];

    private static readonly AOEShape _shapeRingOfMightOut = new AOEShapeCircle(18);
    private static readonly AOEShape _shapeRingOfMightIn = new AOEShapeDonut(18, 30);

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersRingOfMightOut.Count > 0
            ? _castersRingOfMightOut.Select(c => new AOEInstance(_shapeRingOfMightOut, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersRingOfMightIn.Select(c => new AOEInstance(_shapeRingOfMightIn, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
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
        AID.RingOfMight3Out => _castersRingOfMightOut,
        AID.RingOfMight3In => _castersRingOfMightIn,
        _ => null
    };
}

class RushOfMight(BossModule module) : Components.GenericAOEs(module, ActionID.MakeSpell(AID.RingOfMightVisual))
{
    private readonly List<Actor> _castersRushOfMightFront = [];
    private readonly List<Actor> _castersRushOfMightBack = [];

    private static readonly AOEShape _shapeRushOfMightFront = new AOEShapeCone(60, 90.Degrees());
    private static readonly AOEShape _shapeRushOfMightBack = new AOEShapeCone(60, 90.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        return _castersRushOfMightFront.Count > 0
            ? _castersRushOfMightFront.Select(c => new AOEInstance(_shapeRushOfMightFront, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt))
            : _castersRushOfMightBack.Select(c => new AOEInstance(_shapeRushOfMightBack, c.Position, c.CastInfo!.Rotation, c.CastInfo!.NPCFinishAt));
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
        AID.RushOfMightFront => _castersRushOfMightFront,
        AID.RushOfMightBack => _castersRushOfMightBack,
        _ => null
    };
}

class FlashOfSteel1(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlashOfSteel1), "Raidwide");
class FlashOfSteel2(BossModule module) : Components.RaidwideCast(module, ActionID.MakeSpell(AID.FlashOfSteel2), "Raidwide");
class ShatteringSteelMeteor(BossModule module) : Components.CastLineOfSightAOE(module, ActionID.MakeSpell(AID.ShatteringSteel), 60, false)
{
    public override IEnumerable<Actor> BlockerActors() => Module.Enemies(OID.AntiqueBoulder).Where(a => !a.IsDead);
}

class SilverFlame1(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _source;
    private Angle _startingRotation;
    private Angle _increment;
    private DateTime _startingActivation;

    private static readonly AOEShapeRect _shape = new(60, 4);
    private static readonly int _maxCasts = 8;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source == null)
            yield break;

        for (var i = NumCasts + 1; i < _maxCasts; ++i)
            yield return new(_shape, _source.Position, _startingRotation + i * _increment, _startingActivation.AddSeconds(0.5f * i));
        if (NumCasts < _maxCasts)
            yield return new(_shape, _source.Position, _startingRotation + NumCasts * _increment, _startingActivation.AddSeconds(0.5f * NumCasts), ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SilverFlameFirst1)
        {
            _source = caster;
            _startingRotation = spell.Rotation;
            _increment = _startingRotation.Rad > 0 ? 7.Degrees() : -7.Degrees();
            _startingActivation = spell.NPCFinishAt;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SilverFlameFirst1 or AID.SilverFlameRest)
            ++NumCasts;
    }
}

class SilverFlame2(BossModule module) : Components.GenericAOEs(module)
{
    private Actor? _source;
    private Angle _startingRotation;
    private Angle _increment;
    private DateTime _startingActivation;

    private static readonly AOEShapeRect _shape = new(60, 4);
    private static readonly int _maxCasts = 8;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_source == null)
            yield break;

        for (var i = NumCasts + 1; i < _maxCasts; ++i)
            yield return new(_shape, _source.Position, _startingRotation + i * _increment, _startingActivation.AddSeconds(0.5f * i));
        if (NumCasts < _maxCasts)
            yield return new(_shape, _source.Position, _startingRotation + NumCasts * _increment, _startingActivation.AddSeconds(0.5f * NumCasts), ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SilverFlameFirst2)
        {
            _source = caster;
            _startingRotation = spell.Rotation;
            _increment = _startingRotation.Rad > 0 ? 7.Degrees() : -7.Degrees();
            _startingActivation = spell.NPCFinishAt;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if ((AID)spell.Action.ID is AID.SilverFlameFirst2 or AID.SilverFlameRest)
            ++NumCasts;
    }
}

[ModuleInfo(BossModuleInfo.Maturity.WIP, Contributors = "CombatReborn Team", PrimaryActorOID = (uint)OID.Boss, GroupType = BossModuleInfo.GroupType.CFC, Category = BossModuleInfo.Category.Criterion, GroupID = 868, NameID = 11387)]
public class V013Gladiator(WorldState ws, Actor primary) : BossModule(ws, primary, new(-35, -271), new ArenaBoundsSquare(20));

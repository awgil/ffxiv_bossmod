namespace BossMod.RealmReborn.Extreme.Ex1Ultima;

// note that it could be a GenericAOEs, but we customize everything anyway...
class EyeOfTheStormGeocrush(BossModule module) : BossComponent(module)
{
    private Actor? _eotsCaster;
    private Actor? _geocrushCaster;
    public bool Active => _eotsCaster != null || _geocrushCaster != null;

    private static readonly AOEShapeDonut _aoeEOTS = new(12, 25);
    private static readonly AOEShapeCircle _aoeGeocrush = new(18); // TODO: check falloff

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_eotsCaster != null)
            hints.Add("Stand near inner edge", _aoeEOTS.Check(actor.Position, _eotsCaster));
        else if (_aoeGeocrush.Check(actor.Position, _geocrushCaster))
            hints.Add("Go to edge!");
    }

    public override void AddAIHints(int slot, Actor actor, PartyRolesConfig.Assignment assignment, AIHints hints)
    {
        if (_eotsCaster != null)
        {
            // we want to stand in a small ring near inner edge of aoe
            var inner = ShapeContains.Circle(_eotsCaster.Position, _aoeEOTS.InnerRadius - 2);
            var outer = ShapeContains.InvertedCircle(_eotsCaster.Position, _aoeEOTS.InnerRadius);
            hints.AddForbiddenZone(p => inner(p) || outer(p), Module.CastFinishAt(_eotsCaster.CastInfo!));
        }
        else if (_geocrushCaster != null)
        {
            hints.AddForbiddenZone(_aoeGeocrush, _geocrushCaster.Position, new(), Module.CastFinishAt(_geocrushCaster.CastInfo!));
        }
    }

    public override void DrawArenaBackground(int pcSlot, Actor pc)
    {
        if (_eotsCaster != null)
            _aoeEOTS.Draw(Arena, _eotsCaster);
        else if (_geocrushCaster != null)
            _aoeGeocrush.Draw(Arena, _geocrushCaster);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EyeOfTheStorm:
                _eotsCaster = caster;
                break;
            case AID.Geocrush:
                _geocrushCaster = caster;
                break;
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.EyeOfTheStorm:
                _eotsCaster = null;
                break;
            case AID.Geocrush:
                _geocrushCaster = null;
                break;
        }
    }
}

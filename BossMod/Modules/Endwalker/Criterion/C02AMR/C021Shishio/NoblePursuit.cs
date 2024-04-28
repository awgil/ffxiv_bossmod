namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class NoblePursuit(BossModule module) : Components.GenericAOEs(module)
{
    private WPos _posAfterLastCharge;
    private readonly List<AOEInstance> _charges = [];
    private readonly List<AOEInstance> _rings = [];

    private const float _chargeHalfWidth = 6;
    private static readonly AOEShapeRect _shapeRing = new(5, 50, 5);

    public bool Active => _charges.Count + _rings.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var firstActivation = _charges.Count > 0 ? _charges[0].Activation : _rings.Count > 0 ? _rings[0].Activation : default;
        var deadline = firstActivation.AddSeconds(2.5f);
        foreach (var aoe in _charges.Concat(_rings).Where(aoe => aoe.Activation <= deadline))
            yield return aoe;
    }

    public override void OnActorCreated(Actor actor)
    {
        if ((OID)actor.OID is OID.NRairin or OID.SRairin)
        {
            if (_charges.Count == 0)
            {
                ReportError("Ring appeared while no charges are in progress");
                return;
            }

            // see whether this ring shows next charge
            if (!_charges[^1].Check(actor.Position))
            {
                var nextDir = actor.Position - _posAfterLastCharge;
                if (Math.Abs(nextDir.X) < 0.1)
                    nextDir.X = 0;
                if (Math.Abs(nextDir.Z) < 0.1)
                    nextDir.Z = 0;
                nextDir = nextDir.Normalized();
                var ts = Module.Center + nextDir.Sign() * Module.Bounds.Radius - _posAfterLastCharge;
                var t = Math.Min(nextDir.X != 0 ? ts.X / nextDir.X : float.MaxValue, nextDir.Z != 0 ? ts.Z / nextDir.Z : float.MaxValue);
                _charges.Add(new(new AOEShapeRect(t, _chargeHalfWidth), _posAfterLastCharge, Angle.FromDirection(nextDir), _charges[^1].Activation.AddSeconds(1.4f)));
                _posAfterLastCharge += nextDir * t;
            }

            // ensure ring rotations are expected
            if (!_charges[^1].Rotation.AlmostEqual(actor.Rotation, 0.1f))
            {
                ReportError("Unexpected rotation for ring inside last pending charge");
            }

            _rings.Add(new(_shapeRing, actor.Position, actor.Rotation, _charges[^1].Activation.AddSeconds(0.8f)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID is AID.NNoblePursuitFirst or AID.SNoblePursuitFirst)
        {
            var dir = spell.LocXZ - caster.Position;
            _charges.Add(new(new AOEShapeRect(dir.Length(), _chargeHalfWidth), caster.Position, Angle.FromDirection(dir), spell.NPCFinishAt));
            _posAfterLastCharge = spell.LocXZ;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NNoblePursuitFirst:
            case AID.NNoblePursuitRest:
            case AID.SNoblePursuitFirst:
            case AID.SNoblePursuitRest:
                if (_charges.Count > 0)
                    _charges.RemoveAt(0);
                ++NumCasts;
                break;
            case AID.NLevinburst:
            case AID.SLevinburst:
                _rings.RemoveAll(r => r.Origin.AlmostEqual(caster.Position, 1));
                break;
        }
    }
}

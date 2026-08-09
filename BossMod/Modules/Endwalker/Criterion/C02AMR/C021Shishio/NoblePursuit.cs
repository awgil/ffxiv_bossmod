namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

sealed class NoblePursuit(BossModule module) : Components.GenericAOEs(module)
{
    private WPos _posAfterLastCharge;
    private readonly List<AOEInstance> _charges = [];
    private readonly List<AOEInstance> _rings = [];

    private const float _chargeHalfWidth = 6f;
    private readonly AOEShapeRect _shapeRing = new(5f, 50f, 5f);

    public bool Active => _charges.Count + _rings.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countCharges = _charges.Count;
        var countRings = _rings.Count;
        var total = countCharges + countRings;
        if (total == 0)
            return [];

        var firstActivation = countCharges > 0 ? _charges[0].Activation : countRings > 0 ? _rings[0].Activation : default;
        var deadline = firstActivation.AddSeconds(2.5d);

        var aoes = new AOEInstance[total];
        var index = 0;
        var i = 0;

        while (i < countCharges)
        {
            if (_charges[i].Activation <= deadline)
                aoes[index++] = _charges[i];
            ++i;
        }

        i = 0;
        while (i < countRings)
        {
            if (_rings[i].Activation <= deadline)
                aoes[index++] = _rings[i];
            ++i;
        }

        return aoes.AsSpan()[..index];
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID is (uint)OID.NRairin or (uint)OID.SRairin)
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
                var nextDirX = nextDir.X;
                var nextDirZ = nextDir.Z;
                if (Math.Abs(nextDirX) < 0.1f)
                {
                    nextDirX = 0f;
                }
                if (Math.Abs(nextDirZ) < 0.1f)
                {
                    nextDirZ = 0f;
                }
                nextDir = new WDir(nextDirX, nextDirZ).Normalized();
                var ts = Arena.Center + nextDir.Sign() * Arena.Bounds.Radius - _posAfterLastCharge;
                var t = Math.Min(nextDir.X != 0f ? ts.X / nextDir.X : float.MaxValue, nextDir.Z != 0f ? ts.Z / nextDir.Z : float.MaxValue);
                _charges.Add(new(new AOEShapeRect(t, _chargeHalfWidth), _posAfterLastCharge, Angle.FromDirection(nextDir), _charges[^1].Activation.AddSeconds(1.4d)));
                _posAfterLastCharge += nextDir * t;
            }

            // ensure ring rotations are expected
            if (!_charges[^1].Rotation.AlmostEqual(actor.Rotation, 0.1f))
            {
                ReportError("Unexpected rotation for ring inside last pending charge");
            }

            _rings.Add(new(_shapeRing, actor.Position, actor.Rotation, _charges[^1].Activation.AddSeconds(0.8d)));
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.NNoblePursuitFirst or (uint)AID.SNoblePursuitFirst)
        {
            var dir = spell.LocXZ - caster.Position;
            _charges.Add(new(new AOEShapeRect(dir.Length(), _chargeHalfWidth), caster.Position, Angle.FromDirection(dir), Module.CastFinishAt(spell)));
            _posAfterLastCharge = spell.LocXZ;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NNoblePursuitFirst:
            case (uint)AID.NNoblePursuitRest:
            case (uint)AID.SNoblePursuitFirst:
            case (uint)AID.SNoblePursuitRest:
                if (_charges.Count != 0)
                    _charges.RemoveAt(0);
                ++NumCasts;
                break;
            case (uint)AID.NLevinburst:
            case (uint)AID.SLevinburst:
                _rings.RemoveAll(r => r.Origin.AlmostEqual(caster.Position, 1f));
                break;
        }
    }
}

namespace BossMod.Endwalker.VariantCriterion.C02AMR.C021Shishio;

sealed class RokujoRevel(BossModule module) : Components.GenericAOEs(module)
{
    private int _numBreaths;
    private readonly List<Actor> _clouds = [.. module.Enemies((uint)OID.NRaiun), .. module.Enemies((uint)OID.SRaiun)];
    private readonly List<(Angle dir, DateTime activation)> _pendingLines = [];
    private readonly List<(WPos origin, DateTime activation)> _pendingCircles = [];

    private readonly AOEShapeRect _shapeLine = new(30f, 7f, 30f);
    private readonly AOEShapeCircle[] _shapesCircle = [new(8f), new(12f), new(23f)];

    private AOEShapeCircle? ShapeCircle => _numBreaths is > 0 and <= 3 ? _shapesCircle[_numBreaths - 1] : null;

    public bool Active => _pendingLines.Count + _pendingCircles.Count > 0;

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var countLines = _pendingLines.Count;
        var countCircles = _pendingCircles.Count;
        var total = countCircles + countLines;
        if (total == 0)
            return [];
        List<AOEInstance> aoes = [with(total)];
        var color = Colors.Danger;

        if (countLines > 1)
        {
            aoes.Add(new(_shapeLine, Arena.Center, _pendingLines[1].dir, _pendingLines[1].activation, risky: false));
        }

        if (countCircles != 0 && ShapeCircle is var shapeCircle && shapeCircle != null)
        {
            var firstFutureActivation = _pendingCircles[0].activation.AddSeconds(1d);
            var firstFutureIndex = -1;

            for (var i = 0; i < countCircles; ++i)
            {
                if (_pendingCircles[i].activation >= firstFutureActivation)
                {
                    firstFutureIndex = i;
                    break;
                }
            }

            if (firstFutureIndex >= 0)
            {
                var lastFutureActivation = _pendingCircles[firstFutureIndex].activation.AddSeconds(1.5d);

                for (var i = firstFutureIndex; i < countCircles; ++i)
                {
                    var p = _pendingCircles[i];
                    if (p.activation > lastFutureActivation)
                        break;
                    aoes.Add(new(shapeCircle, p.origin, default, p.activation, risky: false));
                }
            }
            else
            {
                firstFutureIndex = countCircles;
            }

            for (var i = 0; i < firstFutureIndex; ++i)
            {
                var p = _pendingCircles[i];
                aoes.Add(new(shapeCircle, p.origin, default, p.activation, color));
            }
        }

        if (countLines != 0)
        {
            aoes.Add(new(_shapeLine, Arena.Center, _pendingLines[0].dir, _pendingLines[0].activation, color));
        }
        return CollectionsMarshal.AsSpan(aoes);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NRokujoRevelAOE:
            case (uint)AID.SRokujoRevelAOE:
                _pendingLines.Add((spell.Rotation, Module.CastFinishAt(spell)));
                AddHitClouds(_clouds.InShape(_shapeLine, caster.Position, spell.Rotation), Module.CastFinishAt(spell), ShapeCircle?.Radius ?? 0f);
                _pendingCircles.Sort(static (a, b) => a.activation.CompareTo(b.activation));
                break;
            case (uint)AID.NLeapingLevin1:
            case (uint)AID.NLeapingLevin2:
            case (uint)AID.NLeapingLevin3:
            case (uint)AID.SLeapingLevin1:
            case (uint)AID.SLeapingLevin2:
            case (uint)AID.SLeapingLevin3:
                var index = _pendingCircles.FindIndex(p => p.origin.AlmostEqual(caster.Position, 1f));
                if (index < 0)
                {
                    ReportError($"Failed to predict levin from {caster.InstanceID:X}");
                    _pendingCircles.Add((caster.Position, Module.CastFinishAt(spell)));
                }
                else if (Math.Abs((_pendingCircles[index].activation - Module.CastFinishAt(spell)).TotalSeconds) > 1d)
                {
                    ReportError($"Mispredicted levin from {caster.InstanceID:X} by {(_pendingCircles[index].activation - Module.CastFinishAt(spell)).TotalSeconds}");
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.NSmokeaterFirst:
            case (uint)AID.NSmokeaterRest:
            case (uint)AID.SSmokeaterFirst:
            case (uint)AID.SSmokeaterRest:
                ++_numBreaths;
                break;
            case (uint)AID.NSmokeaterAbsorb:
            case (uint)AID.SSmokeaterAbsorb:
                _clouds.Remove(caster);
                break;
            case (uint)AID.NRokujoRevelAOE:
            case (uint)AID.SRokujoRevelAOE:
                if (_pendingLines.Count != 0)
                    _pendingLines.RemoveAt(0);
                ++NumCasts;
                break;
            case (uint)AID.NLeapingLevin1:
            case (uint)AID.NLeapingLevin2:
            case (uint)AID.NLeapingLevin3:
            case (uint)AID.SLeapingLevin1:
            case (uint)AID.SLeapingLevin2:
            case (uint)AID.SLeapingLevin3:
                _pendingCircles.RemoveAll(p => p.origin.AlmostEqual(caster.Position, 1f));
                ++NumCasts;
                break;
        }
    }

    private void AddHitClouds(IEnumerable<Actor> clouds, DateTime timeHit, float radius)
    {
        var explodeTime = timeHit.AddSeconds(1.1d);
        foreach (var c in clouds)
        {
            var existing = _pendingCircles.FindIndex(p => p.origin.AlmostEqual(c.Position, 1f));
            if (existing >= 0 && _pendingCircles[existing].activation <= explodeTime)
                continue; // this cloud is already going to be hit by some other earlier aoe

            if (existing < 0)
                _pendingCircles.Add((c.Position, explodeTime));
            else
                _pendingCircles[existing] = (c.Position, explodeTime);
            AddHitClouds(_clouds.InRadiusExcluding(c, radius), explodeTime, radius);
        }
    }
}

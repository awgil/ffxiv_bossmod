namespace BossMod.Endwalker.Criterion.C02AMR.C021Shishio;

class RokujoRevel(BossModule module) : Components.GenericAOEs(module)
{
    private int _numBreaths;
    private readonly List<Actor> _clouds = [.. module.Enemies(OID.NRaiun), .. module.Enemies(OID.SRaiun)];
    private readonly List<(Angle dir, DateTime activation)> _pendingLines = [];
    private readonly List<(WPos origin, DateTime activation)> _pendingCircles = [];

    private static readonly AOEShapeRect _shapeLine = new(30, 7, 30);
    private static readonly AOEShapeCircle[] _shapesCircle = [new(8), new(12), new(23)];

    private AOEShapeCircle? ShapeCircle => _numBreaths is > 0 and <= 3 ? _shapesCircle[_numBreaths - 1] : null;

    public bool Active => _pendingLines.Count + _pendingCircles.Count > 0;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (_pendingLines.Count > 1)
            yield return new(_shapeLine, Module.Center, _pendingLines[1].dir, _pendingLines[1].activation, Risky: false);
        if (_pendingCircles.Count > 0 && ShapeCircle is var shapeCircle && shapeCircle != null)
        {
            var firstFutureActivation = _pendingCircles[0].activation.AddSeconds(1);
            var firstFutureIndex = _pendingCircles.FindIndex(p => p.activation >= firstFutureActivation);
            if (firstFutureIndex >= 0)
            {
                var lastFutureActivation = _pendingCircles[firstFutureIndex].activation.AddSeconds(1.5f);
                foreach (var p in _pendingCircles.Skip(firstFutureIndex).TakeWhile(p => p.activation <= lastFutureActivation))
                    yield return new(shapeCircle, p.origin, default, p.activation, Risky: false);
            }
            else
            {
                firstFutureIndex = _pendingCircles.Count;
            }
            foreach (var p in _pendingCircles.Take(firstFutureIndex))
                yield return new(shapeCircle, p.origin, default, p.activation, ArenaColor.Danger);
        }
        if (_pendingLines.Count > 0)
            yield return new(_shapeLine, Module.Center, _pendingLines[0].dir, _pendingLines[0].activation, ArenaColor.Danger);
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NRokujoRevelAOE:
            case AID.SRokujoRevelAOE:
                _pendingLines.Add((spell.Rotation, spell.NPCFinishAt));
                AddHitClouds(_clouds.InShape(_shapeLine, caster.Position, spell.Rotation), spell.NPCFinishAt, ShapeCircle?.Radius ?? 0);
                _pendingCircles.SortBy(p => p.activation);
                break;
            case AID.NLeapingLevin1:
            case AID.NLeapingLevin2:
            case AID.NLeapingLevin3:
            case AID.SLeapingLevin1:
            case AID.SLeapingLevin2:
            case AID.SLeapingLevin3:
                var index = _pendingCircles.FindIndex(p => p.origin.AlmostEqual(caster.Position, 1));
                if (index < 0)
                {
                    ReportError($"Failed to predict levin from {caster.InstanceID:X}");
                    _pendingCircles.Add((caster.Position, spell.NPCFinishAt));
                }
                else if (Math.Abs((_pendingCircles[index].activation - spell.NPCFinishAt).TotalSeconds) > 1)
                {
                    ReportError($"Mispredicted levin from {caster.InstanceID:X} by {(_pendingCircles[index].activation - spell.NPCFinishAt).TotalSeconds}");
                }
                break;
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch ((AID)spell.Action.ID)
        {
            case AID.NSmokeaterFirst:
            case AID.NSmokeaterRest:
            case AID.SSmokeaterFirst:
            case AID.SSmokeaterRest:
                ++_numBreaths;
                break;
            case AID.NSmokeaterAbsorb:
            case AID.SSmokeaterAbsorb:
                _clouds.Remove(caster);
                break;
            case AID.NRokujoRevelAOE:
            case AID.SRokujoRevelAOE:
                if (_pendingLines.Count > 0)
                    _pendingLines.RemoveAt(0);
                ++NumCasts;
                break;
            case AID.NLeapingLevin1:
            case AID.NLeapingLevin2:
            case AID.NLeapingLevin3:
            case AID.SLeapingLevin1:
            case AID.SLeapingLevin2:
            case AID.SLeapingLevin3:
                _pendingCircles.RemoveAll(p => p.origin.AlmostEqual(caster.Position, 1));
                ++NumCasts;
                break;
        }
    }

    private void AddHitClouds(IEnumerable<Actor> clouds, DateTime timeHit, float radius)
    {
        var explodeTime = timeHit.AddSeconds(1.1f);
        foreach (var c in clouds)
        {
            var existing = _pendingCircles.FindIndex(p => p.origin.AlmostEqual(c.Position, 1));
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

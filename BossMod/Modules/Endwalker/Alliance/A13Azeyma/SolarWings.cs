namespace BossMod.Endwalker.Alliance.A13Azeyma;

abstract class SolarWings(BossModule module, uint aid) : Components.SimpleAOEs(module, aid, new AOEShapeCone(30f, 75f.Degrees()));
sealed class SolarWingsL(BossModule module) : SolarWings(module, (uint)AID.SolarWingsL);
sealed class SolarWingsR(BossModule module) : SolarWings(module, (uint)AID.SolarWingsR);

sealed class SolarFlair(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<WPos> _sunstorms = [with(6)];
    private BitMask _adjusted;

    private const float _kickDistance = 18f;
    private static readonly AOEShapeCircle _shape = new(15f);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _sunstorms.Count;
        if (count == 0)
        {
            return [];
        }
        var aoes = new AOEInstance[count];
        for (var i = 0; i < count; ++i)
        {
            aoes[i] = new(_shape, _sunstorms[i].Quantized());
        }
        return aoes;
    }

    public override void OnActorCreated(Actor actor)
    {
        if (actor.OID == (uint)OID.Sunstorm)
        {
            _sunstorms.Add(actor.Position);
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        switch (spell.Action.ID)
        {
            case (uint)AID.TeleportHeat:
                var count = _sunstorms.Count;
                if (count != 0)
                {
                    WPos? closestSunstorm = null;
                    var minDistance = float.MaxValue;
                    var closestIndex = -1;

                    for (var i = 0; i < count; ++i)
                    {
                        if (_adjusted[i])
                            continue;

                        var distance = (_sunstorms[i] - spell.TargetXZ).LengthSq();
                        if (distance < minDistance)
                        {
                            minDistance = distance;
                            closestSunstorm = _sunstorms[i];
                            closestIndex = i;
                        }
                    }
                    if (closestIndex == -1)
                    {
                        break;
                    }
                    // should teleport within range ~6
                    var pos = closestSunstorm!.Value;
                    if ((pos - spell.TargetXZ).LengthSq() < 50f)
                    {
                        _sunstorms[closestIndex] = pos + _kickDistance * (pos - spell.TargetXZ).Normalized();
                        _adjusted[closestIndex] = true;
                    }
                    else
                    {
                        ReportError($"Unexpected teleport location: {spell.TargetXZ}, closest sunstorm at {pos}");
                    }
                }
                break;
            case (uint)AID.SolarFlair:
                ++NumCasts;
                _adjusted = default;
                var countS = _sunstorms.Count;
                var position = caster.Position;
                for (var i = 0; i < countS; ++i)
                {
                    if (_sunstorms[i].AlmostEqual(position, 1f))
                    {
                        _sunstorms.RemoveAt(i);
                        return;
                    }
                }
                ReportError($"Unexpected solar flair position {position}");
                break;
        }
    }
}

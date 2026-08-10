namespace BossMod.Endwalker.Alliance.A23Halone;

sealed class Tetrapagos(BossModule module) : Components.GenericAOEs(module)
{
    private readonly List<AOEInstance> _aoes = [with(4)];
    private readonly AOEShapeCircle circle = new(10f), circlePredict = new(8f);
    private readonly AOEShapeDonut donut = new(10f, 30f), donutPredict = new(12f, 30f);
    private readonly AOEShapeCone cone = new(30f, 90f.Degrees());

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return [];
        }
        var max = count > 3 ? 3 : count;
        return CollectionsMarshal.AsSpan(_aoes)[..max];
    }

    public override void Update()
    {
        var count = _aoes.Count;
        if (count == 0)
        {
            return;
        }
        var aoes = CollectionsMarshal.AsSpan(_aoes);
        if (count > 1)
        {
            aoes[0].Color = Colors.Danger;
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        AOEShape? shape = spell.Action.ID switch
        {
            (uint)AID.TetrapagosHailstormPrepare => circle,
            (uint)AID.TetrapagosSwirlPrepare => donut,
            (uint)AID.TetrapagosRightrimePrepare or (uint)AID.TetrapagosLeftrimePrepare => cone,
            _ => null
        };
        if (shape != null)
        {
            _aoes.Add(new(shape, spell.LocXZ, caster.Rotation, Module.CastFinishAt(spell, 7.3d)));
            var count = _aoes.Count;
            if (count > 1)
            {
                var aoes = CollectionsMarshal.AsSpan(_aoes);
                ref var curAOE = ref aoes[count - 1];
                ref var prevAOE = ref aoes[count - 2];
                AOEShape? prev2Shape = null;
                if (count > 2)
                {
                    prev2Shape = aoes[count - 3].Shape;
                }
                if (shape == cone && (prevAOE.Shape == cone || prev2Shape == cone))
                {
                    curAOE.Origin += 3f * curAOE.Rotation.ToDirection();
                }
                else if (shape == circle && (prevAOE.Shape == donut || prev2Shape == donut))
                {
                    curAOE.Shape = circlePredict;
                }
                else if (shape == donut && (prevAOE.Shape == circle || prev2Shape == circle))
                {
                    curAOE.Shape = donutPredict;
                }
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID is (uint)AID.TetrapagosHailstormAOE or (uint)AID.TetrapagosSwirlAOE or (uint)AID.TetrapagosRightrimeAOE or (uint)AID.TetrapagosLeftrimeAOE)
        {
            ++NumCasts;
            var count = _aoes.Count;
            if (count != 0)
            {
                ref var aoe0 = ref _aoes.Ref(0);
                var shape0 = aoe0.Shape;
                _aoes.RemoveAt(0);
                if (count > 1)
                {
                    var aoes = CollectionsMarshal.AsSpan(_aoes);
                    var len = aoes.Length;

                    for (var i = 0; i < len; ++i)
                    {
                        ref var aoe = ref aoes[i];
                        if (shape0 == circle && aoe.Shape == donutPredict)
                        {
                            aoe.Shape = donut;
                            return;
                        }
                        else if (shape0 == donut && aoe.Shape == circlePredict)
                        {
                            aoe.Shape = circle;
                            return;
                        }
                        else if (shape0 == cone && aoe.Shape == cone)
                        {
                            aoe.Origin = spell.LocXZ;
                            return;
                        }
                    }
                }
            }
        }
    }
}

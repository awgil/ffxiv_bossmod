namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

sealed class Axioma(BossModule module) : Components.GenericAOEs(module)
{
    private const string riskHint = "GTFO from rifts!";
    private const string risk2Hint = "Walk into a rift!";
    private const string stayHint = "Stay inside rift!";
    public bool ShouldBeInZone;
    private AOEInstance[] _aoe = [], _aoeInv = [];

    // extracted from collision data - material ID: 00027004
    private readonly PolygonCustom vertices1 = new([new(75f, -725f), new(70f, -725f), new(65f, -725f), new(60f, -725f),
    new(58.143f, -725f), new(58.3167f, -726.1496f), new(58.4458f, -726.56305f), new(58.6982f, -727.3783f), new(59.4078f, -728.42175f),
    new(60.3447f, -729.3537f), new(61.2174f, -729.77197f), new(61.5917f, -729.95154f), new(63.0951f, -730.49982f), new(64.5921f, -730.87823f),
    new(65.9846f, -730.99957f), new(67.2878f, -730.97443f), new(68.8071f, -731.1355f), new(70.4964f, -731.46259f), new(72.0493f, -731.90637f),
    new(73.437f, -732.12659f), new(75f, -732.28339f)]);
    private readonly PolygonCustom vertices2 = new([new(25.3281f, -735.35754f), new(26.426f, -735.07697f), new(26.562f, -735.04224f), new(26.7076f, -735.00391f),
    new(26.8697f, -734.96143f), new(27.1617f, -734.84998f), new(28.8127f, -734.22046f), new(30.6479f, -733.09808f), new(32.1464f, -731.73309f), new(33.3692f, -730.21265f),
    new(34.4045f, -728.67902f), new(35.2253f, -726.82794f), new(35.2813f, -726.56305f), new(35.6056f, -724.99994f), new(25f, -725f), new(25f, -735.44147f)]);
    private readonly PolygonCustom vertices3 = new([new(48.1426f, -725f), new(44.1429f, -725f), new(44.1762f, -726.56305f), new(44.1904f, -728.75665f),
    new(44.2046f, -730.93481f), new(44.1878f, -733.24597f), new(44.112f, -735.55365f), new(43.947f, -737.80115f), new(43.6976f, -740.04736f),
    new(43.4119f, -741.89984f), new(43.0117f, -743.62701f), new(42.4717f, -745.34174f), new(41.7833f, -747.04736f), new(40.933f, -748.59662f),
    new(40.001f, -749.91089f), new(38.8251f, -751.18555f), new(37.5358f, -752.14978f), new(35.856f, -753.0025f), new(33.7564f, -753.62677f),
    new(31.7419f, -753.98962f), new(29.5608f, -753.99438f), new(26.5619f, -753.71185f), new(25f, -753.56549f), new(25f, -757.58282f),
    new(26.562f, -757.72894f), new(29.233f, -757.98077f), new(31.985f, -757.98224f), new(34.7063f, -757.51245f), new(34.9706f, -757.4325f),
    new(34.9275f, -757.70605f), new(34.7065f, -759.53778f), new(34.5354f, -761.36975f), new(34.3643f, -763.20166f), new(34.2598f, -765.21283f),
    new(34.3547f, -767.22418f), new(34.6982f, -769.36011f), new(35.3903f, -771.39667f), new(36.3724f, -773.43799f), new(37.3868f, -775f),
    new(42.5529f, -775f), new(41.7799f, -774.16229f), new(41.1997f, -773.43799f), new(40.696f, -772.79456f), new(39.7612f, -771.32257f),
    new(39.0821f, -769.85663f), new(38.5269f, -768.32861f), new(38.3204f, -766.70099f), new(38.2376f, -765.09375f), new(38.354f, -763.48639f),
    new(38.5142f, -761.74622f), new(38.6746f, -760.00574f), new(38.8849f, -758.26562f), new(39.2482f, -755.75745f), new(39.4891f, -755.64038f),
    new(41.5388f, -754.12415f), new(43.1756f, -752.34467f), new(44.4286f, -750.5412f), new(45.4498f, -748.64709f), new(46.3003f, -746.50061f),
    new(46.9228f, -744.46594f), new(47.3805f, -742.40259f), new(47.669f, -740.47266f), new(48.4648f, -740.80334f), new(50.7866f, -741.80865f),
    new(52.4563f, -742.65088f), new(53.9897f, -743.59082f), new(55.3174f, -744.70813f), new(56.5543f, -746.03442f), new(57.4835f, -747.48608f),
    new(58.2031f, -749.04895f), new(58.76f, -750.61432f), new(59.1578f, -752.15216f), new(59.7885f, -755.02167f), new(60.4404f, -758.06543f),
    new(59.5972f, -759.05914f), new(58.526f, -760.86078f), new(58.404f, -761.15997f), new(58.0014f, -762.14795f), new(57.7945f, -762.65558f),
    new(57.2123f, -764.9613f), new(57.0082f, -767.09131f), new(57.2294f, -769.37665f), new(57.7022f, -771.62482f), new(58.521f, -773.80737f),
    new(59.1997f, -775f), new(64.0821f, -775f), new(63.8263f, -774.67194f), new(63.7223f, -774.53857f), new(63.0506f, -773.67694f),
    new(62.9063f, -773.43799f), new(62.1757f, -772.18127f), new(61.5527f, -770.55377f), new(61.2136f, -769.0213f), new(61.0067f, -767.1908f),
    new(61.1576f, -765.61981f), new(61.5483f, -764.03699f), new(62.0992f, -762.65851f), new(62.8508f, -761.34595f), new(63.9279f, -760.12512f),
    new(65.0842f, -759.15027f), new(66.4947f, -758.35822f), new(67.9801f, -758.02155f), new(69.5422f, -757.93488f), new(71.1551f, -758.17871f),
    new(73.12f, -758.72607f), new(73.437f, -758.8465f), new(75f, -759.43982f), new(75f, -755.16443f), new(74.5457f, -754.98895f),
    new(73.437f, -754.62256f), new(72.0707f, -754.28497f), new(69.8698f, -753.96997f), new(67.4927f, -754.05133f), new(65.1489f, -754.59149f),
    new(63.9314f, -755.31134f), new(63.755f, -754.51202f), new(63.101f, -751.4823f), new(62.6245f, -749.58325f), new(61.9516f, -747.65454f),
    new(61.6321f, -746.96094f), new(61.1471f, -745.90784f), new(60.9769f, -745.53845f), new(59.689f, -743.58386f), new(58.2183f, -741.95459f),
    new(57.9486f, -741.724f), new(57.5534f, -741.38574f), new(57.3939f, -741.24945f), new(56.4009f, -740.39966f), new(54.4233f, -739.16846f),
    new(52.4804f, -738.18896f), new(49.9988f, -737.1095f), new(48.0608f, -736.36029f), new(48.1105f, -735.66815f), new(48.1879f, -733.24176f),
    new(48.2047f, -730.95795f), new(48.1904f, -728.75311f), new(48.1761f, -726.56305f)]);

    public override ReadOnlySpan<AOEInstance> ActiveAOEs(int slot, Actor actor) => ShouldBeInZone ? _aoeInv : _aoe;

    public override void OnMapEffect(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001u)
        {
            var center = Arena.Center;
            var shape = new AOEShapeCustom(center, [vertices1, vertices2, vertices3]);
            var act = DateTime.MaxValue;
            _aoe = [new(shape, center, default, act, shapeDistance: shape.Distance(center, default))];
            _aoeInv = [new(shape, center, default, act, Colors.SafeFromAOE, shapeDistance: shape.InvertedDistance(center, default))];
        }
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InexorablePullAOE)
        {
            ShouldBeInZone = true;
            if (_aoeInv.Length != 0)
            {
                ref var aoe = ref _aoeInv[0];
                aoe.Activation = Module.CastFinishAt(spell);
            }
        }
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if (spell.Action.ID == (uint)AID.InexorablePullAOE)
        {
            ShouldBeInZone = false;
        }
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (_aoe.Length == 0)
        {
            return;
        }
        ref var aoe = ref _aoe[0];
        var isInside = aoe.Check(actor.Position);

        if (!ShouldBeInZone && isInside)
        {
            hints.Add(riskHint);
        }
        else if (ShouldBeInZone)
        {
            hints.Add(!isInside ? risk2Hint : stayHint, !isInside);
        }
    }
}

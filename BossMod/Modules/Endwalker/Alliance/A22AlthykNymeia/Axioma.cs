namespace BossMod.Endwalker.Alliance.A22AlthykNymeia;

class Axioma(BossModule module) : Components.GenericAOEs(module)
{
    private const string riskHint = "GTFO from rifts!";
    private const string risk2Hint = "Walk into a rift!";
    private const string stayHint = "Stay inside rift!";
    public bool ShouldBeInZone { get; private set; }
    private bool active;

    private static readonly PolygonCustom shapeCustom1 = new([new(35.65f, -725), new(35.3f, -726.57f), new(35.25f, -726.83f), new(34.42f, -728.66f),
    new(33.38f, -730.2f), new(32.17f, -731.72f), new(30.67f, -733.08f), new(28.83f, -734.21f), new(27.19f, -734.83f), new(25, -735.44f),  new(25, -725)]);
    private static readonly PolygonCustom shapeCustom2 = new([new(75, -725), new(75, -732.32f), new(73.49f, -732.17f), new(72.1f, -731.97f),
    new(70.54f, -731.49f), new(68.88f, -731.16f), new(67.34f, -731.01f), new(66.02f, -731.01f), new(64.64f, -730.92f), new(63.15f, -730.55f),
    new(61.64f, -729.96f), new(60.4f, -729.34f), new(59.45f, -728.39f), new(58.74f, -727.34f), new(58.32f, -726.08f), new(58.1f, -725)]);
    private static readonly PolygonCustom shapeCustom3 = new([new(64.10f, -775), new(59.2f, -775), new(58.54f, -773.85f), new(57.72f, -771.66f),
    new(57.24f, -769.38f), new(57.03f, -767.12f), new(57.24f, -765), new(57.81f, -762.68f), new(58.56f, -760.89f), new(59.61f, -759.09f),
    new(61.06f, -757.38f), new(62.99f, -755.92f), new(65.16f, -754.63f), new(67.51f, -754.09f), new(69.88f, -753.99f), new(72.09f, -754.32f),
    new(73.44f, -754.63f), new(74.57f, -755.01f), new(75, -755.14f), new(75, -759.46f), new(73.47f, -758.86f), new(73.47f, -758.87f),
    new(73.16f, -758.75f), new(71.17f, -758.22f), new(69.58f, -757.95f), new(68.02f, -758.06f), new(66.52f, -758.39f), new(65.11f, -759.16f),
    new(63.96f, -760.15f), new(62.89f, -761.36f), new(62.13f, -762.68f), new(61.59f, -764.06f), new(61.19f, -765.64f), new(61.04f, -767.2f),
    new(61.26f, -769.04f), new(61.6f, -770.57f), new(62.21f, -772.18f), new(62.94f, -773.45f), new(63.75f, -774.56f)]);
    private static readonly PolygonCustom shapeCustom4 = new([new(42.61f, -775), new(37.38f, -775), new(36.4f, -773.45f), new(35.41f, -771.43f),
    new(34.71f, -769.39f), new(34.37f, -767.23f), new(34.28f, -765.23f), new(34.37f, -763.22f), new(34.56f, -761.38f), new(34.72f, -759.54f),
    new(34.95f, -757.71f), new(35.02f, -757.22f), new(39.27f, -755.75f), new(38.92f, -758.26f), new(38.72f, -760.02f), new(38.57f, -761.76f),
    new(38.39f, -763.48f), new(38.28f, -765.1f), new(38.36f, -766.71f), new(38.57f, -768.33f), new(39.13f, -769.86f), new(39.8f, -771.33f),
    new(40.73f, -772.79f), new(41.23f, -773.43f), new(41.81f, -774.16f)]);
    private static readonly PolygonCustom shapeCustom5 = new([new(25, -757.62f), new(25, -753.6f), new(26.54f, -753.73f), new(29.55f, -754.03f),
    new(31.73f, -754.02f), new(33.74f, -753.66f), new(35.83f, -753.04f), new(37.51f, -752.18f), new(38.8f, -751.22f), new(39.97f, -749.94f),
    new(40.91f, -748.65f), new(41.76f, -747.09f), new(42.45f, -745.39f), new(42.99f, -743.66f), new(43.4f, -741.94f), new(43.61f, -740.1f),
    new(43.93f, -737.84f), new(44.1f, -735.59f), new(44.17f, -733.28f), new(44.18f, -730.96f), new(44.21f, -728.77f), new(44.2f, -726.58f),
    new(44.17f, -725), new(48.16f, -725), new(48.2f, -726.57f), new(48.21f, -728.76f), new(48.22f, -730.96f), new(48.21f, -733.25f),
    new(48.13f, -735.67f), new(48.08f, -736.37f), new(47.96f, -738.06f), new(47.69f, -740.49f), new(47.39f, -742.41f), new(46.94f, -744.47f),
    new(46.32f, -746.51f), new(45.47f, -748.65f), new(44.45f, -750.55f), new(43.2f, -752.35f), new(41.56f, -754.13f), new(39.5f, -755.65f),
    new(39.26f, -755.76f), new(37.19f, -756.77f), new(34.98f, -757.44f), new(34.73f, -757.51f), new(32, -757.99f), new(29.25f, -757.99f)]);
    private static readonly PolygonCustom shapeCustom6 = new([new(63.97f, -755.32f), new(60.49f, -758.06f), new(59.79f, -755.03f),
    new(59.17f, -752.17f), new(58.78f, -750.63f), new(58.22f, -749.06f), new(57.51f, -747.51f), new(56.57f, -746.04f), new(55.33f, -744.71f),
    new(54.02f, -743.57f), new(52.47f, -742.63f), new(50.81f, -741.78f), new(48.48f, -740.79f), new(47.65f, -740.45f), new(48.05f, -736.33f),
    new(50.02f, -737.09f), new(52.5f, -738.17f), new(54.44f, -739.15f), new(56.42f, -740.38f), new(58.26f, -741.94f), new(59.73f, -743.55f),
    new(61.01f, -745.52f), new(62, -747.65f), new(62.67f, -749.58f), new(63.14f, -751.48f), new(63.79f, -754.5f)]);
    private static readonly List<Shape> union = [shapeCustom1, shapeCustom2, shapeCustom3, shapeCustom4, shapeCustom5, shapeCustom6];
    private static readonly AOEShapeCustom risky = new(union);
    private static readonly AOEShapeCustom notRisky = new(union, InvertForbiddenZone: true);
    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor)
    {
        if (active)
            yield return new(ShouldBeInZone ? notRisky : risky, Module.Arena.Center, Color: ShouldBeInZone ? ArenaColor.SafeFromAOE : ArenaColor.AOE);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (index == 0x00 && state == 0x00020001)
            active = true;
    }

    public override void OnCastStarted(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InexorablePullAOE)
            ShouldBeInZone = true;
    }

    public override void OnCastFinished(Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.InexorablePullAOE)
            ShouldBeInZone = false;
    }

    public override void AddHints(int slot, Actor actor, TextHints hints)
    {
        if (!ShouldBeInZone && ActiveAOEs(slot, actor).Any(c => c.Risky && c.Check(actor.Position)))
            hints.Add(riskHint);
        if (ShouldBeInZone && ActiveAOEs(slot, actor).Any(c => c.Risky && !c.Check(actor.Position)))
            hints.Add(risk2Hint);
        else if (ShouldBeInZone && ActiveAOEs(slot, actor).Any(c => c.Risky && c.Check(actor.Position)))
            hints.Add(stayHint, false);
    }
}

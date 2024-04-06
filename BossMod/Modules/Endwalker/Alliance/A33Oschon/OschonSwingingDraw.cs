namespace BossMod.Endwalker.Alliance.A33Oschon;

class SwingingDraw : Components.GenericAOEs
{
    private const float maxError = MathF.PI / 180;
    private readonly List<AOEInstance> _aoes = [];

    private static readonly AOEShapeCone _shape = new(60, 60.Degrees());

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes;
    public override void OnCastStarted(BossModule module, Actor caster, ActorCastInfo spell)
    {
        var _activation = spell.NPCFinishAt.AddSeconds(6.1f);
        if ((AID)spell.Action.ID == AID.SwingingDrawCW)
        {
            if (caster.Position.AlmostEqual(new(-10, 740), 1) && caster.Rotation.AlmostEqual(-135.Degrees(), maxError))
                _aoes.Add(new(_shape, new(0, 725), 0.Degrees(), _activation));
            if (caster.Position.AlmostEqual(new(10, 740), 1) && caster.Rotation.AlmostEqual(135.Degrees(), maxError))
                _aoes.Add(new(_shape, new(25, 750), -90.Degrees(), _activation));
            if (caster.Position.AlmostEqual(new(10, 760), 1) && caster.Rotation.AlmostEqual(45.Degrees(), maxError))
                _aoes.Add(new(_shape, new(0, 775), 180.Degrees(), _activation));
            if (caster.Position.AlmostEqual(new(-10, 760), 1) && caster.Rotation.AlmostEqual(-45.Degrees(), maxError))
                _aoes.Add(new(_shape, new(-25, 750), 90.Degrees(), _activation));
        }
        if ((AID)spell.Action.ID == AID.SwingingDrawCCW)
        {
            if (caster.Position.AlmostEqual(new(10, 760), 1) && caster.Rotation.AlmostEqual(45.Degrees(), maxError))
                _aoes.Add(new(_shape, new(25, 750), -90.Degrees(), _activation));
            if (caster.Position.AlmostEqual(new(-10, 760), 1) && caster.Rotation.AlmostEqual(-45.Degrees(), maxError))
                _aoes.Add(new(_shape, new(0, 775), 180.Degrees(), _activation));
            if (caster.Position.AlmostEqual(new(10, 740), 1) && caster.Rotation.AlmostEqual(135.Degrees(), maxError))
                _aoes.Add(new(_shape, new(0, 725), 0.Degrees(), _activation));
            if (caster.Position.AlmostEqual(new(-10, 740), 1) && caster.Rotation.AlmostEqual(-135.Degrees(), maxError))
                _aoes.Add(new(_shape, new(-25, 750), 90.Degrees(), _activation));
        }
    }

    public override void OnCastFinished(BossModule module, Actor caster, ActorCastInfo spell)
    {
        if ((AID)spell.Action.ID == AID.SwingingDraw)
        {
            _aoes.Clear();
            ++NumCasts;
        }
    }
}

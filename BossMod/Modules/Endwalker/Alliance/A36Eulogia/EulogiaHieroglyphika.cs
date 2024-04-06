namespace BossMod.Endwalker.Alliance.A36Eulogia;

class Hieroglyphika : Components.GenericAOEs
{
    private static readonly AOEShapeRect rect = new(12, 6);
    private readonly List<AOEInstance> _aoes = [];
    private const float RadianConversion = MathF.PI / 180;
    private static readonly WPos[] StartingCoords = [new(951, -933), new(939, -933), new(951, -957), new(939, -957), new(927, -933), new(963, -957), new(963, -933), new(951, -945), new(939, -945), new(927, -921), new(939, -921), new(927, -945), new(963, -945), new(963, -921)];

    public override IEnumerable<AOEInstance> ActiveAOEs(BossModule module, int slot, Actor actor) => _aoes.Take(14);

    private static WPos RotateAroundOrigin(float rotatebydegrees, WPos origin, WPos caster) //TODO: consider moving to utils for future use
    {
        float x = MathF.Cos(rotatebydegrees * RadianConversion) * (caster.X - origin.X) - MathF.Sin(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        float z = MathF.Sin(rotatebydegrees * RadianConversion) * (caster.X - origin.X) + MathF.Cos(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        return new WPos(origin.X + x, origin.Z + z);
    }

    public override void OnEventIcon(BossModule module, Actor actor, uint iconID)
    {
        var _activation = module.WorldState.CurrentTime.AddSeconds(16);
        var origin = module.Bounds.Center;
        if (iconID == (uint)IconID.ClockwiseHieroglyphika)
            foreach (var r in StartingCoords)
                _aoes.Add(new(rect, RotateAroundOrigin(0, origin, r), 180.Degrees(), _activation));
        if (iconID == (uint)IconID.CounterClockwiseHieroglyphika)
            foreach (var r in StartingCoords)
                _aoes.Add(new(rect, RotateAroundOrigin(180, origin, r), activation: _activation));
    }

    public override void OnEventCast(BossModule module, Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.HieroglyphikaRect)
            _aoes.Clear();
    }
}

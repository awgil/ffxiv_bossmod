namespace BossMod.Endwalker.Alliance.A34Eulogia;

class Hieroglyphika(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(12, 6);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly WPos[] StartingCoords = [new(951, -933), new(939, -933), new(951, -957), new(939, -957), new(927, -933), new(963, -957), new(963, -933), new(951, -945), new(939, -945), new(927, -921), new(939, -921), new(927, -945), new(963, -945), new(963, -921)];

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var _activation = WorldState.FutureTime(16);
        var origin = Module.Bounds.Center;
        if (iconID == (uint)IconID.ClockwiseHieroglyphika)
            foreach (var r in StartingCoords)
                _aoes.Add(new(rect, Helpers.RotateAroundOrigin(0, origin, r), 180.Degrees(), _activation));
        if (iconID == (uint)IconID.CounterClockwiseHieroglyphika)
            foreach (var r in StartingCoords)
                _aoes.Add(new(rect, Helpers.RotateAroundOrigin(180, origin, r), default, _activation));
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.HieroglyphikaRect)
            _aoes.Clear();
    }
}

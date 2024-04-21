namespace BossMod.Endwalker.Alliance.A31Thaliak;

class Hieroglyphika(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(6, 6, 6);
    private readonly List<AOEInstance> _aoes = [];
    private static readonly WPos[] StartingCoords = [new(-963, 939), new(-963, 963), new(-939, 927), new(-951, 939), new(-951, 927), new(-939, 963), new(-927, 939), new(-939, 951), new(-939, 939), new(-927, 963), new(-963, 951), new(-927, 951), new(-951, 951), new(-963, 927)];
    private byte currentIndex;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes;

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
            currentIndex = index;
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var _activation = WorldState.FutureTime(17);
        var origin = Module.Bounds.Center;
        if (iconID == (uint)IconID.ClockwiseHieroglyphika)
        {
            if (currentIndex == 0x17)
                foreach (var r in StartingCoords)
                    _aoes.Add(new(rect, Helpers.RotateAroundOrigin(0, origin, r), default, _activation));
            if (currentIndex == 0x4A)
                foreach (var r in StartingCoords)
                    _aoes.Add(new(rect, Helpers.RotateAroundOrigin(-90, origin, r), default, _activation));
        }
        if (iconID == (uint)IconID.CounterClockwiseHieroglyphika)
        {
            if (currentIndex == 0x4A)
                foreach (var r in StartingCoords)
                    _aoes.Add(new(rect, Helpers.RotateAroundOrigin(90, origin, r), default, _activation));
            if (currentIndex == 0x17)
                foreach (var r in StartingCoords)
                    _aoes.Add(new(rect, Helpers.RotateAroundOrigin(180, origin, r), default, _activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.HieroglyphikaRect)
            _aoes.Clear();
    }
}

namespace BossMod.Endwalker.Alliance.A31Thaliak;

class Hieroglyphika(BossModule module) : Components.GenericAOEs(module)
{
    private static readonly AOEShapeRect rect = new(6, 6, 6);
    private readonly List<AOEInstance> _aoes = [];
    private const float RadianConversion = MathF.PI / 180;
    private static readonly WPos[] StartingCoords = [new(-963, 939), new(-963, 963), new(-939, 927), new(-951, 939), new(-951, 927), new(-939, 963), new(-927, 939), new(-939, 951), new(-939, 939), new(-927, 963), new(-963, 951), new(-927, 951), new(-951, 951), new(-963, 927)];
    private byte currentIndex;

    public override IEnumerable<AOEInstance> ActiveAOEs(int slot, Actor actor) => _aoes.Take(14);

    private static WPos RotateAroundOrigin(float rotatebydegrees, WPos origin, WPos caster) //TODO: consider moving to utils for future use
    {
        float x = MathF.Cos(rotatebydegrees * RadianConversion) * (caster.X - origin.X) - MathF.Sin(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        float z = MathF.Sin(rotatebydegrees * RadianConversion) * (caster.X - origin.X) + MathF.Cos(rotatebydegrees * RadianConversion) * (caster.Z - origin.Z);
        return new WPos(origin.X + x, origin.Z + z);
    }

    public override void OnEventEnvControl(byte index, uint state)
    {
        if (state == 0x00020001)
        {
            if (index == 0x17)
                currentIndex = 0x17;
            if (index == 0x4A)
                currentIndex = 0x4A;
        }
    }

    public override void OnEventIcon(Actor actor, uint iconID)
    {
        var _activation = WorldState.FutureTime(17);
        var origin = Module.Center;
        if (iconID == (uint)IconID.ClockwiseHieroglyphika)
        {
            if (currentIndex == 0x17)
                foreach (var r in StartingCoords)
                    _aoes.Add(new(rect, RotateAroundOrigin(0, origin, r), default, _activation));
            if (currentIndex == 0x4A)
                foreach (var r in StartingCoords)
                    _aoes.Add(new(rect, RotateAroundOrigin(-90, origin, r), default, _activation));
        }
        if (iconID == (uint)IconID.CounterClockwiseHieroglyphika)
        {
            if (currentIndex == 0x4A)
                foreach (var r in StartingCoords)
                    _aoes.Add(new(rect, RotateAroundOrigin(90, origin, r), default, _activation));
            if (currentIndex == 0x17)
                foreach (var r in StartingCoords)
                    _aoes.Add(new(rect, RotateAroundOrigin(180, origin, r), default, _activation));
        }
    }

    public override void OnEventCast(Actor caster, ActorCastEvent spell)
    {
        if (_aoes.Count > 0 && (AID)spell.Action.ID is AID.HieroglyphikaRect)
            _aoes.Clear();
    }
}

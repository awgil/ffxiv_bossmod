namespace BossMod.Endwalker.Savage.P5SProtoCarbuncle;

class VenomTowers(BossModule module) : BossComponent(module)
{
    private readonly List<WDir> _activeTowerOffsets = [];

    private const float _radius = 3; // not sure...
    private const float _meleeOffset = 7;
    private const float _rangedOffset = 11; // not sure...

    public bool Active => _activeTowerOffsets.Count > 0;

    public override void DrawArenaForeground(int pcSlot, Actor pc)
    {
        foreach (var t in _activeTowerOffsets)
        {
            var origin = Arena.Center + t;
            Arena.ZoneCircleOutline(origin, _radius, Raid.WithoutSlot(false, true, true).InRadius(origin, _radius).Any() ? Colors.Safe : default);
        }
    }

    public override void OnMapEffect(byte index, uint state)
    {
        WDir offset = index switch
        {
            3 => new(-_rangedOffset, -_rangedOffset),
            4 => new(+_rangedOffset, -_rangedOffset),
            5 => new(0, -_meleeOffset),
            6 => new(-_meleeOffset, 0),
            7 => new(+_meleeOffset, 0),
            8 => new(0, +_meleeOffset),
            9 => new(-_rangedOffset, +_rangedOffset),
            10 => new(+_rangedOffset, +_rangedOffset),
            _ => default
        };
        if (offset == new WDir())
            return;

        if (state == 0x00020001u)
            _activeTowerOffsets.Add(offset);
        else if (state is 0x00080004u or 0x00100004u) // soaked or unsoaked
            _activeTowerOffsets.Remove(offset);
    }
}

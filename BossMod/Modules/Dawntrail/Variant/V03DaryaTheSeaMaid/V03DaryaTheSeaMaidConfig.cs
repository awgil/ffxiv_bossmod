namespace BossMod.Dawntrail.Variant.V03DaryaSeaMaid;

[ConfigDisplay(Order = 0x202, Parent = typeof(DawntrailConfig))]
public class V03DaryaTheSeaMaidConfig() : ConfigNode() {
    [PropertyDisplay("TidalWave: automatically use knockback immunity if needed?")]
    public bool TidalWaveAntiKB = true;

    [PropertyDisplay("AlluringOrder: automatically face the correct corner safe spot?", tooltip: "This feature requires Settings -> Action Tweaks -> Smart Character Orientation to be enabled.")]
    public bool AlluringOrderCornerSafeSpot = false;

    public enum AlluringOrderCorners { NW, NE, SW, SE }
    [PropertyDisplay("AlluringOrder: my assigned corner", depends: nameof(AlluringOrderCornerSafeSpot))]
    public AlluringOrderCorners AlluringOrderCorner = AlluringOrderCorners.NW;
}

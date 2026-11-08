namespace BossMod;

[ConfigDisplay(Name = "In-game overlay", Order = -8, Group = "General")]
public class OverlayConfig : ConfigNode
{
    public enum PixelBehavior
    {
        [PropertyDisplay("Disabled")]
        Disabled,
        [PropertyDisplay("In combat")]
        Combat,
        [PropertyDisplay("In duty")]
        Duty,
        [PropertyDisplay("In duty or combat")]
        Either,
        [PropertyDisplay("In duty and combat")]
        Both,
    }

    [PropertyDisplay("Show player hitbox pixel")]
    [PropertyRadio(true)]
    public PixelBehavior ShowPixel;

    [PropertyDisplay("Indicator size", depends: nameof(ShowPixel))]
    [PropertySlider(0, 5)]
    public float PixelSize = 2.5f;

    public enum RingBehavior
    {
        [PropertyDisplay("Disabled")]
        Disabled,
        [PropertyDisplay("As melee or tank")]
        Melee,
        [PropertyDisplay("Always")]
        Everyone
    }

    [PropertyDisplay("Show target range indicator")]
    [PropertyRadio(true)]
    public RingBehavior ShowTargetRing;

    public enum AutoRingBehavior
    {
        [PropertyDisplay("Disabled")]
        Disabled,
        [PropertyDisplay("As tank")]
        Tank,
        [PropertyDisplay("Always")]
        Everyone
    }

    [PropertyDisplay("Show enemy autoattack range indicator", depends: nameof(ShowTargetRing))]
    [PropertyRadio(true)]
    public AutoRingBehavior ShowAutos = AutoRingBehavior.Tank;

    [PropertyDisplay("Highlight current slice", depends: nameof(ShowTargetRing))]
    public bool HighlightSlice = true;

    [PageStart("Colors")]
    [PropertyDisplay("Pixel color")]
    public Color PixelColor = Color.FromRGBA(0xE200FFFF);

    [PropertyDisplay("Default ring color")]
    public Color FallbackColor = Color.FromRGBA(0xFFFFFF80);

    [PropertyDisplay("Ring color: rear")]
    public Color RearColor = Color.FromRGBA(0x35DC8BFF);
    [PropertyDisplay("Ring color: flank")]
    public Color FlankColor = Color.FromRGBA(0xCFC851FF);
    [PropertyDisplay("Ring color: front")]
    public Color FrontColor = Color.FromRGBA(0xE60014FF);

    [PropertyDisplay("Positional (imminent, correct)")]
    public Color PositionalGoodNow = Color.FromRGBA(0x35DC8BFF);
    [PropertyDisplay("Positional (imminent, incorrect)")]
    public Color PositionalBadNow = Color.FromRGBA(0xE60014FF);
    [PropertyDisplay("Positional (future, correct)")]
    public Color PositionalGoodLater = Color.FromRGBA(0xFFFFFFFF);
    [PropertyDisplay("Positional (future, incorrect)")]
    public Color PositionalBadLater = Color.FromRGBA(0xCFC851FF);
}

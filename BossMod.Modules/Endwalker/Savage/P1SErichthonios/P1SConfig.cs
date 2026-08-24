namespace BossMod.Endwalker.Savage.P1SErichthonios;

[ConfigDisplay(Order = 0x110, Parent = typeof(EndwalkerConfig))]
public class P1SConfig() : ConfigNode()
{
    public enum Corner { NW, NE, SE, SW }

    [PropertyDisplay("Intemperance: corner that swaps with N on asymmetrical pattern")]
    public Corner IntemperanceAsymmetricalSwapCorner = Corner.NW;
}

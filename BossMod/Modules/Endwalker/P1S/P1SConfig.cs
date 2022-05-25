namespace BossMod.Endwalker.P1S
{
    [ConfigDisplay(Order = 0x110, Parent = typeof(EndwalkerConfig))]
    public class P1SConfig : ConfigNode
    {
        public enum Corner { NW, NE, SE, SW }

        public Corner IntemperanceAsymmetricalSwapCorner = Corner.NW;

        public override void DrawContents(Tree tree)
        {
            DrawProperty(ref IntemperanceAsymmetricalSwapCorner, "Intemperance: corner that swaps with N on asymmetrical pattern");
        }
    }
}

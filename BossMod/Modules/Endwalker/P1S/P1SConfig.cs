namespace BossMod.Endwalker.P1S
{
    public class P1SConfig : ConfigNode
    {
        public enum Corner { NW, NE, SE, SW }

        public Corner IntemperanceAsymmetricalSwapCorner = Corner.NW;

        public P1SConfig()
        {
            DisplayOrder = 0x110;
        }

        protected override void DrawContents()
        {
            DrawProperty(ref IntemperanceAsymmetricalSwapCorner, "Intemperance: corner that swaps with N on asymmetrical pattern");
        }
    }
}

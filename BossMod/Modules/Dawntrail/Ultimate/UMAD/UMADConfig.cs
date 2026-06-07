namespace BossMod.Dawntrail.Ultimate.UMAD;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class UMADConfig : ConfigNode
{
    public enum P1ArrowShape
    {
        None,
        [PropertyDisplay("Big box (CW)")]
        BigBox
    }

    [PropertyDisplay("P1 Tele-Portent: arrow placement hints")]
    public P1ArrowShape P1Arrows = P1ArrowShape.BigBox;
}

namespace BossMod.Endwalker.Ultimate.DSW1;

[ConfigDisplay(Order = 0x200, Parent = typeof(EndwalkerConfig))]
public class DSW1Config : CooldownPlanningConfigNode
{
    public enum HeavensflameHints
    {
        [PropertyDisplay("Don't show any hints")]
        None,

        [PropertyDisplay("Match waymark colors: circle=red, triangle=green, cross=blue, square=purple")]
        Waymarks,

        [PropertyDisplay("LPDU (inter)cardinals: cross=N/S, square=NE/SW, circle=E/W, triangle=SE/NW")]
        LPDU,
    }

    [PropertyDisplay("Heavensflame resolution hints")]
    public HeavensflameHints Heavensflame = HeavensflameHints.None;

    public DSW1Config() : base(90) { }
}

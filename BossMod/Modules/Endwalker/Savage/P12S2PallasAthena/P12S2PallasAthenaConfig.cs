namespace BossMod.Endwalker.Savage.P12S2PallasAthena;

[ConfigDisplay(Order = 0x1C2, Parent = typeof(EndwalkerConfig))]
public class P12S2PallasAthenaConfig : CooldownPlanningConfigNode
{
    [PropertyDisplay("Pangenesis: tower assignment strategy")]
    [PropertyCombo("2+0: first towers are soaked by short color and 0 unstable; they both then go north for second towers", "2+1: first towers are soaked by short color and 1 unstable; they both then go to different second towers")]
    public bool PangenesisFirstStack = true;

    public P12S2PallasAthenaConfig() : base(90) { }
}

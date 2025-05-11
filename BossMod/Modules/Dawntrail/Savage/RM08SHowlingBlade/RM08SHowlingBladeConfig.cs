namespace BossMod.Dawntrail.Savage.RM08SHowlingBlade;

[ConfigDisplay(Order = 0x300, Parent = typeof(DawntrailConfig))]
public class RM08SHowlingBladeConfig : ConfigNode
{
    public enum ReignStrategy
    {
        [PropertyDisplay("Show both safespots for current role")]
        Any,
        [PropertyDisplay("Assume G1 left, G2 right when looking at boss from arena center")]
        Standard,
        [PropertyDisplay("Assume G1 right, G2 left when looking at boss from arena center")]
        Inverse,
        [PropertyDisplay("None")]
        Disabled
    }

    [PropertyDisplay("Revolutionary Reign positioning hints")]
    public ReignStrategy ReignHints = ReignStrategy.Standard;

    public enum TerrestrialRageStrategy
    {
        [PropertyDisplay("No hints")]
        None,
        [PropertyDisplay("Clocks - stack marker goes to N/NE safe spot, spreads adjust")]
        Clock,
        // Toxic, DN TODO
    }

    [PropertyDisplay("Terrestrial Rage")]
    public TerrestrialRageStrategy TRHints = TerrestrialRageStrategy.None;

    [PropertyDisplay("Beckon Moonlight: highlight first and third safe quadrants on minimap (aka \"quad moonlight\")")]
    public bool QuadMoonlightHints = false;

    public enum LamentTowerStrategy
    {
        [PropertyDisplay("No tower hints")]
        None,
        [PropertyDisplay("Rinon - healers S, long tank SW, long tank partner SE, long healer partner N, short tank and partner N")]
        Rinon,
    }

    [PropertyDisplay("Lone Wolf's Lament")]
    public LamentTowerStrategy TowerHints = LamentTowerStrategy.Rinon;

    [PropertyDisplay("Windfang/Stonefang clock spots", tooltip: "Only used by AI")]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("Default", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique WindfangStonefangSpots = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };
}

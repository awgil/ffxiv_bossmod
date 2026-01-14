namespace BossMod.Dawntrail.Savage.RM11STheTyrant;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class RM11STheTyrantConfig : ConfigNode
{
    [PropertyDisplay("Trophy Weapons: show positioning hints on minimap", tooltip: "Also enables in-world movement hints (see Settings -> Boss Modules and Radar -> Show movement hints in world)")]
    public bool WeaponHints = true;

    [PropertyDisplay("Trophy Weapons: protean bait positions (relative to boss facing direction)", depends: nameof(WeaponHints))]
    [GroupDetails(["N", "NE", "E", "SE", "S", "SW", "W", "NW"])]
    [GroupPreset("PF standard clock spots", [0, 4, 6, 2, 5, 3, 7, 1])]
    public GroupAssignmentUnique WeaponHintsScythe = new() { Assignments = [0, 4, 6, 2, 5, 3, 7, 1] };

    public enum HintSword
    {
        [PropertyDisplay("No hint")]
        None,
        [PropertyDisplay("LP1 left, LP2 right looking at boss")]
        Standard,
        [PropertyDisplay("LP1 right, LP2 left looking at boss")]
        Flipped
    }

    [PropertyDisplay("Trophy Weapons: sword bait positions")]
    public HintSword WeaponHintsSword = HintSword.Standard;
}

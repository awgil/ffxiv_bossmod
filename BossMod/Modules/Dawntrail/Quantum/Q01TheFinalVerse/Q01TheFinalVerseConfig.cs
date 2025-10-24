namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class Q01TheFinalVerseConfig : ConfigNode
{
    // TODO are there any other usable orders
    public enum SinBearer
    {
        [PropertyDisplay("Don't assume any order")]
        None,
        [PropertyDisplay("MMRR -> Roles -> MMRR (accelerated 1st pass)")]
        AccelFirst,
    }

    [PropertyDisplay("Sin Bearer pass order", tooltip: "You must have each party member assigned a unique role in the Party Roles configuration to calculate pass order correctly, i.e. one DPS should be assigned to Melee and one should be assigned to Ranged")]
    public SinBearer SinBearerOrder = SinBearer.AccelFirst;
}

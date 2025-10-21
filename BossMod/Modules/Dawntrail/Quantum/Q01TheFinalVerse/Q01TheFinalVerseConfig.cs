namespace BossMod.Dawntrail.Quantum.Q01TheFinalVerse;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class Q01TheFinalVerseConfig : ConfigNode
{
    // TODO figure out if there are other sane orders to use? maybe without accelerated first pass?
    public enum SinBearer
    {
        [PropertyDisplay("Don't assume any order")]
        None,
        [PropertyDisplay("Partners (melee/ranged) -> roles (support/dps) -> partners")]
        PRP,
    }

    [PropertyDisplay("Sin Bearer pass order")]
    public SinBearer SinBearerOrder = SinBearer.PRP;
}

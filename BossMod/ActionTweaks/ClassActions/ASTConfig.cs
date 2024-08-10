namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class ASTConfig : ConfigNode
{
    public enum Placement
    {
        [PropertyDisplay("No override - use native targeting")]
        None,
        [PropertyDisplay("On self")]
        Self,
        [PropertyDisplay("On current target")]
        Target,
        [PropertyDisplay("At current arena center if a module is active, otherwise self")]
        ArenaOrSelf,
        [PropertyDisplay("At current arena center if a module is active, otherwise current target")]
        ArenaOrTarget,
    }

    [PropertyDisplay("Earthly Star position override")]
    public Placement StarPlacement = Placement.None;
}

namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class DRGConfig : ConfigNode
{
    // TODO: generalize to common utility
    public enum ElusiveJumpBehavior : uint
    {
        [PropertyDisplay("Game default (character-relative, backwards)")]
        Default = 0,

        [PropertyDisplay("Character-relative, forwards")]
        CharacterForward = 1,

        [PropertyDisplay("Camera-relative, backwards")]
        CameraBackward = 2,

        [PropertyDisplay("Camera-relative, forwards")]
        CameraForward = 3,
    }

    [PropertyDisplay("Elusive Jump direction")]
    public ElusiveJumpBehavior ElusiveJump = ElusiveJumpBehavior.Default;
}

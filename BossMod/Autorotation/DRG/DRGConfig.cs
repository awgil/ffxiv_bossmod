namespace BossMod;

[ConfigDisplay(Parent = typeof(AutorotationConfig))]
class DRGConfig : ConfigNode
{
    [PropertyDisplay("Execute optimal rotations on True Thrust (ST) or Doom Spike (AOE)")]
    public bool FullRotation = true;

    [PropertyDisplay("Smart targeting for Dragon Sight (target if friendly, otherwise mouseover if friendly, otherwise best player by class ranking)")]
    public bool SmartDragonSightTarget = true;

    public enum ElusiveJumpBehavior : uint {
        [PropertyDisplay("Unchanged (character-relative, backwards)")]
        Default = 0,
        [PropertyDisplay("Character-relative, forwards")]
        CharacterForward = 1,
        [PropertyDisplay("Camera-relative, backwards")]
        CameraBackward = 2,
        [PropertyDisplay("Camera-relative, forwards")]
        CameraForward = 3,
    }

    [PropertyDisplay("Elusive Jump behavior")]
    public ElusiveJumpBehavior ElusiveJump = ElusiveJumpBehavior.Default;
}

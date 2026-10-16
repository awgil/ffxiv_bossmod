namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig), Group = "Job-specific")]
class PLDConfig : ConfigNode
{
    public enum WingsBehavior : uint
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

    [PropertyDisplay("Passage of Arms direction")]
    public WingsBehavior Wings = WingsBehavior.Default;
}

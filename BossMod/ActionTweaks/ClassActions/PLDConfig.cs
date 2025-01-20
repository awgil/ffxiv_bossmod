namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class PLDConfig : ConfigNode
{
    [PropertyDisplay("Prevent use of 'Holy Spirit' too early when in pre-pull")]
    public bool ForbidEarlyHolySpirit = true;

    [PropertyDisplay("Prevent use of 'Shield Lob' too early when in pre-pull (if Holy Spirit is not unlocked)")]
    public bool ForbidEarlyShieldLob = true;

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

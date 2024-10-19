namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class PLDConfig : ConfigNode
{
    [PropertyDisplay("Prevent use of 'Holy Spirit' too early when in pre-pull")]
    public bool ForbidEarlyHolySpirit = true;

    [PropertyDisplay("Prevent use of 'Shield Lob' too early when in pre-pull (if Holy Spirit is not unlocked)")]
    public bool ForbidEarlyShieldLob = true;
}

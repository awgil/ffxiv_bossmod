namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class GNBConfig : ConfigNode
{
    [PropertyDisplay("Prevent use of 'Lightning Shot' too early when in pre-pull")]
    public bool ForbidEarlyLightningShot = true;
}

namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class SCHConfig : ConfigNode
{
    [PropertyDisplay("Prevent use of 'Broil' too early when in pre-pull")]
    public bool ForbidEarlyBroil = true;
}

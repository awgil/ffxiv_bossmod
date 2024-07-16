namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
class MCHConfig : ConfigNode
{
    [PropertyDisplay("Pause autorotation while channeling Flamethrower")]
    public bool PauseForFlamethrower = false;
}

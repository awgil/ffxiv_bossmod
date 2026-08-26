namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig))]
public class MCHConfig : ConfigNode
{
    [PropertyDisplay("Pause autorotation while channeling Flamethrower")]
    public bool PauseForFlamethrower = false;
}

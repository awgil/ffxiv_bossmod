namespace BossMod;

[ConfigDisplay(Parent = typeof(ActionTweaksConfig), Group = "Job-specific")]
public class MCHConfig : ConfigNode
{
    [PropertyDisplay("Pause autorotation while channeling Flamethrower")]
    public bool PauseForFlamethrower = false;
}

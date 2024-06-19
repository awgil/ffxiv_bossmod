namespace BossMod.Autorotation;

[ConfigDisplay(Name = "Autorotation settings", Order = 5)]
public sealed class AutorotationConfig : ConfigNode
{
    [PropertyDisplay("Show in-game UI")]
    public bool ShowUI = false;

    [PropertyDisplay("Show positional hints in world")]
    public bool ShowPositionals = false;

    [PropertyDisplay("Early pull threshold: if player enters combat when countdown is larger than this value, we consider it a ninja-pull and force disable autorotation")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;
}

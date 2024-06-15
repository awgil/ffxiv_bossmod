namespace BossMod;

// TODO: move to Autorotation namespace
[ConfigDisplay(Name = "Autorotation settings (experimental!)", Order = 5)]
public sealed class AutorotationConfig : ConfigNode
{
    [PropertyDisplay("Enable autorotation")]
    public bool Enabled = false;

    [PropertyDisplay("Log messages")]
    public bool Logging = false;

    [PropertyDisplay("Show in-game UI")]
    public bool ShowUI = false;

    [PropertyDisplay("Show positional hints in world")]
    public bool ShowPositionals = false;

    [PropertyDisplay("Enable actions that affect position (e.g. Onslaught, Primal Rend)")]
    public bool EnableMovement = true;

    [PropertyDisplay("Sticky auto actions")]
    public bool StickyAutoActions = false;

    [PropertyDisplay("Early pull threshold: if player enters combat when countdown is larger than this value, we consider it a ninja-pull and force disable autorotation")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;
}

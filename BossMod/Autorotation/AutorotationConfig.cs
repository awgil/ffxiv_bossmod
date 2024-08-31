namespace BossMod.Autorotation;

[ConfigDisplay(Name = "Autorotation settings", Order = 5)]
public sealed class AutorotationConfig : ConfigNode
{
    [PropertyDisplay("Show in-game UI")]
    public bool ShowUI = false;

    public enum DtrStatus
    {
        [PropertyDisplay("Disabled")]
        None,
        [PropertyDisplay("Text only")]
        TextOnly,
        [PropertyDisplay("With icon")]
        Icon
    }

    [PropertyDisplay("Preset display in DTR bar")]
    public DtrStatus ShowDTR = DtrStatus.None;

    [PropertyDisplay("Show positional hints in world")]
    public bool ShowPositionals = false;

    [PropertyDisplay("Automatically end autorotation when exiting combat")]
    public bool ClearPresetOnCombatEnd = false;

    [PropertyDisplay("Early pull threshold: if player enters combat when countdown is larger than this value, we consider it a ninja-pull and force disable autorotation")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;
}

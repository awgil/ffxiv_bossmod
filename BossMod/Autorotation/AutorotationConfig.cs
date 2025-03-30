﻿using BossMod.Endwalker.Criterion.C02AMR.C020Trash1;

namespace BossMod.Autorotation;

[ConfigDisplay(Name = "Autorotation", Order = 5)]
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

    [PropertyDisplay("Show autorotation preset in the server info bar")]
    public DtrStatus ShowDTR = DtrStatus.None;

    [PropertyDisplay("Hide VBM Default preset", tooltip: "If you've created your own presets and no longer need the included default, this option will prevent it from being shown in the Autorotation and Preset Editor windows.", since: "0.0.0.253")]
    public bool HideDefaultPreset = false;

    [PropertyDisplay("Show positional hints in world", tooltip: "Show tips for positional abilities, indicating to move to the flank or rear of your target")]
    public bool ShowPositionals = false;

    [PropertyDisplay("Automatically disable autorotation when exiting combat")]
    public bool ClearPresetOnCombatEnd = false;

    [PropertyDisplay("Automatically reenable force-disabled autorotation when exiting combat")]
    public bool ClearForceDisableOnCombatEnd = true;

    [PropertyDisplay("Early pull threshold", tooltip: "If someone enters combat with a boss when the countdown is longer than this value, it's consider a ninja-pull and autorotation is force disabled")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;

    [PropertyDisplay("Show player coordinates inside autorotation window", tooltip: "Shows your current XYZ coordinates in the autorotation window\nCan be useful for easier setup of AI plans")]
    public bool EnableCoordinates;
}

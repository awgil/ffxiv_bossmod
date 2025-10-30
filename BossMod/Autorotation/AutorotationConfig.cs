﻿namespace BossMod.Autorotation;

[ConfigDisplay(Name = "Autorotation", Order = 5)]
public sealed class AutorotationConfig : ConfigNode
{
    [PropertyDisplay("Show in-game UI")]
    public bool ShowUI = false;
    [PropertyDisplay("Disable Timers inside Autorotation window")]
    public bool DisableTimers = false;
    [PropertyDisplay("Show explicitly which GCD & OGCD is being queued next as well as last action used inside Autorotation window")]
    public bool EnableGCDs = false;
    [PropertyDisplay("Show if ability is a GCD or an OGCD when queuing actions inside Autorotation window")]
    public bool ShowGCDs = false;

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

    [PropertyDisplay("Show performance stats in the server info bar")]
    public bool ShowStatsDTR = false;

    [PropertyDisplay("Hide built-in presets", tooltip: "If you've created your own presets and no longer need the included defaults, this option will prevent them from being shown in the Autorotation and Preset Editor windows.", since: "0.0.0.253")]
    public bool HideDefaultPreset = false;

    [PropertyDisplay("Show positional hints in world", tooltip: "Show tips for positional abilities, indicating to move to the flank or rear of your target")]
    public bool ShowPositionals = false;

    [PropertyDisplay("Automatically disable autorotation on death", since: "0.4.4.1")]
    public bool ClearPresetOnDeath = true;

    [PropertyDisplay("Automatically disable autorotation when exiting combat")]
    public bool ClearPresetOnCombatEnd = false;

    [PropertyDisplay("Automatically disable autorotation if a Luring Trap is triggered", tooltip: "Only applicable in Deep Dungeons", since: "0.4.4.1")]
    public bool ClearPresetOnLuring = false;

    [PropertyDisplay("Automatically reenable force-disabled autorotation when exiting combat")]
    public bool ClearForceDisableOnCombatEnd = true;

    [PropertyDisplay("Early pull threshold", tooltip: "If someone enters combat with a boss when the countdown is longer than this value, it's consider a ninja-pull and autorotation is force disabled")]
    [PropertySlider(0, 30, Speed = 1)]
    public float EarlyPullThreshold = 1.5f;
}

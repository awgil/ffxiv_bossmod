namespace BossMod;

[ConfigDisplay(Name = "Boss Modules and Radar", Order = 1)]
public class BossModuleConfig : ConfigNode
{
    // boss module settings
    [PropertyDisplay("Minimal maturity for the module to be loaded", tooltip: "Some modules will have the \"WIP\" status and will not automatically load unless you change this")]
    public BossModuleInfo.Maturity MinMaturity = BossModuleInfo.Maturity.Contributed;

    [PropertyDisplay("Allow modules to automatically use actions", tooltip: "Example: modules can automatically use anti-knockback abilities before a knockback happens")]
    public bool AllowAutomaticActions = true;

    [PropertyDisplay("Show testing radar and hint window", tooltip: "Useful for configuring your radar and hint windows without being inside of a boss encounter", separator: true)]
    public bool ShowDemo = false;

    // radar window settings
    [PropertyDisplay("Enable radar")]
    public bool Enable = true;

    [PropertyDisplay("Lock radar and hint window movement and mouse interaction")]
    public bool Lock = false;

    [PropertyDisplay("Transparent radar window background", tooltip: "Removes the black window around the radar; this will not work if you move the radar to a different monitor")]
    public bool TrishaMode = false;

    [PropertyDisplay("Add opaque background to the arena in the radar")]
    public bool OpaqueArenaBackground = false;

    [PropertyDisplay("Show outlines and shadows on various radar markings")]
    public bool ShowOutlinesAndShadows = false;

    [PropertyDisplay("Radar arena scale factor", tooltip: "Scale of the arena inside of the radar window")]
    [PropertySlider(0.1f, 10, Speed = 0.1f, Logarithmic = true)]
    public float ArenaScale = 1;

    [PropertyDisplay("Rotate radar to match camera orientation")]
    public bool RotateArena = true;

    [PropertyDisplay("Give radar extra space for rotations", tooltip: "If you are using the above setting, this gives the radar extra space on the sides before the edges are clipped in order to account for rotating your camera during an encounter")]
    public bool AddSlackForRotations = true;

    [PropertyDisplay("Show arena border in radar")]
    public bool ShowBorder = true;

    [PropertyDisplay("Change arena border color if player is at risk", tooltip: "Changes the white border to red when you are standing somewhere you are likely to be hit by a mechanic")]
    public bool ShowBorderRisk = true;

    [PropertyDisplay("Show cardinal direction names on radar")]
    public bool ShowCardinals = false;

    [PropertyDisplay("Cardinal direction font size")]
    [PropertySlider(0.1f, 100, Speed = 1)]
    public float CardinalsFontSize = 17;

    [PropertyDisplay("Show waymarks on radar")]
    public bool ShowWaymarks = false;

    [PropertyDisplay("Always show all alive party members", separator: true)]
    public bool ShowIrrelevantPlayers = false;

    // hint window settings
    [PropertyDisplay("Show text hints in separate window", tooltip: "Separates the radar window from the hints window, allowing you to reposition the hints window")]
    public bool HintsInSeparateWindow = false;

    [PropertyDisplay("Show mechanic sequence and timer hints")]
    public bool ShowMechanicTimers = true;

    [PropertyDisplay("Show raidwide hints")]
    public bool ShowGlobalHints = true;

    [PropertyDisplay("Show player hints and warnings", separator: true)]
    public bool ShowPlayerHints = true;

    // misc. settings
    [PropertyDisplay("Show movement hints in world", tooltip: "Not used very much, but can show you arrows in the game world to indicate where to move for certain mechanics")]
    public bool ShowWorldArrows = false;
}

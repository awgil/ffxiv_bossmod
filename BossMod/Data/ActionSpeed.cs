namespace BossMod;

// reimplementation of game's action speed (i.e. effective gcd recast) calculations
// see ActionManager.AdjustRecastTimeByStatusesAndStats for details
// internally, all calculations are done in milliseconds with integer precision; when action request is executed, the cooldown is set to this value
// however, on server the value is rounded down to 10ms, and the extra packet later updates client's recast total; this rounding is also implemented for tooltips
public static class ActionSpeed
{
    // speed (sks/sps) stat is converted to modifier based on player's level
    public static int SpeedStatToModifier(int stat, int level)
    {
        var paramGrow = Service.LuminaRow<Lumina.Excel.Sheets.ParamGrow>((uint)level)!.Value;
        return 1000 - 130 * (stat - paramGrow.BaseSpeed) / paramGrow.LevelModifier;
    }

    public static int AdjustRecastMS(int baselineMS, int speedMod, int hasteMod) => Math.Max(baselineMS * speedMod / 1000 * hasteMod / 100, 1500);
    public static float Round(int ms) => (ms / 10) * 0.01f;

    public static int GCDRawMS(int speedStat, int hasteStat, int level, int baselineMS = 2500) => AdjustRecastMS(baselineMS, SpeedStatToModifier(speedStat, level), hasteStat);
    public static float GCDRounded(int speedStat, int hasteStat, int level, int baselineMS = 2500) => Round(GCDRawMS(speedStat, hasteStat, level, baselineMS));
}

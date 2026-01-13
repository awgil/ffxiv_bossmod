namespace BossMod.Dawntrail.Savage.RM10STheXtremes;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
public class RM10STheXtremesConfig : ConfigNode
{
    public enum CleanseOrder
    {
        [PropertyDisplay("Don't assume any order")]
        None,
        [PropertyDisplay("Healers -> Melee -> Ranged")]
        HMR
    }

    [PropertyDisplay("Insane Air 2: Cleanse order")]
    public CleanseOrder IA2CleanseOrder = CleanseOrder.HMR;
}

namespace BossMod.Dawntrail.Foray.ForkedTowerMagic.Normal.FTMN1TwoHeadedAevis;

[ConfigDisplay(Order = 0x171, Parent = typeof(DawntrailConfig))]
public sealed class TwoHeadedAevisConfig : ConfigNode
{
    [PropertyDisplay("Force AI to target assigned head")]
    public bool ForceTargeting = false;
}

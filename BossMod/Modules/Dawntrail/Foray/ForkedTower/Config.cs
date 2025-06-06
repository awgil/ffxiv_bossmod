namespace BossMod.Dawntrail.Foray.ForkedTower;

[ConfigDisplay(Parent = typeof(DawntrailConfig))]
class ForkedTowerConfig : ConfigNode
{
    public enum Alliance
    {
        [PropertyDisplay("None - only show generic hints")]
        None,
        A,
        B,
        C,
        [PropertyDisplay("D/1")]
        D1,
        [PropertyDisplay("E/2")]
        E2,
        [PropertyDisplay("F/3")]
        F3
    }

    [PropertyDisplay("Alliance assignment for hints")]
    public Alliance PlayerAlliance = Alliance.None;
}

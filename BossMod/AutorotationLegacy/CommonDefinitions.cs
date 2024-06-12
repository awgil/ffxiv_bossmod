namespace BossMod;

public static class CommonDefinitions
{
    // check whether given actor has tank stance
    public static bool HasTankStance(Actor a)
    {
        var stanceSID = a.Class switch
        {
            Class.WAR => (uint)WAR.SID.Defiance,
            Class.PLD => (uint)PLD.SID.IronWill,
            Class.GNB => (uint)GNB.SID.RoyalGuard,
            _ => 0u
        };
        return stanceSID != 0 && a.FindStatus(stanceSID) != null;
    }
}

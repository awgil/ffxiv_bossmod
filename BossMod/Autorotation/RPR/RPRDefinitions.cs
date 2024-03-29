namespace BossMod.RPR;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

    // single target GCDs
    Slice = 24373,
    WaxingSlice = 24374, 
    InfernalSlice = 24375,
    ShadowofDeath = 24378,
    SoulSlice = 24380,
    SoulScythe = 24381,
    Gibbet = 24382,
    Gallows = 24383,
    PlentifulHarvest = 24385,
    VoidReaping = 24395,
    CrossReaping = 24396,

    // aoe GCDs
    SpinningScythe = 24376, 
    NightmareScythe = 24377, 
    WhorlofDeath = 24379, 
    Guillotine = 24384,
    GrimReaping = 24397,
    HarvestMoon = 24388,

    // oGCDs
    BloodStalk = 24389,
    UnveiledGibbet = 24390,
    UnveiledGallows = 24391,
    GrimSwathe = 24392,
    Gluttony = 24393,
    Enshroud = 24394,
    LemuresSlice = 24399,
    LemuresScythe = 24400,

    // offsensive CDs
    ArcaneCircle = 24405,

    // defensive CDs
    ArcaneCrest = 24404,
    SecondWind = 7541,
    Bloodbath = 7542, 
    Feint = 7549,
    ArmsLength = 7548,

    // misc
    Harpe = 24386,
    SoulSow = 24387,
    Communio = 24398,
    TrueNorth = 7546,
    LegSweep = 7863, 

    //LB3
}

public enum TraitID : uint
{
    None = 0,
    SoulGauge = 379,
    DeathScytheMastery1 = 380, 
    EnhancedAvatar = 381,
    Hellsgate = 382, 
    TemperedSoul = 383,
    ShroudGauge = 384,  
    EnhancedArcaneCrest = 385, 
    DeathScytheMastery2 = 523, 
    EnhancedShroud = 386, 
    EnhancedArcaneCircle = 387,
}

public enum CDGroup : int
{
    BloodStalk = 0, 
    GrimSwathe = 0, 
    Gluttony = 9, 
    Enshroud = 2, 
    ArcaneCircle = 14, 
    ArcaneCrest = 5, 
    UnveiledGibbet = 0,
    UnveiledGallows = 0,
    LemuresSlice = 0,
    LemuresScythe = 0,
    SoulSlice = 4,
    SoulScythe = 4,
    LegSweep = 41, 
    TrueNorth = 45, 
    Bloodbath = 46, 
    Feint = 47, 
    ArmsLength = 48,
    SecondWind = 49,
}

public enum SID : uint
{
    None = 0,
    DeathsDesign = 2586,
    SoulReaver = 2587,
    ImmortalSacrifice = 2592,
    ArcaneCircle = 2599,
    EnhancedGibbet = 2588,
    EnhancedGallows = 2589,
    EnhancedVoidReaping = 2590,
    EnhancedCrossReaping = 2591,
    EnhancedHarpe = 2845,
    Enshrouded = 2593,
    Soulsow = 2594,
    Threshold = 2595,
    CircleofSacrifice = 2600,
    BloodsownCircle = 2972,
    TrueNorth = 1250,
    Bloodbath = 84, 
    Feint = 1195, 
    Stun = 2, 
}

public static class Definitions
{
    public static uint[] UnlockQuests = { 69609, 69614 };

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
        {
            AID.Slice => level >= 1,
            AID.WaxingSlice => level >= 5,
            AID.SecondWind => level >= 8,
            AID.ShadowofDeath => level >= 10,
            AID.LegSweep => level >= 10,
            AID.Bloodbath => level >= 12,
            AID.Harpe => level >= 15,
            AID.Feint => level >= 22,
            AID.SpinningScythe => level >= 25,
            AID.InfernalSlice => level >= 30,
            AID.WhorlofDeath => level >= 35,
            AID.ArmsLength => level >= 32,
            AID.NightmareScythe => level >= 45,
            AID.TrueNorth => level >= 50,
            AID.SoulSlice => level >= 60,
            AID.SoulScythe => level >= 65,
            AID.Gibbet => level >= 70,
            AID.Gallows => level >= 70,
            AID.Guillotine => level >= 70,
            AID.PlentifulHarvest => level >= 88,
            AID.SoulSow => level >= 82,
            AID.Communio => level >= 90 ,
            AID.ArcaneCrest => level >= 40,
            AID.BloodStalk => level >= 50,
            AID.GrimSwathe => level >= 55,
            AID.ArcaneCircle => level >= 72,
            AID.Gluttony => level >= 76,
            AID.Enshroud => level >= 80,
            AID.UnveiledGallows => level >= 70,
            AID.UnveiledGibbet => level >= 70,
            AID.VoidReaping => level >= 80,
            AID.CrossReaping => level >= 80,
            AID.GrimReaping => level >= 80,
            AID.HarvestMoon => level >= 82,
            AID.LemuresSlice => level >= 86,
            AID.LemuresScythe => level >= 86,
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
        {
            TraitID.SoulGauge => level >= 50,
            TraitID.DeathScytheMastery1 => level >= 60,
            TraitID.EnhancedAvatar => level >= 70,
            TraitID.Hellsgate => level >= 74,
            TraitID.TemperedSoul => level >= 78,
            TraitID.ShroudGauge => level >= 80,
            TraitID.EnhancedArcaneCrest => level >= 84,
            TraitID.DeathScytheMastery2 => level >= 84,
            TraitID.EnhancedShroud => level >= 86,
            TraitID.EnhancedArcaneCircle => level >= 88,
            _ => true,
        };
    }

    public static Dictionary<ActionID, ActionDefinition> SupportedActions;
    static Definitions()
    {
        SupportedActions = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
        SupportedActions.GCD(AID.Slice, 3);
        SupportedActions.GCD(AID.WaxingSlice, 3);
        SupportedActions.GCD(AID.InfernalSlice, 3);
        SupportedActions.GCD(AID.SpinningScythe, 5);
        SupportedActions.GCD(AID.NightmareScythe, 5);
        SupportedActions.GCD(AID.ShadowofDeath, 3);
        SupportedActions.GCD(AID.VoidReaping, 3);
        SupportedActions.GCD(AID.CrossReaping, 3);
        SupportedActions.GCD(AID.GrimReaping, 8);
        SupportedActions.GCD(AID.WhorlofDeath, 5);
        SupportedActions.GCD(AID.Harpe, 25);
        SupportedActions.GCD(AID.SoulSow, 0);
        SupportedActions.GCDWithCharges(AID.SoulSlice, 3, CDGroup.SoulSlice, 30.0f, 2);
        SupportedActions.GCDWithCharges(AID.SoulScythe, 5, CDGroup.SoulScythe, 30.0f, 2);
        SupportedActions.GCD(AID.Gibbet, 3);
        SupportedActions.GCD(AID.Gallows, 3);
        SupportedActions.GCD(AID.Guillotine, 8);
        SupportedActions.GCD(AID.PlentifulHarvest, 15);
        SupportedActions.GCD(AID.HarvestMoon, 25);
        SupportedActions.GCDCast(AID.Communio, 25, 1.3f);
        SupportedActions.OGCD(AID.ArcaneCrest, 0, CDGroup.ArcaneCrest, 30.0f).EffectDuration = 5;
        SupportedActions.OGCD(AID.BloodStalk, 3, CDGroup.BloodStalk, 1.0f);
        SupportedActions.OGCD(AID.UnveiledGallows, 3, CDGroup.UnveiledGallows, 1.0f);
        SupportedActions.OGCD(AID.UnveiledGibbet, 3, CDGroup.UnveiledGibbet, 1.0f);
        SupportedActions.OGCD(AID.GrimSwathe, 8, CDGroup.GrimSwathe, 1.0f);
        SupportedActions.OGCD(AID.ArcaneCircle, 0, CDGroup.ArcaneCircle, 120.0f).EffectDuration = 20;
        SupportedActions.OGCD(AID.Gluttony, 25, CDGroup.Gluttony, 60.0f);
        SupportedActions.OGCD(AID.Enshroud, 0, CDGroup.Enshroud, 15.0f);
        SupportedActions.OGCD(AID.LemuresSlice, 3, CDGroup.LemuresSlice, 1.0f);
        SupportedActions.OGCD(AID.LemuresScythe, 5, CDGroup.LemuresScythe, 1.0f);
        SupportedActions.OGCD(AID.SecondWind, 0, CDGroup.SecondWind, 120.0f);
        SupportedActions.OGCD(AID.Bloodbath, 0, CDGroup.Bloodbath, 90.0f).EffectDuration = 20;
        SupportedActions.OGCD(AID.Feint, 10, CDGroup.Feint, 90.0f).EffectDuration = 10;
        SupportedActions.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
        SupportedActions.OGCDWithCharges(AID.TrueNorth, 0, CDGroup.TrueNorth, 45.0f, 2).EffectDuration = 10;
        SupportedActions.OGCD(AID.LegSweep, 3, CDGroup.LegSweep, 40.0f);
    }
}

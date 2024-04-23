namespace BossMod.GNB;

public enum AID : uint
{
    None = 0,
    Sprint = 3,

    // GCDs
    KeenEdge = 16137, // L1, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    BrutalShell = 16139, // L4, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    DemonSlice = 16141, // L10, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    LightningShot = 16143, // L15, instant, range 20, single-target 0/0, targets=hostile, animLock=???
    SolidBarrel = 16145, // L26, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    BurstStrike = 16162, // L30, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    DemonSlaughter = 16149, // L40, instant, range 0, AOE circle 5/0, targets=self, animLock=???
    WickedTalon = 16150, // L60, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    SavageClaw = 16147, // L60, instant, range 3, single-target 0/0, targets=hostile, animLock=???
    FatedCircle = 16163, // L72, instant, range 0, AOE circle 5/0, targets=self, animLock=???

    // oGCDs
    NoMercy = 16138, // L2, instant, 60.0s CD (group 10), range 0, single-target 0/0, targets=self, animLock=???
    Camouflage = 16140, // L6, instant, 90.0s CD (group 15), range 0, single-target 0/0, targets=self, animLock=???
    Rampart = 7531, // L8, instant, 90.0s CD (group 46), range 0, single-target 0/0, targets=self, animLock=???
    RoyalGuard = 16142, // L10, instant, 2.0s CD (group 1), range 0, single-target 0/0, targets=self, animLock=???
    ReleaseRoyalGuard = 32068, // L10, instant, 1.0s CD (group 1), range 0, single-target 0/0, targets=self, animLock=???
    LowBlow = 7540, // L12, instant, 25.0s CD (group 41), range 3, single-target 0/0, targets=hostile, animLock=???
    Provoke = 7533, // L15, instant, 30.0s CD (group 42), range 25, single-target 0/0, targets=hostile, animLock=???
    Interject = 7538, // L18, instant, 30.0s CD (group 43), range 3, single-target 0/0, targets=hostile, animLock=???
    DangerZone = 16144, // L18, instant, 30.0s CD (group 4), range 3, single-target 0/0, targets=hostile, animLock=???
    Reprisal = 7535, // L22, instant, 60.0s CD (group 44), range 0, AOE circle 5/0, targets=self, animLock=???
    ArmsLength = 7548, // L32, instant, 120.0s CD (group 48), range 0, single-target 0/0, targets=self, animLock=???
    Nebula = 16148, // L38, instant, 120.0s CD (group 21), range 0, single-target 0/0, targets=self, animLock=???
    Aurora = 16151, // L45, instant, 60.0s CD (group 19), range 30, single-target 0/0, targets=self/party/friendly, animLock=???
    Shirk = 7537, // L48, instant, 120.0s CD (group 49), range 25, single-target 0/0, targets=party, animLock=???
    Superbolide = 16152, // L50, instant, 360.0s CD (group 24), range 0, single-target 0/0, targets=self, animLock=???
    SonicBreak = 16153, // L54, instant, 60.0s CD (group 13), range 3, single-target 0/0, targets=hostile, animLock=???
    RoughDivide = 16154, // L56, instant, 30.0s CD (group 9) (2 charges), range 20, single-target 0/0, targets=hostile, animLock=???
    GnashingFang = 16146, // L60, instant, 30.0s CD (group 5), range 3, single-target 0/0, targets=hostile, animLock=???
    BowShock = 16159, // L62, instant, 60.0s CD (group 11), range 0, AOE circle 5/0, targets=self, animLock=???
    HeartOfLight = 16160, // L64, instant, 90.0s CD (group 16), range 0, AOE circle 30/0, targets=self, animLock=???
    HeartOfStone = 16161, // L68, instant, 25.0s CD (group 3), range 30, single-target 0/0, targets=self/party, animLock=???
    EyeGouge = 16158, // L70, instant, 1.0s CD (group 0), range 5, single-target 0/0, targets=hostile, animLock=???
    JugularRip = 16156, // L70, instant, 1.0s CD (group 0), range 5, single-target 0/0, targets=hostile, animLock=???
    Continuation = 16155, // L70, instant, 1.0s CD (group 0), range 0, single-target 0/0, targets=self, animLock=???
    AbdomenTear = 16157, // L70, instant, 1.0s CD (group 0), range 5, single-target 0/0, targets=hostile, animLock=???
    Bloodfest = 16164, // L76, instant, 120.0s CD (group 14), range 25, single-target 0/0, targets=hostile, animLock=???
    BlastingZone = 16165, // L80, instant, 30.0s CD (group 4), range 3, single-target 0/0, targets=hostile, animLock=???
    HeartOfCorundum = 25758, // L82, instant, 25.0s CD (group 3), range 30, single-target 0/0, targets=self/party, animLock=???
    Hypervelocity = 25759, // L86, instant, 1.0s CD (group 0), range 5, single-target 0/0, targets=hostile, animLock=???
    DoubleDown = 25760, // L90, instant, 60.0s CD (group 12), range 0, AOE circle 5/0, targets=self, animLock=???

    // special
    GunmetalSoul = 17105, // LB3
}

public enum TraitID : uint
{
    None = 0,
    TankMastery = 320, // L1
    CartridgeCharge = 257, // L30
    EnhancedBrutalShell = 258, // L52
    DangerZoneMastery = 259, // L80
    HeartOfStoneMastery = 424, // L82
    EnhancedAurora = 425, // L84
    MeleeMastery = 507, // L84
    EnhancedContinuation = 426, // L86
    CartridgeChargeII = 427, // L88
}

public enum CDGroup : int
{
    EyeGouge = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    JugularRip = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    Continuation = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    AbdomenTear = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    Hypervelocity = 0, // 1.0 max, shared by Eye Gouge, Jugular Rip, Continuation, Abdomen Tear, Hypervelocity
    RoyalGuard = 1, // variable max, shared by Royal Guard, Release Royal Guard
    ReleaseRoyalGuard = 1, // variable max, shared by Royal Guard, Release Royal Guard
    HeartOfStone = 3, // 25.0 max, shared by Heart of Stone, Heart of Corundum
    HeartOfCorundum = 3, // 25.0 max, shared by Heart of Stone, Heart of Corundum
    DangerZone = 4, // 30.0 max, shared by Danger Zone, Blasting Zone
    BlastingZone = 4, // 30.0 max, shared by Danger Zone, Blasting Zone
    GnashingFang = 5, // 30.0 max
    RoughDivide = 9, // 2*30.0 max
    NoMercy = 10, // 60.0 max
    BowShock = 11, // 60.0 max
    DoubleDown = 12, // 60.0 max
    SonicBreak = 13, // 60.0 max
    Bloodfest = 14, // 120.0 max
    Camouflage = 15, // 90.0 max
    HeartOfLight = 16, // 90.0 max
    Aurora = 19, // 2*60.0 max
    Nebula = 21, // 120.0 max
    Superbolide = 24, // 360.0 max
    LowBlow = 41, // 25.0 max
    Provoke = 42, // 30.0 max
    Interject = 43, // 30.0 max
    Reprisal = 44, // 60.0 max
    Rampart = 46, // 90.0 max
    ArmsLength = 48, // 120.0 max
    Shirk = 49, // 120.0 max
    LimitBreak = 71, // special/fake (TODO: remove need for it?)
}

public enum SID : uint
{
    None = 0,
    BrutalShell = 1898, // applied by Brutal Shell to self
    NoMercy = 1831, // applied by No Mercy to self
    ReadyToRip = 1842, // applied by Gnashing Fang to self
    SonicBreak = 1837, // applied by Sonic Break to target
    BowShock = 1838, // applied by Bow Shock to target
    ReadyToTear = 1843, // applied by Savage Claw to self
    ReadyToGouge = 1844, // applied by Wicked Talon to self
    ReadyToBlast = 2686, // applied by Burst Strike to self
    Nebula = 1834, // applied by Nebula to self
    Rampart = 1191, // applied by Rampart to self
    Reprisal = 1193, // applied by Reprisal to target
    Camouflage = 1832, // applied by Camouflage to self
    ArmsLength = 1209, // applied by Arm's Length to self
    HeartOfLight = 1839, // applied by Heart of Light to self
    Aurora = 1835, // applied by Aurora to self
    Superbolide = 1836, // applied by Superbolide to self
    HeartOfCorundum = 2683, // applied by Heart of Corundum to self
    ClarityOfCorundum = 2684, // applied by Heart of Corundum to self
    CatharsisOfCorundum = 2685, // applied by Heart of Corundum to self
    RoyalGuard = 1833, // applied by Royal Guard to self
    Stun = 2, // applied by Low Blow to target
}

public static class Definitions
{
    public static readonly uint[] UnlockQuests = [68802];

    public static bool Unlocked(AID aid, int level, int questProgress)
    {
        return aid switch
        {
            AID.NoMercy => level >= 2,
            AID.BrutalShell => level >= 4,
            AID.Camouflage => level >= 6,
            AID.Rampart => level >= 8,
            AID.RoyalGuard => level >= 10,
            AID.DemonSlice => level >= 10,
            AID.ReleaseRoyalGuard => level >= 10,
            AID.LowBlow => level >= 12,
            AID.Provoke => level >= 15,
            AID.LightningShot => level >= 15,
            AID.Interject => level >= 18,
            AID.DangerZone => level >= 18,
            AID.Reprisal => level >= 22,
            AID.SolidBarrel => level >= 26,
            AID.BurstStrike => level >= 30,
            AID.ArmsLength => level >= 32,
            AID.Nebula => level >= 38,
            AID.DemonSlaughter => level >= 40,
            AID.Aurora => level >= 45,
            AID.Shirk => level >= 48,
            AID.Superbolide => level >= 50,
            AID.SonicBreak => level >= 54,
            AID.RoughDivide => level >= 56,
            AID.WickedTalon => level >= 60,
            AID.GnashingFang => level >= 60,
            AID.SavageClaw => level >= 60,
            AID.BowShock => level >= 62,
            AID.HeartOfLight => level >= 64,
            AID.HeartOfStone => level >= 68,
            AID.EyeGouge => level >= 70 && questProgress > 0,
            AID.JugularRip => level >= 70 && questProgress > 0,
            AID.Continuation => level >= 70 && questProgress > 0,
            AID.AbdomenTear => level >= 70 && questProgress > 0,
            AID.FatedCircle => level >= 72,
            AID.Bloodfest => level >= 76,
            AID.BlastingZone => level >= 80,
            AID.HeartOfCorundum => level >= 82,
            AID.Hypervelocity => level >= 86,
            AID.DoubleDown => level >= 90,
            _ => true,
        };
    }

    public static bool Unlocked(TraitID tid, int level, int questProgress)
    {
        return tid switch
        {
            TraitID.CartridgeCharge => level >= 30,
            TraitID.EnhancedBrutalShell => level >= 52,
            TraitID.DangerZoneMastery => level >= 80,
            TraitID.HeartOfStoneMastery => level >= 82,
            TraitID.EnhancedAurora => level >= 84,
            TraitID.MeleeMastery => level >= 84,
            TraitID.EnhancedContinuation => level >= 86,
            TraitID.CartridgeChargeII => level >= 88,
            _ => true,
        };
    }

    public static readonly Dictionary<ActionID, ActionDefinition> SupportedActions = BuildSupportedActions();
    private static Dictionary<ActionID, ActionDefinition> BuildSupportedActions()
    {
        var res = CommonDefinitions.CommonActionData(CommonDefinitions.IDPotionStr);
        res.GCD(AID.KeenEdge, 3);
        res.OGCD(AID.NoMercy, 0, CDGroup.NoMercy, 60.0f);
        res.GCD(AID.BrutalShell, 3);
        res.OGCD(AID.Camouflage, 0, CDGroup.Camouflage, 90.0f).EffectDuration = 20;
        res.OGCD(AID.Rampart, 0, CDGroup.Rampart, 90.0f).EffectDuration = 20;
        res.OGCD(AID.RoyalGuard, 0, CDGroup.RoyalGuard, 2.0f);
        res.GCD(AID.DemonSlice, 0);
        res.OGCD(AID.ReleaseRoyalGuard, 0, CDGroup.ReleaseRoyalGuard, 1.0f);
        res.OGCD(AID.LowBlow, 3, CDGroup.LowBlow, 25.0f);
        res.OGCD(AID.Provoke, 25, CDGroup.Provoke, 30.0f);
        res.GCD(AID.LightningShot, 20);
        res.OGCD(AID.Interject, 3, CDGroup.Interject, 30.0f);
        res.OGCD(AID.DangerZone, 3, CDGroup.DangerZone, 30.0f);
        res.OGCD(AID.Reprisal, 0, CDGroup.Reprisal, 60.0f).EffectDuration = 10;
        res.GCD(AID.SolidBarrel, 3);
        res.GCD(AID.BurstStrike, 3);
        res.OGCD(AID.ArmsLength, 0, CDGroup.ArmsLength, 120.0f).EffectDuration = 6;
        res.OGCD(AID.Nebula, 0, CDGroup.Nebula, 120.0f).EffectDuration = 15;
        res.GCD(AID.DemonSlaughter, 0);
        res.OGCDWithCharges(AID.Aurora, 30, CDGroup.Aurora, 60.0f, 2).EffectDuration = 18;
        res.OGCD(AID.Shirk, 25, CDGroup.Shirk, 120.0f);
        res.OGCD(AID.Superbolide, 0, CDGroup.Superbolide, 360.0f).EffectDuration = 10;
        res.OGCD(AID.SonicBreak, 3, CDGroup.SonicBreak, 60.0f);
        res.OGCDWithCharges(AID.RoughDivide, 20, CDGroup.RoughDivide, 30.0f, 2);
        res.GCD(AID.WickedTalon, 3, 0.770f);
        res.OGCD(AID.GnashingFang, 3, CDGroup.GnashingFang, 30.0f, 0.700f);
        res.GCD(AID.SavageClaw, 3, 0.500f);
        res.OGCD(AID.BowShock, 0, CDGroup.BowShock, 60.0f);
        res.OGCD(AID.HeartOfLight, 0, CDGroup.HeartOfLight, 90.0f).EffectDuration = 15;
        res.OGCD(AID.HeartOfStone, 30, CDGroup.HeartOfStone, 25.0f).EffectDuration = 4;
        res.OGCD(AID.EyeGouge, 5, CDGroup.EyeGouge, 1.0f);
        res.OGCD(AID.JugularRip, 5, CDGroup.JugularRip, 1.0f);
        res.OGCD(AID.Continuation, 0, CDGroup.Continuation, 1.0f);
        res.OGCD(AID.AbdomenTear, 5, CDGroup.AbdomenTear, 1.0f);
        res.GCD(AID.FatedCircle, 0);
        res.OGCD(AID.Bloodfest, 25, CDGroup.Bloodfest, 120.0f);
        res.OGCD(AID.BlastingZone, 3, CDGroup.BlastingZone, 30.0f);
        res.OGCD(AID.HeartOfCorundum, 30, CDGroup.HeartOfCorundum, 25.0f).EffectDuration = 4;
        res.OGCD(AID.Hypervelocity, 5, CDGroup.Hypervelocity, 1.0f);
        res.OGCD(AID.DoubleDown, 0, CDGroup.DoubleDown, 60.0f);
        res.OGCD(AID.GunmetalSoul, 0, CDGroup.LimitBreak, 0, 3.86f);
        return res;
    }
}

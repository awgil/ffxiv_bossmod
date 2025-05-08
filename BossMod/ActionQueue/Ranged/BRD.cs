namespace BossMod.BRD;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    SagittariusArrow = 4244, // LB3, 4.5s cast, range 30, AOE 30+R width 8 rect, targets=Hostile, animLock=3.700s?
    HeavyShot = 97, // L1, instant, GCD, range 25, single-target, targets=Hostile
    StraightShot = 98, // L2, instant, GCD, range 25, single-target, targets=Hostile
    RagingStrikes = 101, // L4, instant, 120.0s CD (group 14), range 0, single-target, targets=Self
    VenomousBite = 100, // L6, instant, GCD, range 25, single-target, targets=Hostile
    Bloodletter = 110, // L12, instant, 15.0s CD (group 9/70) (2-3 charges), range 25, single-target, targets=Hostile
    RepellingShot = 112, // L15, instant, 30.0s CD (group 5), range 15, single-target, targets=Hostile, animLock=0.800s
    QuickNock = 106, // L18, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=Hostile
    WideVolley = 36974, // L25, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    MagesBallad = 114, // L30, instant, 120.0s CD (group 15), range 0, single-target, targets=Self
    Windbite = 113, // L30, instant, GCD, range 25, single-target, targets=Hostile
    WardensPaean = 3561, // L35, instant, 45.0s CD (group 10), range 30, single-target, targets=Self/Party
    Barrage = 107, // L38, instant, 120.0s CD (group 19), range 0, single-target, targets=Self
    ArmysPaeon = 116, // L40, instant, 120.0s CD (group 16), range 0, single-target, targets=Self
    RainOfDeath = 117, // L45, instant, 15.0s CD (group 9/70) (2-3 charges), range 25, AOE 8 circle, targets=Hostile
    BattleVoice = 118, // L50, instant, 120.0s CD (group 18), range 0, AOE 30 circle, targets=Self
    WanderersMinuet = 3559, // L52, instant, 120.0s CD (group 17), range 0, single-target, targets=Self
    PitchPerfect = 7404, // L52, instant, 1.0s CD (group 0), range 25, AOE 5 circle, targets=Hostile
    EmpyrealArrow = 3558, // L54, instant, 15.0s CD (group 2), range 25, single-target, targets=Hostile
    IronJaws = 3560, // L56, instant, GCD, range 25, single-target, targets=Hostile
    Sidewinder = 3562, // L60, instant, 60.0s CD (group 12), range 25, single-target, targets=Hostile
    Troubadour = 7405, // L62, instant, 120.0s CD (group 20), range 0, AOE 30 circle, targets=Self
    Stormbite = 7407, // L64, instant, GCD, range 25, single-target, targets=Hostile
    CausticBite = 7406, // L64, instant, GCD, range 25, single-target, targets=Hostile
    NaturesMinne = 7408, // L66, instant, 120.0s CD (group 21), range 0, AOE 30 circle, targets=Self
    RefulgentArrow = 7409, // L70, instant, GCD, range 25, single-target, targets=Hostile
    Shadowbite = 16494, // L72, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    BurstShot = 16495, // L76, instant, GCD, range 25, single-target, targets=Hostile
    ApexArrow = 16496, // L80, instant, GCD, range 25, AOE 25+R width 4 rect, targets=Hostile
    Ladonsbite = 25783, // L82, instant, GCD, range 12, AOE 12+R ?-degree cone, targets=Hostile
    BlastArrow = 25784, // L86, instant, GCD, range 25, AOE 25+R width 4 rect, targets=Hostile
    RadiantFinale = 25785, // L90, instant, 110.0s CD (group 13), range 0, AOE 30 circle, targets=Self
    HeartbreakShot = 36975, // L92, instant, 15.0s CD (group 9/70) (3 charges), range 25, single-target, targets=Hostile
    ResonantArrow = 36976, // L96, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    RadiantEncore = 36977, // L100, instant, GCD, range 25, AOE 5 circle, targets=Hostile

    // Shared
    BigShot = ClassShared.AID.BigShot, // LB1, 2.0s cast, range 30, AOE 30+R width 4 rect, targets=Hostile, animLock=3.100s?
    Desperado = ClassShared.AID.Desperado, // LB2, 3.0s cast, range 30, AOE 30+R width 5 rect, targets=Hostile, animLock=3.100s?
    LegGraze = ClassShared.AID.LegGraze, // L6, instant, 30.0s CD (group 42), range 25, single-target, targets=Hostile
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=Self
    FootGraze = ClassShared.AID.FootGraze, // L10, instant, 30.0s CD (group 41), range 25, single-target, targets=Hostile
    Peloton = ClassShared.AID.Peloton, // L20, instant, 5.0s CD (group 40), range 0, AOE 30 circle, targets=Self
    HeadGraze = ClassShared.AID.HeadGraze, // L24, instant, 30.0s CD (group 43), range 25, single-target, targets=Hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    HeavierShot = 17, // L2
    IncreasedActionDamage1 = 18, // L20
    EnhancedQuickNock = 283, // L25
    IncreasedActionDamage2 = 20, // L40
    BiteMastery1 = 168, // L64
    EnhancedEmpyrealArrow = 169, // L68
    StraightShotMastery = 282, // L70
    WideVolleyMastery = 598, // L72
    BiteMastery2 = 284, // L76
    HeavyShotMastery = 285, // L76
    EnhancedArmysPaeon = 286, // L78
    SoulVoice = 287, // L80
    QuickNockMastery = 444, // L82
    EnhancedBloodletter = 445, // L84
    EnhancedApexArrow = 446, // L86
    EnhancedTroubadour = 447, // L88
    MinstrelsCoda = 448, // L90
    BloodletterMastery = 599, // L92
    EnhancedSecondWind = 642, // L94
    RangedMastery = 668, // L94
    EnhancedBarrage = 600, // L96
    EnhancedTroubadourII = 601, // L98
    EnhancedRadiantFinale = 602, // L100
}

public enum SID : uint
{
    None = 0,
    RagingStrikes = 125, // applied by Raging Strikes to self
    ArmsLength = 1209, // applied by Arm's Length to self
    Barrage = 128, // applied by Barrage to self
    BattleVoice = 141, // applied by Battle Voice to self/target
    HawksEye = 3861, // applied by Iron Jaws, Stormbite, Caustic Bite, Burst Shot to self
    CausticBite = 1200, // applied by Iron Jaws, Caustic Bite to target
    Stormbite = 1201, // applied by Iron Jaws, Stormbite to target
    BlastArrowReady = 2692, // applied by Apex Arrow to self
    RadiantFinale = 2964, // applied by Radiant Finale to self/target
    RadiantFinaleVisual = 2722, // applied by Radiant Finale to self
    //StraightShotReady = 122, // applied by Barrage, Iron Jaws, Caustic Bite, Stormbite, Burst Shot to self
    VenomousBite = 124, // applied by Venomous Bite, dot
    Windbite = 129, // applied by Windbite, dot
    //ShadowbiteReady = 3002, // applied by Ladonsbite to self
    NaturesMinne = 1202, // applied by Nature's Minne to self
    WardensPaean = 866, // applied by the Warden's Paean to self
    Troubadour = 1934, // applied by Troubadour to self
    Bind = 13, // applied by Foot Graze to target
    ArmysMuse = 1932, // applied when using song when either army's paeon is active or army's ethos is up
    ArmysEthos = 1933, // applied when leaving army's paeon without starting new song
    ResonantArrowReady = 3862, // applied by Barrage to self
    RadiantEncoreReady = 3863, // applied by Radiant Finale to self

    //Shared
    Peloton = ClassShared.SID.Peloton, // applied by Peloton to self/party
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.SagittariusArrow, true, castAnimLock: 3.70f); // animLock=3.700s?
        d.RegisterSpell(AID.HeavyShot, true);
        d.RegisterSpell(AID.StraightShot, true);
        d.RegisterSpell(AID.RagingStrikes, true);
        d.RegisterSpell(AID.VenomousBite, true);
        d.RegisterSpell(AID.Bloodletter, true);
        d.RegisterSpell(AID.RepellingShot, true, instantAnimLock: 0.80f);
        d.RegisterSpell(AID.QuickNock, true);
        d.RegisterSpell(AID.WideVolley, true);
        d.RegisterSpell(AID.MagesBallad, true);
        d.RegisterSpell(AID.Windbite, true);
        d.RegisterSpell(AID.WardensPaean, true);
        d.RegisterSpell(AID.Barrage, true);
        d.RegisterSpell(AID.ArmysPaeon, true);
        d.RegisterSpell(AID.RainOfDeath, true);
        d.RegisterSpell(AID.BattleVoice, true);
        d.RegisterSpell(AID.WanderersMinuet, true);
        d.RegisterSpell(AID.PitchPerfect, true);
        d.RegisterSpell(AID.EmpyrealArrow, true);
        d.RegisterSpell(AID.IronJaws, true);
        d.RegisterSpell(AID.Sidewinder, true);
        d.RegisterSpell(AID.Troubadour, true);
        d.RegisterSpell(AID.Stormbite, true);
        d.RegisterSpell(AID.CausticBite, true);
        d.RegisterSpell(AID.NaturesMinne, true);
        d.RegisterSpell(AID.RefulgentArrow, true);
        d.RegisterSpell(AID.Shadowbite, true);
        d.RegisterSpell(AID.BurstShot, true);
        d.RegisterSpell(AID.ApexArrow, true);
        d.RegisterSpell(AID.Ladonsbite, true);
        d.RegisterSpell(AID.BlastArrow, true);
        d.RegisterSpell(AID.RadiantFinale, true);
        d.RegisterSpell(AID.HeartbreakShot, true);
        d.RegisterSpell(AID.ResonantArrow, true);
        d.RegisterSpell(AID.RadiantEncore, true);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // hardcoded mechanics
        d.RegisterChargeIncreaseTrait(AID.Bloodletter, TraitID.EnhancedBloodletter);
        d.RegisterChargeIncreaseTrait(AID.RainOfDeath, TraitID.EnhancedBloodletter);

        // smart targets
        d.Spell(AID.WardensPaean)!.SmartTarget = ActionDefinitions.SmartTargetEsunable;

        d.Spell(AID.RepellingShot)!.ForbidExecute = ActionDefinitions.BackdashCheck(10);
    }
}


using BossMod.Interfaces;

namespace BossMod.NIN;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Chimatsuri = 4243, // LB3, 4.5s cast (0 charges), range 8, single-target, targets=Hostile, animLock=3.700s?
    SpinningEdge = 2240, // L1, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    ShadeShift = 2241, // L2, instant, 120.0s CD (group 20) (0 charges), range 0, single-target, targets=Self
    GustSlash = 2242, // L4, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    Hide = 2245, // L10, instant, 20.0s CD (group 2) (0 charges), range 0, single-target, targets=Self
    ThrowingDagger = 2247, // L15, instant, GCD (0 charges), range 20, single-target, targets=Hostile
    Mug = 2248, // L15, instant, 120.0s CD (group 21) (0 charges), range 3, single-target, targets=Hostile
    TrickAttack = 2258, // L18, instant, 60.0s CD (group 8) (0 charges), range 3, single-target, targets=Hostile
    AeolianEdge = 2255, // L26, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    FumaShuriken = 2265, // L30, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    Ninjutsu = 2260, // L30, instant, GCD (0 charges), range 0, single-target, targets=Self
    FumaJin = 18875, // L30, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    FumaChi = 18874, // L30, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    FumaTen = 18873, // L30, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    Ten1 = 2259, // L30, instant, 20.0s CD (group 3/57) (2 charges), range 0, single-target, targets=Self, animLock=0.350
    RabbitMedium = 2272, // L30, instant, GCD (0 charges), range 0, single-target, targets=Self
    Ten2 = 18805, // L30, instant, GCD (0 charges), range 0, single-target, targets=Self, animLock=0.350
    TCJRaiton = 18877, // L35, instant, GCD (0 charges), range 20, single-target, targets=Hostile
    Chi2 = 18806, // L35, instant, GCD (0 charges), range 0, single-target, targets=Self, animLock=0.350
    Raiton = 2267, // L35, instant, GCD (0 charges), range 20, single-target, targets=Hostile
    TCJKaton = 18876, // L35, instant, GCD (0 charges), range 20, AOE 5 circle, targets=Hostile
    Chi1 = 2261, // L35, instant, 20.0s CD (group 3/57) (2 charges), range 0, single-target, targets=Self, animLock=0.350s?
    Katon = 2266, // L35, instant, GCD (0 charges), range 20, AOE 5 circle, targets=Hostile
    DeathBlossom = 2254, // L38, instant, GCD (0 charges), range 0, AOE 5 circle, targets=Self
    Assassinate = 2246, // L40, instant, 60.0s CD (group 9) (0 charges), range 3, single-target, targets=Hostile
    Shukuchi = 2262, // L40, instant, 60.0s CD (group 15/70), range 20, ???, targets=Area, animLock=0.800s?
    Jin1 = 2263, // L45, instant, 20.0s CD (group 3/57) (2 charges), range 0, single-target, targets=Self, animLock=0.350s?
    TCJSuiton = 18881, // L45, instant, GCD (0 charges), range 20, single-target, targets=Hostile
    TCJDoton = 18880, // L45, instant, GCD (0 charges), range 0, ???, targets=Self
    TCJHuton = 18879, // L45, instant, GCD (0 charges), range 20, AOE 5 circle, targets=Hostile
    TCJHyoton = 18878, // L45, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    Hyoton = 2268, // L45, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    Huton = 2269, // L45, instant, GCD (0 charges), range 20, AOE 5 circle, targets=Hostile
    Suiton = 2271, // L45, instant, GCD (0 charges), range 20, single-target, targets=Hostile
    Jin2 = 18807, // L45, instant, GCD (0 charges), range 0, single-target, targets=Self, animLock=0.350
    Doton = 2270, // L45, instant, GCD (0 charges), range 0, ???, targets=Self
    Kassatsu = 2264, // L50, instant, 60.0s CD (group 10) (0 charges), range 0, single-target, targets=Self
    HakkeMujinsatsu = 16488, // L52, instant, GCD (0 charges), range 0, AOE 5 circle, targets=Self
    ArmorCrush = 3563, // L54, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    DreamWithinADream = 3566, // L56, instant, 60.0s CD (group 11) (0 charges), range 3, single-target, targets=Hostile
    HellfrogMedium = 7401, // L62, instant, 1.0s CD (group 0) (0 charges), range 25, AOE 6 circle, targets=Hostile
    Dokumori = 36957, // L66, instant, 120.0s CD (group 21) (0 charges), range 3, single-target, targets=Hostile
    Bhavacakra = 7402, // L68, instant, 1.0s CD (group 0) (0 charges), range 3, single-target, targets=Hostile
    TenChiJin = 7403, // L70, instant, 120.0s CD (group 19) (0 charges), range 0, single-target, targets=Self
    Meisui = 16489, // L72, instant, 120.0s CD (group 18) (0 charges), range 0, single-target, targets=Self
    HyoshoRanryu = 16492, // L76, instant, GCD (0 charges), range 25, single-target, targets=Hostile
    GokaMekkyaku = 16491, // L76, instant, GCD (0 charges), range 20, AOE 5 circle, targets=Hostile
    Bunshin = 16493, // L80, instant, 90.0s CD (group 14) (0 charges), range 0, single-target, targets=Self
    PhantomKamaitachi = 25774, // L82, instant, GCD (0 charges), range 20, single-target, targets=Hostile
    HollowNozuchi = 25776, // L86, instant (0 charges), range 100, AOE 5 circle, targets=Self/Area
    FleetingRaiju = 25778, // L90, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    ForkedRaiju = 25777, // L90, instant, GCD (0 charges), range 20, single-target, targets=Hostile
    KunaisBane = 36958, // L92, instant, 60.0s CD (group 8) (0 charges), range 3, AOE 5 circle, targets=Hostile
    DeathfrogMedium = 36959, // L96, instant, 1.0s CD (group 0) (0 charges), range 25, AOE 6 circle, targets=Hostile
    ZeshoMeppo = 36960, // L96, instant, 1.0s CD (group 0) (0 charges), range 3, single-target, targets=Hostile
    TenriJindo = 36961, // L100, instant, 1.0s CD (group 1) (0 charges), range 20, AOE 5 circle, targets=Hostile

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast (0 charges), range 8, single-target, targets=Hostile, animLock=3.860s?
    Bladedance = ClassShared.AID.Bladedance, // LB2, 3.0s cast (0 charges), range 8, single-target, targets=Hostile, animLock=3.860s?
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49) (0 charges), range 0, single-target, targets=Self
    LegSweep = ClassShared.AID.LegSweep, // L10, instant, 40.0s CD (group 43) (0 charges), range 3, single-target, targets=Hostile
    Bloodbath = ClassShared.AID.Bloodbath, // L12, instant, 90.0s CD (group 46) (0 charges), range 0, single-target, targets=Self
    Feint = ClassShared.AID.Feint, // L22, instant, 90.0s CD (group 47) (0 charges), range 10, single-target, targets=Hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48) (0 charges), range 0, single-target, targets=Self
    TrueNorth = ClassShared.AID.TrueNorth, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    AllFours = 90, // L14
    FleetOfFoot = 93, // L20
    IncreaseAttackSpeed = 584, // L45
    AdeptAssassination = 515, // L56
    Shukiho = 165, // L62
    EnhancedShukuchi = 166, // L64
    MugMastery = 585, // L66
    EnhancedShukuchiII = 279, // L74
    MeleeMastery = 516, // L74
    EnhancedKassatsu = 250, // L76
    ShukihoII = 280, // L78
    MeleeMasteryII = 522, // L84
    ShukihoIII = 439, // L84
    EnhancedMeisui = 440, // L88
    EnhancedRaiton = 441, // L90
    TrickAttackMastery = 586, // L92
    EnhancedSecondWind = 642, // L94
    MeleeMasteryIII = 661, // L94
    EnhancedDokumori = 587, // L96
    EnhancedFeint = 641, // L98
    EnhancedTenChiJin = 588, // L100
}

public enum SID : uint
{
    None = 0,
    ShadeShift = 488, // applied by Shade Shift to self
    Hidden = 614, // applied by Hide to self
    Bloodbath = 84, // applied by Bloodbath to self
    TrickAttack = 3254, // applied by Trick Attack to target
    KunaisBane = 3906, // applied by Kunai's Bane to target
    TenChiJin = 1186, // applied by Ten Chi Jin to self
    Mudra = 496, // applied by Ten, Ten, Chi, Chi, Jin, Jin to self
    RaijuReady = 2690, // applied by Raiton, Raiton to self
    ShadowWalker = 3848, // applied by Suiton, Suiton, Huton, Huton to self
    Kassatsu = 497, // applied by Kassatsu to self
    Higi = 3850, // applied by Dokumori to self
    VulnerabilityUp = 638, // applied by Mug to target
    Dokumori = 3849, // applied by Dokumori to target
    TenriJindoReady = 3851, // applied by Ten Chi Jin to self
    Meisui = 2689, // applied by Meisui to self
    Bunshin = 1954, // applied by Bunshin to self
    PhantomKamaitachiReady = 2723, // applied by Bunshin to self

    //Shared
    Feint = ClassShared.SID.Feint, // applied by Feint to target
    TrueNorth = ClassShared.SID.TrueNorth, // applied by True North to self
}

public sealed class Definitions : IDefinitions
{
    public void Initialize(ActionDefinitions defs)
    {
        defs.RegisterSpell(AID.Chimatsuri, castAnimLock: 3.70f); // animLock=3.700s?
        defs.RegisterSpell(AID.SpinningEdge);
        defs.RegisterSpell(AID.ShadeShift);
        defs.RegisterSpell(AID.GustSlash);
        defs.RegisterSpell(AID.Hide);
        defs.RegisterSpell(AID.ThrowingDagger);
        defs.RegisterSpell(AID.Mug);
        defs.RegisterSpell(AID.TrickAttack);
        defs.RegisterSpell(AID.AeolianEdge);
        defs.RegisterSpell(AID.FumaShuriken);
        defs.RegisterSpell(AID.Ninjutsu);
        defs.RegisterSpell(AID.FumaJin);
        defs.RegisterSpell(AID.FumaChi);
        defs.RegisterSpell(AID.FumaTen);
        defs.RegisterSpell(AID.Ten1, instantAnimLock: 0.35f); // animLock=0.350
        defs.RegisterSpell(AID.RabbitMedium);
        defs.RegisterSpell(AID.Ten2, instantAnimLock: 0.35f); // animLock=0.350
        defs.RegisterSpell(AID.TCJRaiton);
        defs.RegisterSpell(AID.Chi2, instantAnimLock: 0.35f); // animLock=0.350
        defs.RegisterSpell(AID.Raiton);
        defs.RegisterSpell(AID.TCJKaton);
        defs.RegisterSpell(AID.Chi1, instantAnimLock: 0.35f); // animLock=0.350
        defs.RegisterSpell(AID.Katon);
        defs.RegisterSpell(AID.DeathBlossom);
        defs.RegisterSpell(AID.Assassinate);
        defs.RegisterSpell(AID.Shukuchi, instantAnimLock: 0.80f); // animLock=0.800s?
        defs.RegisterSpell(AID.Jin1, instantAnimLock: 0.35f); // animLock=0.350
        defs.RegisterSpell(AID.TCJSuiton);
        defs.RegisterSpell(AID.TCJDoton);
        defs.RegisterSpell(AID.TCJHuton);
        defs.RegisterSpell(AID.TCJHyoton);
        defs.RegisterSpell(AID.Hyoton);
        defs.RegisterSpell(AID.Huton);
        defs.RegisterSpell(AID.Suiton);
        defs.RegisterSpell(AID.Jin2, instantAnimLock: 0.35f); // animLock=0.350
        defs.RegisterSpell(AID.Doton);
        defs.RegisterSpell(AID.Kassatsu);
        defs.RegisterSpell(AID.HakkeMujinsatsu);
        defs.RegisterSpell(AID.ArmorCrush);
        defs.RegisterSpell(AID.DreamWithinADream);
        defs.RegisterSpell(AID.HellfrogMedium);
        defs.RegisterSpell(AID.Dokumori);
        defs.RegisterSpell(AID.Bhavacakra);
        defs.RegisterSpell(AID.TenChiJin);
        defs.RegisterSpell(AID.Meisui);
        defs.RegisterSpell(AID.HyoshoRanryu);
        defs.RegisterSpell(AID.GokaMekkyaku);
        defs.RegisterSpell(AID.Bunshin);
        defs.RegisterSpell(AID.PhantomKamaitachi);
        defs.RegisterSpell(AID.HollowNozuchi);
        defs.RegisterSpell(AID.FleetingRaiju);
        defs.RegisterSpell(AID.ForkedRaiju);
        defs.RegisterSpell(AID.KunaisBane);
        defs.RegisterSpell(AID.DeathfrogMedium);
        defs.RegisterSpell(AID.ZeshoMeppo);
        defs.RegisterSpell(AID.TenriJindo);

        Customize(defs);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.Shukuchi, TraitID.EnhancedShukuchiII);

        d.Spell(AID.ForkedRaiju)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
        d.Spell(AID.Shukuchi)!.ForbidExecute = ActionDefinitions.DashToPositionCheck;
    }
}


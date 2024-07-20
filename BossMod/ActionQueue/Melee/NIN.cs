namespace BossMod.NIN;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    Chimatsuri = 4243, // LB3, 4.5s cast, range 8, single-target, targets=hostile, animLock=???, castAnimLock=3.700
    SpinningEdge = 2240, // L1, instant, GCD, range 3, single-target, targets=hostile
    ShadeShift = 2241, // L2, instant, 120.0s CD (group 20), range 0, single-target, targets=self
    GustSlash = 2242, // L4, instant, GCD, range 3, single-target, targets=hostile
    Hide = 2245, // L10, instant, 20.0s CD (group 1), range 0, single-target, targets=self
    ThrowingDagger = 2247, // L15, instant, GCD, range 20, single-target, targets=hostile
    Mug = 2248, // L15, instant, 120.0s CD (group 21), range 3, single-target, targets=hostile
    TrickAttack = 2258, // L18, instant, 60.0s CD (group 11), range 3, single-target, targets=hostile
    AeolianEdge = 2255, // L26, instant, GCD, range 3, single-target, targets=hostile
    Ten1 = 2259, // L30, instant, 20.0s CD (group 8/57) (2? charges), range 0, single-target, targets=self, animLock=0.350
    Ten2 = 18805, // L30, instant, GCD, range 0, single-target, targets=self, animLock=0.350
    RabbitMedium = 2272, // L30, instant, GCD, range 0, single-target, targets=self
    FumaShuriken1 = 2265, // L30, instant, GCD, range 25, single-target, targets=hostile
    FumaShuriken2 = 18873, // L30, instant, GCD, range 25, single-target, targets=hostile
    FumaShuriken3 = 18874, // L30, instant, GCD, range 25, single-target, targets=hostile
    FumaShuriken4 = 18875, // L30, instant, GCD, range 25, single-target, targets=hostile
    Ninjutsu = 2260, // L30, instant, GCD, range 0, single-target, targets=self, animLock=???
    Raiton1 = 2267, // L35, instant, GCD, range 20, single-target, targets=hostile
    Raiton2 = 18877, // L35, instant, GCD, range 20, single-target, targets=hostile
    Chi1 = 2261, // L35, instant, 20.0s CD (group 8/57) (2? charges), range 0, single-target, targets=self, animLock=0.350
    Chi2 = 18806, // L35, instant, GCD, range 0, single-target, targets=self, animLock=0.350
    Katon1 = 2266, // L35, instant, GCD, range 20, AOE 5 circle, targets=hostile
    Katon2 = 18876, // L35, instant, GCD, range 20, AOE 5 circle, targets=hostile
    DeathBlossom = 2254, // L38, instant, GCD, range 0, AOE 5 circle, targets=self
    Assassinate = 2246, // L40, instant, 60.0s CD (group 9), range 3, single-target, targets=hostile, animLock=???
    Shukuchi = 2262, // L40, instant, 60.0s CD (group 16/70), range 20, ???, targets=area, animLock=0.800
    Hyoton1 = 2268, // L45, instant, GCD, range 25, single-target, targets=hostile
    Hyoton2 = 18878, // L45, instant, GCD, range 25, single-target, targets=hostile
    Suiton1 = 2271, // L45, instant, GCD, range 20, single-target, targets=hostile
    Suiton2 = 18881, // L45, instant, GCD, range 20, single-target, targets=hostile
    Doton1 = 2270, // L45, instant, GCD, range 0, ???, targets=self
    Doton2 = 18880, // L45, instant, GCD, range 0, ???, targets=self
    Huton1 = 2269, // L45, instant, GCD, range 0, single-target, targets=self
    Huton2 = 18879, // L45, instant, GCD, range 0, single-target, targets=self
    Jin1 = 2263, // L45, instant, 20.0s CD (group 8/57) (2? charges), range 0, single-target, targets=self, animLock=0.350
    Jin2 = 18807, // L45, instant, GCD, range 0, single-target, targets=self, animLock=0.350
    Kassatsu = 2264, // L50, instant, 60.0s CD (group 13), range 0, single-target, targets=self
    HakkeMujinsatsu = 16488, // L52, instant, GCD, range 0, AOE 5 circle, targets=self
    ArmorCrush = 3563, // L54, instant, GCD, range 3, single-target, targets=hostile
    DreamWithinADream = 3566, // L56, instant, 60.0s CD (group 15), range 3, single-target, targets=hostile
    Huraijin = 25876, // L60, instant, GCD, range 3, single-target, targets=hostile
    HellfrogMedium = 7401, // L62, instant, 1.0s CD (group 0), range 25, AOE 6 circle, targets=hostile
    Bhavacakra = 7402, // L68, instant, 1.0s CD (group 0), range 3, single-target, targets=hostile
    TenChiJin = 7403, // L70, instant, 120.0s CD (group 19), range 0, single-target, targets=self
    Meisui = 16489, // L72, instant, 120.0s CD (group 22), range 0, single-target, targets=self
    HyoshoRanryu = 16492, // L76, instant, GCD, range 25, single-target, targets=hostile
    GokaMekkyaku = 16491, // L76, instant, GCD, range 20, AOE 5 circle, targets=hostile
    Bunshin = 16493, // L80, instant, 90.0s CD (group 14), range 0, single-target, targets=self
    PhantomKamaitachi = 25774, // L82, instant, GCD, range 20, single-target, targets=hostile
    HollowNozuchi = 25776, // L86, instant, range 100, AOE 5 circle, targets=self/area/!dead, animLock=???
    ForkedRaiju = 25777, // L90, instant, GCD, range 20, single-target, targets=hostile
    FleetingRaiju = 25778, // L90, instant, GCD, range 3, single-target, targets=hostile

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
    Bladedance = ClassShared.AID.Bladedance, // LB2, 3.0s cast, range 8, single-target, targets=hostile, castAnimLock=3.860
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49), range 0, single-target, targets=self
    LegSweep = ClassShared.AID.LegSweep, // L10, instant, 40.0s CD (group 41), range 3, single-target, targets=hostile
    Bloodbath = ClassShared.AID.Bloodbath, // L12, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    Feint = ClassShared.AID.Feint, // L22, instant, 90.0s CD (group 47), range 10, single-target, targets=hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    TrueNorth = ClassShared.AID.TrueNorth, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=self
}

public enum TraitID : uint
{
    None = 0,
    AllFours = 90, // L14
    FleetOfFoot = 93, // L20
    AdeptAssassination = 515, // L56
    Shukiho = 165, // L62
    EnhancedShukuchi = 166, // L64
    EnhancedMug = 167, // L66
    EnhancedShukuchiII = 279, // L74
    MeleeMastery = 516, // L74
    EnhancedKassatsu = 250, // L76
    ShukihoII = 280, // L78
    ShukihoIII = 439, // L84
    MeleeMasteryII = 522, // L84
    EnhancedMeisui = 440, // L88
    EnhancedRaiton = 441, // L90
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Chimatsuri, castAnimLock: 3.70f);
        d.RegisterSpell(AID.SpinningEdge);
        d.RegisterSpell(AID.ShadeShift);
        d.RegisterSpell(AID.GustSlash);
        d.RegisterSpell(AID.Hide);
        d.RegisterSpell(AID.ThrowingDagger);
        d.RegisterSpell(AID.Mug);
        d.RegisterSpell(AID.TrickAttack);
        d.RegisterSpell(AID.AeolianEdge);
        d.RegisterSpell(AID.Ten1, instantAnimLock: 0.35f);
        d.RegisterSpell(AID.Ten2, instantAnimLock: 0.35f);
        d.RegisterSpell(AID.RabbitMedium);
        d.RegisterSpell(AID.FumaShuriken1);
        d.RegisterSpell(AID.FumaShuriken2);
        d.RegisterSpell(AID.FumaShuriken3);
        d.RegisterSpell(AID.FumaShuriken4);
        d.RegisterSpell(AID.Ninjutsu); // animLock=???
        d.RegisterSpell(AID.Raiton1);
        d.RegisterSpell(AID.Raiton2);
        d.RegisterSpell(AID.Chi1, instantAnimLock: 0.35f);
        d.RegisterSpell(AID.Chi2, instantAnimLock: 0.35f);
        d.RegisterSpell(AID.Katon1);
        d.RegisterSpell(AID.Katon2);
        d.RegisterSpell(AID.DeathBlossom);
        d.RegisterSpell(AID.Assassinate); // animLock=???
        d.RegisterSpell(AID.Shukuchi, instantAnimLock: 0.80f);
        d.RegisterSpell(AID.Hyoton1);
        d.RegisterSpell(AID.Hyoton2);
        d.RegisterSpell(AID.Suiton1);
        d.RegisterSpell(AID.Suiton2);
        d.RegisterSpell(AID.Doton1);
        d.RegisterSpell(AID.Doton2);
        d.RegisterSpell(AID.Huton1);
        d.RegisterSpell(AID.Huton2);
        d.RegisterSpell(AID.Jin1, instantAnimLock: 0.35f);
        d.RegisterSpell(AID.Jin2, instantAnimLock: 0.35f);
        d.RegisterSpell(AID.Kassatsu);
        d.RegisterSpell(AID.HakkeMujinsatsu);
        d.RegisterSpell(AID.ArmorCrush);
        d.RegisterSpell(AID.DreamWithinADream);
        d.RegisterSpell(AID.Huraijin);
        d.RegisterSpell(AID.HellfrogMedium);
        d.RegisterSpell(AID.Bhavacakra);
        d.RegisterSpell(AID.TenChiJin);
        d.RegisterSpell(AID.Meisui);
        d.RegisterSpell(AID.HyoshoRanryu);
        d.RegisterSpell(AID.GokaMekkyaku);
        d.RegisterSpell(AID.Bunshin);
        d.RegisterSpell(AID.PhantomKamaitachi);
        d.RegisterSpell(AID.HollowNozuchi); // animLock=???
        d.RegisterSpell(AID.ForkedRaiju);
        d.RegisterSpell(AID.FleetingRaiju);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}

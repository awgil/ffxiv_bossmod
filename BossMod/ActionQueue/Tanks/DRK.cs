namespace BossMod.DRK;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    DarkForce = 4241, // LB3, instant, range 0, AOE 50 circle, targets=Self, animLock=3.860s?
    HardSlash = 3617, // L1, instant, GCD, range 3, single-target, targets=Hostile
    SyphonStrike = 3623, // L2, instant, GCD, range 3, single-target, targets=Hostile
    Unleash = 3621, // L6, instant, GCD, range 0, AOE 5 circle, targets=Self
    Grit = 3629, // L10, instant, 2.0s CD (group 1), range 0, single-target, targets=Self
    ReleaseGrit = 32067, // L10, instant, 1.0s CD (group 1), range 0, single-target, targets=Self
    Unmend = 3624, // L15, instant, GCD, range 20, single-target, targets=Hostile
    Souleater = 3632, // L26, instant, GCD, range 3, single-target, targets=Hostile
    FloodOfDarkness = 16466, // L30, instant, 1.0s CD (group 0), range 10, AOE 10+R width 4 rect, targets=Hostile
    BloodWeapon = 3625, // L35, instant, 60.0s CD (group 10), range 0, single-target, targets=Self
    ShadowWall = 3636, // L38, instant, 120.0s CD (group 20), range 0, single-target, targets=Self
    EdgeOfDarkness = 16467, // L40, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile
    StalwartSoul = 16468, // L40, instant, GCD, range 0, AOE 5 circle, targets=Self
    DarkMind = 3634, // L45, instant, 60.0s CD (group 8), range 0, single-target, targets=Self
    LivingDead = 3638, // L50, instant, 300.0s CD (group 24), range 0, single-target, targets=Self
    SaltedEarth = 3639, // L52, instant, 90.0s CD (group 15), range 0, AOE 5 circle, targets=Self
    Shadowstride = 36926, // L54, instant, 30.0s CD (group 7/70) (2? charges), range 20, single-target, targets=Hostile, animLock=???
    AbyssalDrain = 3641, // L56, instant, 60.0s CD (group 9), range 20, AOE 5 circle, targets=Hostile
    CarveAndSpit = 3643, // L60, instant, 60.0s CD (group 9), range 3, single-target, targets=Hostile
    Bloodspiller = 7392, // L62, instant, GCD, range 3, single-target, targets=Hostile
    Quietus = 7391, // L64, instant, GCD, range 0, AOE 5 circle, targets=Self
    Delirium = 7390, // L68, instant, 60.0s CD (group 10), range 0, single-target, targets=Self
    TheBlackestNight = 7393, // L70, instant, 15.0s CD (group 2), range 30, single-target, targets=Self/Party
    EdgeOfShadow = 16470, // L74, instant, 1.0s CD (group 0), range 3, single-target, targets=Hostile
    FloodOfShadow = 16469, // L74, instant, 1.0s CD (group 0), range 10, AOE 10+R width 4 rect, targets=Hostile
    DarkMissionary = 16471, // L76, instant, 90.0s CD (group 14), range 0, AOE 30 circle, targets=Self
    LivingShadow = 16472, // L80, instant, 120.0s CD (group 21), range 0, single-target, targets=Self
    Oblation = 25754, // L82, instant, 60.0s CD (group 18/71) (2 charges), range 30, single-target, targets=Self/Party
    SaltAndDarkness = 25755, // L86, instant, 20.0s CD (group 5), range 0, single-target, targets=Self
    SaltAndDarknessEnd = 25756, // L86, instant, range 100, AOE 5 circle, targets=Self/Area
    Shadowbringer = 25757, // L90, instant, 60.0s CD (group 22/72) (2 charges), range 10, AOE 10+R width 4 rect, targets=Hostile
    ShadowedVigil = 36927, // L92, instant, 120.0s CD (group 20), range 0, single-target, targets=Self, animLock=???
    ScarletDelirium = 36928, // L96, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    Comeuppance = 36929, // L96, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    Torcleaver = 36930, // L96, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    Impalement = 36931, // L96, instant, GCD, range 0, AOE 5 circle, targets=Self, animLock=???
    Disesteem = 36932, // L100, instant, GCD, range 10, AOE 10+R width 4 rect, targets=Hostile, animLock=???

    // Shared
    ShieldWall = ClassShared.AID.ShieldWall, // LB1, instant, range 0, AOE 50 circle, targets=Self, animLock=1.930s?
    Stronghold = ClassShared.AID.Stronghold, // LB2, instant, range 0, AOE 50 circle, targets=Self, animLock=3.860s?
    Rampart = ClassShared.AID.Rampart, // L8, instant, 90.0s CD (group 46), range 0, single-target, targets=Self
    LowBlow = ClassShared.AID.LowBlow, // L12, instant, 25.0s CD (group 41), range 3, single-target, targets=Hostile
    Provoke = ClassShared.AID.Provoke, // L15, instant, 30.0s CD (group 42), range 25, single-target, targets=Hostile
    Interject = ClassShared.AID.Interject, // L18, instant, 30.0s CD (group 43), range 3, single-target, targets=Hostile
    Reprisal = ClassShared.AID.Reprisal, // L22, instant, 60.0s CD (group 44), range 0, AOE 5 circle, targets=Self
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
    Shirk = ClassShared.AID.Shirk, // L48, instant, 120.0s CD (group 49), range 25, single-target, targets=Party
}

public enum TraitID : uint
{
    None = 0,
    TankMastery = 319, // L1
    Blackblood = 158, // L62
    EnhancedBlackblood = 159, // L66
    BloodWeaponMastery = 570, // L68
    DarksideMastery = 271, // L74
    EnhancedUnmend = 422, // L84
    MeleeMastery = 506, // L84
    EnhancedLivingShadow = 511, // L88
    EnhancedLivingShadowII = 423, // L90
    ShadowWallMastery = 571, // L92
    EnhancedRampart = 639, // L94
    MeleeMasteryII = 663, // L94
    EnhancedDelirium = 572, // L96
    EnhancedReprisal = 640, // L98
    EnhancedLivingShadowIII = 573, // L100
}

public enum SID : uint
{
    None = 0,
    BloodWeapon = 742, // applied by BloodWeapon to self
    Grit = 743, // applied by Grit to self
    SaltedEarth = 749, // applied by Salted Earth
    TheBlackestNight = 1308, // applied by The Blackest Night to target
    Delirium = 1972, // applied by Delirium to self
    Oblation = 2682, // applied by Oblation to target
    EnhancedDelirium = 3836, // applied by Delirium to self (Lv96+)
    Scorn = 3837, // applied by Living Shadow to self

    //Shared
    Reprisal = ClassShared.SID.Reprisal, // applied by Reprisal to target
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.DarkForce, instantAnimLock: 3.86f); // animLock=3.860s?
        d.RegisterSpell(AID.HardSlash);
        d.RegisterSpell(AID.SyphonStrike);
        d.RegisterSpell(AID.Unleash);
        d.RegisterSpell(AID.Grit);
        d.RegisterSpell(AID.ReleaseGrit);
        d.RegisterSpell(AID.Unmend);
        d.RegisterSpell(AID.Souleater);
        d.RegisterSpell(AID.FloodOfDarkness);
        d.RegisterSpell(AID.BloodWeapon);
        d.RegisterSpell(AID.ShadowWall);
        d.RegisterSpell(AID.EdgeOfDarkness);
        d.RegisterSpell(AID.StalwartSoul);
        d.RegisterSpell(AID.DarkMind);
        d.RegisterSpell(AID.LivingDead);
        d.RegisterSpell(AID.SaltedEarth);
        d.RegisterSpell(AID.Shadowstride); // animLock=???
        d.RegisterSpell(AID.AbyssalDrain);
        d.RegisterSpell(AID.CarveAndSpit);
        d.RegisterSpell(AID.Bloodspiller);
        d.RegisterSpell(AID.Quietus);
        d.RegisterSpell(AID.Delirium);
        d.RegisterSpell(AID.TheBlackestNight);
        d.RegisterSpell(AID.EdgeOfShadow);
        d.RegisterSpell(AID.FloodOfShadow);
        d.RegisterSpell(AID.DarkMissionary);
        d.RegisterSpell(AID.LivingShadow);
        d.RegisterSpell(AID.Oblation);
        d.RegisterSpell(AID.SaltAndDarkness);
        d.RegisterSpell(AID.SaltAndDarknessEnd);
        d.RegisterSpell(AID.Shadowbringer);
        d.RegisterSpell(AID.ShadowedVigil); // animLock=???
        d.RegisterSpell(AID.ScarletDelirium); // animLock=???
        d.RegisterSpell(AID.Comeuppance); // animLock=???
        d.RegisterSpell(AID.Torcleaver); // animLock=???
        d.RegisterSpell(AID.Impalement); // animLock=???
        d.RegisterSpell(AID.Disesteem); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.Spell(AID.Shadowstride)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
    }
}

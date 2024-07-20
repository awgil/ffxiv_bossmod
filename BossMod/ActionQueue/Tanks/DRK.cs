namespace BossMod.DRK;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    DarkForce = 4241, // LB3, instant, range 0, AOE 50 circle, targets=self, animLock=3.860s
    HardSlash = 3617, // L1, instant, GCD, range 3, single-target, targets=hostile
    SyphonStrike = 3623, // L2, instant, GCD, range 3, single-target, targets=hostile
    Unleash = 3621, // L6, instant, GCD, range 0, AOE 5 circle, targets=self
    Grit = 3629, // L10, instant, 2.0s CD (group 1), range 0, single-target, targets=self
    ReleaseGrit = 32067, // L10, instant, 1.0s CD (group 1), range 0, single-target, targets=self
    Unmend = 3624, // L15, instant, GCD, range 20, single-target, targets=hostile
    Souleater = 3632, // L26, instant, GCD, range 3, single-target, targets=hostile
    FloodOfDarkness = 16466, // L30, instant, 1.0s CD (group 0), range 10, AOE 10+R width 4 rect, targets=hostile, animLock=???
    BloodWeapon = 3625, // L35, instant, 60.0s CD (group 11), range 0, single-target, targets=self
    ShadowWall = 3636, // L38, instant, 120.0s CD (group 20), range 0, single-target, targets=self
    EdgeOfDarkness = 16467, // L40, instant, 1.0s CD (group 0), range 3, single-target, targets=hostile, animLock=???
    StalwartSoul = 16468, // L40, instant, GCD, range 0, AOE 5 circle, targets=self
    DarkMind = 3634, // L45, instant, 60.0s CD (group 12), range 0, single-target, targets=self
    LivingDead = 3638, // L50, instant, 300.0s CD (group 23), range 0, single-target, targets=self
    SaltedEarth = 3639, // L52, instant, 90.0s CD (group 15), range 0, AOE 5 circle, targets=self
    Plunge = 3640, // L54, instant, 30.0s CD (group 9/70), range 20, single-target, targets=hostile
    AbyssalDrain = 3641, // L56, instant, 60.0s CD (group 13), range 20, AOE 5 circle, targets=hostile
    CarveAndSpit = 3643, // L60, instant, 60.0s CD (group 13), range 3, single-target, targets=hostile
    Bloodspiller = 7392, // L62, instant, GCD, range 3, single-target, targets=hostile
    Quietus = 7391, // L64, instant, GCD, range 0, AOE 5 circle, targets=self
    Delirium = 7390, // L68, instant, 60.0s CD (group 10), range 0, single-target, targets=self
    TheBlackestNight = 7393, // L70, instant, 15.0s CD (group 2), range 30, single-target, targets=self/party
    EdgeOfShadow = 16470, // L74, instant, 1.0s CD (group 0), range 3, single-target, targets=hostile
    FloodOfShadow = 16469, // L74, instant, 1.0s CD (group 0), range 10, AOE 10+R width 4 rect, targets=hostile
    DarkMissionary = 16471, // L76, instant, 90.0s CD (group 17), range 0, AOE 30 circle, targets=self
    LivingShadow = 16472, // L80, instant, 120.0s CD (group 21), range 0, single-target, targets=self
    Oblation = 25754, // L82, instant, 60.0s CD (group 22/71) (2? charges), range 30, single-target, targets=self/party
    SaltAndDarkness = 25755, // L86, instant, 20.0s CD (group 3), range 0, single-target, targets=self
    SaltAndDarknessEnd = 25756, // L86, instant, range 100, AOE 5 circle, targets=self/area/!dead, animLock=???
    Shadowbringer = 25757, // L90, instant, 60.0s CD (group 19/72) (2? charges), range 10, AOE 10+R width 4 rect, targets=hostile

    // Shared
    ShieldWall = ClassShared.AID.ShieldWall, // LB1, instant, range 0, AOE 50 circle, targets=self, animLock=1.930
    Stronghold = ClassShared.AID.Stronghold, // LB2, instant, range 0, AOE 50 circle, targets=self, animLock=3.860
    Rampart = ClassShared.AID.Rampart, // L8, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    LowBlow = ClassShared.AID.LowBlow, // L12, instant, 25.0s CD (group 41), range 3, single-target, targets=hostile
    Provoke = ClassShared.AID.Provoke, // L15, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
    Interject = ClassShared.AID.Interject, // L18, instant, 30.0s CD (group 43), range 3, single-target, targets=hostile
    Reprisal = ClassShared.AID.Reprisal, // L22, instant, 60.0s CD (group 44), range 0, AOE 5 circle, targets=self
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    Shirk = ClassShared.AID.Shirk, // L48, instant, 120.0s CD (group 49), range 25, single-target, targets=party
}

public enum TraitID : uint
{
    None = 0,
    TankMastery = 319, // L1
    Blackblood = 158, // L62
    EnhancedBlackblood = 159, // L66
    DarksideMastery = 271, // L74
    EnhancedPlunge = 272, // L78
    EnhancedUnmend = 422, // L84
    MeleeMastery = 506, // L84
    EnhancedLivingShadow = 511, // L88
    EnhancedLivingShadowII = 423, // L90
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.DarkForce, instantAnimLock: 3.86f);
        d.RegisterSpell(AID.HardSlash);
        d.RegisterSpell(AID.SyphonStrike);
        d.RegisterSpell(AID.Unleash);
        d.RegisterSpell(AID.Grit);
        d.RegisterSpell(AID.ReleaseGrit);
        d.RegisterSpell(AID.Unmend);
        d.RegisterSpell(AID.Souleater);
        d.RegisterSpell(AID.FloodOfDarkness); // animLock=???
        d.RegisterSpell(AID.BloodWeapon);
        d.RegisterSpell(AID.ShadowWall);
        d.RegisterSpell(AID.EdgeOfDarkness); // animLock=???
        d.RegisterSpell(AID.StalwartSoul);
        d.RegisterSpell(AID.DarkMind);
        d.RegisterSpell(AID.LivingDead);
        d.RegisterSpell(AID.SaltedEarth);
        d.RegisterSpell(AID.Plunge);
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
        d.RegisterSpell(AID.SaltAndDarknessEnd); // animLock=???
        d.RegisterSpell(AID.Shadowbringer);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}

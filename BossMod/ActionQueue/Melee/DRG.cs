namespace BossMod.DRG;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    DragonsongDive = 4242, // LB3, 4.5s cast, range 8, single-target, targets=hostile, castAnimLock=3.700
    TrueThrust = 75, // L1, instant, GCD, range 3, single-target, targets=hostile
    VorpalThrust = 78, // L4, instant, GCD, range 3, single-target, targets=hostile
    LifeSurge = 83, // L6, instant, 40.0s CD (group 14/70), range 0, single-target, targets=self
    PiercingTalon = 90, // L15, instant, GCD, range 20, single-target, targets=hostile
    Disembowel = 87, // L18, instant, GCD, range 3, single-target, targets=hostile
    FullThrust = 84, // L26, instant, GCD, range 3, single-target, targets=hostile
    LanceCharge = 85, // L30, instant, 60.0s CD (group 9), range 0, single-target, targets=self
    Jump = 92, // L30, instant, 30.0s CD (group 4), range 20, single-target, targets=hostile, animLock=0.800
    ElusiveJump = 94, // L35, instant, 30.0s CD (group 5), range 0, single-target, targets=self, animLock=0.800
    DoomSpike = 86, // L40, instant, GCD, range 10, AOE 10+R width 4 rect, targets=hostile
    SpineshatterDive = 95, // L45, instant, 60.0s CD (group 19/71), range 20, single-target, targets=hostile, animLock=0.800
    DragonfireDive = 96, // L50, instant, 120.0s CD (group 20), range 20, AOE 5 circle, targets=hostile, animLock=0.800
    ChaosThrust = 88, // L50, instant, GCD, range 3, single-target, targets=hostile
    BattleLitany = 3557, // L52, instant, 120.0s CD (group 23), range 0, AOE 30 circle, targets=self
    FangAndClaw = 3554, // L56, instant, GCD, range 3, single-target, targets=hostile
    WheelingThrust = 3556, // L58, instant, GCD, range 3, single-target, targets=hostile
    Geirskogul = 3555, // L60, instant, 30.0s CD (group 7), range 15, AOE 15+R width 4 rect, targets=hostile
    SonicThrust = 7397, // L62, instant, GCD, range 10, AOE 10+R width 4 rect, targets=hostile
    DragonSight = 7398, // L66, instant, 120.0s CD (group 21), range 30, single-target, targets=self/party
    MirageDive = 7399, // L68, instant, 1.0s CD (group 0), range 20, single-target, targets=hostile
    Nastrond = 7400, // L70, instant, 10.0s CD (group 2), range 15, AOE 15+R width 4 rect, targets=hostile
    CoerthanTorment = 16477, // L72, instant, GCD, range 10, AOE 10+R width 4 rect, targets=hostile
    HighJump = 16478, // L74, instant, 30.0s CD (group 8), range 20, single-target, targets=hostile, animLock=0.800
    RaidenThrust = 16479, // L76, instant, GCD, range 3, single-target, targets=hostile
    Stardiver = 16480, // L80, instant, 30.0s CD (group 6), range 20, AOE 5 circle, targets=hostile, animLock=1.500
    DraconianFury = 25770, // L82, instant, GCD, range 10, AOE 10+R width 4 rect, targets=hostile
    HeavensThrust = 25771, // L86, instant, GCD, range 3, single-target, targets=hostile
    ChaoticSpring = 25772, // L86, instant, GCD, range 3, single-target, targets=hostile
    WyrmwindThrust = 25773, // L90, instant, 10.0s CD (group 1), range 15, AOE 15+R width 4 rect, targets=hostile

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
    BloodOfTheDragon = 434, // L54, jump potency increase
    LanceMastery1 = 162, // L64
    LifeOfTheDragon = 163, // L70
    JumpMastery = 275, // L74, jump -> high jump upgrade
    LanceMastery2 = 247, // L76
    LifeOfTheDragonMastery = 276, // L78, duration increase
    EnhancedCoerthanTorment = 435, // L82
    EnhancedSpineshatterDive = 436, // L84, second charge
    LanceMastery3 = 437, // L86, full thrust -> heavens thrust, chaos thrust -> chaos spring upgrade
    EnhancedLifeSurge = 438, // L88, second charge
    LanceMastery4 = 508, // L90, potency increase
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.DragonsongDive, castAnimLock: 3.70f);
        d.RegisterSpell(AID.TrueThrust);
        d.RegisterSpell(AID.VorpalThrust);
        d.RegisterSpell(AID.LifeSurge, maxCharges: 2);
        d.RegisterSpell(AID.PiercingTalon);
        d.RegisterSpell(AID.Disembowel);
        d.RegisterSpell(AID.FullThrust);
        d.RegisterSpell(AID.LanceCharge);
        d.RegisterSpell(AID.Jump, instantAnimLock: 0.80f);
        d.RegisterSpell(AID.ElusiveJump, instantAnimLock: 0.80f);
        d.RegisterSpell(AID.DoomSpike);
        d.RegisterSpell(AID.SpineshatterDive, maxCharges: 2, instantAnimLock: 0.80f);
        d.RegisterSpell(AID.DragonfireDive, instantAnimLock: 0.80f);
        d.RegisterSpell(AID.ChaosThrust);
        d.RegisterSpell(AID.BattleLitany);
        d.RegisterSpell(AID.FangAndClaw);
        d.RegisterSpell(AID.WheelingThrust);
        d.RegisterSpell(AID.Geirskogul);
        d.RegisterSpell(AID.SonicThrust);
        d.RegisterSpell(AID.DragonSight);
        d.RegisterSpell(AID.MirageDive);
        d.RegisterSpell(AID.Nastrond);
        d.RegisterSpell(AID.CoerthanTorment);
        d.RegisterSpell(AID.HighJump, instantAnimLock: 0.80f);
        d.RegisterSpell(AID.RaidenThrust);
        d.RegisterSpell(AID.Stardiver, instantAnimLock: 1.50f);
        d.RegisterSpell(AID.DraconianFury);
        d.RegisterSpell(AID.HeavensThrust);
        d.RegisterSpell(AID.ChaoticSpring);
        d.RegisterSpell(AID.WyrmwindThrust);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}

namespace BossMod.BST;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    SmashAxe = 44879, // L1, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    Capture = 44880, // L1, instant, GCD, range 10, single-target, targets=Hostile, animLock=???
    FirstBattlehorn = 44881, // L1, 1.0s cast, 2.0s CD (group 3), range 0, single-target, targets=Self, animLock=???
    Gauge = 44882, // L1, instant, 3.0s CD (group 17), range 30, single-target, targets=Hostile, animLock=???
    AxebladeBite = 44883, // L2, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    AvalancheAxe = 44884, // L4, instant, 5.0s CD (group 15), range 3, single-target, targets=Hostile, animLock=???
    Shieldsplitter = 44885, // L12, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    BeastMode = 44886, // L22, instant, GCD, range 0, single-target, targets=Self, animLock=???
    MistralAxe = 44887, // L8, instant, 5.0s CD (group 15), range 3, single-target, targets=Hostile, animLock=???
    SpinningAxe = 44888, // L14, instant, 5.0s CD (group 15), range 3, single-target, targets=Hostile, animLock=???
    GaleAxe = 44889, // L16, instant, 5.0s CD (group 15), range 3, single-target, targets=Hostile, animLock=???
    TemperedRelease1 = 44890, // L18, instant, 30.0s CD (group 6), range 0, single-target, targets=Self, animLock=???
    PartingBlow = 44891, // L6, instant, 10.0s CD (group 2), range 25, single-target, targets=Hostile, animLock=???
    SecondBattlehorn = 44892, // L10, 1.0s cast, 2.0s CD (group 4), range 0, single-target, targets=Self, animLock=???
    ShieldCharge = 44893, // L24, instant, 60.0s CD (group 19/70), range 20, AOE 6 circle, targets=Hostile, animLock=???
    ThirdBattlehorn = 44894, // L20, 1.0s cast, 2.0s CD (group 5), range 0, single-target, targets=Self, animLock=???
    TemperedRelease2 = 47092, // L18, instant, 30.0s CD (group 6), range 30, single-target, targets=Hostile, animLock=???
    Trick = 47093, // L8, instant, 3.0s CD (group 18), range 30, single-target, targets=Hostile, animLock=???
    Borrow = 44895, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    BorrowBeast = 47238, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    BorrowVile = 47239, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    BorrowCloud = 47240, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    BorrowSeed = 47241, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    BorrowWave = 47242, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    BorrowScale = 47243, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    BorrowSoul = 47244, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    BorrowAsh = 47245, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Beastskin = 44896, // L22, instant, 90.0s CD (group 1), range 0, single-target, targets=Self, animLock=???
    Vileskin = 44897, // L22, instant, 90.0s CD (group 0), range 0, single-target, targets=Self, animLock=???
    CloudSkim1 = 44898, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    CloudSkim2 = 45039, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    CloudSkim3 = 45038, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    CloudSkim4 = 45040, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    CloudSkim5 = 45041, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    Seedsower = 44899, // L22, instant, 90.0s CD (group 9), range 0, AOE 6 circle, targets=Self, animLock=???
    QuellingWave = 44900, // L22, instant, GCD, range 30, single-target, targets=Hostile, animLock=???
    Scaleskin = 44901, // L22, instant, 90.0s CD (group 10), range 0, single-target, targets=Self, animLock=???
    SoulCrush = 44902, // L22, instant, 30.0s CD (group 11), range 3, single-target, targets=Hostile, animLock=???
    ScouringAsh = 44903, // L22, instant, 60.0s CD (group 12), range 30, single-target, targets=Self/Party/OwnPet, animLock=???
    Rally = 44905, // L28, instant, 120.0s CD (group 14), range 0, single-target, targets=Self, animLock=???
    RallyingCheer = 44904, // L40, instant, 120.0s CD (group 13), range 0, single-target, targets=Self, animLock=???
    RisenFall = 44932, // L50, instant, 5.0s CD (group 15), range 25, AOE 6 circle, targets=Hostile, animLock=???
    HawkishTalons = 44931, // L50, instant, 5.0s CD (group 15), range 25, AOE 6 circle, targets=Hostile, animLock=???
    BrutalRage = 44930, // L50, instant, 5.0s CD (group 15), range 25, AOE 6 circle, targets=Hostile, animLock=???
    Calamity = 44933, // L50, instant, 5.0s CD (group 15), range 10, AOE 10+R width 3 rect, targets=Hostile, animLock=???
    InstinctualComboVolant = 44910, // L8, instant, range 100, single-target, targets=Hostile, animLock=0 (triggered)
    InstinctualComboRampant = 44911, // L8, instant, range 100, single-target, targets=Hostile, animLock=0 (triggered)
    InstinctualComboDurant = 44912, // L8, instant, range 100, single-target, targets=Hostile, animLock=0 (triggered)
    InstinctualComboEldritch = 44913, // L8, instant, range 100, single-target, targets=Hostile, animLock=0 (triggered)
    IntentionalComboSunstrider = 44914, // L8, instant, range 100, single-target, targets=Hostile, animLock=0 (triggered)
    IntentionalComboMoonstalker = 44915, // L8, instant, range 100, single-target, targets=Hostile, animLock=0 (triggered)
    InfinitiveComboUniversality = 44916, // L50, instant, range 100, single-target, targets=Hostile, animLock=0 (triggered)
}

public enum TraitID : uint
{
    None = 0,
    WildHeart = 690, // L4
    WildHeartII = 691, // L8
    BattlehornMastery = 692, // L18
    WildHeartIII = 694, // L28
    BattlehornMasteryII = 754, // L30
    BattlehornMasteryIII = 755, // L34
    EnhancedShieldCharge = 751, // L36
    Beastmastery = 752, // L38
    WildHeartIV = 693, // L40
    EnhancedRally = 756, // L42
    TemperedReleaseMastery = 749, // L44
    EnhancedBorrow = 750, // L46
    EnhancedRallyingCheer = 757, // L48
    InstinctualMastery = 758, // L50
}

public enum SID : uint
{
    None = 0,
}

public sealed class Definitions : Defs
{
    public override void Define(ActionDefinitions d)
    {
        d.RegisterSpell(AID.SmashAxe); // animLock=???
        d.RegisterSpell(AID.Capture); // animLock=???
        d.RegisterSpell(AID.FirstBattlehorn); // animLock=???
        d.RegisterSpell(AID.Gauge); // animLock=???
        d.RegisterSpell(AID.AxebladeBite); // animLock=???
        d.RegisterSpell(AID.AvalancheAxe); // animLock=???
        d.RegisterSpell(AID.PartingBlow); // animLock=???
        d.RegisterSpell(AID.Trick); // animLock=???
        d.RegisterSpell(AID.MistralAxe); // animLock=???
        d.RegisterSpell(AID.SecondBattlehorn); // animLock=???
        d.RegisterSpell(AID.Shieldsplitter); // animLock=???
        d.RegisterSpell(AID.SpinningAxe); // animLock=???
        d.RegisterSpell(AID.GaleAxe); // animLock=???
        d.RegisterSpell(AID.TemperedRelease1); // animLock=???
        d.RegisterSpell(AID.TemperedRelease2); // animLock=???
        d.RegisterSpell(AID.ThirdBattlehorn); // animLock=???
        d.RegisterSpell(AID.Borrow); // animLock=???
        d.RegisterSpell(AID.BorrowBeast); // animLock=???
        d.RegisterSpell(AID.BorrowVile); // animLock=???
        d.RegisterSpell(AID.BorrowCloud); // animLock=???
        d.RegisterSpell(AID.BorrowSeed); // animLock=???
        d.RegisterSpell(AID.BorrowWave); // animLock=???
        d.RegisterSpell(AID.BorrowScale); // animLock=???
        d.RegisterSpell(AID.BorrowSoul); // animLock=???
        d.RegisterSpell(AID.BorrowAsh); // animLock=???
        d.RegisterSpell(AID.BeastMode); // animLock=???
        d.RegisterSpell(AID.Beastskin); // animLock=???
        d.RegisterSpell(AID.Vileskin); // animLock=???
        d.RegisterSpell(AID.CloudSkim1, instantAnimLock: 0.8f);
        d.RegisterSpell(AID.CloudSkim2, instantAnimLock: 0.8f);
        d.RegisterSpell(AID.CloudSkim3, instantAnimLock: 0.8f);
        d.RegisterSpell(AID.CloudSkim4, instantAnimLock: 0.8f);
        d.RegisterSpell(AID.CloudSkim5, instantAnimLock: 0.8f);
        d.RegisterSpell(AID.Seedsower); // animLock=???
        d.RegisterSpell(AID.QuellingWave); // animLock=???
        d.RegisterSpell(AID.Scaleskin); // animLock=???
        d.RegisterSpell(AID.SoulCrush); // animLock=???
        d.RegisterSpell(AID.ScouringAsh); // animLock=???
        d.RegisterSpell(AID.ShieldCharge, instantAnimLock: 1.1f);
        d.RegisterSpell(AID.Rally); // animLock=0.600
        d.RegisterSpell(AID.RallyingCheer); // animLock=???
        d.RegisterSpell(AID.BrutalRage, instantAnimLock: 0.9f);
        d.RegisterSpell(AID.HawkishTalons, instantAnimLock: 0.9f);
        d.RegisterSpell(AID.RisenFall, instantAnimLock: 0.9f);
        d.RegisterSpell(AID.Calamity, instantAnimLock: 0.9f);

        d.RegisterSpell(AID.InstinctualComboVolant);
        d.RegisterSpell(AID.InstinctualComboRampant);
        d.RegisterSpell(AID.InstinctualComboDurant);
        d.RegisterSpell(AID.InstinctualComboEldritch);
        d.RegisterSpell(AID.IntentionalComboSunstrider);
        d.RegisterSpell(AID.IntentionalComboMoonstalker);
        d.RegisterSpell(AID.InfinitiveComboUniversality);

        Customize(d);
    }

    private void Customize(ActionDefinitions d)
    {

    }
}

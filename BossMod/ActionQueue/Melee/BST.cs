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
    PartingBlow = 44891, // L6, instant, 10.0s CD (group 2), range 25, single-target, targets=Hostile, animLock=???
    InstinctualComboDurant = 44912, // L8, instant, range 100, single-target, targets=Hostile, animLock=???
    InstinctualComboRampant = 44911, // L8, instant, range 100, single-target, targets=Hostile, animLock=???
    InstinctualComboVolant = 44910, // L8, instant, range 100, single-target, targets=Hostile, animLock=???
    IntentionalComboMoonstalker = 44915, // L8, instant, range 100, single-target, targets=Hostile, animLock=???
    Trick = 47093, // L8, instant, 3.0s CD (group 18), range 30, single-target, targets=Hostile, animLock=???
    InstinctualComboEldritch = 44913, // L8, instant, range 100, single-target, targets=Hostile, animLock=???
    IntentionalComboSunstrider = 44914, // L8, instant, range 100, single-target, targets=Hostile, animLock=???
    MistralAxe = 44887, // L8, instant, 5.0s CD (group 15), range 3, single-target, targets=Hostile, animLock=???
    SecondBattlehorn = 44892, // L10, 1.0s cast, 2.0s CD (group 4), range 0, single-target, targets=Self, animLock=???
    Shieldsplitter = 44885, // L12, instant, GCD, range 3, single-target, targets=Hostile, animLock=???
    SpinningAxe = 44888, // L14, instant, 5.0s CD (group 15), range 3, single-target, targets=Hostile, animLock=???
    GaleAxe = 44889, // L16, instant, 5.0s CD (group 15), range 3, single-target, targets=Hostile, animLock=???
    TemperedRelease1 = 44890, // L18, instant, 30.0s CD (group 6), range 0, single-target, targets=Self, animLock=???
    TemperedRelease2 = 47092, // L18, instant, 30.0s CD (group 6), range 30, single-target, targets=Hostile, animLock=???
    ThirdBattlehorn = 44894, // L20, 1.0s cast, 2.0s CD (group 5), range 0, single-target, targets=Self, animLock=???
    BeastMode = 44886, // L22, instant, GCD, range 0, single-target, targets=Self, animLock=???
    Beastskin = 44896, // L22, instant, 90.0s CD (group 1), range 0, single-target, targets=Self, animLock=???
    Borrow1 = 44895, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Borrow2 = 47238, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Borrow3 = 47239, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Borrow4 = 47240, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Borrow5 = 47241, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Borrow6 = 47242, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Borrow7 = 47243, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Borrow8 = 47244, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    Borrow9 = 47245, // L22, instant, 30.0s CD (group 7), range 0, single-target, targets=Self, animLock=???
    CloudSkim1 = 44898, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    CloudSkim2 = 45039, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    CloudSkim3 = 45038, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    CloudSkim4 = 45040, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    CloudSkim5 = 45041, // L22, instant, 3.0s CD (group 8), range 10, ???, targets=Area, animLock=???
    QuellingWave = 44900, // L22, instant, GCD, range 30, single-target, targets=Hostile, animLock=???
    Scaleskin = 44901, // L22, instant, 90.0s CD (group 10), range 0, single-target, targets=Self, animLock=???
    ScouringAsh = 44903, // L22, instant, 60.0s CD (group 12), range 30, single-target, targets=Self/Party/OwnPet, animLock=???
    Seedsower = 44899, // L22, instant, 90.0s CD (group 9), range 0, AOE 6 circle, targets=Self, animLock=???
    SoulCrush = 44902, // L22, instant, 30.0s CD (group 11), range 3, single-target, targets=Hostile, animLock=???
    Vileskin = 44897, // L22, instant, 90.0s CD (group 0), range 0, single-target, targets=Self, animLock=???
    ShieldCharge = 44893, // L24, instant, 60.0s CD (group 19/70), range 20, AOE 6 circle, targets=Hostile, animLock=???
    Rally = 44905, // L28, instant, 120.0s CD (group 14), range 0, single-target, targets=Self, animLock=???
    RallyingCheer = 44904, // L40, instant, 120.0s CD (group 13), range 0, single-target, targets=Self, animLock=???
    RisenFall = 44932, // L50, instant, 5.0s CD (group 15), range 25, AOE 6 circle, targets=Hostile, animLock=???
    HawkishTalons = 44931, // L50, instant, 5.0s CD (group 15), range 25, AOE 6 circle, targets=Hostile, animLock=???
    BrutalRage = 44930, // L50, instant, 5.0s CD (group 15), range 25, AOE 6 circle, targets=Hostile, animLock=???
    Calamity = 44933, // L50, instant, 5.0s CD (group 15), range 10, AOE 10+R width 3 rect, targets=Hostile, animLock=???
    InfinitiveComboUniversality = 44916, // L50, instant, range 100, single-target, targets=Hostile, animLock=???
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
        d.RegisterSpell(AID.InstinctualComboDurant); // animLock=???
        d.RegisterSpell(AID.InstinctualComboRampant); // animLock=???
        d.RegisterSpell(AID.InstinctualComboVolant); // animLock=???
        d.RegisterSpell(AID.IntentionalComboMoonstalker); // animLock=???
        d.RegisterSpell(AID.Trick); // animLock=???
        d.RegisterSpell(AID.InstinctualComboEldritch); // animLock=???
        d.RegisterSpell(AID.IntentionalComboSunstrider); // animLock=???
        d.RegisterSpell(AID.MistralAxe); // animLock=???
        d.RegisterSpell(AID.SecondBattlehorn); // animLock=???
        d.RegisterSpell(AID.Shieldsplitter); // animLock=???
        d.RegisterSpell(AID.SpinningAxe); // animLock=???
        d.RegisterSpell(AID.GaleAxe); // animLock=???
        d.RegisterSpell(AID.TemperedRelease1); // animLock=???
        d.RegisterSpell(AID.TemperedRelease2); // animLock=???
        d.RegisterSpell(AID.ThirdBattlehorn); // animLock=???
        d.RegisterSpell(AID.Borrow7); // animLock=???
        d.RegisterSpell(AID.CloudSkim3); // animLock=???
        d.RegisterSpell(AID.CloudSkim5); // animLock=???
        d.RegisterSpell(AID.CloudSkim4); // animLock=???
        d.RegisterSpell(AID.Borrow2); // animLock=???
        d.RegisterSpell(AID.Borrow3); // animLock=???
        d.RegisterSpell(AID.Borrow4); // animLock=???
        d.RegisterSpell(AID.Borrow5); // animLock=???
        d.RegisterSpell(AID.CloudSkim2); // animLock=???
        d.RegisterSpell(AID.Borrow6); // animLock=???
        d.RegisterSpell(AID.Borrow9); // animLock=???
        d.RegisterSpell(AID.QuellingWave); // animLock=???
        d.RegisterSpell(AID.BeastMode); // animLock=???
        d.RegisterSpell(AID.Borrow1); // animLock=???
        d.RegisterSpell(AID.Beastskin); // animLock=???
        d.RegisterSpell(AID.Borrow8); // animLock=???
        d.RegisterSpell(AID.ScouringAsh); // animLock=???
        d.RegisterSpell(AID.SoulCrush); // animLock=???
        d.RegisterSpell(AID.Scaleskin); // animLock=???
        d.RegisterSpell(AID.Seedsower); // animLock=???
        d.RegisterSpell(AID.CloudSkim1); // animLock=???
        d.RegisterSpell(AID.Vileskin); // animLock=???
        d.RegisterSpell(AID.ShieldCharge, instantAnimLock: 1.1f); // animLock=???
        d.RegisterSpell(AID.Rally); // animLock=???
        d.RegisterSpell(AID.RallyingCheer); // animLock=???
        d.RegisterSpell(AID.RisenFall); // animLock=???
        d.RegisterSpell(AID.HawkishTalons); // animLock=???
        d.RegisterSpell(AID.BrutalRage); // animLock=???
        d.RegisterSpell(AID.Calamity); // animLock=???
        d.RegisterSpell(AID.InfinitiveComboUniversality); // animLock=???

        Customize(d);
    }

    private void Customize(ActionDefinitions d)
    {

    }
}

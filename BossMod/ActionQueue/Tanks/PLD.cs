namespace BossMod.PLD;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    LastBastion = 199, // LB3, instant, range 0, AOE 50 circle, targets=self, animLock=3.860s?
    FastBlade = 9, // L1, instant, GCD, range 3, single-target, targets=hostile
    FightOrFlight = 20, // L2, instant, 60.0s CD (group 10), range 0, single-target, targets=self
    RiotBlade = 15, // L4, instant, GCD, range 3, single-target, targets=hostile
    TotalEclipse = 7381, // L6, instant, GCD, range 0, AOE 5 circle, targets=self
    ReleaseIronWill = 32065, // L10, instant, 1.0s CD (group 3), range 0, single-target, targets=self
    IronWill = 28, // L10, instant, 2.0s CD (group 3), range 0, single-target, targets=self
    ShieldBash = 16, // L10, instant, GCD, range 3, single-target, targets=hostile
    ShieldLob = 24, // L15, instant, GCD, range 20, single-target, targets=hostile
    RageOfHalone = 21, // L26, instant, GCD, range 3, single-target, targets=hostile
    SpiritsWithin = 29, // L30, instant, 30.0s CD (group 5), range 3, single-target, targets=hostile
    Sheltron = 3542, // L35, instant, 5.0s CD (group 0), range 0, single-target, targets=self
    Sentinel = 17, // L38, instant, 120.0s CD (group 19), range 0, single-target, targets=self
    Prominence = 16457, // L40, instant, GCD, range 0, AOE 5 circle, targets=self
    Cover = 27, // L45, instant, 120.0s CD (group 20), range 10, single-target, targets=party
    CircleOfScorn = 23, // L50, instant, 30.0s CD (group 4), range 0, AOE 5 circle, targets=self
    HallowedGround = 30, // L50, instant, 420.0s CD (group 24), range 0, single-target, targets=self
    Bulwark = 22, // L52, instant, 90.0s CD (group 15), range 0, single-target, targets=self
    GoringBlade = 3538, // L54, instant, 60.0s CD (group 12/57), range 3, single-target, targets=hostile
    DivineVeil = 3540, // L56, instant, 90.0s CD (group 14), range 0, AOE 30 circle, targets=self
    Clemency = 3541, // L58, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    RoyalAuthority = 3539, // L60, instant, GCD, range 3, single-target, targets=hostile
    Intervention = 7382, // L62, instant, 10.0s CD (group 1), range 30, single-target, targets=party
    HolySpirit = 7384, // L64, 1.5s cast, GCD, range 25, single-target, targets=hostile
    Requiescat = 7383, // L68, instant, 60.0s CD (group 11), range 3, single-target, targets=hostile
    PassageOfArms = 7385, // L70, instant, 120.0s CD (group 21), range 0, ???, targets=self
    HolyCircle = 16458, // L72, 1.5s cast, GCD, range 0, AOE 5 circle, targets=self
    Intervene = 16461, // L74, instant, 30.0s CD (group 9/70) (2 charges), range 20, single-target, targets=hostile
    Atonement = 16460, // L76, instant, GCD, range 3, single-target, targets=hostile
    Confiteor = 16459, // L80, instant, GCD, range 25, AOE 5 circle, targets=hostile
    HolySheltron = 25746, // L82, instant, 5.0s CD (group 2), range 0, single-target, targets=self
    Expiacion = 25747, // L86, instant, 30.0s CD (group 5), range 3, AOE 5 circle, targets=hostile
    BladeOfFaith = 25748, // L90, instant, GCD, range 25, AOE 5 circle, targets=hostile
    BladeOfTruth = 25749, // L90, instant, GCD, range 25, AOE 5 circle, targets=hostile
    BladeOfValor = 25750, // L90, instant, GCD, range 25, AOE 5 circle, targets=hostile

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
    TankMastery = 317, // L1
    OathMastery = 209, // L35, gauge unlock
    Chivalry = 246, // L58, riot blade & spirits within restore mp
    RageOfHaloneMastery = 260, // L60, rage of halone -> royal authority upgrade
    DivineMagicMastery1 = 207, // L64, reduce mp cost and prevent interruptions
    EnhancedProminence = 261, // L72, prominence restores mp
    EnhancedSheltron = 262, // L74, duration increase
    SwordOath = 264, // L76
    EnhancedRequiescat = 263, // L80
    SheltronMastery = 412, // L82, sheltron -> holy sheltron upgrade
    EnhancedIntervention = 413, // L82
    DivineMagicMastery2 = 414, // L84, adds heal
    MeleeMastery = 504, // L84, potency increase
    SpiritsWithinMastery = 415, // L86, spirits within -> expiacion upgrade
    EnhancedDivineVeil = 416, // L88, adds heal
}

public sealed class Definitions : IDisposable
{
    public static readonly uint[] UnlockQuests = [65798, 66591, 66592, 66593, 66595, 66596, 67570, 67571, 67572, 67573, 68111];

    public static bool Unlocked(AID id, int level, int questProgress) => id switch
    {
        AID.FightOrFlight => level >= 2,
        AID.RiotBlade => level >= 4,
        AID.TotalEclipse => level >= 6,
        AID.Rampart => level >= 8,
        AID.ReleaseIronWill => level >= 10,
        AID.IronWill => level >= 10,
        AID.ShieldBash => level >= 10,
        AID.LowBlow => level >= 12,
        AID.ShieldLob => level >= 15 && questProgress > 0,
        AID.Provoke => level >= 15,
        AID.Interject => level >= 18,
        AID.Reprisal => level >= 22,
        AID.RageOfHalone => level >= 26,
        AID.SpiritsWithin => level >= 30 && questProgress > 1,
        AID.ArmsLength => level >= 32,
        AID.Sheltron => level >= 35 && questProgress > 2,
        AID.Sentinel => level >= 38,
        AID.Prominence => level >= 40 && questProgress > 3,
        AID.Cover => level >= 45 && questProgress > 4,
        AID.Shirk => level >= 48,
        AID.CircleOfScorn => level >= 50,
        AID.HallowedGround => level >= 50 && questProgress > 5,
        AID.Bulwark => level >= 52,
        AID.GoringBlade => level >= 54 && questProgress > 6,
        AID.DivineVeil => level >= 56 && questProgress > 7,
        AID.Clemency => level >= 58 && questProgress > 8,
        AID.RoyalAuthority => level >= 60 && questProgress > 9,
        AID.Intervention => level >= 62,
        AID.HolySpirit => level >= 64,
        AID.Requiescat => level >= 68,
        AID.PassageOfArms => level >= 70 && questProgress > 10,
        AID.HolyCircle => level >= 72,
        AID.Intervene => level >= 74,
        AID.Atonement => level >= 76,
        AID.Confiteor => level >= 80,
        AID.HolySheltron => level >= 82,
        AID.Expiacion => level >= 86,
        AID.BladeOfFaith => level >= 90,
        AID.BladeOfTruth => level >= 90,
        AID.BladeOfValor => level >= 90,
        _ => true
    };

    public static bool Unlocked(TraitID id, int level, int questProgress) => id switch
    {
        TraitID.OathMastery => level >= 35 && questProgress > 2,
        TraitID.Chivalry => level >= 58,
        TraitID.RageOfHaloneMastery => level >= 60 && questProgress > 9,
        TraitID.DivineMagicMastery1 => level >= 64,
        TraitID.EnhancedProminence => level >= 72,
        TraitID.EnhancedSheltron => level >= 74,
        TraitID.SwordOath => level >= 76,
        TraitID.EnhancedRequiescat => level >= 80,
        TraitID.SheltronMastery => level >= 82,
        TraitID.EnhancedIntervention => level >= 82,
        TraitID.DivineMagicMastery2 => level >= 84,
        TraitID.MeleeMastery => level >= 84,
        TraitID.SpiritsWithinMastery => level >= 86,
        TraitID.EnhancedDivineVeil => level >= 88,
        _ => true
    };

    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.LastBastion, instantAnimLock: 3.86f);
        d.RegisterSpell(AID.FastBlade);
        d.RegisterSpell(AID.FightOrFlight);
        d.RegisterSpell(AID.RiotBlade);
        d.RegisterSpell(AID.TotalEclipse);
        d.RegisterSpell(AID.ReleaseIronWill);
        d.RegisterSpell(AID.IronWill);
        d.RegisterSpell(AID.ShieldBash);
        d.RegisterSpell(AID.ShieldLob);
        d.RegisterSpell(AID.RageOfHalone);
        d.RegisterSpell(AID.SpiritsWithin);
        d.RegisterSpell(AID.Sheltron);
        d.RegisterSpell(AID.Sentinel);
        d.RegisterSpell(AID.Prominence);
        d.RegisterSpell(AID.Cover);
        d.RegisterSpell(AID.CircleOfScorn);
        d.RegisterSpell(AID.HallowedGround);
        d.RegisterSpell(AID.Bulwark);
        d.RegisterSpell(AID.GoringBlade);
        d.RegisterSpell(AID.DivineVeil);
        d.RegisterSpell(AID.Clemency);
        d.RegisterSpell(AID.RoyalAuthority);
        d.RegisterSpell(AID.Intervention);
        d.RegisterSpell(AID.HolySpirit);
        d.RegisterSpell(AID.Requiescat);
        d.RegisterSpell(AID.PassageOfArms);
        d.RegisterSpell(AID.HolyCircle);
        d.RegisterSpell(AID.Intervene, maxCharges: 2);
        d.RegisterSpell(AID.Atonement);
        d.RegisterSpell(AID.Confiteor);
        d.RegisterSpell(AID.HolySheltron);
        d.RegisterSpell(AID.Expiacion);
        d.RegisterSpell(AID.BladeOfFaith);
        d.RegisterSpell(AID.BladeOfTruth);
        d.RegisterSpell(AID.BladeOfValor);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        //d.Spell(AID.LastBastion)!.EffectDuration = 8;
        //d.Spell(AID.FightOrFlight)!.EffectDuration = 20;
        //d.Spell(AID.Sheltron)!.EffectDuration = 4; // TODO: duration increases to 6...
        //d.Spell(AID.Sentinel)!.EffectDuration = 15;
        // TODO: Cover effect duration 12?..
        //d.Spell(AID.HallowedGround)!.EffectDuration = 10;
        //d.Spell(AID.DivineVeil)!.EffectDuration = 30;
        // TODO: Intervention effect duration?
    }
}

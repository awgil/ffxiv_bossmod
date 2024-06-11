namespace BossMod.WHM;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    PulseOfLife = 208, // LB3, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=8.100
    Stone1 = 119, // L1, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Cure1 = 120, // L2, 1.5s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly, animLock=???
    Aero1 = 121, // L4, instant, GCD, range 25, single-target, targets=hostile, animLock=???
    Medica1 = 124, // L10, 2.0s cast, GCD, range 0, AOE 15 circle, targets=self
    Raise = 125, // L12, 8.0s cast, GCD, range 30, single-target, targets=party/alliance/friendly
    Stone2 = 127, // L18, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Cure2 = 135, // L30, 2.0s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    PresenceOfMind = 136, // L30, instant, 120.0s CD (group 22), range 0, single-target, targets=self
    Regen = 137, // L35, instant, GCD, range 30, single-target, targets=self/party/alliance/friendly
    Cure3 = 131, // L40, 2.0s cast, GCD, range 30, AOE 10 circle, targets=self/party
    Holy1 = 139, // L45, 2.5s cast, GCD, range 0, AOE 8 circle, targets=self, animLock=???
    Aero2 = 132, // L46, instant, GCD, range 25, single-target, targets=hostile
    Benediction = 140, // L50, instant, 180.0s CD (group 23), range 30, single-target, targets=self/party/alliance/friendly
    Medica2 = 133, // L50, 2.0s cast, GCD, range 0, AOE 20 circle, targets=self
    Asylum = 3569, // L52, instant, 90.0s CD (group 17), range 30, ???, targets=area
    AfflatusSolace = 16531, // L52, instant, GCD, range 30, single-target, targets=self/party/alliance/friendly
    Stone3 = 3568, // L54, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    Assize = 3571, // L56, instant, 40.0s CD (group 5), range 0, AOE 15 circle, targets=self
    ThinAir = 7430, // L58, instant, 60.0s CD (group 20/70) (2? charges), range 0, single-target, targets=self
    Tetragrammaton = 3570, // L60, instant, 60.0s CD (group 13), range 30, single-target, targets=self/party/alliance/friendly
    Stone4 = 7431, // L64, 1.5s cast, GCD, range 25, single-target, targets=hostile
    DivineBenison = 7432, // L66, instant, 30.0s CD (group 9/71), range 30, single-target, targets=self/party
    PlenaryIndulgence = 7433, // L70, instant, 60.0s CD (group 10), range 0, AOE 20 circle, targets=self
    Dia = 16532, // L72, instant, GCD, range 25, single-target, targets=hostile
    Glare1 = 16533, // L72, 1.5s cast, GCD, range 25, single-target, targets=hostile, animLock=???
    AfflatusMisery = 16535, // L74, instant, GCD, range 25, AOE 5 circle, targets=hostile
    AfflatusRapture = 16534, // L76, instant, GCD, range 0, AOE 20 circle, targets=self
    Temperance = 16536, // L80, instant, 120.0s CD (group 21), range 0, single-target, targets=self
    Glare3 = 25859, // L82, 1.5s cast, GCD, range 25, single-target, targets=hostile
    Holy3 = 25860, // L82, 2.5s cast, GCD, range 0, AOE 8 circle, targets=self
    Aquaveil = 25861, // L86, instant, 60.0s CD (group 11), range 30, single-target, targets=self/party
    LiturgyOfTheBell = 25862, // L90, instant, 180.0s CD (group 24), range 30, ???, targets=area
    LiturgyOfTheBellEnd = 28509, // L90, instant, 1.0s CD (group 0), range 0, single-target, targets=self

    // Shared
    HealingWind = ClassShared.AID.HealingWind, // LB1, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=2.100
    BreathOfTheEarth = ClassShared.AID.BreathOfTheEarth, // LB2, 2.0s cast, range 0, AOE 50 circle, targets=self, castAnimLock=5.130
    Repose = ClassShared.AID.Repose, // L8, 2.5s cast, GCD, range 30, single-target, targets=hostile
    Esuna = ClassShared.AID.Esuna, // L10, 1.0s cast, GCD, range 30, single-target, targets=self/party/alliance/friendly
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 45), range 0, single-target, targets=self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 44), range 0, single-target, targets=self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    Rescue = ClassShared.AID.Rescue, // L48, instant, 120.0s CD (group 49), range 30, single-target, targets=party
}

public enum TraitID : uint
{
    None = 0,
    StoneMastery1 = 179, // L18, stone1 -> stone2 upgrade
    MaimAndMend1 = 23, // L20, heal and damage increase
    Freecure = 25, // L32, cure1 can proc a buff that makes cure2 free
    MaimAndMend2 = 26, // L40, heal and damage increase
    AeroMastery1 = 180, // L46, aero1 -> aero2 upgrade
    SecretOfTheLily = 196, // L52, lily gauge
    StoneMastery2 = 181, // L54, stone2 -> stone3 upgrade
    StoneMastery3 = 182, // L64, store3 -> stone4 upgrade
    AeroMastery2 = 307, // L72, aero2 -> dia upgrade
    StoneMastery4 = 308, // L72, stone4 -> glare1 upgrade
    TranscendentAfflatus = 309, // L74, blood lily
    EnhancedAsylum = 310, // L78, healing taken buff
    GlareMastery = 487, // L82, glare1 -> glare3 upgrade
    HolyMastery = 488, // L82, holy1 -> holy3 upgrade
    EnhancedHealingMagic = 489, // L85, potency increase
    EnhancedDivineBenison = 490, // L88, second charge
}

public sealed class Definitions : IDisposable
{
    public static readonly uint[] UnlockQuests = [65977, 66615, 66616, 66617, 66619, 66620, 67256, 67257, 67258, 67259, 67261, 67954];

    public static bool Unlocked(AID id, int level, int questProgress) => id switch
    {
        AID.Cure1 => level >= 2,
        AID.Aero1 => level >= 4,
        AID.Repose => level >= 8,
        AID.Esuna => level >= 10,
        AID.Medica1 => level >= 10,
        AID.Raise => level >= 12,
        AID.LucidDreaming => level >= 14,
        AID.Stone2 => level >= 18,
        AID.Swiftcast => level >= 18,
        AID.Cure2 => level >= 30 && questProgress > 0,
        AID.PresenceOfMind => level >= 30 && questProgress > 1,
        AID.Regen => level >= 35 && questProgress > 2,
        AID.Cure3 => level >= 40 && questProgress > 3,
        AID.Surecast => level >= 44,
        AID.Holy1 => level >= 45 && questProgress > 4,
        AID.Aero2 => level >= 46,
        AID.Rescue => level >= 48,
        AID.Benediction => level >= 50 && questProgress > 5,
        AID.Medica2 => level >= 50,
        AID.Asylum => level >= 52 && questProgress > 6,
        AID.AfflatusSolace => level >= 52,
        AID.Stone3 => level >= 54 && questProgress > 7,
        AID.Assize => level >= 56 && questProgress > 8,
        AID.ThinAir => level >= 58 && questProgress > 9,
        AID.Tetragrammaton => level >= 60 && questProgress > 10,
        AID.Stone4 => level >= 64,
        AID.DivineBenison => level >= 66,
        AID.PlenaryIndulgence => level >= 70 && questProgress > 11,
        AID.Dia => level >= 72,
        AID.Glare1 => level >= 72,
        AID.AfflatusMisery => level >= 74,
        AID.AfflatusRapture => level >= 76,
        AID.Temperance => level >= 80,
        AID.Glare3 => level >= 82,
        AID.Holy3 => level >= 82,
        AID.Aquaveil => level >= 86,
        AID.LiturgyOfTheBell => level >= 90,
        AID.LiturgyOfTheBellEnd => level >= 90,
        _ => true
    };

    public static bool Unlocked(TraitID id, int level, int questProgress) => id switch
    {
        TraitID.StoneMastery1 => level >= 18,
        TraitID.MaimAndMend1 => level >= 20,
        TraitID.Freecure => level >= 32,
        TraitID.MaimAndMend2 => level >= 40,
        TraitID.AeroMastery1 => level >= 46,
        TraitID.SecretOfTheLily => level >= 52 && questProgress > 6,
        TraitID.StoneMastery2 => level >= 54 && questProgress > 7,
        TraitID.StoneMastery3 => level >= 64,
        TraitID.AeroMastery2 => level >= 72,
        TraitID.StoneMastery4 => level >= 72,
        TraitID.TranscendentAfflatus => level >= 74,
        TraitID.EnhancedAsylum => level >= 78,
        TraitID.GlareMastery => level >= 82,
        TraitID.HolyMastery => level >= 82,
        TraitID.EnhancedHealingMagic => level >= 85,
        TraitID.EnhancedDivineBenison => level >= 88,
        _ => true
    };

    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.PulseOfLife, castAnimLock: 8.10f);
        d.RegisterSpell(AID.Stone1); // animLock=???
        d.RegisterSpell(AID.Cure1); // animLock=???
        d.RegisterSpell(AID.Aero1); // animLock=???
        d.RegisterSpell(AID.Medica1);
        d.RegisterSpell(AID.Raise);
        d.RegisterSpell(AID.Stone2); // animLock=???
        d.RegisterSpell(AID.Cure2);
        d.RegisterSpell(AID.PresenceOfMind);
        d.RegisterSpell(AID.Regen);
        d.RegisterSpell(AID.Cure3);
        d.RegisterSpell(AID.Holy1); // animLock=???
        d.RegisterSpell(AID.Aero2);
        d.RegisterSpell(AID.Benediction);
        d.RegisterSpell(AID.Medica2);
        d.RegisterSpell(AID.Asylum);
        d.RegisterSpell(AID.AfflatusSolace);
        d.RegisterSpell(AID.Stone3); // animLock=???
        d.RegisterSpell(AID.Assize);
        d.RegisterSpell(AID.ThinAir, maxCharges: 2);
        d.RegisterSpell(AID.Tetragrammaton);
        d.RegisterSpell(AID.Stone4);
        d.RegisterSpell(AID.DivineBenison);
        d.RegisterSpell(AID.PlenaryIndulgence);
        d.RegisterSpell(AID.Dia);
        d.RegisterSpell(AID.Glare1); // animLock=???
        d.RegisterSpell(AID.AfflatusMisery);
        d.RegisterSpell(AID.AfflatusRapture);
        d.RegisterSpell(AID.Temperance);
        d.RegisterSpell(AID.Glare3);
        d.RegisterSpell(AID.Holy3);
        d.RegisterSpell(AID.Aquaveil);
        d.RegisterSpell(AID.LiturgyOfTheBell);
        d.RegisterSpell(AID.LiturgyOfTheBellEnd);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // *** add any properties that can't be autogenerated here ***
    }
}

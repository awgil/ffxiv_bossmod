namespace BossMod.WHM;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    PulseOfLife = 208, // LB3, 2.0s cast, range 0, AOE 50 circle, targets=Self, animLock=8.100s?
    Stone = 119, // L1, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    Cure = 120, // L2, 1.5s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    Aero = 121, // L4, instant, GCD, range 25, single-target, targets=Hostile
    Medica = 124, // L10, 2.0s cast, GCD, range 0, AOE 15 circle, targets=Self
    Raise = 125, // L12, 8.0s cast, GCD, range 30, single-target, targets=Party/Alliance/Friendly/Dead
    StoneII = 127, // L18, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    PresenceOfMind = 136, // L30, instant, 120.0s CD (group 20), range 0, single-target, targets=Self
    CureII = 135, // L30, 2.0s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    Regen = 137, // L35, instant, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    AetherialShift = 37008, // L40, instant, 60.0s CD (group 12), range 0, single-target, targets=Self, animLock=???
    CureIII = 131, // L40, 2.0s cast, GCD, range 30, AOE 10 circle, targets=Self/Party
    Holy = 139, // L45, 2.5s cast, GCD, range 0, AOE 8 circle, targets=Self
    AeroII = 132, // L46, instant, GCD, range 25, single-target, targets=Hostile
    MedicaII = 133, // L50, 2.0s cast, GCD, range 0, AOE 20 circle, targets=Self
    Benediction = 140, // L50, instant, 180.0s CD (group 23), range 30, single-target, targets=Self/Party/Alliance/Friendly
    AfflatusSolace = 16531, // L52, instant, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    Asylum = 3569, // L52, instant, 90.0s CD (group 14), range 30, ???, targets=Area
    StoneIII = 3568, // L54, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    Assize = 3571, // L56, instant, 40.0s CD (group 7), range 0, AOE 15 circle, targets=Self
    ThinAir = 7430, // L58, instant, 60.0s CD (group 18/70) (2 charges), range 0, single-target, targets=Self
    Tetragrammaton = 3570, // L60, instant, 60.0s CD (group 19/72), range 30, single-target, targets=Self/Party/Alliance/Friendly
    StoneIV = 7431, // L64, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    DivineBenison = 7432, // L66, instant, 30.0s CD (group 9/71) (1-2 charges), range 30, single-target, targets=Self/Party
    PlenaryIndulgence = 7433, // L70, instant, 60.0s CD (group 10), range 0, AOE 20 circle, targets=Self
    Dia = 16532, // L72, instant, GCD, range 25, single-target, targets=Hostile
    GlareI = 16533, // L72, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    AfflatusMisery = 16535, // L74, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    AfflatusRapture = 16534, // L76, instant, GCD, range 0, AOE 20 circle, targets=Self
    Temperance = 16536, // L80, instant, 120.0s CD (group 21), range 0, single-target, targets=Self
    GlareIII = 25859, // L82, 1.5s cast, GCD, range 25, single-target, targets=Hostile
    HolyIII = 25860, // L82, 2.5s cast, GCD, range 0, AOE 8 circle, targets=Self
    Aquaveil = 25861, // L86, instant, 60.0s CD (group 11), range 30, single-target, targets=Self/Party
    LiturgyOfTheBell = 25862, // L90, instant, 180.0s CD (group 22), range 30, ???, targets=Area
    LiturgyOfTheBellEnd = 28509, // L90, instant, 1.0s CD (group 0), range 0, single-target, targets=Self
    GlareIV = 37009, // L92, instant, GCD, range 25, AOE 5 circle, targets=Hostile
    MedicaIII = 37010, // L96, 2.0s cast, GCD, range 0, AOE 20 circle, targets=Self, animLock=???
    DivineCaress = 37011, // L100, instant, 1.0s CD (group 1), range 0, AOE 15 circle, targets=Self

    // Shared
    HealingWind = ClassShared.AID.HealingWind, // LB1, 2.0s cast, range 0, AOE 50 circle, targets=Self, animLock=2.100s?
    BreathOfTheEarth = ClassShared.AID.BreathOfTheEarth, // LB2, 2.0s cast, range 0, AOE 50 circle, targets=Self, animLock=5.130s?
    Repose = ClassShared.AID.Repose, // L8, 2.5s cast, GCD, range 30, single-target, targets=Hostile
    Esuna = ClassShared.AID.Esuna, // L10, 1.0s cast, GCD, range 30, single-target, targets=Self/Party/Alliance/Friendly
    LucidDreaming = ClassShared.AID.LucidDreaming, // L14, instant, 60.0s CD (group 44), range 0, single-target, targets=Self
    Swiftcast = ClassShared.AID.Swiftcast, // L18, instant, 60.0s CD (group 43), range 0, single-target, targets=Self
    Surecast = ClassShared.AID.Surecast, // L44, instant, 120.0s CD (group 48), range 0, single-target, targets=Self
    Rescue = ClassShared.AID.Rescue, // L48, instant, 120.0s CD (group 49), range 30, single-target, targets=Party

    #region PvP
    GlareIIIPvP = 29223,
    CureIIPvP = 29224,
    AfflatusMiseryPvP = 29226,
    AquaveilPvP = 29227,
    MiracleOfNaturePvP = 29228,
    SeraphStrikePvP = 29229,
    GlareIVPvP = 41499,
    CureIIIPvP = 29225,

    // LB
    AfflatusPurgationPvP = 29230,

    // Role
    HaelanPvP = 43255,
    StoneskinIIPvP = 43256,
    DiabrosisPvP = 43257,

    //Shared
    ElixirPvP = ClassShared.AID.ElixirPvP,
    RecuperatePvP = ClassShared.AID.RecuperatePvP,
    PurifyPvP = ClassShared.AID.PurifyPvP,
    GuardPvP = ClassShared.AID.GuardPvP,
    SprintPvP = ClassShared.AID.SprintPvP
    #endregion
}

public enum TraitID : uint
{
    None = 0,
    StoneMastery1 = 179, // L18
    MaimAndMend1 = 23, // L20
    Freecure = 25, // L32
    MaimAndMend2 = 26, // L40
    AeroMastery1 = 180, // L46
    SecretOfTheLily = 196, // L52
    StoneMastery2 = 181, // L54
    StoneMastery3 = 182, // L64
    AeroMastery2 = 307, // L72
    StoneMastery4 = 308, // L72
    TranscendentAfflatus = 309, // L74
    EnhancedAsylum = 310, // L78
    GlareMastery = 487, // L82
    HolyMastery = 488, // L82
    EnhancedHealingMagic = 489, // L85
    EnhancedDivineBenison = 490, // L88
    EnhancedPresenceOfMind = 623, // L92
    EnhancedSwiftcast = 644, // L94
    WhiteMagicMastery = 651, // L94
    MedicaMastery = 624, // L96
    EnhancedTetragrammaton = 625, // L98
    EnhancedTemperance = 626, // L100
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    Aero = 143, // applied by Aero1 to target, dot
    AeroII = 144, // applied by Aero2 to target, dot
    Dia = 1871, // applied by Dia to target, dot
    MedicaII = 150, // applied by Medica2 to targets, hot
    Freecure = 155, // applied by Cure1 to self, next cure2 is free
    ThinAir = 1217, // applied by Thin Air to self, next gcd costs no mp
    DivineBenison = 1218, // applied by Divine Benison to target, shield
    Confession = 1219, // applied by Plenary Indulgence to self, heal buff
    Temperance = 1872, // applied by Temperance to self, heal and mitigate buff
    PresenceOfMind = 157, // applied by Presence of Mind to self, damage buff
    Regen = 158, // applied by Regen to target, hp regen
    Asylum = 1911, // applied by Asylum to target, hp regen
    Aquaveil = 2708, // applied by Aquaveil to target, mitigate
    LiturgyOfTheBell = 2709, // applied by Liturgy of the Bell to target, heal on hit
    Sleep = 3, // applied by Repose to target
    Raise = 148, // applied by Raise to target
    SacredSight = 3879, // applied by Presence of Mind to self
    MedicaIII = 3880, // applied by Medica III to self/target
    DivineGrace = 3881, // applied by Temperance to self
    DivineCaress = 3903, // applied by Divine Caress to self/target

    //Shared
    Surecast = ClassShared.SID.Surecast, // applied by Surecast to self
    LucidDreaming = ClassShared.SID.LucidDreaming, // applied by Lucid Dreaming to self
    Swiftcast = ClassShared.SID.Swiftcast, // applied by Swiftcast to self

    #region PvP
    MiracleOfNaturePvP = 3085,
    CureIIIReadyPvP = 3083,
    SacredSightPvP = 4326,
    //SacredSightPvP = 3879,

    //Role
    HaelanEquippedPvP = ClassShared.SID.HaelanEquippedPvP,
    StoneskinEquippedPvP = ClassShared.SID.StoneskinEquippedPvP,
    DiabrosisEquippedPvP = ClassShared.SID.DiabrosisEquippedPvP,

    //Shared
    GuardPvP = ClassShared.SID.GuardPvP,
    SprintPvP = ClassShared.SID.SprintPvP,
    SilencePvP = ClassShared.SID.SilencePvP,
    BindPvP = ClassShared.SID.BindPvP,
    StunPvP = ClassShared.SID.StunPvP,
    HalfAsleepPvP = ClassShared.SID.HalfAsleepPvP,
    SleepPvP = ClassShared.SID.SleepPvP,
    DeepFreezePvP = ClassShared.SID.DeepFreezePvP,
    HeavyPvP = ClassShared.SID.HeavyPvP,
    UnguardedPvP = ClassShared.SID.UnguardedPvP,
    #endregion
}

public sealed class Definitions : IDisposable
{
    private readonly WHMConfig _config = Service.Config.Get<WHMConfig>();
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.PulseOfLife, castAnimLock: 8.10f); // animLock=8.100s?
        d.RegisterSpell(AID.Stone);
        d.RegisterSpell(AID.Cure);
        d.RegisterSpell(AID.Aero);
        d.RegisterSpell(AID.Medica);
        d.RegisterSpell(AID.Raise);
        d.RegisterSpell(AID.StoneII);
        d.RegisterSpell(AID.PresenceOfMind);
        d.RegisterSpell(AID.CureII);
        d.RegisterSpell(AID.Regen);
        d.RegisterSpell(AID.AetherialShift); // animLock=???
        d.RegisterSpell(AID.CureIII);
        d.RegisterSpell(AID.Holy);
        d.RegisterSpell(AID.AeroII);
        d.RegisterSpell(AID.MedicaII);
        d.RegisterSpell(AID.Benediction);
        d.RegisterSpell(AID.AfflatusSolace);
        d.RegisterSpell(AID.Asylum);
        d.RegisterSpell(AID.StoneIII);
        d.RegisterSpell(AID.Assize);
        d.RegisterSpell(AID.ThinAir);
        d.RegisterSpell(AID.Tetragrammaton);
        d.RegisterSpell(AID.StoneIV);
        d.RegisterSpell(AID.DivineBenison);
        d.RegisterSpell(AID.PlenaryIndulgence);
        d.RegisterSpell(AID.Dia);
        d.RegisterSpell(AID.GlareI);
        d.RegisterSpell(AID.AfflatusMisery);
        d.RegisterSpell(AID.AfflatusRapture);
        d.RegisterSpell(AID.Temperance);
        d.RegisterSpell(AID.GlareIII);
        d.RegisterSpell(AID.HolyIII);
        d.RegisterSpell(AID.Aquaveil);
        d.RegisterSpell(AID.LiturgyOfTheBell);
        d.RegisterSpell(AID.LiturgyOfTheBellEnd);
        d.RegisterSpell(AID.GlareIV);
        d.RegisterSpell(AID.MedicaIII); // animLock=???
        d.RegisterSpell(AID.DivineCaress);

        d.RegisterSpell(AID.GlareIIIPvP);
        d.RegisterSpell(AID.CureIIPvP);
        d.RegisterSpell(AID.AfflatusMiseryPvP);
        d.RegisterSpell(AID.AquaveilPvP);
        d.RegisterSpell(AID.MiracleOfNaturePvP);
        d.RegisterSpell(AID.SeraphStrikePvP);
        d.RegisterSpell(AID.GlareIVPvP);
        d.RegisterSpell(AID.CureIIIPvP);
        d.RegisterSpell(AID.AfflatusPurgationPvP);

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.DivineBenison, TraitID.EnhancedDivineBenison);
        d.RegisterChargeIncreaseTrait(AID.Tetragrammaton, TraitID.EnhancedTetragrammaton);

        d.Spell(AID.AetherialShift)!.TransformAngle = (ws, _, _, _) => _config.AlignDashToCamera
            ? ws.Client.CameraAzimuth + 180.Degrees()
            : null;
        d.Spell(AID.AetherialShift)!.ForbidExecute = ActionDefinitions.DashFixedDistanceCheck(15);
    }
}

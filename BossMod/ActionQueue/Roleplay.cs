namespace BossMod.Roleplay;

public enum AID : uint
{
    // deep dungeon transformations
    Pummel = 6273,
    VoidFireII = 6274,
    HeavenlyJudge = 6871,
    Rotosmash = 32781,
    WreckingBall = 32782,
    RockyRoll = 44628,
    BigBurst = 44629,

    // magitek reaper in Fly Free, My Pretty
    MagitekCannon = 7619, // range 30 radius 6 ground targeted aoe
    PhotonStream = 7620, // range 10 width 4 rect aoe
    DiffractiveMagitekCannon = 7621, // range 30 radius 10 ground targeted aoe
    HighPoweredMagitekCannon = 7622, // range 42 width 8 rect aoe

    // Alphinaud - Emissary of the Dawn (StB)
    RuinIII = 11191,
    Physick = 11192,
    TriShackle = 11482,
    Starstorm = 11193,

    // Y'shtola - Will of the Moon (StB)
    StoneIVSeventhDawn = 13423,
    AeroIISeventhDawn = 13424,
    CureIISeventhDawn = 13425,
    Aetherwell = 13426,

    // Hien - Requiem for Heroes (StB)
    Kyokufu = 14840,
    Gofu = 19046,
    Yagetsu = 19047,
    Ajisai = 14841,
    HissatsuGyoten = 14842,
    SecondWind = 15375,

    // Nyelbert - Nyelbert's Lament (ShB)
    RonkanFire3 = 16574,
    RonkanBlizzard3 = 16575,
    RonkanThunder3 = 16576,
    RonkanFlare = 16577,
    FallingStar = 16578,

    // Renda-Rae - The Hunter's Legacy (ShB)
    HeavyShot = 17123,
    AcidicBite = 17122,
    RadiantArrow = 17124,
    HuntersPrudence = 17596,
    DullingArrow = 17125,

    // Branden - The Hardened Heart (ShB)
    FastBlade = 16788,
    RightfulSword = 16269,
    Swashbuckler = 16984,
    Sunshadow = 16789,
    GreatestEclipse = 16985,
    FightOrFlight = 15870,
    ChivalrousSpirit = 17236,
    Interject = 15537,

    // Lamitt - The Lost and the Found (ShB)
    RonkanStoneII = 17003,
    RonkanCureII = 17000,
    RonkanMedica = 17001,
    RonkanEsuna = 17002,
    RonkanRenew = 17004,

    // Thancred - Coming Clean (ShB)
    KeenEdge = 16434,
    BrutalShell = 16418,
    SolidBarrel = 16435,
    RoughDivide = 16804,
    Nebula = 17839,
    SoothingPotion = 16436,
    Smackdown = 17901,
    ShiningBlade = 16437,
    PerfectDeception = 16438,
    SouldeepInvisibility = 17291,
    LeapOfFaith = 16439,

    // Estinien - VOVDOC (ShB)
    DoomSpike = 18772,
    SonicThrust = 18773,
    CoerthanTorment = 18774,
    SkydragonDive = 18775,
    AquaVitae = 19218,
    // quest part 2
    AlaMorn = 18776,
    Drachenlance = 18777,
    HorridRoar = 18778,
    Stardiver = 18780,
    DragonshadowDive = 18781,

    AetherCannon = 20489,
    Aethersaber = 20490,
    Aethercut = 20491,
    FinalFlourish = 20492,
    UltimaBuster = 20493,
    PyreticBooster = 20494,
    AetherialAegis = 20495,
    AetherMine = 20496,
    AutoRestoration = 20940,

    // Alisaie - DUD
    ShbVerfire = 20529,
    ShbVeraero = 20530,
    ShbVerstone = 20531,
    ShbVerflare = 20532,
    ShbCorpsACorps = 24917,
    ShbEnchantedRiposte = 24918,
    ShbEnchantedZwerchhau = 24919,
    ShbEnchantedRedoublement = 24920,
    ShbDisplacement = 21496,
    ShbVerholy = 21923,
    ShbScorch = 24831,
    ShbCrimsonSavior = 20533,
    ShbFleche = 21494,
    ShbContreSixte = 21495,
    ShbVercure = 21497,

    // Urianger - DUD
    MaleficIII = 21498,
    Benefic = 21608,
    AspectedHelios = 21609,
    DestinyDrawn = 21499,
    LordOfCrowns = 21607,
    DestinysSleeve = 24066,
    TheScroll = 21610,
    FixedSign = 21611,

    // Graha - DUD
    FireIV = 21612,
    FireIV2 = 22502,
    FireIV3 = 22817,
    Foul = 21613,
    AllaganBlizzardIV = 21852,
    ThunderIV = 21884,
    CureII = 21886,
    MedicaII = 21888,
    Break = 21921,

    // ShB alliance raid 3 hacking minigame
    LiminalFireWhite = 24619,
    LiminalFireBlack = 24620,
    F0SwitchToBlack = 24621,
    F0SwitchToWhite = 24622,

    // Thancred - Frosty Reception
    SwiftDeception = 27432,
    SilentTakedown = 27433,
    KeenEdgeFR = 27427,
    BrutalShellFR = 27428,
    SolidBarrelFR = 27429,
    BewildermentBomb = 27434,

    FastBladeIFTC = 26249,
    RiotBladeIFTC = 26250,
    RageOfHaloneIFTC = 26251,
    FightOrFlightIFTC = 26252,
    RampartIFTC = 26253,
    MedicalKit = 27315,
    MagitekCannonIFTC = 26231,
    DiffractiveMagitekCannonIFTC = 26232,
    HighPoweredMagitekCannonIFTC = 26233,

    // Alphinaud - As the Heavens Burn
    Diagnosis = 26224,
    Prognosis = 27043,
    LeveilleurDiagnosis = 27042,
    LeveilleurDruochole = 27044,
    DosisIII = 27045,
    LeveilleurDosisIII = 28439,
    LeveilleurToxikon = 27047,

    // Alisaie - As the Heavens Burn
    EWVerfire = 27048,
    EWVeraero = 27049,
    EWVerstone = 27050,
    EWVerthunder = 27051,
    EWVerflare = 27052,
    EWCrimsonSavior = 27053,
    EWCorpsACorps = 27054,
    EWEnchantedRiposte = 27055,
    EWEnchantedZwerchhau = 27056,
    EWEnchantedRedoublement = 27057,
    EWEngagement = 27058,
    EWVerholy = 27059,
    EWScorch = 24898,
    EWContreSixte = 27060,
    EWVercure = 27061,
    EWEmbolden = 26225,
    VermilionPledge = 27062,

    // Zero
    Slice = 31786,
    WaxingSlice = 31787,
    InfernalSlice = 31788,
    SpinningScythe = 31789,
    NightmareScythe = 31790,
    Communio = 31794,
    Engravement = 31785,
    Bloodbath = 33013,

    // Godbert - Generational Bonding
    GentlemanlySmash = 31393,
    GentlemanlyThrust = 31394,
    RageOfTheGentleman = 31395,
    MandervilleDropkick = 31397,
    MandervilleSprint = 31398,
    ManderdoubleLariat = 31396,
    MandervilleLB = 31399,

    // Wuk Lamat
    ClawOfTheBraax = 37120,
    FangsOfTheBraax = 37121,
    TailOfTheBraax = 37122,
    RunOfTheRroneek = 37123,
    LuwatenaPulse = 37124,
    BeakOfTheLuwatena = 37125,
    TuraliJudgment = 37126,
    TrialsOfTural = 37127,
    TuraliFervor = 37128,
    DawnlitConviction = 37129
}

public enum TraitID : uint { }

public enum SID : uint
{
    RolePlaying = 1534,
    BorrowedFlesh = 2760,

    // Hien
    Ajisai = 1779,

    // Nyelbert
    Electrocution = 271,

    // Renda-Rae
    AcidicBite = 2073,

    // thancred
    PerfectDeception = 1906,
    SouldeepInvisibility = 1956,

    // estinien VOVDOC phase 2
    StabWound = 1466,

    // sapphire weapon duty
    FreshPerspective = 2379,
    SafetyLockPyreticBooster = 2307,
    SafetyLockAetherialAegis = 2308,
    PyreticBooster = 2294,
    AetherialAegis = 2295,

    // urianger
    DestinyDrawn = 2571,

    // graha
    ThunderIV = 1210,
    Break = 2573,

    // thancred - Frosty Reception
    SwiftDeception = 2957,

    // alphinaud
    LeveilleurDosisIII = 2650,

    // godbert
    GlovesOff = 3458,
}

public sealed class Definitions : IDisposable
{
    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.Pummel);
        d.RegisterSpell(AID.VoidFireII);
        d.RegisterSpell(AID.HeavenlyJudge);
        d.RegisterSpell(AID.Rotosmash);
        d.RegisterSpell(AID.WreckingBall);
        d.RegisterSpell(AID.RockyRoll);
        d.RegisterSpell(AID.BigBurst);

        d.RegisterSpell(AID.MagitekCannon);
        d.RegisterSpell(AID.PhotonStream);
        d.RegisterSpell(AID.DiffractiveMagitekCannon);
        d.RegisterSpell(AID.HighPoweredMagitekCannon);

        d.RegisterSpell(AID.RuinIII);
        d.RegisterSpell(AID.Physick);
        d.RegisterSpell(AID.TriShackle);
        d.RegisterSpell(AID.Starstorm, castAnimLock: 5.10f);

        d.RegisterSpell(AID.StoneIVSeventhDawn);
        d.RegisterSpell(AID.AeroIISeventhDawn);
        d.RegisterSpell(AID.CureIISeventhDawn);
        d.RegisterSpell(AID.Aetherwell);

        d.RegisterSpell(AID.Kyokufu);
        d.RegisterSpell(AID.Gofu);
        d.RegisterSpell(AID.Yagetsu);
        d.RegisterSpell(AID.Ajisai);
        d.RegisterSpell(AID.HissatsuGyoten);
        d.RegisterSpell(AID.SecondWind);

        d.RegisterSpell(AID.RonkanFire3);
        d.RegisterSpell(AID.RonkanBlizzard3);
        d.RegisterSpell(AID.RonkanThunder3);
        d.RegisterSpell(AID.RonkanFlare);
        d.RegisterSpell(AID.FallingStar, castAnimLock: 5.10f);

        d.RegisterSpell(AID.HeavyShot);
        d.RegisterSpell(AID.AcidicBite);
        d.RegisterSpell(AID.RadiantArrow);
        d.RegisterSpell(AID.HuntersPrudence);
        d.RegisterSpell(AID.DullingArrow);

        d.RegisterSpell(AID.FastBlade);
        d.RegisterSpell(AID.RightfulSword);
        d.RegisterSpell(AID.Swashbuckler);
        d.RegisterSpell(AID.Sunshadow);
        d.RegisterSpell(AID.GreatestEclipse);
        d.RegisterSpell(AID.FightOrFlight);
        d.RegisterSpell(AID.ChivalrousSpirit);
        d.RegisterSpell(AID.Interject);

        d.RegisterSpell(AID.RonkanStoneII);
        d.RegisterSpell(AID.RonkanCureII);
        d.RegisterSpell(AID.RonkanMedica);
        d.RegisterSpell(AID.RonkanEsuna);
        d.RegisterSpell(AID.RonkanRenew);

        d.RegisterSpell(AID.KeenEdge);
        d.RegisterSpell(AID.BrutalShell);
        d.RegisterSpell(AID.SolidBarrel);
        d.RegisterSpell(AID.RoughDivide);
        d.RegisterSpell(AID.Nebula);
        d.RegisterSpell(AID.SoothingPotion, instantAnimLock: 1.1f);
        d.RegisterSpell(AID.Smackdown);
        d.RegisterSpell(AID.PerfectDeception);
        d.RegisterSpell(AID.SouldeepInvisibility);
        d.RegisterSpell(AID.ShiningBlade);
        d.RegisterSpell(AID.LeapOfFaith);

        d.RegisterSpell(AID.DoomSpike);
        d.RegisterSpell(AID.SonicThrust);
        d.RegisterSpell(AID.CoerthanTorment);
        d.RegisterSpell(AID.SkydragonDive, instantAnimLock: 0.8f);
        d.RegisterSpell(AID.AquaVitae, instantAnimLock: 1.1f);
        d.RegisterSpell(AID.AlaMorn);
        d.RegisterSpell(AID.Drachenlance);
        d.RegisterSpell(AID.HorridRoar);
        d.RegisterSpell(AID.Stardiver, instantAnimLock: 1.5f);
        d.RegisterSpell(AID.DragonshadowDive);

        d.RegisterSpell(AID.AetherCannon, instantAnimLock: 0.2f);
        d.RegisterSpell(AID.Aethersaber, instantAnimLock: 0.2f);
        d.RegisterSpell(AID.Aethercut, instantAnimLock: 0.2f);
        d.RegisterSpell(AID.FinalFlourish, instantAnimLock: 0.2f);
        d.RegisterSpell(AID.UltimaBuster);
        d.RegisterSpell(AID.PyreticBooster, instantAnimLock: 1.1f);
        d.RegisterSpell(AID.AetherialAegis);
        d.RegisterSpell(AID.AetherMine);
        d.RegisterSpell(AID.AutoRestoration, instantAnimLock: 0.2f);

        d.RegisterSpell(AID.ShbVerfire);
        d.RegisterSpell(AID.ShbVeraero);
        d.RegisterSpell(AID.ShbVerstone);
        d.RegisterSpell(AID.ShbVerflare);
        d.RegisterSpell(AID.ShbCorpsACorps);
        d.RegisterSpell(AID.ShbEnchantedRiposte);
        d.RegisterSpell(AID.ShbEnchantedZwerchhau);
        d.RegisterSpell(AID.ShbEnchantedRedoublement);
        d.RegisterSpell(AID.ShbDisplacement);
        d.RegisterSpell(AID.ShbVerholy);
        d.RegisterSpell(AID.ShbScorch);
        d.RegisterSpell(AID.ShbCrimsonSavior);
        d.RegisterSpell(AID.ShbFleche);
        d.RegisterSpell(AID.ShbContreSixte);
        d.RegisterSpell(AID.ShbVercure);

        d.RegisterSpell(AID.MaleficIII);
        d.RegisterSpell(AID.Benefic);
        d.RegisterSpell(AID.AspectedHelios);
        d.RegisterSpell(AID.DestinyDrawn);
        d.RegisterSpell(AID.LordOfCrowns);
        d.RegisterSpell(AID.DestinysSleeve);
        d.RegisterSpell(AID.TheScroll);
        d.RegisterSpell(AID.FixedSign);

        d.RegisterSpell(AID.FireIV);
        d.RegisterSpell(AID.FireIV2);
        d.RegisterSpell(AID.FireIV3);
        d.RegisterSpell(AID.Foul);
        d.RegisterSpell(AID.AllaganBlizzardIV);
        d.RegisterSpell(AID.ThunderIV);
        d.RegisterSpell(AID.CureII);
        d.RegisterSpell(AID.MedicaII);
        d.RegisterSpell(AID.Break);

        d.RegisterSpell(AID.LiminalFireWhite);
        d.RegisterSpell(AID.LiminalFireBlack);
        d.RegisterSpell(AID.F0SwitchToBlack);
        d.RegisterSpell(AID.F0SwitchToWhite);

        d.RegisterSpell(AID.SwiftDeception);
        d.RegisterSpell(AID.SilentTakedown);
        d.RegisterSpell(AID.KeenEdgeFR);
        d.RegisterSpell(AID.BrutalShellFR);
        d.RegisterSpell(AID.SolidBarrelFR);
        d.RegisterSpell(AID.BewildermentBomb);

        d.RegisterSpell(AID.FastBladeIFTC);
        d.RegisterSpell(AID.RiotBladeIFTC);
        d.RegisterSpell(AID.RageOfHaloneIFTC);
        d.RegisterSpell(AID.FightOrFlightIFTC);
        d.RegisterSpell(AID.RampartIFTC);
        d.RegisterSpell(AID.MedicalKit);
        d.RegisterSpell(AID.MagitekCannonIFTC);
        d.RegisterSpell(AID.DiffractiveMagitekCannonIFTC);
        d.RegisterSpell(AID.HighPoweredMagitekCannonIFTC);

        d.RegisterSpell(AID.Diagnosis);
        d.RegisterSpell(AID.Prognosis);
        d.RegisterSpell(AID.LeveilleurDiagnosis);
        d.RegisterSpell(AID.LeveilleurDruochole);
        d.RegisterSpell(AID.DosisIII);
        d.RegisterSpell(AID.LeveilleurDosisIII);
        d.RegisterSpell(AID.LeveilleurToxikon);

        d.RegisterSpell(AID.EWVerfire);
        d.RegisterSpell(AID.EWVeraero);
        d.RegisterSpell(AID.EWVerstone);
        d.RegisterSpell(AID.EWVerthunder);
        d.RegisterSpell(AID.EWVerflare);
        d.RegisterSpell(AID.EWCrimsonSavior);
        d.RegisterSpell(AID.EWCorpsACorps);
        d.RegisterSpell(AID.EWEnchantedRiposte);
        d.RegisterSpell(AID.EWEnchantedZwerchhau);
        d.RegisterSpell(AID.EWEnchantedRedoublement);
        d.RegisterSpell(AID.EWEngagement);
        d.RegisterSpell(AID.EWVerholy);
        d.RegisterSpell(AID.EWScorch);
        d.RegisterSpell(AID.EWContreSixte);
        d.RegisterSpell(AID.EWVercure);
        d.RegisterSpell(AID.EWEmbolden);
        d.RegisterSpell(AID.VermilionPledge);

        d.RegisterSpell(AID.Slice);
        d.RegisterSpell(AID.WaxingSlice);
        d.RegisterSpell(AID.InfernalSlice);
        d.RegisterSpell(AID.SpinningScythe);
        d.RegisterSpell(AID.NightmareScythe);
        d.RegisterSpell(AID.Communio, instantAnimLock: 1.1f);
        d.RegisterSpell(AID.Engravement, instantAnimLock: 1.1f);
        d.RegisterSpell(AID.Bloodbath);

        d.RegisterSpell(AID.GentlemanlySmash);
        d.RegisterSpell(AID.GentlemanlyThrust);
        d.RegisterSpell(AID.RageOfTheGentleman);
        d.RegisterSpell(AID.MandervilleDropkick);
        d.RegisterSpell(AID.MandervilleSprint);
        d.RegisterSpell(AID.ManderdoubleLariat);
        d.RegisterSpell(AID.MandervilleLB);

        d.RegisterSpell(AID.ClawOfTheBraax);
        d.RegisterSpell(AID.FangsOfTheBraax);
        d.RegisterSpell(AID.TailOfTheBraax);
        d.RegisterSpell(AID.RunOfTheRroneek);
        d.RegisterSpell(AID.LuwatenaPulse);
        d.RegisterSpell(AID.BeakOfTheLuwatena);
        d.RegisterSpell(AID.TuraliJudgment);
        d.RegisterSpell(AID.TrialsOfTural);
        d.RegisterSpell(AID.TuraliFervor);
        d.RegisterSpell(AID.DawnlitConviction, castAnimLock: 3.86f);
    }

    public void Dispose() { }
}

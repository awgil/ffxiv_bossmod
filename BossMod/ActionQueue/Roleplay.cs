using BossMod.Interfaces;

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
    DawnlitConviction = 37129,

    // The Patisserie
    BakeOff = 44039, // instant cast, range 5 60-degree cone, 3 charges
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

public sealed class Definitions : IDefinitions
{
    public void Initialize(ActionDefinitions defs)
    {
        defs.RegisterSpell(AID.Pummel);
        defs.RegisterSpell(AID.VoidFireII);
        defs.RegisterSpell(AID.HeavenlyJudge);
        defs.RegisterSpell(AID.Rotosmash);
        defs.RegisterSpell(AID.WreckingBall);
        defs.RegisterSpell(AID.RockyRoll);
        defs.RegisterSpell(AID.BigBurst);

        defs.RegisterSpell(AID.MagitekCannon);
        defs.RegisterSpell(AID.PhotonStream);
        defs.RegisterSpell(AID.DiffractiveMagitekCannon);
        defs.RegisterSpell(AID.HighPoweredMagitekCannon);

        defs.RegisterSpell(AID.RuinIII);
        defs.RegisterSpell(AID.Physick);
        defs.RegisterSpell(AID.TriShackle);
        defs.RegisterSpell(AID.Starstorm, castAnimLock: 5.10f);

        defs.RegisterSpell(AID.StoneIVSeventhDawn);
        defs.RegisterSpell(AID.AeroIISeventhDawn);
        defs.RegisterSpell(AID.CureIISeventhDawn);
        defs.RegisterSpell(AID.Aetherwell);

        defs.RegisterSpell(AID.Kyokufu);
        defs.RegisterSpell(AID.Gofu);
        defs.RegisterSpell(AID.Yagetsu);
        defs.RegisterSpell(AID.Ajisai);
        defs.RegisterSpell(AID.HissatsuGyoten);
        defs.RegisterSpell(AID.SecondWind);

        defs.RegisterSpell(AID.RonkanFire3);
        defs.RegisterSpell(AID.RonkanBlizzard3);
        defs.RegisterSpell(AID.RonkanThunder3);
        defs.RegisterSpell(AID.RonkanFlare);
        defs.RegisterSpell(AID.FallingStar, castAnimLock: 5.10f);

        defs.RegisterSpell(AID.HeavyShot);
        defs.RegisterSpell(AID.AcidicBite);
        defs.RegisterSpell(AID.RadiantArrow);
        defs.RegisterSpell(AID.HuntersPrudence);
        defs.RegisterSpell(AID.DullingArrow);

        defs.RegisterSpell(AID.FastBlade);
        defs.RegisterSpell(AID.RightfulSword);
        defs.RegisterSpell(AID.Swashbuckler);
        defs.RegisterSpell(AID.Sunshadow);
        defs.RegisterSpell(AID.GreatestEclipse);
        defs.RegisterSpell(AID.FightOrFlight);
        defs.RegisterSpell(AID.ChivalrousSpirit);
        defs.RegisterSpell(AID.Interject);

        defs.RegisterSpell(AID.RonkanStoneII);
        defs.RegisterSpell(AID.RonkanCureII);
        defs.RegisterSpell(AID.RonkanMedica);
        defs.RegisterSpell(AID.RonkanEsuna);
        defs.RegisterSpell(AID.RonkanRenew);

        defs.RegisterSpell(AID.KeenEdge);
        defs.RegisterSpell(AID.BrutalShell);
        defs.RegisterSpell(AID.SolidBarrel);
        defs.RegisterSpell(AID.RoughDivide);
        defs.RegisterSpell(AID.Nebula);
        defs.RegisterSpell(AID.SoothingPotion, instantAnimLock: 1.1f);
        defs.RegisterSpell(AID.Smackdown);
        defs.RegisterSpell(AID.PerfectDeception);
        defs.RegisterSpell(AID.SouldeepInvisibility);
        defs.RegisterSpell(AID.ShiningBlade);
        defs.RegisterSpell(AID.LeapOfFaith);

        defs.RegisterSpell(AID.DoomSpike);
        defs.RegisterSpell(AID.SonicThrust);
        defs.RegisterSpell(AID.CoerthanTorment);
        defs.RegisterSpell(AID.SkydragonDive, instantAnimLock: 0.8f);
        defs.RegisterSpell(AID.AquaVitae, instantAnimLock: 1.1f);
        defs.RegisterSpell(AID.AlaMorn);
        defs.RegisterSpell(AID.Drachenlance);
        defs.RegisterSpell(AID.HorridRoar);
        defs.RegisterSpell(AID.Stardiver, instantAnimLock: 1.5f);
        defs.RegisterSpell(AID.DragonshadowDive);

        defs.RegisterSpell(AID.AetherCannon, instantAnimLock: 0.2f);
        defs.RegisterSpell(AID.Aethersaber, instantAnimLock: 0.2f);
        defs.RegisterSpell(AID.Aethercut, instantAnimLock: 0.2f);
        defs.RegisterSpell(AID.FinalFlourish, instantAnimLock: 0.2f);
        defs.RegisterSpell(AID.UltimaBuster);
        defs.RegisterSpell(AID.PyreticBooster, instantAnimLock: 1.1f);
        defs.RegisterSpell(AID.AetherialAegis);
        defs.RegisterSpell(AID.AetherMine);
        defs.RegisterSpell(AID.AutoRestoration, instantAnimLock: 0.2f);

        defs.RegisterSpell(AID.ShbVerfire);
        defs.RegisterSpell(AID.ShbVeraero);
        defs.RegisterSpell(AID.ShbVerstone);
        defs.RegisterSpell(AID.ShbVerflare);
        defs.RegisterSpell(AID.ShbCorpsACorps);
        defs.RegisterSpell(AID.ShbEnchantedRiposte);
        defs.RegisterSpell(AID.ShbEnchantedZwerchhau);
        defs.RegisterSpell(AID.ShbEnchantedRedoublement);
        defs.RegisterSpell(AID.ShbDisplacement);
        defs.RegisterSpell(AID.ShbVerholy);
        defs.RegisterSpell(AID.ShbScorch);
        defs.RegisterSpell(AID.ShbCrimsonSavior);
        defs.RegisterSpell(AID.ShbFleche);
        defs.RegisterSpell(AID.ShbContreSixte);
        defs.RegisterSpell(AID.ShbVercure);

        defs.RegisterSpell(AID.MaleficIII);
        defs.RegisterSpell(AID.Benefic);
        defs.RegisterSpell(AID.AspectedHelios);
        defs.RegisterSpell(AID.DestinyDrawn);
        defs.RegisterSpell(AID.LordOfCrowns);
        defs.RegisterSpell(AID.DestinysSleeve);
        defs.RegisterSpell(AID.TheScroll);
        defs.RegisterSpell(AID.FixedSign);

        defs.RegisterSpell(AID.FireIV);
        defs.RegisterSpell(AID.FireIV2);
        defs.RegisterSpell(AID.FireIV3);
        defs.RegisterSpell(AID.Foul);
        defs.RegisterSpell(AID.AllaganBlizzardIV);
        defs.RegisterSpell(AID.ThunderIV);
        defs.RegisterSpell(AID.CureII);
        defs.RegisterSpell(AID.MedicaII);
        defs.RegisterSpell(AID.Break);

        defs.RegisterSpell(AID.LiminalFireWhite);
        defs.RegisterSpell(AID.LiminalFireBlack);
        defs.RegisterSpell(AID.F0SwitchToBlack);
        defs.RegisterSpell(AID.F0SwitchToWhite);

        defs.RegisterSpell(AID.SwiftDeception);
        defs.RegisterSpell(AID.SilentTakedown);
        defs.RegisterSpell(AID.KeenEdgeFR);
        defs.RegisterSpell(AID.BrutalShellFR);
        defs.RegisterSpell(AID.SolidBarrelFR);
        defs.RegisterSpell(AID.BewildermentBomb);

        defs.RegisterSpell(AID.FastBladeIFTC);
        defs.RegisterSpell(AID.RiotBladeIFTC);
        defs.RegisterSpell(AID.RageOfHaloneIFTC);
        defs.RegisterSpell(AID.FightOrFlightIFTC);
        defs.RegisterSpell(AID.RampartIFTC);
        defs.RegisterSpell(AID.MedicalKit);
        defs.RegisterSpell(AID.MagitekCannonIFTC);
        defs.RegisterSpell(AID.DiffractiveMagitekCannonIFTC);
        defs.RegisterSpell(AID.HighPoweredMagitekCannonIFTC);

        defs.RegisterSpell(AID.Diagnosis);
        defs.RegisterSpell(AID.Prognosis);
        defs.RegisterSpell(AID.LeveilleurDiagnosis);
        defs.RegisterSpell(AID.LeveilleurDruochole);
        defs.RegisterSpell(AID.DosisIII);
        defs.RegisterSpell(AID.LeveilleurDosisIII);
        defs.RegisterSpell(AID.LeveilleurToxikon);

        defs.RegisterSpell(AID.EWVerfire);
        defs.RegisterSpell(AID.EWVeraero);
        defs.RegisterSpell(AID.EWVerstone);
        defs.RegisterSpell(AID.EWVerthunder);
        defs.RegisterSpell(AID.EWVerflare);
        defs.RegisterSpell(AID.EWCrimsonSavior);
        defs.RegisterSpell(AID.EWCorpsACorps);
        defs.RegisterSpell(AID.EWEnchantedRiposte);
        defs.RegisterSpell(AID.EWEnchantedZwerchhau);
        defs.RegisterSpell(AID.EWEnchantedRedoublement);
        defs.RegisterSpell(AID.EWEngagement);
        defs.RegisterSpell(AID.EWVerholy);
        defs.RegisterSpell(AID.EWScorch);
        defs.RegisterSpell(AID.EWContreSixte);
        defs.RegisterSpell(AID.EWVercure);
        defs.RegisterSpell(AID.EWEmbolden);
        defs.RegisterSpell(AID.VermilionPledge);

        defs.RegisterSpell(AID.Slice);
        defs.RegisterSpell(AID.WaxingSlice);
        defs.RegisterSpell(AID.InfernalSlice);
        defs.RegisterSpell(AID.SpinningScythe);
        defs.RegisterSpell(AID.NightmareScythe);
        defs.RegisterSpell(AID.Communio, instantAnimLock: 1.1f);
        defs.RegisterSpell(AID.Engravement, instantAnimLock: 1.1f);
        defs.RegisterSpell(AID.Bloodbath);

        defs.RegisterSpell(AID.GentlemanlySmash);
        defs.RegisterSpell(AID.GentlemanlyThrust);
        defs.RegisterSpell(AID.RageOfTheGentleman);
        defs.RegisterSpell(AID.MandervilleDropkick);
        defs.RegisterSpell(AID.MandervilleSprint);
        defs.RegisterSpell(AID.ManderdoubleLariat);
        defs.RegisterSpell(AID.MandervilleLB);

        defs.RegisterSpell(AID.ClawOfTheBraax);
        defs.RegisterSpell(AID.FangsOfTheBraax);
        defs.RegisterSpell(AID.TailOfTheBraax);
        defs.RegisterSpell(AID.RunOfTheRroneek);
        defs.RegisterSpell(AID.LuwatenaPulse);
        defs.RegisterSpell(AID.BeakOfTheLuwatena);
        defs.RegisterSpell(AID.TuraliJudgment);
        defs.RegisterSpell(AID.TrialsOfTural);
        defs.RegisterSpell(AID.TuraliFervor);
        defs.RegisterSpell(AID.DawnlitConviction, castAnimLock: 3.86f);

        defs.RegisterSpell(AID.BakeOff);
    }

    public void Dispose() { }
}

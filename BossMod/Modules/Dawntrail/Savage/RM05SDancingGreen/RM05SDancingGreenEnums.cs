namespace BossMod.Dawntrail.Savage.RM05SDancingGreen;

public enum OID : uint
{
    Boss = 0x47B9,
    Helper = 0x233C,
    Frogtourage = 0x47BA, // R3.142, x8
    Spotlight = 0x47BB, // R1.000, x8
}

public enum SID : uint
{
    _Gen_MagicVulnerabilityUp = 2941, // Helper->player, extra=0x0
    _Gen_Bleeding = 2088, // Helper->player, extra=0x0
    _Gen_NeedlePoised = 4459, // Boss->Boss, extra=0x0
    _Gen_BurnBabyBurn = 4461, // none->player, extra=0x0
    _Gen_ = 2056, // Frogtourage->Spotlight/Frogtourage, extra=0x37D/0x386/0xE1
    _Gen_1 = 4515, // none->player, extra=0x0
    _Gen_Spotlightless = 4472, // none->player, extra=0x0
    _Gen_InTheSpotlight = 4471, // none->player, extra=0x0
    _Gen_Weakness = 43, // none->player, extra=0x0
    _Gen_Transcendent = 418, // none->player, extra=0x0
    _Gen_DirectionalDisregard = 3808, // none->Boss, extra=0x0
    _Gen_WavelengthA = 4462, // none->player, extra=0x0
    _Gen_WavelengthB = 4463, // none->player, extra=0x0
    _Gen_DamageDown = 2911, // Helper/Boss/Frogtourage->player, extra=0x0
    _Gen_PerfectGroove = 4464, // none->player, extra=0x0
    _Gen_SustainedDamage = 2935, // Helper->player, extra=0x0
    _Gen_BrinkOfDeath = 44, // none->player, extra=0x0
    _Gen_FrogtourageFan = 3998, // Helper->player, extra=0x0
}

public enum AID : uint
{
    _AutoAttack_ = 41767, // Boss->player, no cast, single-target
    _Weaponskill_DeepCut = 42785, // Boss->self, 5.0s cast, single-target
    _Weaponskill_DeepCut1 = 42786, // Helper->self, no cast, range 60 ?-degree cone
    _Ability_ = 42693, // Boss->location, no cast, single-target
    _Weaponskill_FlipToBSide = 42881, // Boss->self, 4.0s cast, single-target
    _Weaponskill_2SnapTwistDropTheNeedle = 42794, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_2SnapTwistDropTheNeedle1 = 42798, // Helper->self, 1.5s cast, range 25 width 50 rect
    _Weaponskill_2SnapTwistDropTheNeedle2 = 42799, // Helper->self, 3.5s cast, range 25 width 50 rect
    _Weaponskill_PlayBSide = 37833, // Boss->self, no cast, single-target
    _Weaponskill_PlayBSide1 = 42884, // Helper->self, no cast, range 50 width 8 rect
    _Weaponskill_FlipToASide = 42880, // Boss->self, 4.0s cast, single-target
    _Weaponskill_3SnapTwistDropTheNeedle = 42800, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_3SnapTwistDropTheNeedle1 = 42806, // Helper->self, 1.2s cast, range 25 width 50 rect
    _Weaponskill_3SnapTwistDropTheNeedle2 = 42807, // Helper->self, 1.9s cast, range 25 width 50 rect
    _Weaponskill_3SnapTwistDropTheNeedle3 = 42808, // Helper->self, 3.5s cast, range 25 width 50 rect
    _Weaponskill_PlayASide = 37832, // Boss->self, no cast, single-target
    _Weaponskill_PlayASide1 = 42883, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_CelebrateGoodTimes = 42787, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_DiscoInfernal = 42838, // Boss->self, 4.0s cast, range 60 circle
    _Weaponskill_FunkyFloor = 42834, // Boss->self, 2.5+0.5s cast, single-target
    _Weaponskill_FunkyFloor1 = 42835, // Helper->self, no cast, ???
    _Weaponskill_InsideOut = 37826, // Helper->self, no cast, range 7 circle
    _Weaponskill_InsideOut1 = 42876, // Boss->self, 5.0s cast, single-target
    _Weaponskill_InsideOut2 = 42877, // Boss->self, no cast, single-target
    _Weaponskill_InsideOut3 = 37827, // Helper->self, no cast, range ?-40 donut
    _Weaponskill_Shame = 42840, // Helper->player, 1.0s cast, single-target
    _Weaponskill_3SnapTwistDropTheNeedle4 = 42801, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_EnsembleAssemble = 39474, // Boss->self, 3.0s cast, single-target
    _Weaponskill_ArcadyNightFever = 42848, // Boss->self, 4.8s cast, single-target
    _Weaponskill_ = 42849, // Boss->self, no cast, single-target
    _Weaponskill_GetDown = 39908, // Helper->self, 5.2s cast, range 7 circle
    _Weaponskill_GetDown1 = 42852, // Helper->self, no cast, range 40 ?-degree cone
    _Weaponskill_1 = 37825, // Helper->Frogtourage, 1.2s cast, single-target
    _Weaponskill_2 = 42764, // Frogtourage->self, 1.7s cast, single-target
    _Weaponskill_3 = 37830, // Boss->self, no cast, single-target
    _Weaponskill_GetDown2 = 42851, // Helper->self, no cast, range ?-40 donut
    _Weaponskill_GetDown3 = 42853, // Helper->self, 2.5s cast, range 40 ?-degree cone
    _Weaponskill_4 = 42765, // Frogtourage->self, 1.7s cast, single-target
    _Weaponskill_5 = 38464, // Boss->self, no cast, single-target
    _Weaponskill_GetDown4 = 42850, // Helper->self, no cast, range 7 circle
    _Weaponskill_6 = 38465, // Boss->self, no cast, single-target
    _Weaponskill_7 = 39091, // Boss->self, no cast, single-target
    _Weaponskill_Fire = 39093, // Boss->self, no cast, single-target
    _Weaponskill_LetsDance = 42858, // Boss->self, 5.8s cast, single-target
    _Weaponskill_8 = 39906, // Frogtourage->self, no cast, single-target
    _Weaponskill_LetsDance1 = 42861, // Boss->self, no cast, single-target
    _Weaponskill_LetsDance2 = 39901, // Helper->self, no cast, range 25 width 50 rect
    _Weaponskill_9 = 39907, // Frogtourage->self, no cast, single-target
    _Weaponskill_LetsDance3 = 42862, // Boss->self, no cast, single-target
    _Weaponskill_MinorFreakOut = 42856, // Helper->location, no cast, range 2 circle
    _Weaponskill_MinorFreakOut1 = 39475, // Helper->location, no cast, range 2 circle
    _Weaponskill_MinorFreakOut2 = 39476, // Helper->location, no cast, range 2 circle
    _Weaponskill_MinorFreakOut3 = 39478, // Helper->location, no cast, range 2 circle
    _Weaponskill_10 = 37844, // Frogtourage->self, 5.0s cast, single-target
    _Weaponskill_11 = 37843, // Frogtourage->self, 5.0s cast, single-target
    _Weaponskill_LetsPose = 42863, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_4SnapTwistDropTheNeedle = 42814, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_4SnapTwistDropTheNeedle1 = 42815, // Helper->self, 1.0s cast, range 25 width 50 rect
    _Weaponskill_4SnapTwistDropTheNeedle2 = 42816, // Helper->self, 1.5s cast, range 25 width 50 rect
    _Weaponskill_4SnapTwistDropTheNeedle3 = 42817, // Helper->self, 2.0s cast, range 25 width 50 rect
    _Weaponskill_4SnapTwistDropTheNeedle4 = 42818, // Helper->self, 3.5s cast, range 25 width 50 rect
    _Weaponskill_2SnapTwistDropTheNeedle3 = 42792, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_3SnapTwistDropTheNeedle5 = 42804, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_FreakOut = 42854, // Helper->player, no cast, single-target
    _Weaponskill_FreakOut1 = 42855, // Helper->player, no cast, single-target
    _Weaponskill_2SnapTwistDropTheNeedle4 = 42795, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_4SnapTwistDropTheNeedle5 = 42810, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_2SnapTwistDropTheNeedle5 = 42797, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_2SnapTwistDropTheNeedle6 = 42796, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_RideTheWaves = 42836, // Boss->self, 3.5+0.5s cast, single-target
    _Weaponskill_RideTheWaves1 = 42837, // Helper->self, no cast, ???
    _Weaponskill_QuarterBeats = 42844, // Helper->players, 5.0s cast, range 4 circle
    _Weaponskill_QuarterBeats1 = 42843, // Boss->self, 5.0s cast, single-target
    _Weaponskill_EighthBeats = 42846, // Helper->players, 5.0s cast, range 5 circle
    _Weaponskill_EighthBeats1 = 42845, // Boss->self, 5.0s cast, single-target
    _Weaponskill_Frogtourage = 42847, // Boss->self, 3.0s cast, single-target
    _Weaponskill_12 = 42781, // Frogtourage->self, no cast, single-target
    _Weaponskill_13 = 42782, // Frogtourage->self, 1.0s cast, single-target
    _Weaponskill_Moonburn = 42868, // Helper->self, 10.5s cast, range 40 width 15 rect
    _Weaponskill_Moonburn1 = 42867, // Helper->self, 10.5s cast, range 40 width 15 rect
    _Weaponskill_BackUpDance = 42871, // Frogtourage->self, 8.9s cast, single-target
    _Weaponskill_BackUpDance1 = 42872, // Helper->self, no cast, range 60 ?-degree cone
    _Weaponskill_3SnapTwistDropTheNeedle6 = 42205, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_3SnapTwistDropTheNeedle7 = 42802, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_4SnapTwistDropTheNeedle6 = 42809, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_OutsideIn = 37828, // Helper->self, no cast, range ?-40 donut
    _Weaponskill_OutsideIn1 = 42878, // Boss->self, 5.0s cast, single-target
    _Weaponskill_OutsideIn2 = 37829, // Helper->self, no cast, range 7 circle
    _Weaponskill_OutsideIn3 = 42879, // Boss->self, no cast, single-target
    _Weaponskill_4SnapTwistDropTheNeedle7 = 42813, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_4SnapTwistDropTheNeedle8 = 42812, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_2SnapTwistDropTheNeedle7 = 42793, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_2SnapTwistDropTheNeedle8 = 42204, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_ArcadyNightEncore = 41840, // Boss->self, 4.8s cast, single-target
    _Weaponskill_14 = 42763, // Frogtourage->self, 1.7s cast, single-target
    _Weaponskill_15 = 42762, // Frogtourage->self, 1.7s cast, single-target
    _Weaponskill_LetsDanceRemix = 41872, // Boss->self, 5.8s cast, single-target
    _Weaponskill_LetsDanceRemix1 = 41874, // Boss->self, no cast, single-target
    _Weaponskill_16 = 41839, // Frogtourage->self, no cast, single-target
    _Weaponskill_LetsDanceRemix2 = 41877, // Helper->self, no cast, range 25 width 50 rect
    _Weaponskill_17 = 41836, // Frogtourage->self, no cast, single-target
    _Weaponskill_LetsDanceRemix3 = 41875, // Boss->self, no cast, single-target
    _Weaponskill_18 = 41838, // Frogtourage->self, no cast, single-target
    _Weaponskill_LetsDanceRemix4 = 41873, // Boss->self, no cast, single-target
    _Weaponskill_LetsDanceRemix5 = 41876, // Boss->self, no cast, single-target
    _Weaponskill_19 = 41837, // Frogtourage->self, no cast, single-target
    _Weaponskill_20 = 39904, // Frogtourage->self, 5.0s cast, single-target
    _Weaponskill_21 = 39905, // Frogtourage->self, 5.0s cast, single-target
    _Weaponskill_LetsPose1 = 42864, // Boss->self, 5.0s cast, range 60 circle
    _Weaponskill_DoTheHustle = 42869, // Frogtourage->self, 5.0s cast, range 50 ?-degree cone
    _Weaponskill_DoTheHustle1 = 42870, // Frogtourage->self, 5.0s cast, range 50 ?-degree cone
    _Weaponskill_DoTheHustle2 = 42789, // Boss->self, 5.0s cast, range 50 ?-degree cone
    _Weaponskill_4SnapTwistDropTheNeedle9 = 42811, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_3SnapTwistDropTheNeedle8 = 42803, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_3SnapTwistDropTheNeedle9 = 42805, // Boss->self, 5.0s cast, range 20 width 40 rect
    _Weaponskill_DoTheHustle3 = 42788, // Boss->self, 5.0s cast, range 50 ?-degree cone
    _Weaponskill_FrogtourageFinale = 42209, // Boss->self, 3.0s cast, single-target
    _Weaponskill_22 = 42874, // Frogtourage->self, no cast, single-target
    _Weaponskill_HiNRGFever = 42873, // Boss->self, 12.0s cast, range 60 circle
    _Weaponskill_23 = 42875, // Frogtourage->self, no cast, single-target
}


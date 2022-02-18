namespace BossMod.P1S
{
    public enum OID : uint
    {
        Boss = 0x3522,
        Helper = 0x233C,
        FlailLR = 0x3523, // "anchor" weapon, purely visual
        FlailI = 0x3524, // "ball" weapon, also used for knockbacks
        FlailO = 0x3525, // "chakram" weapon
    };

    public enum AID : uint
    {
        GaolerFlailRL = 26102, // Boss->Boss
        GaolerFlailLR = 26103, // Boss->Boss
        GaolerFlailIO1 = 26104, // Boss->Boss
        GaolerFlailIO2 = 26105, // Boss->Boss
        GaolerFlailOI1 = 26106, // Boss->Boss
        GaolerFlailOI2 = 26107, // Boss->Boss
        AetherflailRX = 26114, // Boss->Boss -- seen BlueRI & RedRO
        AetherflailLX = 26115, // Boss->Boss -- seen BlueLO, RedLI, RedLO - maybe it's *L*?
        AetherflailIL = 26116, // never seen one, inferred
        AetherflailIR = 26117, // Boss->Boss -- RedIR
        AetherflailOL = 26118, // Boss->Boss -- seen BlueOL, RedOL
        AetherflailOR = 26119, // Boss->Boss -- seen RedOR
        KnockbackGrace = 26126, // Boss->MT
        KnockbackPurge = 26127, // Boss->MT
        TrueHoly1 = 26128, // Boss->Boss, no cast, ???
        TrueFlare1 = 26129, // Boss->Boss, no cast, ???
        TrueHoly2 = 26130, // Helper->tank shared, no cast, damage after KnockbackGrace (range=6)
        TrueFlare2 = 26131, // Helper->tank and nearby, no cast, damage after KnockbackPurge (range=50??)
        ShiningCells = 26134, // Boss->Boss, raidwide aoe
        SlamShut = 26135, // Boss->Boss, raidwide aoe
        Aetherchain = 26137, // Boss->Boss
        PowerfulFire = 26138, // Helper->???, no cast, damage during aetherflails for incorrect segments?..
        ShacklesOfTime = 26140, // Boss->Boss
        OutOfTime = 26141, // Helper->???, no cast, after SoT resolve
        Intemperance = 26142, // Boss->Boss
        IntemperateTormentUp = 26143, // Boss->Boss (bottom->top)
        IntemperateTormentDown = 26144, // Boss->Boss (bottom->top)
        HotSpell = 26145, // Helper->player, no cast, red cube explosion
        ColdSpell = 26146, // Helper->player, no cast, blue cube explosion
        DisastrousSpell = 26147, // Helper->player, no cast, purple cube explosion
        PainfulFlux = 26148, // Helper->player, no cast, separator cube explosion
        AetherialShackles = 26149, // Boss->Boss
        FourShackles = 26150, // Boss->Boss
        ChainPainBlue = 26151, // Helper->chain target, no cast, damage during chain resolve
        ChainPainRed = 26152, // Helper->chain target
        HeavyHand = 26153, // Boss->MT, generic tankbuster
        WarderWrath = 26154, // Boss->Boss, generic raidwide
        GaolerFlailR1 = 28070, // Helper->Helper, first hit, right-hand cone
        GaolerFlailL1 = 28071, // Helper->Helper, first hit, left-hand cone
        GaolerFlailI1 = 28072, // Helper->Helper, first hit, point-blank
        GaolerFlailO1 = 28073, // Helper->Helper, first hit, donut
        GaolerFlailR2 = 28074, // Helper->Helper, second hit, right-hand cone
        GaolerFlailL2 = 28075, // Helper->Helper, second hit, left-hand cone
        GaolerFlailI2 = 28076, // Helper->Helper, second hit, point-blank
        GaolerFlailO2 = 28077, // Helper->Helper, second hit, donut
        InevitableFlame = 28353, // Helper->Helper no cast, after SoT resolve to red - hit others standing in fire?
    };

    public enum SID : uint
    {
        AetherExplosion = 2195, // hidden and unnamed, extra determines red/blue segments explosion (0x14D for blue, 0x14C for red)
        ColdSpell = 2739, // intemperance: after blue cube explosion
        HotSpell = 2740, // intemperance: after red cube explosion
        ShacklesOfTime = 2741, // shackles of time: hits segments matching color on expiration
        ShacklesOfCompanionship0 = 2742, // shackles: purple (tether to 3 closest) - normal 13s duration
        ShacklesOfLoneliness0 = 2743, // shackles: red (tether to 3 farthest) - normal 13s duration
        InescapableCompanionship = 2744, // replaces corresponding shackles in 13s, 5s duration
        InescapableLoneliness = 2745,
        ShacklesOfCompanionship1 = 2885, // fourfold 3s duration
        ShacklesOfCompanionship2 = 2886, // fourfold 8s duration
        ShacklesOfCompanionship3 = 2887, // fourfold 13s duration
        ShacklesOfLoneliness1 = 2888, // fourfold 3s duration
        ShacklesOfLoneliness2 = 2889, // fourfold 8s duration
        ShacklesOfLoneliness3 = 2890, // fourfold 13s duration
        ShacklesOfCompanionship4 = 2923, // fourfold 18s duration
        ShacklesOfLoneliness4 = 2924, // fourfold 18s duration
        DamageDown = 2911, // applied by two successive cubes of the same color
        MagicVulnerabilityUp = 2941, // applied by shackle resolve, knockbacks
    }
}

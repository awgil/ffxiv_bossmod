namespace BossMod.Endwalker.NormalTrials.Trial7Zeromus
{
    public enum OID : uint
    {
        Boss = 0x404E, // R20.0, 1x
        Helper = 0x233C, // R0.500
        Comet = 0x40AB, // R2.250
        ToxicBubble = 0x40AC, // R1.700, spawn during fight
        FlareTower = 0x1EB94E, // R0.500, EventObj type, spawn during fight
        FlareRay = 0x1EB94F, // R0.500, EventObj type, spawn during fight
        BlackHole = 0x1EB94C, // R0.500, EventObj type, spawn during fight
        ArenaFeatures = 0x1EA1A1, // R2.000, EventObj type
    };

    public enum AID : uint
    {
        AutoAttack = 35912, // Boss->player, no cast, single-target

        AbyssalNox  = 35575, // Boss->self, 5.0s cast, range 60 circle
        AbyssalEchoesVisualCardinal = 35576, // Helper->self, 9.0s cast, single-target
        AbyssalEchoesVisualIntercardinal = 35577, // Helper->self, 9.0s cast, single-target
        AbyssalEchoes = 35578, // Helper->self, 16.0s cast, range 12 circle
        
        SableThread = 35566, // Boss->self, 5.0s cast, single-target
        SableThreadTarget = 35567, // Helper->player, no cast, single-target, visual (target selection)
        SableThreadAOE = 35570, // Helper->self, no cast, range 60 width 12 rect
        SableThreadVisualHitIntermediate = 35568, // Boss->self, no cast, single-target
        SableThreadVisualHitLast = 35569, // Boss->self, no cast, single-target

        VisceralWhirlR = 35579, // Boss->self, 8.0s cast, single-target
        VisceralWhirlRAOE1 = 35580, // Helper->self, 8.8s cast, range 29 width 28 rect
        VisceralWhirlRAOE2 = 35581, // Helper->self, 8.8s cast, range 60 width 28 rect
        VisceralWhirlL = 35582, // Boss->self, 8.0s cast, single-target
        VisceralWhirlLAOE1 = 35583, // Helper->self, 8.8s cast, range 29 width 28 rect
        VisceralWhirlLAOE2 = 35584, // Helper->self, 8.8s cast, range 60 width 28 rect
        
        DarkMatter = 35638, // Boss->self, 4.0s cast, single-target
        DarkMatterAOE = 35639, // Helper->player, no cast, range 8 circle

        Flare = 35602, // Boss->self, 7.0s cast, single-target
        FlareAOE = 35603, // Helper->self, 8.0s cast, range 5 circle
        FlareScald = 35765, // Helper->self, no cast, range 5 circle
        FlareKill = 35605, // Helper->self, 5.0s cast, range 5 circle
        ProminenceSpine = 35606, // Helper->self, 5.0s cast, range 60 width 10 rect

        Nox = 36135, // Boss->self, 4.0s cast, single-target
        NoxAOEFirst = 36137, // Helper->self, 5.0s cast, range 10 circle
        NoxAOERest = 36131, // Helper->self, no cast, range 10 circle

        VoidBio = 35607, // Boss->self, 5.0s cast, single-target
        VoidBioBurst = 35608, // ToxicBubble->player, no cast, single-target

        BigBang = 35587, // Boss->self, 10.0s cast, range 60 circle
        BigBangAOE = 35589, // Helper->location, 8.0s cast, range 5 circle
        BigCrunch = 35588, // Boss->self, 10.0s cast, range 60 circle
        BigCrunchAOE = 36144, // Helper->location, 8.0s cast, range 5 circle

        DarkBeckons = 35594, // Helper->players, no cast, range 6 circle stack
        DarkDivides = 35593, // Helper->player, no cast, range 5 circle spread

        VoidMeteor = 35596, // Boss->self, 4.8s cast, single-target
        MeteorImpactProximity = 35601, // Comet->self, 6.0s cast, range 40 circle
        MeteorImpact = 35595, // Boss->self, 11.0s cast, single-target
        MeteorImpactChargeNormal = 35597, // Comet->location, no cast, width 4 rect charge
        MeteorImpactChargeClipping = 35598, // Comet->location, no cast, width 4 rect charge
        MeteorImpactAppearNormal = 36024, // Helper->location, no cast, range 2 circle
        MeteorImpactAppearClipping = 35599, // Helper->location, no cast, range 60 circle
        MeteorImpactMassiveExplosion = 35600, // Comet->self, no cast, range 60 circle
        MeteorImpactExplosion = 36147, // Comet->self, 5.0s cast, range 10 circle

        BlackHole = 36133, // Boss->self, 5.0s cast, single-target
        _Spell_BlackHole = 36134, // Helper->self, 5.7s cast, range 4 circle // no clue what this is
        FracturedEventideWE = 35571, // Boss->self, 8.0s cast, single-target
        FracturedEventideEW = 35572, // Boss->self, 8.0s cast, single-target
        FracturedEventideAOEFirst = 35573, // Helper->self, 8.5s cast, range 60 width 8 rect
        FracturedEventideAOEFirst2 = 35910, // Helper->self, 8.5s cast, range 60 width 8 rect // no clue what this is either
        FracturedEventideAOERest = 35574, // Helper->self, no cast, range 60 width 8 rect
        
        RendTheRift = 35609, // Boss->self, 6.0s cast, range 60 circle
        NostalgiaDimensionalSurge = 35633, // Helper->location, 4.0s cast, range 5 circle
        NostalgiaDimensionalSurgeSmall = 35634, // Helper->location, 4.0s cast, range 2 circle
        NostalgiaDimensionalSurgeLine = 35635, // Helper->self, 4.0s cast, range 60 width 2 rect

        Nostalgia = 35610, // Boss->self, 5.0s cast, single-target, visual (multiple raidwides)
        NostalgiaBury1 = 35612, // Helper->self, 0.7s cast, range 60 circle
        NostalgiaBury2 = 35613, // Helper->self, 1.7s cast, range 60 circle
        NostalgiaBury3 = 35614, // Helper->self, 2.7s cast, range 60 circle
        NostalgiaBury4 = 35615, // Helper->self, 3.7s cast, range 60 circle
        NostalgiaRoar1 = 35616, // Helper->self, 5.7s cast, range 60 circle
        NostalgiaRoar2 = 35617, // Helper->self, 6.7s cast, range 60 circle
        NostalgiaPrimalRoar = 35618, // Helper->self, 9.7s cast, range 60 circle

        FlowOfTheAbyss = 36089, // Boss->self, 7.0s cast, single-target (spread/stack + unsafe line)
        FlowOfTheAbyssDimensionalSurge = 35637, // Helper->self, 9.0s cast, range 60 width 14 rect
        AkhRhaiStart = 35619, // Helper->self, 3.0s cast, range 5 circle
        AkhRhaiAOE = 35620, // Helper->self, no cast, range 5 circle
        ChasmicNails = 35622, // Boss->self, 7.0s cast, single-target
        ChasmicNailsAOE1 = 35628, // Helper->self, 7.7s cast, range 60 ?-degree cone
        ChasmicNailsAOE2 = 35629, // Helper->self, 8.4s cast, range 60 ?-degree cone
        ChasmicNailsAOE3 = 35630, // Helper->self, 9.1s cast, range 60 ?-degree cone
        ChasmicNailsAOE4 = 35631, // Helper->self, 9.8s cast, range 60 ?-degree cone
        ChasmicNailsAOE5 = 35632, // Helper->self, 10.5s cast, range 60 ?-degree cone
        ChasmicNailsVisual1 = 35623, // Helper->self, 1.5s cast, range 60 ?-degree cone
        ChasmicNailsVisual2 = 35624, // Helper->self, 3.0s cast, range 60 ?-degree cone
        ChasmicNailsVisual3 = 35625, // Helper->self, 4.0s cast, range 60 ?-degree cone
        ChasmicNailsVisual4 = 35626, // Helper->self, 5.0s cast, range 60 ?-degree cone
        ChasmicNailsVisual5 = 35627, // Helper->self, 6.0s cast, range 60 ?-degree cone
    };

    public enum SID : uint
    {
        Doom = 1769, // Boss->player, extra=0x0
        VulnUp = 1789, // Boss->player, extra=0x0
        BigBounce = 3761, // Boss->player, extra=0x0
        BluntResistDown = 2248, // Comet->player, extra=0x0
        Weakness = 43, // none->player, extra=0x0
        HPPenalty = 1089, // none->player, extra=0x0
        AccelerationBomb = 2657, // none->player, extra=0x0
        DivisiveDark = 3762, // none->player, extra=0x0
        FleshWound = 264, // none->player, extra=0x0        
    };

    public enum IconID : uint
    {
        DarkMatter = 364, // player
        BigBang = 376, // player
        Chain = 326, // player
        AccelerationBomb = 267, // player
        DarkBeckonsUmbralRays = 62, // player
        BlackHole = 330, // player
        Nox = 197, // player
        AkhRhai = 23, // player
        UmbralPrism = 211, // player
    };

    public enum TetherID : uint
    {
        VoidMeteorCloseClipping = 252, // Comet->player
        VoidMeteorCloseGood = 253, // Comet->player
        VoidMeteorStretchedClipping = 254, // Comet->player
        VoidMeteorStretchedGood = 255, // Comet->player
        BondsOfDarkness = 163, // player->player
        FlowOfTheAbyss = 265, // FlowOfTheAbyss->Boss
    };
}

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

        AbyssalNox  = 35575, // Boss->self, 5.0s cast, range 60 circle, 1hp/doom with 5s application delay
        AbyssalEchoesVisualCardinal = 35576, // Helper->self, 9.0s cast, single-target, visual (line in cardinal direction)
        AbyssalEchoesVisualIntercardinal = 35577, // Helper->self, 9.0s cast, single-target, visual (line in diagonal direction)
        AbyssalEchoesVisualCenter = 36139, // Helper->self, 9.0s cast, single-target, visual (central spot) (Extreme only)
        AbyssalEchoes = 35578, // Helper->self, 16.0s cast, range 12 circle
        
        SableThread = 35566, // Boss->self, 5.0s cast, single-target, visual (wild charge)
        SableThreadTarget = 35567, // Helper->player, no cast, single-target, visual (target selection)
        SableThreadAOE = 35570, // Helper->self, no cast, range 60 width 12 rect wild charge
        SableThreadVisualHitIntermediate = 35568, // Boss->self, no cast, single-target, visual (before intermediate hits)
        SableThreadVisualHitLast = 35569, // Boss->self, no cast, single-target, visual (before last hit)

        DarkMatter = 35638, // Boss->self, 4.0s cast, single-target, visual (tankbusters)
        DarkMatterAOE = 35639, // Helper->player, no cast, range 8 circle tankbuster

        VisceralWhirlR = 35579, // Boss->self, 8.0s cast, single-target, visual (two of lines with right safespot)
        VisceralWhirlRAOE1 = 35580, // Helper->self, 8.8s cast, range 29 width 28 rect
        VisceralWhirlRAOE2 = 35581, // Helper->self, 8.8s cast, range 60 width 28 rect
        VisceralWhirlL = 35582, // Boss->self, 8.0s cast, single-target, visual  (two of lines with left safespot)
        VisceralWhirlLAOE1 = 35583, // Helper->self, 8.8s cast, range 29 width 28 rect
        VisceralWhirlLAOE2 = 35584, // Helper->self, 8.8s cast, range 60 width 28 rect
        MiasmicBlast = 35657, // Helper->self, 8.0s cast, range 60 width 10 cross (Extreme only)
        MiasmicBlastVisual = 36088, // Helper->self, 8.0s cast, range 60 width 10 cross, visual (animation) (Extreme only)

        Flare = 35602, // Boss->self, 7.0s cast, single-target, visual (towers)
        FlareAOE = 35603, // Helper->self, 8.0s cast, range 5 circle tower
        FlareScald = 35765, // Helper->self, no cast, range 5 circle (tower aftereffect, damage + vuln)
        FlareKill = 35605, // Helper->self, 5.0s cast, range 5 circle (tower aftereffect, kill)
        ProminenceSpine = 35606, // Helper->self, 5.0s cast, range 60 width 10 rect (tower aftereffect, ray)

        VoidBio = 35607, // Boss->self, 5.0s cast, single-target, visual (spawn toxic bubbles)
        VoidBioBurst = 35608, // ToxicBubble->player, no cast, single-target (bubble touch, damage + vuln + dot)

        BigBang = 35587, // Boss->self, 10.0s cast, range 60 circle, raidwide with dot
        BigBangAOE = 35589, // Helper->location, 8.0s cast, range 5 circle puddle
        BigBangSpread = 35806, // Helper->player, 5.0s cast, range 5 circle spread (Extreme only)
        BigCrunch = 35588, // Boss->self, 10.0s cast, range 60 circle, raidwide with dot
        BigCrunchAOE = 36144, // Helper->location, 8.0s cast, range 5 circle puddle
        BigCrunchSpread = 36146, // Helper->player, 5.0s cast, range 5 circle spread (Extreme only)

        VoidMeteor = 35596, // Boss->self, 4.8s cast, single-target, visual (initial meteors)
        MeteorImpactProximity = 35601, // Comet->self, 6.0s cast, range 40 circle with ? falloff
        MeteorImpact = 35595, // Boss->self, 11.0s cast, single-target, visual (tethered meteors)
        MeteorImpactChargeNormal = 35597, // Comet->location, no cast, width 4 rect charge (non-clipping, fatal damage if distance is less than ~25, otherwise small damage)
        MeteorImpactChargeClipping = 35598, // Comet->location, no cast, width 4 rect charge (clipping, fatal damage followed by raidwide and vuln)
        MeteorImpactAppearNormal = 36024, // Helper->location, no cast, range 2 circle (???)
        MeteorImpactAppearClipping = 35599, // Helper->location, no cast, range 60 circle, raidwide with vuln (followed by wipe)
        MeteorImpactMassiveExplosion = 35600, // Comet->self, no cast, range 60 circle (wipe if meteor is clipped)
        MeteorImpactExplosion = 36147, // Comet->self, 5.0s cast, range 10 circle

        DarkBinds = 35669, // Helper->self, no cast, damage + vuln if chains were not broken in 5s (Extreme only)
        DarkBeckons = 35594, // Helper->players, no cast, range 6 circle stack
        DarkDivides = 35593, // Helper->player, no cast, range 5 circle spread
        ForkedLightning = 35668, // Helper->self, no cast, range 5 circle spread (Extreme only)
        // AccelerationBomb = 35663 player->player

        BlackHole = 36133, // Boss->self, 5.0s cast, single-target, visual (placeable black hole + laser)
        _Spell_BlackHole = 36134, // Helper->self, 5.7s cast, range 4 circle // no clue what this is
        FracturedEventideWE = 35571, // Boss->self, 8.0s cast, single-target, visual (laser W to E)
        FracturedEventideEW = 35572, // Boss->self, 8.0s cast, single-target, visual (laser E to W)
        FracturedEventideAOEFirst = 35573, // Helper->self, 8.5s cast, range 60 width 8 rect
        FracturedEventideAOEFirst2 = 35910, // Helper->self, 8.5s cast, range 60 width 8 rect // no clue what this is either
        FracturedEventideAOERest = 35574, // Helper->self, no cast, range 60 width 8 rect

        SparkingFlare = 35678, // Boss->self, 7.0s cast, single-target, visual (towers + spreads) (Extreme only)
        BrandingFlare = 35679, // Boss->self, 7.0s cast, single-target, visual (towers + pairs) (Extreme only)
        SparkingFlareAOE = 35684, // Helper->player, 10.0s cast, range 4 circle spread (Extreme only)
        BrandingFlareAOE = 35685, // Helper->players, 10.0s cast, range 4 circle 2-man stack (Extreme only)
        Nox = 36135, // Boss->self, 4.0s cast, single-target, visual (chasing aoe)
        NoxAOEFirst = 36137, // Helper->self, 5.0s cast, range 10 circle
        NoxAOERest = 36131, // Helper->self, no cast, range 10 circle

        RendTheRift = 35609, // Boss->self, 6.0s cast, range 60 circle, raidwide
        NostalgiaDimensionalSurge = 35633, // Helper->location, 4.0s cast, range 5 circle puddle
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
        AkhRhaiAOE = 35620, // Helper->self, no cast, range 5 circle spread
        UmbralRays = 35702, // Helper->players, 5.0s cast, range 6 circle, 8-man stack (Extreme only)
        UmbralPrism = 35703, // Helper->players, 5.0s cast, range 5 circle, 2-man enumeration stack (Extreme only)
        ChasmicNails = 35622, // Boss->self, 7.0s cast, single-target, visual (pizzas)
        ChasmicNailsAOE1 = 35628, // Helper->self, 7.7s cast, range 60 40-degree cone
        ChasmicNailsAOE2 = 35629, // Helper->self, 8.4s cast, range 60 40-degree cone
        ChasmicNailsAOE3 = 35630, // Helper->self, 9.1s cast, range 60 40-degree cone
        ChasmicNailsAOE4 = 35631, // Helper->self, 9.8s cast, range 60 40-degree cone
        ChasmicNailsAOE5 = 35632, // Helper->self, 10.5s cast, range 60 40-degree cone
        ChasmicNailsVisual1 = 35623, // Helper->self, 1.5s cast, range 60 40-degree cone, visual (telegraph)
        ChasmicNailsVisual2 = 35624, // Helper->self, 3.0s cast, range 60 40-degree cone, visual (telegraph)
        ChasmicNailsVisual3 = 35625, // Helper->self, 4.0s cast, range 60 40-degree cone, visual (telegraph)
        ChasmicNailsVisual4 = 35626, // Helper->self, 5.0s cast, range 60 40-degree cone, visual (telegraph)
        ChasmicNailsVisual5 = 35627, // Helper->self, 6.0s cast, range 60 40-degree cone, visual (telegraph)
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
        FleshWound = 264, // none->player, extra=0x0
        DivisiveDark = 3762, // none->player, extra=0x0 (Extreme only)
        ForkedLightning = 3799, // none->player, extra=0x0 (Extreme only)
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

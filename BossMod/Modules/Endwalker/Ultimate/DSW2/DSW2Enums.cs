namespace BossMod.Endwalker.Ultimate.DSW2
{
    public enum OID : uint
    {
        BossP2 = 0x313C, // R5.001, x1 - king thordan - p2
        SerZephirin = 0x3130, // R4.002, x1 - ??
        SerAdelphel = 0x3139, // R4.002, x1 - p1, p2
        SerGrinnaux = 0x313A, // R4.002, x1 - p1, p2
        SerCharibert = 0x313B, // R4.002, x1 - ??
        SerJanlenoux = 0x3158, // R4.002, x1 - p2
        SerVellguine = 0x3159, // R4.002, x1 - p2
        SerPaulecrain = 0x315A, // R4.002, x1 - p2
        SerIgnasse = 0x315B, // R4.002, x1 - p2
        SerHermenost = 0x315C, // R4.002, x1 - p2
        SerGuerrique = 0x315D, // R4.002, x1 - p2
        SerHaumeric = 0x315E, // R4.002, x1 - p2
        SerNoudenet = 0x315F, // R4.002, x1 - p2
        //_Gen_Actor_3316 = 0x3316, // R3.800, x1
        Helper = 0x233C, // R0.500, x24
        Brightsphere = 0x330E, // R1.000, spawn during fight by Shining Blade
        HolyComet = 0x312F, // R1.440, spawn during fight by Holy Comet
        VoidzoneFire = 0x1EB686, // R0.500, EventObj type, spawn during fight - fire voidzones spawned on intercards during meteors
        VoidzoneIce = 0x1EB682, // R0.500, EventObj type, spawn during fight - ice voidzones spawned on hiemal storm targets during meteors

        BossP3 = 0x313D, // R8.019, nidhogg - p3
        NidhoggDrake = 0x313E, // R8.019, x8 - p3
        // 34FB (x3), 34FC (x3), 34FD, 34FE, 34FF, 3500 (x3) 3501, 3502 - wtf is this shit, spawn together with nidhogg and attack each other with 870's for 5 dmg...
    };

    public enum AID : uint
    {
        // phase 2
        AutoAttackP2 = 25531, // BossP2->mt, no cast, range 10 ?-degree cone
        Teleport = 25540, // BossP2->location, no cast
        Reappear = 25532, // BossP2->self, no cast
        WalkTo = 25535, // BossP2->location, no cast

        AscalonsMercyConcealed = 25544, // BossP2->self, 3.0s cast, visual
        AscalonsMercyConcealedAOE = 25545, // Helper->self, 1.5s cast, range 50 30-degree (?) cone
        AscalonsMight = 25541, // BossP2->self, no cast, range 50 60-degree (?) cone tankbuster

        KnightsOfTheRound = 25581, // BossP2->self, no cast, visual
        StrengthOfTheWard = 25555, // BossP2->self, 4.0s cast, visual
        LightningStorm = 25548, // BossP2->self, 5.7s cast, visual
        LightningStormAOE = 25549, // Helper->player, no cast, range 5 aoe
        SpiralThrust = 25556, // SerIgnasse/SerVellguine/SerPaulecrain->self, 6.0s cast, range 52 width 16 rect aoe
        HeavyImpact = 25557, // SerGuerrique->self, 4.3s cast, visual
        HeavyImpactHit1 = 25558, // Helper->self, 6.0s cast, range 6 aoe
        HeavyImpactHit2 = 25559, // Helper->self, no cast, range 6-12 donut
        HeavyImpactHit3 = 25560, // Helper->self, no cast, range 12-18 donut
        HeavyImpactHit4 = 25561, // Helper->self, no cast, range 18-24 donut
        HeavyImpactHit5 = 25562, // Helper->self, no cast, range 24-30 donut

        DragonsRage = 25550, // BossP2->self, 4.7s cast, visual
        DragonsRageAOE = 25551, // Helper->players, no cast, range 8 shared aoe
        DimensionalCollapse = 25563, // SerGrinnaux->self, 8.0s cast, visual (growing void zones)
        DimensionalCollapseAOE = 25564, // Helper->location, 9.0s cast, range 9 aoe
        SkywardLeap = 25565, // SerIgnasse/SerVellguine/SerPaulecrain->player, no cast, range 24 aoe on player with blue mark
        Conviction1 = 25566, // SerHermenost->self, 8.2s cast, visual towers
        Conviction1AOE = 25567, // Helper->location, 11.0s cast, range 3 aoe, soaked towers
        EternalConviction = 25568, // Helper->self, no cast, raidwide from unsoaked towers
        HolyShieldBash = 25297, // SerJanlenoux/SerAdelphel->tethered player, no cast, width 8 rect tankbuster
        HolyShieldBashAOE = 25579, // Helper->self, no cast, range 6 ??-degree cone, ??
        HolyBladedanceVisual = 25298, // SerJanlenoux/SerAdelphel->self, no cast, visual
        HolyBladedanceAOE = 25299, // Helper->self, no cast, range 16 90-degree cone aoe (follows tankbuster, multiple hits)

        AncientQuaga = 25542, // BossP2->self, 6.0s cast, raidwide
        HeavenlyHeel = 25543, // BossP2->player, 4.0s cast, tankbuster forcing tankswap

        SanctityofTheWard = 25569, // BossP2->self, 4.0s cast, visual
        DragonsGaze = 25552, // BossP2->self, 4.0s cast, visual
        DragonsGazeAOE = 25553, // Helper->self, no cast, face away from caster
        DragonsGlory = 25554, // Helper->self, no cast, face away from caster
        ShiningBlade = 25570, // SerAdelphel/SerJanlenoux->location, no cast, half-width 3 rect (?) charge
        SacredSever = 25571, // SerZephirin->players, no cast, range 6 shared aoe
        BrightFlare = 25295, // Brightsphere->self, 1.0s cast, range 9 aoe

        HiemalStorm = 25574, // SerHaumeric->self, 7.0s cast, visual
        HiemalStormAOE = 25575, // Helper->players, no cast, range 7 aoe, baited at 4 dd or tanks/healers
        HeavensStake = 28590, // SerCharibert->self, 7.0s cast, visual
        HeavensStakeAOE = 28591, // Helper->location, 7.5s cast, range 7 aoe (at four intercardinals)
        HeavensStakeDonut = 28592, // Helper->self, 7.5s cast, range 15?-30 donut aoe
        Conviction2 = 29563, // SerHermenost->self, 9.2s cast, visual
        Conviction2AOE = 29564, // Helper->location, 12.0s cast, range 3 aoe, soaked towers
        HolyComet = 25576, // SerNoudenet->self, 12.0s cast, visual
        HolyCometAOE = 25577, // HolyComet->self, no cast, range 20 aoe on comet drop
        HolyImpact = 25578, // HolyComet->self, no cast, raidwide on comet fail (link range ~5)
        FaithUnmoving = 25308, // SerGrinnaux->self, 4.0s cast, knockback 16
        Conviction3 = 28650, // SerHermenost->self, 8.2s cast, visual
        Conviction3AOE = 28651, // Helper->location, 11.0s cast, range 3 aoe, soaked towers

        UltimateEnd = 25533, // BossP2->self, no cast, visual
        UltimateEndAOE = 25534, // Helper->self, no cast, raidwide

        BroadSwingRL = 25536, // BossP2->self, 3.0s cast, visual
        BroadSwingLR = 25537, // BossP2->self, 3.0s cast, visual
        BroadSwingAOE = 25538, // Helper->self, no cast, range 40 120-degree cone
        AethericBurst = 25539, // BossP2->self, 6.0s cast, enrage

        // phase 3
        FinalChorus = 26376, // BossP3->self, no cast, visual
        FinalChorusAOE = 26377, // Helper->self, no cast, raidwide
        AutoAttackP3 = 26416, // BossP3->player, no cast, range 60 half-width width 3 rect aoe

        DiveFromGrace = 26381, // BossP3->self, 5.0s cast, visual
        GnashAndLash = 26386, // BossP3->self, 7.6s cast, visual
        LashAndGnash = 26387, // BossP3->self, 7.6s cast, visual
        EyeOfTheTyrant = 26388, // BossP3->player, no cast, range 6 shared aoe
        GnashingWheel = 26389, // BossP3->self, no cast, range 8 aoe
        LashingWheel = 26390, // BossP3->self, no cast, range 8-40 donut aoe
        DarkHighJump = 26382, // NidhoggDrake->players, no cast, range 5 aoe
        DarkSpineshatterDive = 26383, // NidhoggDrake->player, no cast, range 5 aoe
        DarkElusiveJump = 26384, // NidhoggDrake->player, no cast, range 5 aoe
        DarkdragonDive = 26385, // NidhoggDrake->self, 2.5s cast, range 5 tower aoe
        DarkdragonDiveFail = 26395, // Helper->self, no cast, raidwide if tower is not soaked
        Geirskogul = 26378, // NidhoggDrake->self, 4.5s cast, range 62 width 8 baited rect aoe

        Drachenlance = 26379, // BossP3->self, 2.9s cast, visual
        DrachenlanceAOE = 26380, // Helper->self, 3.5s cast, range 13 90-degree cone

        DarkdragonDive1 = 26391,// NidhoggDrake->self, 2.5s cast, range 5 tower aoe, should be soaked by 1 person
        DarkdragonDive2 = 26392,// NidhoggDrake->self, 2.5s cast, range 5 tower aoe, should be soaked by 2 persons
        DarkdragonDive3 = 26393,// NidhoggDrake->self, 2.5s cast, range 5 tower aoe, should be soaked by 3 persons
        DarkdragonDive4 = 26394,// NidhoggDrake->self, 2.5s cast, range 5 tower aoe, should be soaked by 4 persons
        SoulTether = 26396, // BossP3/NidhoggDrake->player, no cast, range 5 aoe tankbuster on tether targets
        RevengeOfTheHorde = 29750, // BossP3->self, 11.0s cast, enrage
    };

    public enum SID : uint
    {
        None = 0,
        Discomposed = 2733,
        Jump1 = 3004, // 'First in Line'
        Jump2 = 3005, // 'Second in Line'
        Jump3 = 3006, // 'Third in Line'
        JumpCenter = 2755, // 'High Jump Target'
        JumpForward = 2756, // 'Spineshatter Dive Target'
        JumpBackward = 2757, // 'Elusive Jump Target'
    }

    public enum TetherID : uint
    {
        None = 0,
        HolyShieldBash = 84, // SerJanlenoux/SerAdelphel->player
    }

    public enum IconID : uint
    {
        None = 0,
        SacredSever1 = 50, // player
        SacredSever2 = 51, // player
        SkywardLeap = 330, // player
        Prey = 285, // player
        Jump1 = 319, // player
        Jump2 = 320, // player
        Jump3 = 321, // player
    }
}

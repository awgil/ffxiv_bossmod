namespace BossMod.Endwalker.Ultimate.DSW1
{
    public enum OID : uint
    {
        SerAdelphel = 0x3139, // R4.002, x1, initial main target
        SerGrinnaux = 0x313A, // R4.002, x1, initial main target
        SerCharibert = 0x313B, // R4.002, x1, initially untargetable
        SerZephirin = 0x3130, // R4.002, x1, does nothing?..
        Helper = 0x233C, // R0.500, x24
        AetherialTear = 0x330F, // R1.000, spawn during fight by Hyperdimensional Slash
        Brightsphere = 0x330E, // R1.000, spawn during fight by Shining Blade
        SpearOfTheFury = 0x2E22, // R2.000,  during fight (pure-of-heart)
        //_Gen_Haurchefant = 0x333D, // R0.500, x1
        //_Gen_Actorfd8a1 = 0xFD8A1, // R0.500-1.000, x1, EventNpc type
        //_Gen_Actor1eb681 = 0x1EB681, // R0.500, x0, EventObj type, and more spawn during fight
    };

    public enum AID : uint
    {
        AutoAttackAdelphel = 28531, // SerAdelphel->player, no cast, single-target
        AutoAttackGrinnaux = 28532, // SerGrinnaux->player, no cast, single-target
        Teleport = 25321, // SerGrinnaux->location, no cast
        HoliestOfHoly = 25300, // SerAdelphel->self, 4.0s cast, raidwide

        EmptyDimension = 25306, // SerGrinnaux->self, 5.0s cast, range 6-70 donut
        FullDimension = 25307, // SerGrinnaux->self, 5.0s cast, range 6 circle
        HolyShieldBash = 25297, // SerAdelphel->tethered player, no cast, width 8 rect charge tankbuster
        HolyShieldBashVisual = 25579, // Helper->self, no cast, range 6 ?-degree cone
        HolyBladedanceVisual = 25298, // SerAdelphel->self, no cast, single-target
        HolyBladedanceAOE = 25299, // Helper->self, no cast, range 16 90-degree cone aoe (follows tankbuster, 6 hits)
        Heavensblaze = 25309, // SerCharibert->players, 5.0s cast, range 4 shared aoe

        HyperdimensionalSlash = 25302, // SerGrinnaux->self, 5.0s cast, visual
        HyperdimensionalSlashSecond = 25365, // SerGrinnaux->self, no cast, visual
        HyperdimensionalSlashAOEIcon = 25303, // Helper->self, no cast, range 70 width 8 rect at players with icons
        HyperdimensionalSlashAOERest = 25582, // Helper->self, no cast, range 40 120-degree cone at player without icon
        DimensionalTorsion = 25304, // AetherialTear->player, no cast, hit if player is too close to tear (range < 9)
        DimensionalPurgation = 25305, // AetherialTear->self, no cast, aoe if two tears are too close to each other?

        FaithUnmoving = 25308, // SerGrinnaux->self, 4.0s cast, knockback 16
        ShiningBlade = 25294, // SerAdelphel->location, no cast, width 6 rect charge
        BrightFlare = 25295, // Brightsphere->self, 1.0s cast, range 9 aoe
        Execution = 25301, // SerAdelphel->location, no cast, range 5 aoe around tank

        HoliestHallowing = 25296, // SerAdelphel->SerGrinnaux, 4.0s cast, heal (interruptible)

        Heavensflame = 25310, // SerCharibert->self, 7.0s cast, visual
        HeavensflameAOE = 25311, // Helper->players, no cast, range 10 aoe around everyone
        HolyChain = 25312, // Helper->self, no cast, hit if player's tether is not broken by Heavensflame cast end

        BrightbladesSteel = 25292, // SerAdelphel->self, 3.0s cast, enrage (heal + invuln + damage-up)
        TheBullsSteel = 25293, // SerGrinnaux->self, 3.0s cast, enrage (heal + invuln + damage-up)

        PlanarPrison = 25313, // SerGrinnaux->self, no cast, range 70 circle, applies small damage, pulls to grinnaux and stuns for a second the whole raid right before pure-of-heart phase
        PlanarPrison2 = 25580, // Helper->self, no cast, range ?-70 donut, ??? (cast extremely often and does nothing, some sort of fail mechanic?)
        SpearOfTheFury = 25314, // SerZephirin->self, 10.0s cast, range 22 width 10 rect, ??? (hits haurchefant??)
        BrightwingedFlight = 25366, // SerAdelphel->self, no cast, range 8 ?-degree cone, ??? (applies two buffs on ser charibert)
        PureOfHeart = 25316, // SerCharibert->self, 35.5s cast, raidwide, damage depends on caster hp %?
        Shockwave = 25315, // SpearOfTheFury->self, no cast, raidwide, 7 casts every ~1.1s
        Brightwing = 25369, // Helper->self, no cast, range 18 ?-degree cone, baited on 2 closest targets
        Skyblind = 25370, // Helper->location, 2.5s cast, range 3 puddle
    };

    public enum SID : uint
    {
        None = 0,
        Skyblind = 2661, // Helper->player, after baiting cones
    }

    public enum TetherID : uint
    {
        None = 0,
        HolyBladedance = 84, // SerAdelphel->player
        Heavensflame = 9, // player->player
    }

    public enum IconID : uint
    {
        None = 0,
        HyperdimensionalSlash = 234,
        HeavensflameCircle = 281,
        HeavensflameTriangle = 282,
        HeavensflameSquare = 283,
        HeavensflameCross = 284,
    }
}

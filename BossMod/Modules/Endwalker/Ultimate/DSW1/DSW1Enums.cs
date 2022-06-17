namespace BossMod.Endwalker.Ultimate.DSW1
{
    public enum OID : uint
    {
        SerAdelphel = 0x3139, // x1, initial main target
        SerGrinnaux = 0x313A, // x1, initial main target
        SerCharibert = 0x313B, // x1, initially untargetable
        SerZephirin = 0x3130, // x1, does nothing?..
        Helper = 0x233C, // x24
        AetherialTear = 0x330F, // spawn during fight by Hyperdimensional Slash
        Brightsphere = 0x330E, // spawn during fight by Shining Blade
        SpearOfTheFury = 0x2E22, // spawn during fight (pure-of-heart)
    };

    public enum AID : uint
    {
        AutoAttackAdelphel = 28531,
        AutoAttackGrinnaux = 28532,
        Teleport = 25321, // SerGrinnaux->location, no cast
        HoliestOfHoly = 25300, // SerAdelphel->self, 4.0s cast, raidwide

        EmptyDimension = 25306, // SerGrinnaux->self, 5.0s cast, range 6-70 donut
        FullDimension = 25307, // SerGrinnaux->self, 5.0s cast, range 6 aoe
        HolyShieldBash = 25297, // SerAdelphel->tethered player, no cast, half-width 4 rect tankbuster
        HolyShieldBashVisual = 25579, // Helper->self, no cast
        HolyBladedanceVisual = 25298, // SerAdelphel->self, no cast
        HolyBladedanceAOE = 25299, // Helper->self, no cast, range 16 90-degree cone aoe (follows tankbuster, 6 hits)
        Heavensblaze = 25309, // SerCharibert->players, 5.0s cast, range 4 shared aoe

        HyperdimensionalSlash = 25302, // SerGrinnaux->self, 5.0s cast, visual
        HyperdimensionalSlashSecond = 25365, // SerGrinnaux->self, no cast, visual
        HyperdimensionalSlashAOEIcon = 25303, // Helper->self, no cast, range 70 half-width 4 rect at players with icons
        HyperdimensionalSlashAOERest = 25582, // Helper->self, no cast, range 40 120-degree cone at player without icon
        DimensionalTorsion = 25304, // AetherialTear->player, no cast, hit if player is too close to tear (range < 9)
        DimensionalPurgation = 25305, // AetherialTear->self, no cast, aoe if two tears are too close to each other?

        FaithUnmoving = 25308, // SerGrinnaux->self, 4.0s cast, knockback 16
        ShiningBlade = 25294, // SerAdelphel->location, no cast, half-width 3 rect (?) charge
        BrightFlare = 25295, // Brightsphere->self, 1.0s cast, range 9 aoe
        Execution = 25301, // SerAdelphel->location, no cast, range 5 aoe around tank

        HoliestHallowing = 25296, // SerAdelphel->SerGrinnaux, 4.0s cast, heal (interruptible)

        Heavensflame = 25310, // SerCharibert->self, 7.0s cast, visual
        HeavensflameAOE = 25311, // Helper->players, no cast, range 10 aoe around everyone
        HolyChain = 25312, // Helper->self, no cast, hit if player's tether is not broken by Heavensflame cast end

        BrightbladesSteel = 25292, // SerAdelphel->self, 3.0s cast, enrage (heal + invuln + damage-up)
        TheBullsSteel = 25293, // SerGrinnaux->self, 3.0s cast, enrage (heal + invuln + damage-up)

        PlanarPrison = 25313, // SerGrinnaux->self, no cast, ??? (applies small damage and knockback to whole raid right before pure-of-heart phase?..)
        PlanarPrison2 = 25580, // Helper->self, no cast, ??? (cast extremely often and does nothing, some sort of fail mechanic?)
        SpearOfTheFury = 25314, // SerZephirin->self, 10.0s cast, ??? (hits haurchefant??)
        BrightwingedFlight = 25366, // SerAdelphel->self, no cast, ??? (applies two buffs on ser charibert)
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

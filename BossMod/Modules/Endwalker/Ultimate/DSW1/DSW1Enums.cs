namespace BossMod.Endwalker.Ultimate.DSW1
{
    public enum OID : uint
    {
        Boss = 0x3139, // Ser Adelphel, initial main target
        SerGrinnaux = 0x313A, // x1, initial main target
        SerCharibert = 0x313B, // x1, untargetable
        SerZephirin = 0x3130, // x1, does nothing?..
        Helper = 0x233C, // x24
        AetherialTear = 0x330F, // spawn during fight by Hyperdimensional Slash
        Brightsphere = 0x330E, // spawn during fight by Shining Blade
    };

    public enum AID : uint
    {
        AutoAttackAdelphel = 28531,
        AutoAttackGrinnaux = 28532,
        Teleport = 25321, // SerGrinnaux->location, no cast
        HoliestOfHoly = 25300, // Boss->self, 4.0s cast, raidwide

        EmptyDimension = 25306, // SerGrinnaux->self, 5.0s cast, range 6-70 donut
        FullDimension = 25307, // SerGrinnaux->self, 5.0s cast, range 6 aoe
        HolyShieldBash = 25297, // Boss->tethered player, no cast, half-width 4 rect tankbuster
        HolyShieldBashVisual = 25579, // Helper->self, no cast
        HolyBladedanceVisual = 25298, // Boss->self, no cast
        HolyBladedanceAOE = 25299, // Helper->self, no cast, range 16 90-degree cone aoe (follows tankbuster, 6 hits)
        Heavensblaze = 25309, // SerCharibert->players, 5.0s cast, range 4 shared aoe

        HyperdimensionalSlash = 25302, // SerGrinnaux->self, 5.0s cast, visual
        HyperdimensionalSlashSecond = 25365, // SerGrinnaux->self, no cast, visual
        HyperdimensionalSlashAOEIcon = 25303, // Helper->self, no cast, range 70 half-width 4 rect at players with icons
        HyperdimensionalSlashAOERest = 25582, // Helper->self, no cast, range 40 120-degree cone at player without icon
        DimensionalTorsion = 25304, // AetherialTear->player, no cast, hit if player is too close to tear (range < 9)
        DimensionalPurgation = 25305, // AetherialTear->self, no cast, aoe if two tears are too close to each other?

        FaithUnmoving = 25308, // SerGrinnaux->self, 4.0s cast, knockback 16
        ShiningBlade = 25294, // Boss->location, no cast, half-width 3 rect (?) charge
        BrightFlare = 25295, // Brightsphere->self, 1.0s cast, range 9 aoe
        Execution = 25301, // Boss->location, no cast, range 5 aoe around tank

        HoliestHallowing = 25296, // Boss->SerGrinnaux, 4.0s cast, heal (interruptible)

        Heavensflame = 25310, // SerCharibert->self, 7.0s cast, visual
        HeavensflameAOE = 25311, // Helper->players, no cast, range 10 aoe around everyone
        HolyChain = 25312, // Helper->self, no cast, hit if player's tether is not broken by Heavensflame cast end

        BrightbladesSteel = 25292, // Boss->self, 3.0s cast, enrage (heal + invuln + damage-up)
        TheBullsSteel = 25293, // SerGrinnaux->self, 3.0s cast, enrage (heal + invuln + damage-up)
    };

    public enum SID : uint
    {
        None = 0,
    }

    public enum TetherID : uint
    {
        None = 0,
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

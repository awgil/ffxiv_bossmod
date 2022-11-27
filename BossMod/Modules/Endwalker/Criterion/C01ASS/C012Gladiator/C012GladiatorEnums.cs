namespace BossMod.Endwalker.Criterion.C01ASS.C012Gladiator
{
    public enum OID : uint
    {
        Boss = 0x39A5, // R6.500, x1
        Helper = 0x233C, // R0.500, x18
        GladiatorMirage = 0x39A6, // R6.500, spawns during fight, specter of might
        HatefulVisage = 0x39A7, // R2.250, spawns during fight
        Regret = 0x39A8, // R1.000, spawns during fight
    };

    public enum AID : uint
    {
        AutoAttack = 870, // Boss->player, no cast, single-target
        FlashOfSteel = 30321, // Boss->self, 5.0s cast, raidwide
        MightySmite = 30322, // Boss->player, 5.0s cast, single-target tankbuster
        Teleport = 30265, // Boss->location, no cast, single-target, teleport

        SpecterOfMight = 30323, // Boss->self, 4.0s cast, single-target, visual
        RushOfMight1 = 30296, // GladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
        RushOfMight2 = 30297, // GladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
        RushOfMight3 = 30298, // GladiatorMirage->location, 10.0s cast, range 35 width 3 rect aoe
        RushOfMightFront = 30299, // Helper->self, 10.5s cast, range 60 180-degree cone aoe
        RushOfMightBack = 30300, // Helper->self, 12.5s cast, range 60 180-degree cone aoe

        SculptorsPassionTargetSelection = 26708, // Helper->player, no cast, single-target, cast right before the beginning of visual cast
        SculptorsPassion = 30316, // Boss->self, 5.0s cast, single-target, visual
        SculptorsPassionAOE = 31219, // Boss->self, no cast, range 60 width 8 rect shared

        CurseOfTheFallen = 30324, // Boss->self, 5.0s cast, single-target, visual
        RingOfMightVisual = 30655, // Helper->self, 10.0s cast, range 18 circle, ???
        RingOfMight1Out = 30301, // Boss->self, 10.0s cast, range 8 circle
        RingOfMight2Out = 30302, // Boss->self, 10.0s cast, range 13 circle
        RingOfMight3Out = 30303, // Boss->self, 10.0s cast, range 18 circle
        RingOfMight1In = 30304, // Helper->self, 12.0s cast, range 8-30 donut
        RingOfMight2In = 30305, // Helper->self, 12.0s cast, range 13-30 donut
        RingOfMight3In = 30306, // Helper->self, 12.0s cast, range 18-30 donut
        EchoOfTheFallen = 30325, // Helper->players, no cast, range 6 circle spread
        ThunderousEcho = 30326, // Helper->players, no cast, range 5 circle stack
        LingeringEcho = 30327, // Helper->location, no cast, range 5 circle baited 5x repeated aoe
        EchoOfTheFallenDeath = 30950, // Helper->location, no cast, range 8 circle aoe on debuffed player death

        HatefulVisage = 30318, // Boss->self, 3.0s cast, single-target, visual
        AccursedVisage = 30349, // Boss->self, 3.0s cast, single-target, visual
        WrathOfRuin = 30307, // Boss->self, 3.0s cast, single-target, visual
        RackAndRuin = 30308, // Regret->location, 4.0s cast, range 40 width 5 rect
        GoldenFlame = 30319, // HatefulVisage->self, 10.0s cast, range 60 width 10 rect
        SilverFlame = 30320, // HatefulVisage->self, 10.0s cast, range 60 width 10 rect
        NothingBesideRemains = 30347, // Boss->self, 5.0s cast, single-target, visual
        NothingBesideRemainsAOE = 30348, // Helper->players, 5.0s cast, range 8 circle spread

        CurseOfTheMonument = 30310, // Boss->self, 4.0s cast, single-target, visual
        ChainsOfResentment = 30311, // Helper->self, no cast, raidwide if tether is not broken in time
        SunderedRemains = 30312, // Helper->self, 6.0s cast, range 10 circle aoe
        ScreamOfTheFallen = 30328, // Helper->players, no cast, range 15 circle spread
        ScreamOfTheFallenDeath = 30951, // Helper->location, no cast, range 15 circle on debuffed player death
        ColossalWreck = 30313, // Boss->self, 6.0s cast, single-target, visual
        Explosion = 30314, // Helper->self, 6.5s cast, range 3 circle tower
        MassiveExplosion = 30315, // Helper->self, no cast, raidwide if tower is unsoaked

        Enrage = 30329, // Boss->self, 10.0s cast, enrage
        Enrage2 = 31282, // Boss->self, no cast, range 60 circle ???
    };

    public enum SID : uint
    {
        EchoOfTheFallen = 3290, // none->player, extra=0x0, spread
        ThunderousEcho = 3293, // none->player, extra=0x0, stack
        LingeringEchoes = 3292, // none->player, extra=0x0, voidzone

        GildedFate = 3295, // none->player, extra=0x1/0x2 (stand under SilverFlame)
        SilveredFate = 3296, // none->player, extra=0x1/0x2 (stand under GoldenFlame)

        FirstInLine = 3004, // none->player, extra=0x0
        SecondInLine = 3005, // none->player, extra=0x0
        ScreamOfTheFallen = 3291, // none->player, extra=0x0
        ChainsOfResentment = 3294, // none->player, extra=0x0
    };
}

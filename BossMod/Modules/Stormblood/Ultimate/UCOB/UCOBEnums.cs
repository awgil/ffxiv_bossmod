namespace BossMod.Stormblood.Ultimate.UCOB
{
    public enum OID : uint
    {
        Twintania = 0x1FDF, // R3.960, x1
        Oviform = 0x1FE0, // R1.000, spawn during fight (hatch orb)

        NaelDeusDarnus = 0x1FE1, // R2.550, x1
        NaelGeminus = 0x1FE2, // R2.550, x4, 523 type (helper that for meteor streams)
        Firehorn = 0x1FE3, // R4.000, spawn during fight (fire dragon)
        Iceclaw = 0x1FE4, // R4.000, spawn during fight (ice dragon)
        Thunderwing = 0x1FE5, // R4.000, spawn during fight (thunder dragon)
        TailOfDarkness = 0x1FE6, // R4.000, spawn during fight (dark dragon)
        FangOfLight = 0x1FE7, // R4.000, spawn during fight (light dragon)

        //_Gen_Phoenix = 0x1FE9, // R2.800, x1
        //_Gen_BahamutPrime = 0x1FE8, // R4.200, x1

        Helper = 0x18D6, // R0.500, x42, mixed types

        VoidzoneTwister = 0x1E8910, // R0.500, EventObj type, spawn during fight
        VoidzoneLiquidHell = 0x1E88FE, // R0.500, EventObj type, spawn during fight
        Neurolink = 0x1E88FF, // R0.500, EventObj type, spawn during fight
        VoidzoneSalvation = 0x1E91D4, // R0.500, EventObj type, spawn during fight
        //_Gen_Actor1e8536 = 0x1E8536, // R2.000, x1, EventObj type

        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x6, EventObj type
        //_Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type
    };

    public enum AID : uint
    {
        AutoAttackP1 = 9895, // Twintania->player, no cast, single-target
        Plummet = 9896, // Twintania->self, no cast, range 8+R ?-degree cone
        DeathSentence = 9897, // Twintania->player, 4.0s cast, single-target, tankbuster
        Twister = 9898, // Twintania->self, 2.0s cast, single-target, visual (spawn twister)
        TwisterAOE = 9899, // Helper->self, no cast, ???, kill target and knockback 50 players in radius 8-9
        Fireball = 9900, // Twintania->players, no cast, range 4 circle stack
        LiquidHell = 9901, // Twintania->location, no cast, range 6 circle voidzone
        Generate = 9902, // Twintania->self, 3.0s cast, single-target, visual (spawn hatch)
        Hatch = 9903, // Oviform->self, no cast, range 8 circle
        DeepHatch = 9904, // Helper->self, no cast, range 80 circle (hatch fail)

        AutoAttackP2 = 9908, // NaelDeusDarnus->player, no cast, single-target
        BahamutsClaw = 9909, // NaelDeusDarnus->player, no cast, single-target, 5-hit tankbuster
        Heavensfall = 9912, // Helper->self, no cast, range 80 circle, knockback 11 from center
        ThermionicBurst = 9913, // Helper->self, 3.0s cast, range 24+R 22.5-degree cone
        IronChariot = 9915, // NaelDeusDarnus->self, no cast, range 6+R circle aoe
        LunarDynamo = 9916, // NaelDeusDarnus->self, no cast, range ?-22 donut aoe
        ThermionicBeam = 9917, // NaelDeusDarnus->players, no cast, range 4 circle stack
        MeteorStream = 9920, // NaelGeminus->players, no cast, range 4 circle
        DalamudDive = 9921, // NaelDeusDarnus->location, no cast, range 5 circle tankbuster
        BahamutsFavor = 9922, // NaelDeusDarnus->self, 3.0s cast, single-target, visual (damage-up on self, spawn dragons)
        FireballP2 = 9925, // Firehorn->players, no cast, range 4 circle, applies firescorched on targets
        Iceball = 9926, // Iceclaw->player, no cast, single-target, applies icebitten on 1 player
        ChainLightning = 9927, // Thunderwing->self, no cast, ???, applies thunderstruck on 2 players
        ChainLightningAOE = 9928, // Helper->self, no cast, ???, paralysis on targets within radius 5 except main target
        Deathstorm = 9929, // TailOfDarkness->self, no cast, ???, applies dooms on 2-3 players
        WingsOfSalvation = 9930, // FangOfLight->location, 3.0s cast, range 4 circle aoe that leaves cleanse voidzone
    };

    public enum TetherID : uint
    {
        Fireball = 5, // Firehorn->player
    };

    public enum IconID : uint
    {
        Fireball = 117, // player
        Generate = 118, // player
    };
}

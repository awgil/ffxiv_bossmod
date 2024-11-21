namespace BossMod.Dawntrail.Unreal.Un1Byakko;

public enum OID : uint
{
    Boss = 0x4579, // R4.300, x1
    Hakutei = 0x4581, // R4.750, x1 - tiger
    Helper = 0x464D, // R0.500, x29, mixed types
    AratamaForce = 0x4589, // R0.700, x0 (spawn during fight) - orbs from center
    IntermissionHakutei = 0x458B, // R4.750, x0 (spawn during fight)
    AramitamaSoul = 0x458A, // R1.000, x0 (spawn during fight) - orbs from edge
    AratamaPuddle = 0x1E8EA9, // R0.500, x0 (spawn during fight), EventObj type
    IntermissionHelper = 0x1EA87E, // R0.500, x0 (spawn during fight), EventObj type
    VacuumClaw = 0x1EA957, // R0.500, x0 (spawn during fight), EventObj type
    ArenaFeatures = 0x1EA1A1, // R2.000, x1, EventObj type
}

public enum AID : uint
{
    AutoAttackBoss = 39987, // Boss->player, no cast, single-target
    AutoAttackAdd = 39988, // Hakutei->player, no cast, single-target
    TeleportBoss = 39929, // Boss->location, no cast, single-target
    TeleportAdd = 39927, // Hakutei->location, no cast, single-target
    StormPulse = 39933, // Boss->self, 4.0s cast, range 100 circle, raidwide
    StormPulseRepeat = 39964, // Boss->self, no cast, range 100 circle, raidwide
    HeavenlyStrike = 39931, // Boss->players, 4.0s cast, range 3 circle tankbuster

    StateOfShock = 39937, // Boss->player, 4.0s cast, single-target, stun target to grab & throw it
    StateOfShockSecond = 39928, // Boss->player, no cast, single-target, stun second target to grab & throw it
    Clutch = 39938, // Boss->player, no cast, single-target, grab target
    HighestStakes = 39939, // Boss->location, 5.0s cast, single-target, jump
    HighestStakesAOE = 39940, // Helper->location, no cast, range 6 circle tower

    UnrelentingAnguish = 39950, // Boss->self, 3.0s cast, single-target, visual (orbs)
    UnrelentingAnguishAratama = 39958, // AratamaForce->self, no cast, range 2 circle, orb explosion
    OminousWind = 39948, // Boss->self, no cast, single-target, apply bubbles
    OminousWindAOE = 39949, // Helper->self, no cast, range 6 circle, if bubbles touch
    FireAndLightningBoss = 39930, // Boss->self, 4.0s cast, range 50+R width 20 rect

    DanceOfTheIncomplete = 39924, // Boss->self, no cast, single-target, visual (split off tiger)
    AddAppear = 39923, // Hakutei->self, no cast, single-target, visual (start appear animation)
    AratamaPuddle = 39926, // Helper->location, no cast, range 4 circle, puddle drop
    SteelClaw = 39936, // Hakutei->self, no cast, range 13+R ?-degree cone, cleave
    WhiteHerald = 39962, // Hakutei->self, no cast, range 50 circle with ? falloff
    DistantClap = 39934, // Boss->self, 5.0s cast, range 4-25 donut
    FireAndLightningAdd = 39935, // Hakutei->self, 4.0s cast, range 50+R width 20 rect

    VoiceOfThunder = 39959, // Hakutei->self, no cast, single-target, visual (physical damage down orbs)
    VoiceOfThunderAratama = 39965, // AramitamaSoul->self, no cast, range 2 circle, orb explosion if soaked
    VoiceOfThunderAratamaFail = 39960, // AramitamaSoul->Hakutei, no cast, single-target, orb explosion if it reaches the tiger, heals
    RoarOfThunder = 39961, // Hakutei->self, 20.0s cast, range 100 circle, raidwide scaled by remaining hp
    RoarOfThunderEnd1 = 39968, // Hakutei->Boss, no cast, single-target, visual (???)
    RoarOfThunderEnd2 = 39967, // Boss->self, no cast, single-target, visual (???)
    IntermissionOrbVisual = 39951, // Boss->self, no cast, single-target, visual (start next set of orbs)
    IntermissionOrbSpawn = 39952, // Helper->location, no cast, single-target, visual (location of next orb)
    IntermissionOrbAratama = 39953, // Helper->location, no cast, range 2 circle
    ImperialGuard = 39954, // IntermissionHakutei->self, 3.0s cast, range 40+R width 5 rect
    IntermissionSweepTheLegVisual = 39956, // Boss->self, no cast, single-target, visual (start donut)
    IntermissionSweepTheLeg = 39957, // Helper->self, 5.1s cast, range 5-25 donut
    IntermissionEnd = 39970, // Boss->self, no cast, single-target, visual (intermission end)
    FellSwoop = 39963, // Helper->self, no cast, range 100 circle, raidwide

    AnswerOnHigh = 39941, // Boss->self, no cast, single-target, visual (exaflare start)
    HundredfoldHavocFirst = 39942, // Helper->self, 5.0s cast, range 5 circle exaflare
    HundredfoldHavocRest = 39943, // Helper->self, no cast, range 5 circle exaflare
    SweepTheLegBoss = 39932, // Boss->self, 4.0s cast, range 24+R 270-degree cone

    Bombogenesis = 39944, // Boss->self, no cast, single-target, visual (3 baited puddle icons)
    GaleForce = 39945, // Helper->self, no cast, range 6 circle, baited puddle
    VacuumClaw = 39946, // Helper->self, no cast, range ? circle, growing voidzone aoe
    VacuumBlade = 39947, // Helper->self, no cast, range 100 circle, ??? (raidwide with vuln, sometimes happens after vacuum claws)

    StormPulseEnrage = 39969, // Boss->self, 8.0s cast, range 100 circle
}

public enum SID : uint
{
    Stun = 201, // Boss->player, extra=0x0
    OminousWind = 1481, // none->player, extra=0x0
}

public enum IconID : uint
{
    HighestStakes = 62, // Helper->self
    AratamaPuddle = 4, // player->self
    WhiteHerald = 87, // player->self
    Bombogenesis = 101, // player->self
}

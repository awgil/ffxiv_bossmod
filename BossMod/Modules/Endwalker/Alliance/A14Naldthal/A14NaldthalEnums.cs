namespace BossMod.Endwalker.Alliance.A14Naldthal;

public enum OID : uint
{
    Boss = 0x3903,
    FortuneFluxHelper = 0x38A0, // x3
    SoulVesselFake = 0x3907, // untargetable/tethered placeholders that spawn during fight
    SoulVesselReal = 0x38A1, // adds that spawn during fight
    Helper = 0x233C, // x31
};

// nald = orange, thal = blue
public enum AID : uint
{
    AutoAttackNald = 29579, // Boss->player, no cast, single-target
    AutoAttackThal = 29580, // Boss->player, no cast, single-target
    AutoAttackSoulVessel = 28629, // SoulVesselReal->player, no cast, single-target
    Teleport = 28941, // Boss->self, no cast, single-target

    AsAboveSoBelowNald = 28904, // Boss->self, 5.0s cast, range 60 circle, raidwide (the version seems to be based on previous mechanic)
    AsAboveSoBelowThal = 28905, // Boss->self, 5.0s cast, range 60 circle, raidwide (the version seems to be based on previous mechanic)
    GoldenTenet = 28954, // Boss->self, 5.0s cast, visual
    GoldenTenetAOE = 28955, // Helper->players, 5.5s cast, range 6 shared tankbuster
    StygianTenet = 28956, // Boss->self, 5.0s cast, visual
    StygianTenetAOE = 28957, // Helper->player, 5.5s cast, range 3 tankbuster on 3 tanks
    HeavensTrial = 28958, // Boss->self, 5.0s cast, visual
    HeavensTrialAOE = 28959, // Helper->players, 5.5s cast, range 6 stack
    HeavensTrialConeStart = 29645, // Helper->player, no cast, visual (show 3 cone markers)
    HeavensTrialSmelting = 28960, // Helper->self, no cast, range 60 30-degree cone
    HellsTrial = 28961, // Boss->self, 5.0s cast, range 60 circle, raidwide

    HeatAboveFlamesBelowNald = 29604, // Boss->self, 11.0s cast, visual (circle real -> stay out)
    HeatAboveFlamesBelowThal = 29605, // Boss->self, 12.0s cast, visual (donut real -> stay in)
    FlamesOfTheDeadFake = 29606, // Helper->self, 12.0s cast, range 8-30 donut, no effect
    FlamesOfTheDeadReal = 29607, // Helper->self, 12.0s cast, range 8-30 donut aoe
    LivingHeatFake = 29608, // Helper->location, 12.0s cast, range 8 circle, no effect
    LivingHeatReal = 29609, // Helper->location, 12.0s cast, range 8 circle aoe
    FarAboveDeepBelowThal = 29610, // Boss->self, 12.0s cast, visual (chaser real; starts as orange, but changes color mid-cast)
    FarAboveDeepBelowNald = 29611, // Boss->self, 12.0s cast, visual (line stack real; starts as blue, but changes color mid-cast)
    FarFlungFireVisual = 29612, // Helper->player, no cast, show stack marker
    FarFlungFireAOE = 29613, // Helper->self, no cast, range 40 half-width 3 shared rect
    DeepestPitFirst = 29639, // Helper->location, 5.0s cast, range 6 aoe
    DeepestPitRest = 29640, // Helper->location, 1.5s cast, range 6 aoe, casts 2 to 5
    HearthAboveFlightBelowThalNald = 29723, // Boss->self, 11.0s cast, single-target, visual (orange = line stack + out; starts as blue)
    HearthAboveFlightBelowThal = 29642, // Boss->self, 12.0s cast, single-target, visual (blue = in + chaser; without swap)
    HearthAboveFlightBelowNald = 29643, // Boss->self, 12.0s cast, single-target, visual (orange = line stack + out; without swap)
    HearthAboveFlightBelowNaldThal = 29644, // Boss->self, 12.0s cast, single-target, visual (blue = in + chaser; starts as orange)

    OnceAboveEverBelowThalNald = 29631, // Boss->self, 11.0s cast, visual, orange aoes, starts as blue but changes color mid cast
    OnceAboveEverBelowThal = 29632, // Boss->self, 12.0s cast, visual, blue aoes, without swap
    OnceAboveEverBelowNaldThal = 29724, // Boss->self, 12.0s cast, visual, blue aoes, starts as orange but changes color mid cast
    OnceAboveEverBelowNald = 29725, // Boss->self, 12.0s cast, visual, orange aoes, without swap
    OnceBurnedFake = 29633, // Helper->self, 12.6s cast, orange, visual, no effect
    OnceBurnedFirst = 29637, // Helper->self, 12.6s cast, orange, range 6 aoe
    OnceBurnedRest = 29638, // Helper->self, no cast, orange, range 6 aoe
    EverfireFirst = 29634, // Helper->self, 12.6s cast, blue, range 6 aoe
    EverfireRest = 29635, // Helper->self, no cast, blue, range 6 aoe
    EverfireFake = 29636, // Helper->self, 12.6s cast, blue, visual, no effect

    HellOfFireFront = 29367, // Boss->self, 8.0s cast, visual, frontal
    HellOfFireFrontAOE = 29368, // Helper->self, 9.0s cast, range 60 180-degree cone
    HellOfFireBack = 29369, // Boss->self, 8.0s cast, visual, turns around during cast
    HellOfFireBackAOE = 29370, // Helper->self, 9.0s cast, range 60 180-degree cone (note that caster faces aoe direction)

    WaywardSoul = 28942, // Boss->self, 3.0s cast, visual
    WaywardSoulAOE = 28944, // Helper->location, 8.0s cast, range 18 aoe
    WaywardSoulEnd = 28943, // Boss->self, no cast, visual

    FiredUp1Knockback = 28945, // Boss->self, 4.0s cast, visual
    FiredUp1AOE = 28946, // Boss->self, 4.0s cast, visual
    FiredUp2Knockback = 29577, // Boss->self, 4.0s cast, visual
    FiredUp2AOE = 29578, // Boss->self, 4.0s cast, visual
    FiredUp3Knockback = 29721, // Boss->self, 4.0s cast, visual
    FiredUp3AOE = 29722, // Boss->self, 4.0s cast, visual
    FortuneFlux = 28947, // Boss->self, 8.0s cast, visual
    FortuneFluxKnockback1 = 29744, // Helper->location, 10.5s cast, visual - knockback 30 will happen from cast location
    FortuneFluxAOE1 = 29746, // Helper->location, 10.5s cast
    FortuneFluxKnockback2 = 29747, // Helper->location, 12.5s cast
    FortuneFluxAOE2 = 29745, // Helper->location, 12.5s cast, visual - range 20 aoe will happen from cast location
    FortuneFluxKnockback3 = 29749,  // Helper->location, 14.0s cast
    FortuneFluxAOE3 = 29748, // Helper->location, 14.0s cast
    FortuneFluxJumpFirst = 28948, // Boss->location, no cast, teleport to first location
    FortuneFluxJumpMid = 28949, // Boss->location, no cast, teleport to intermediate location
    FortuneFluxJumpLast = 29616, // Boss->location, no cast, teleport to last location
    SeventhPassageKnockback = 29615, // Helper->self, no cast, knockback 30
    SeventhPassageKnockbackVisualLast = 28950, // Boss->self, no cast, single-target, visual
    SeventhPassageKnockbackVisualCont = 28951, // FortuneFluxHelper->self, no cast, ? (cast before Knockback)
    SeventhPassageAOELast = 28952, // Boss->self, no cast, range 20 aoe (last/2nd)
    SeventhPassageAOECont = 28953, // FortuneFluxHelper->self, no cast, range 20 aoe (first)

    SoulsMeasure = 28962, // Boss->location, 6.0s cast, visual
    SoulsMeasureTeleport = 28963, // Boss->self, no cast, visual
    EqualWeight = 28964, // SoulVesselFake->self, 5.0s cast, visual
    SoulsMeasureUnk1 = 29574, // Helper->location, no cast, ?
    Twingaze = 28970, // SoulVesselReal->self, 5.0s cast, range 60 30-degree cone
    MagmaticSpell = 28972, // SoulVesselReal->self, 3.0s cast, visual
    MagmaticSpellAOE = 28973, // Helper->players, 5.0s cast, range 6 stack
    Balance = 28965, // Boss->self, 12.5s cast, visual
    BalanceSuccess = 29359, // Helper->self, no cast, visual (balance maintained)
    BalanceFail1 = 29742, // Helper->self, no cast, single-target, visual (balance not maintained)
    BalanceFail2 = 29743, // Helper->self, no cast, single-target, visual (balance not maintained)
    TippedScales = 29576, // Boss->self, no cast, visual
    TippedScalesAOE = 29561, // Helper->self, 34.0s cast, raidwide (on success)
    TippedScalesEnrage = 29562, // Helper->self, 34.0s cast, wipe (on failure)
};

public enum SID : uint
{
    None = 0,
    FiredUp = 2193, // Boss, 0x190 = knockback, 0x191 = aoe, 0x199 = ?
}

public enum TetherID : uint
{
    None = 0,
    FiredUp = 12, // Helper->Boss
    SoulVessel = 7, // player->SoulVesselFake
}

public enum IconID : uint
{
    None = 0,
    HeavensTrialStack = 62,
    HeavensTrialCone = 237,
    DeepestPitTarget = 340,
    MagmaticSpell = 62, // stack marker
}

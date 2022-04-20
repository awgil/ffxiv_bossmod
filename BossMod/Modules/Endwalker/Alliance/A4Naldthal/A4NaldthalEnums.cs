namespace BossMod.Endwalker.Alliance.A4Naldthal
{
    public enum OID : uint
    {
        Boss = 0x3903,
        FortuneFluxHelper = 0x38A0, // x3
        SoulVesselFake = 0x3907, // untargetable/tethered placeholders that spawn during fight
        SoulVesselReal = 0x38A1, // adds that spawn during fight
        Helper = 0x233C, // x31
    };

    public enum AID : uint
    {
        AutoAttack1 = 29580,
        AutoAttack2 = 29579,
        AutoAttackSoulVessel = 28629, // SoulVesselReal->player, no cast
        Teleport = 28941, // Boss->self, no cast, ?

        AsAboveSoBelow1 = 28905, // Boss->self, 5.0s cast, raidwide
        AsAboveSoBelow2 = 28904, // Boss->self, 5.0s cast, raidwide
        GoldenTenet = 28954, // Boss->self, 5.0s cast, visual
        GoldenTenetAOE = 28955, // Helper->players, 5.5s cast, range 6 shared tankbuster
        StygianTenet = 28956, // Boss->self, 5.0s cast, visual
        StygianTenetAOE = 28957, // Helper->player, 5.5s cast, range 3 tankbuster on 3 tanks

        HeatAboveFlamesBelowInner = 29604, // Boss->self, 11.0s cast, visual
        HeatAboveFlamesBelowOuter = 29605, // Boss->self, 12.0s cast, visual
        FlamesOfTheDeadFake = 29606, // Helper->self, 12.0s cast, no effect
        FlamesOfTheDeadReal = 29607, // Helper->self, 12.0s cast, range 30 donut aoe, 8 inner
        LivingHeatFake = 29608, // Helper->location, 12.0s cast, no effect
        LivingHeatReal = 29609, // Helper->location, 12.0s cast, range 8 aoe

        HeavensTrial = 28958, // Boss->self, 5.0s cast, visual
        HeavensTrialAOE = 28959, // Helper->players, 5.5s cast, range 6 stack
        HeavensTrialConeStart = 29645, // Helper->player, no cast, show 3 cone markers
        HeavensTrialSmelting= 28960, // Helper->self, no cast, range 60 30-degree ? cone

        FarAboveDeepBelow = 29610, // Boss->self, 12.0s cast, visual (without FarFlungFire aoe?)
        FarFlungFireVisual = 29612, // Helper->player, no cast, show stack marker
        FarFlungFireAOE = 29613, // Helper->self, no cast, range 40 half-width 3 shared rect
        DeepestPitFirst = 29639, // Helper->location, 5.0s cast, range 6 aoe
        DeepestPitRest = 29640, // Helper->location, 1.5s cast, range 6 aoe, casts 2 to 5
        HearthAboveFlightBelow = 29723, // Boss->self, 11.0s cast, visual (FarFlungFire without deep? + HeatAboveFlamesBelow inner?)

        OnceAboveEverBelowRed = 29631, // Boss->self, 11.0s cast, visual, red aoes
        OnceAboveEverBelowBlue = 29632, // Boss->self, 12.0s cast, visual, blue aoes
        OnceBurnedFake = 29633, // Helper->self, 12.6s cast, blue, no effect?
        EverfireFirst = 29634, // Helper->self, 12.6s cast, blue, range 6 aoe
        EverfireRest = 29635, // Helper->self, no cast, blue, range 6 aoe
        EverfireFake = 29636, // Helper->self, 12.6s cast, blue, no effect?
        OnceBurnedFirst = 29637, // Helper->self, 12.6s cast, red, range 6 aoe
        OnceBurnedRest = 29638, // Helper->self, no cast, red, range 6 aoe

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
        FiredUp3Knockback = 29721, // Boss->self, 4.0s cast, visual (? not seen)
        FiredUp3AOE = 29722, // Boss->self, 4.0s cast, visual
        FortuneFlux = 28947, // Boss->self, 8.0s cast, visual
        FortuneFluxKnockback1 = 29744, // Helper->location, 10.5s cast, visual - knockback 30 will happen from cast location
        FortuneFluxAOE2 = 29745, // Helper->location, 12.5s cast, visual - range 20 aoe will happen from cast location
        FortuneFluxAOE1 = 29746, // Helper->location, 10.5s cast
        FortuneFluxKnockback2 = 29747, // Helper->location, 12.5s cast
        FortuneFluxAOE3 = 29748, // Helper->location, 14.0s cast
        FortuneFluxKnockback3 = 29749,  // Helper->location, 14.0s cast (? not seen)
        FortuneFluxUnk1 = 28948, // Boss->location, no cast, ? (cast immediately after flux end)
        FortuneFluxUnk2 = 28949, // Boss->location, no cast, ?
        FortuneFluxUnk3 = 29616, // Boss->location, no cast, visual?
        SeventhPassageKnockback = 29615, // Helper->self, no cast, knockback 30
        SeventhPassageUnk1 = 28951, // FortuneFluxHelper->self, no cast, ? (cast before Knockback)
        SeventhPassageAOE1 = 28952, // Boss->self, no cast, range 20 aoe
        SeventhPassageAOE2 = 28953, // FortuneFluxHelper->self, no cast, range 20 aoe ?

        SoulsMeasure = 28962, // Boss->location, 6.0s cast, visual
        SoulsMeasureTeleport = 28963, // Boss->self, no cast, visual
        EqualWeight = 28964, // SoulVesselFake->self, 5.0s cast, visual
        SoulsMeasureUnk1 = 29574, // Helper->location, no cast, ?
        Twingaze = 28970, // SoulVesselReal->self, 5.0s cast, range 60 30-degree (?) cone
        MagmaticSpell = 28972, // SoulVesselReal->self, 3.0s cast, visual
        MagmaticSpellAOE = 28973, // Helper->players, 5.0s cast, range 6 stack
        Balance = 28965, // Boss->self, 12.5s cast, visual
        BalanceUnk = 29359, // Helper->self, no cast, ?
        TippedScales = 29576, // Boss->self, no cast, visual
        TippedScalesAOE = 29561, // Helper->self, 34.0s cast, raidwide (on success)
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
}

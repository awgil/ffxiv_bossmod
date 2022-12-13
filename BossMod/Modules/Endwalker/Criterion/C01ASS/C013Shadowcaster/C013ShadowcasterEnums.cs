namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    public enum OID : uint
    {
        NBoss = 0x39AA, // R9.000, x1
        NLaser = 0x39AC, // R2.000, x14
        NBeacon = 0x39AE, // R2.000, x4
        NBallOfFire = 0x39B0, // R1.000, spawn during fight
        NArcaneFont = 0x39B2, // R0.500-1.000, spawn during fight (mirror)

        SBoss = 0x39E1, // R9.000, x1
        SLaser = 0x39E2, // R2.000, x14
        SBeacon = 0x39E3, // R2.000, x4
        SBallOfFire = 0x39E4, // R1.000, spawn during fight
        SArcaneFont = 0x39E5, // R0.500-1.000, spawn during fight

        Helper = 0x233C, // R0.500, x32
        Portal = 0x1EB761, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 872, // NBoss/SBoss->player, no cast, single-target
        Teleport = 29886, // NBoss/SBoss->location, no cast, single-target

        NShowOfStrength = 29871, // NBoss->self, 5.0s cast, raidwide
        NFiresteelFracture = 29869, // NBoss->self, 5.0s cast, range 40 60-degree cone cleave
        SShowOfStrength = 30405, // NBoss->self, 5.0s cast, raidwide
        SFiresteelFracture = 30404, // NBoss->self, 5.0s cast, range 40 60-degree cone cleave

        InfernBrand = 29841, // NBoss/SBoss->self, 4.0s cast, single-target, visual
        CrypticPortal = 29844, // NBoss/SBoss->self, 4.0s cast, single-target, visual
        FiresteelStrike = 29872, // NBoss/SBoss->self, 4.9s cast, single-target, visual
        NFiresteelStrikeAOE1 = 29873, // NBoss->location, no cast, range 10 circle aoe (first jump)
        NFiresteelStrikeAOE2 = 29874, // NBoss->location, no cast, range 10 circle aoe (second jump)
        SFiresteelStrikeAOE1 = 30406, // SBoss->location, no cast, range 10 circle aoe (first jump)
        SFiresteelStrikeAOE2 = 30407, // SBoss->location, no cast, range 10 circle aoe (second jump)
        NBurn = 29840, // NBallOfFire->self, no cast, range 12 circle aoe
        SBurn = 30395, // SBallOfFire->self, no cast, range 12 circle aoe
        BlessedBeacon = 29875, // NBoss/SBoss->self, 4.9s cast, single-target, visual
        NBlessedBeaconAOE1 = 29876, // NBoss->self, no cast, range 65 width 8 rect aoe (first cleave)
        NBlessedBeaconAOE2 = 29877, // NBoss->self, no cast, range 65 width 8 rect aoe (second cleave)
        SBlessedBeaconAOE1 = 30408, // SBoss->self, no cast, range 65 width 8 rect aoe (first cleave)
        SBlessedBeaconAOE2 = 30409, // SBoss->self, no cast, range 65 width 8 rect aoe (second cleave)

        CrypticFlames = 29878, // NBoss/SBoss->self, 8.3s cast, single-target, visual
        NCrypticFlamesBreak = 29879, // Helper->player, no cast, single-target, small damage when correct laser is broken
        NCrypticFlamesFail = 29880, // Helper->player, no cast, single-target, small damage + stun + damage down when incorrect laser is broken
        NBigBurst = 29881, // Helper->self, no cast, raidwide cast when player with any flame debuff dies
        SCrypticFlamesBreak = 30410, // Helper->player, no cast, single-target, small damage when correct laser is broken
        //SCrypticFlamesFail = 30411, // Helper->player, no cast, single-target, small damage + stun + damage down when incorrect laser is broken
        //SBigBurst = 30412, // Helper->self, no cast, raidwide cast when player with any flame debuff dies
        NBlazingBenifice = 29862, // NArcaneFont->self, no cast, range 100 width 10 rect aoe (mirror explosion)
        SBlazingBenifice = 30401, // SArcaneFont->self, no cast, range 100 width 10 rect aoe (mirror explosion)
        CastShadow = 29850, // NBoss/SBoss->self, 4.8s cast, single-target, visual
        NCastShadowAOE1 = 29852, // Helper->self, 5.5s cast, range 65 30-degree cone aoe
        NCastShadowAOE2 = 29854, // Helper->self, 7.5s cast, range 65 30-degree cone aoe
        SCastShadowAOE1 = 30397, // Helper->self, 5.5s cast, range 65 30-degree cone aoe
        SCastShadowAOE2 = 30398, // Helper->self, 7.5s cast, range 65 30-degree cone aoe

        InfernWave = 29882, // NBoss/SBoss->self, 4.0s cast, single-target, visual
        NInfernWaveAOE = 29883, // Helper->self, no cast, range 60 90-degree cone aoe
        SInfernWaveAOE = 30413, // Helper->self, no cast, range 60 90-degree cone aoe
        Banishment = 29884, // NBoss/SBoss->self, 4.0s cast, single-target, visual
        ActivateBeacon = 29845, // Helper->NBeacon/NLaser/SBeacon/SLaser, no cast, single-target, visual
        InfernWard = 29846, // NBoss/SBoss->self, 4.0s cast, single-target, visual
        ActivateBeacon2 = 29847, // Helper->self, no cast, single-target, visual ???
        Brandfire = 29885, // Helper->player, no cast, single-target, kill anyone who doesn't take portal
        TrespassersPyre = 29849, // Helper->player, 1.0s cast, single-target, damage if player crosses unbreakable laser
        TrespassersPyreAttract = 29890, // Helper->self, no cast, range 60 width 100 rect, attract ?, bumps player away from unbreakable laser

        PureFire = 29855, // NBoss/SBoss->self, 3.0s cast, single-target, visual
        NPureFireAOE = 29857, // Helper->location, 3.0s cast, range 6 circle puddle
        SPureFireAOE = 30399, // Helper->location, 3.0s cast, range 6 circle puddle
    };

    public enum SID : uint
    {
        FirstBrand = 3268, // none->player, extra=0x0
        SecondBrand = 3269, // none->player, extra=0x0
        ThirdBrand = 3270, // none->player, extra=0x0
        FourthBrand = 3271, // none->player, extra=0x0
        FirstFlame = 3272, // none->player, extra=0x0
        SecondFlame = 3273, // none->player, extra=0x0
        ThirdFlame = 3274, // none->player, extra=0x0
        FourthFlame = 3275, // none->player, extra=0x0
        Counter = 2397, // none->NLaser/NBeacon, extra=0x1C2/0x1C3/0x1C4/0x1C5/0x1C6/0x1C7/0x1C8/0x1C9/0x1C1/0x1CC/0x1F3 (1C2-1C5 = #1-#4 NS, 1C6-1C9 = #1-#4 EW, 1C1 = unbreakable)
        PlayerPortal = 2970, // NBoss->player, extra=0x1CD/0x1CE/0x1D2/0x1D3 (1CD = CCW E->N, 1CE = CW W->N, 1D2 = CW E->S, 1D3 = CCW W->S)
        CallOfThePortal = 3276, // NBoss->player, extra=0x0
    };
}

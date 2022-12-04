namespace BossMod.Endwalker.Criterion.C01ASS.C013Shadowcaster
{
    public enum OID : uint
    {
        Boss = 0x39AA, // R9.000, x1
        Helper = 0x233C, // R0.500, x32
        Laser = 0x39AC, // R2.000, x14
        Beacon = 0x39AE, // R2.000, x4
        BallOfFire = 0x39B0, // R1.000, spawn during fight
        ArcaneFont = 0x39B2, // R0.500-1.000, spawn during fight (mirror)
        Portal = 0x1EB761, // R0.500, EventObj type, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 872, // Boss->player, no cast, single-target
        ShowOfStrength = 29871, // Boss->self, 5.0s cast, raidwide
        FiresteelFracture = 29869, // Boss->self, 5.0s cast, range 40 60-degree cone cleave
        Teleport = 29886, // Boss->location, no cast, single-target

        InfernBrand = 29841, // Boss->self, 4.0s cast, single-target, visual
        CrypticPortal = 29844, // Boss->self, 4.0s cast, single-target, visual
        FiresteelStrike = 29872, // Boss->self, 4.9s cast, single-target, visual
        FiresteelStrikeAOE1 = 29873, // Boss->location, no cast, range 10 circle aoe (first jump)
        FiresteelStrikeAOE2 = 29874, // Boss->location, no cast, range 10 circle aoe (second jump)
        Burn = 29840, // BallOfFire->self, no cast, range 12 circle aoe
        BlessedBeacon = 29875, // Boss->self, 4.9s cast, single-target, visual
        BlessedBeaconAOE1 = 29876, // Boss->self, no cast, range 65 width 8 rect aoe (first cleave)
        BlessedBeaconAOE2 = 29877, // Boss->self, no cast, range 65 width 8 rect aoe (second cleave)

        CrypticFlames = 29878, // Boss->self, 8.3s cast, single-target, visual
        CrypticFlamesBreak = 29879, // Helper->player, no cast, single-target, small damage when correct laser is broken
        CrypticFlamesFail = 29880, // Helper->player, no cast, single-target, small damage + stun + damage down when incorrect laser is broken
        BigBurst = 29881, // Helper->self, no cast, raidwide cast when player with any flame debuff dies
        BlazingBenifice = 29862, // ArcaneFont->self, no cast, range 100 width 10 rect aoe (mirror explosion)
        CastShadow = 29850, // Boss->self, 4.8s cast, single-target, visual
        CastShadowAOE1 = 29852, // Helper->self, 5.5s cast, range 65 30-degree cone aoe
        CastShadowAOE2 = 29854, // Helper->self, 7.5s cast, range 65 30-degree cone aoe

        InfernWave = 29882, // Boss->self, 4.0s cast, single-target, visual
        InfernWaveAOE = 29883, // Helper->self, no cast, range 60 90-degree cone aoe
        Banishment = 29884, // Boss->self, 4.0s cast, single-target, visual
        ActivateBeacon = 29845, // Helper->Beacon/Laser, no cast, single-target, visual
        InfernWard = 29846, // Boss->self, 4.0s cast, single-target, visual
        ActivateBeacon2 = 29847, // Helper->self, no cast, single-target, visual ???
        Brandfire = 29885, // Helper->player, no cast, single-target, kill anyone who doesn't take portal
        TrespassersPyre = 29849, // Helper->player, 1.0s cast, single-target, damage if player crosses unbreakable laser
        TrespassersPyreAttract = 29890, // Helper->self, no cast, range 60 width 100 rect, attract ?, bumps player away from unbreakable laser

        PureFire = 29855, // Boss->self, 3.0s cast, single-target, visual
        PureFireAOE = 29857, // Helper->location, 3.0s cast, range 6 circle puddle
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
        Counter = 2397, // none->Laser/39AE, extra=0x1C2/0x1C3/0x1C4/0x1C5/0x1C6/0x1C7/0x1C8/0x1C9/0x1C1/0x1CC/0x1F3 (1C2-1C5 = #1-#4 NS, 1C6-1C9 = #1-#4 EW, 1C1 = unbreakable)
        PlayerPortal = 2970, // Boss->player, extra=0x1CD/0x1CE/0x1D2/0x1D3 (1CD = CCW W->N, 1CE = CW W->N, 1D2 = CW E->S, 1D3 = CCW W->S)
    };
}

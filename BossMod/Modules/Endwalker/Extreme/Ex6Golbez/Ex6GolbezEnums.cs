namespace BossMod.Endwalker.Extreme.Ex6Golbez
{
    public enum OID : uint
    {
        Boss = 0x3F57, // R6.000, x1
        ShadowDragon = 0x3F58, // R7.000, x1
        GaleSphere = 0x3F59, // R1.000, x16
        GolbezsShadow = 0x3F5A, // R7.500, x4
        Helper = 0x233C, // R0.500, x25
    };

    public enum AID : uint
    {
        AutoAttack = 33868, // Boss->player, no cast, single-target
        Teleport = 33976, // Boss->location, no cast, single-target
        Terrastorm = 33892, // Boss->self, 3.0s cast, single-target, visual
        TerrastormAOE = 33894, // Helper->self, 8.0s cast, range 16 circle
        LingeringSpark = 33896, // Boss->self, 3.0s cast, single-target, visual
        LingeringSparkAOE = 33898, // Helper->location, 3.0s cast, range 5 circle
        PhasesOfTheBlade = 34523, // Boss->self, 5.0s cast, range 22 180-degree cone (set own model state to 7)
        PhasesOfTheBladeVisualEnd = 34525, // Boss->self, no cast, single-target, visual (restore own model state to 0)
        PhasesOfTheBladeBack = 34546, // Helper->self, 8.4s cast, range 22 180-degree cone
        BindingCold = 33971, // Boss->self, 5.0s cast, range 40 circle, raidwide

        GaleSphere = 33869, // Boss->self, 3.0s cast, single-target, visual (mechanic start)
        GaleSpherePrepareN = 33871, // GolbezsShadow->self, no cast, single-target, visual (ghost moving N)
        GaleSpherePrepareE = 33872, // GolbezsShadow->self, no cast, single-target, visual (ghost moving E)
        GaleSpherePrepareW = 33873, // GolbezsShadow->self, no cast, single-target, visual (ghost moving W)
        GaleSpherePrepareS = 33874, // GolbezsShadow->self, no cast, single-target, visual (ghost moving S)
        GaleSphereVisualEnd = 33870, // Boss->self, no cast, single-target, visual (restore model state)
        GaleSphereAOE1 = 33880, // GaleSphere->self, 4.0s cast, range 30 width 5 rect
        GaleSphereAOE2 = 33881, // GaleSphere->self, 7.5s cast, range 30 width 5 rect
        GaleSphereAOE3 = 33882, // GaleSphere->self, 11.0s cast, range 30 width 5 rect
        GaleSphereAOE4 = 33883, // GaleSphere->self, 14.5s cast, range 30 width 5 rect
        GaleSphereVisual = 33875, // GolbezsShadow->self, no cast, single-target, visual (ghost after aoes happen on corresponding side)

        ArcticAssault = 33887, // Boss->self, 3.0s cast, single-target, visual
        ArcticAssaultAOE = 33889, // Helper->self, 4.0s cast, range 15 width 15 rect
        VoidMeteor = 33965, // Boss->self, 5.0s cast, single-target, visual (multihit tankbuster)
        VoidComet = 33968, // Helper->player, no cast, range 4 circle, micro tankbuster
        VoidMeteorAOE = 33969, // Helper->player, no cast, range 6 circle, tankbuster

        AzdajasShadowBlackFang = 34559, // Boss->self, 5.0s cast, single-target, visual
        BlackFang = 33905, // Boss->self, 4.0s cast, single-target, visual
        BlackFangVisual = 33906, // ShadowDragon->self, 4.0s cast, single-target, visual
        BlackFangAOE1 = 34586, // Helper->self, no cast, range 40 circle, raidwide hit 1
        BlackFangAOE2 = 34512, // Helper->self, no cast, range 40 circle, raidwide hits 2-5
        BlackFangAOE3 = 33908, // Helper->self, 10.7s cast, range 40 circle, raidwide hit 6
        AzdajasShadowVisualEnd = 33909, // Boss->self, no cast, single-target, visual

        AzdajasShadowCircleStack = 33912, // Boss->self, 8.0s cast, single-target, visual (out+stack variant)
        AzdajasShadowDonutSpread = 33913, // Boss->self, 8.0s cast, single-target, visual (in+spread variant)
        FlamesOfEventide = 33915, // Boss->self, no cast, range 50 width 6 rect, 3-hit tankbuster requiring swap
        Dragonflame = 33916, // Helper->player, 0.5s cast, single-target, one-shot if target gets 3 stacks of above
        PhasesOfTheShadow = 34535, // Boss->self, 5.0s cast, range 22 180-degree cone
        PhasesOfTheShadowVisualEnd = 34537, // Boss->self, no cast, single-target, visual
        PhasesOfTheShadowBack = 34543, // Helper->self, 8.4s cast, range 22 180-degree cone
        RisingBeacon = 33933, // ShadowDragon->self, 6.7s cast, single-target, visual
        RisingRing = 33934, // ShadowDragon->self, 6.7s cast, single-target, visual
        RisingBeaconAOE = 34540, // Helper->self, 9.6s cast, range 10 circle
        RisingRingAOE = 34541, // Helper->self, 9.6s cast, range 6-22 donut
        BurningShade = 33940, // Helper->player, 5.0s cast, range 5 circle spread
        ImmolatingShade = 33942, // Helper->players, 5.0s cast, range 6 circle stack

        DoubleMeteor = 33973, // Boss->self, 11.0s cast, single-target, visual (flares)
        DragonsDescent = 33944, // ShadowDragon->self, no cast, range 45 circle knockback 13
        ExplosionDouble = 33949, // Helper->self, 10.5s cast, range 4 circle (2-man tower)
        ExplosionTriple = 33950, // Helper->self, 10.5s cast, range 4 circle (3-man tower)
        MassiveExplosion = 33952, // Helper->self, no cast, range 40 circle (tower fail)
        DoubleMeteorAOE1 = 33975, // Helper->players, no cast, range 40 circle with ? falloff (on tank)
        DoubleMeteorAOE2 = 34700, // Helper->players, no cast, range 40 circle with ? falloff (on dps)
        Cauterize = 33954, // ShadowDragon->self, no cast, range 50 width 12 rect

        VoidStardust = 33956, // Boss->self, 3.0s cast, single-target, visual
        VoidStardustFirst = 33958, // Helper->self, 5.8s cast, range 6 circle
        VoidStardustRestVisual = 33960, // Helper->self, 1.0s cast, range 6 circle, visual (indication where exaflare will hit)
        VoidStardustRestAOE = 33962, // Helper->self, no cast, range 6 circle
        AbyssalQuasar = 33963, // Helper->players, 8.0s cast, range 3 circle 2-man stack
        EventideTriad = 33920, // Boss->self, 5.0s cast, single-target, visual (mechanic)
        EventideTriadVisual = 33921, // Boss->self, no cast, single-target, visual (for each hit)
        EventideTriadAOE = 34482, // Helper->self, no cast, range 50 ?-degree cone (targets random player of each role)
        EventideFall = 33925, // Boss->self, 5.0s cast, single-target, visual (mechanic)
        EventideFallVisual = 33926, // Boss->self, no cast, single-target, visual (for each hit)
        EventideFallAOE = 34484, // Helper->self, no cast, range 50 width 6 rect 4-man stack

        VoidBlizzard = 33890, // Helper->players, 6.0s cast, range 6 circle 4-man stack
        VoidAero = 33884, // Helper->players, 6.0s cast, range 3 circle 2-man stack
        VoidTornado = 33885, // Helper->players, 6.0s cast, range 6 circle 4-man stack
    };

    public enum SID : uint
    {
        FlamesOfEventide = 3573, // Boss->player, extra=0x1/0x2
    };

    public enum TetherID : uint
    {
        Cauterize = 17, // ShadowDragon->player
    };

    public enum IconID : uint
    {
        VoidMeteor = 344, // player
        BurningShade = 376, // player
        ImmolatingShade = 161, // player
        DoubleMeteor = 473, // player
        DragonsDescent = 474, // player
        Cauterize = 1, // player
        AbyssalQuasar = 347, // player
        VoidBlizzard = 318, // player - also VoidTornado
        VoidAero = 451, // player
    };
}

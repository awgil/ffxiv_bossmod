namespace BossMod.Endwalker.Alliance.A24Menphina
{
    public enum OID : uint
    {
        Boss = 0x3D70, // R6.016-11.501, x1
        Helper = 0x233C, // R0.500, x39
        CeremonialPillar = 0x3E79, // R2.000, x4
        IceSprite = 0x3E74, // R0.720, spawn during fight
    };

    public enum AID : uint
    {
        AutoAttack = 31748, // Boss->player, no cast, single-target
        Teleport = 31699, // Boss->location, no cast, single-target
        BlueMoonNormal = 31738, // Boss->self, 5.0s cast, single-target, visual (raidwide while not mounted)
        BlueMoonMounted = 31739, // Boss->self, 5.0s cast, single-target, visual (raidwide while mounted)
        BlueMoonAOE = 31740, // Helper->self, no cast, range 60 circle, raidwide
        LunarKissNormal = 31735, // Boss->self, 7.0s cast, single-target, visual (triple tankbuster, while not mounted)
        LunarKissMounted = 31736, // Boss->self, 7.0s cast, single-target, visual (triple tankbuster, while mounted)
        LunarKissAOE = 31737, // Helper->self, no cast, range 60 width 6 rect tankbuster

        LovesLightNormalOne = 31672, // Boss->self, 3.0s cast, single-target, visual (spawn moon outside arena, while not mounted)
        LovesLightNormalFour = 31673, // Boss->self, 3.0s cast, single-target, visual (spawn 4 moons inside arena, while not mounted)
        LovesLightMountedOne = 31682, // Boss->self, 3.0s cast, single-target, visual (spawn moon outside arena, while mounted)
        LovesLightMountedFour = 31683, // Boss->self, 3.0s cast, single-target, visual (spawn 4 moons inside arena, while mounted)
        FullBrightNormal = 31675, // Boss->self, 3.0s cast, single-target, visual (wax moon, while not mounted)
        FullBrightMounted = 31685, // Boss->self, 3.0s cast, single-target, visual (wax moon, while mounted)
        FirstBlush = 31676, // Helper->self, 10.5s cast, range 80 width 25 rect
        LoversBridgeShort = 31677, // Helper->self, 6.0s cast, range 19 circle
        LoversBridgeLong = 31678, // Helper->self, 12.0s cast, range 19 circle

        MidnightFrostShortNormalFront = 31691, // Boss->self, 6.0s cast, single-target, visual (half-arena cleave, not mounted, facing boss)
        MidnightFrostShortNormalBack = 31692, // Boss->self, 6.0s cast, single-target, visual (half-arena cleave, not mounted, behind boss)
        MidnightFrostShortMountedFront = 31695, // Boss->self, 6.0s cast, single-target, visual (half-arena cleave, mounted, facing boss)
        MidnightFrostShortMountedBack = 31696, // Boss->self, 6.0s cast, single-target, visual (half-arena cleave, mounted, behind boss)
        MidnightFrostShortNormalFrontAOE = 31693, // Helper->self, 6.2s cast, range 60 180-degree cone (facing boss)
        MidnightFrostShortNormalBackAOE = 31694, // Helper->self, 6.2s cast, range 60 180-degree cone (behind boss)
        MidnightFrostShortMountedFrontAOE = 31697, // Helper->self, 6.2s cast, range 60 180-degree cone (facing boss)
        MidnightFrostShortMountedBackAOE = 31698, // Helper->self, 6.2s cast, range 60 180-degree cone (behind boss)

        MidnightFrostLongMountedFrontRight = 31703, // Boss->self, 8.0s cast, single-target, visual (mounted 3/4 arena cleave, frost facing boss, claw right)
        MidnightFrostLongMountedFrontLeft = 31704, // Boss->self, 8.0s cast, single-target, visual (mounted 3/4 arena cleave, frost facing boss, claw left)
        MidnightFrostLongMountedBackRight = 31705, // Boss->self, 8.0s cast, single-target, visual (mounted 3/4 arena cleave, frost behind boss, claw right)
        MidnightFrostLongMountedBackLeft = 31706, // Boss->self, 8.0s cast, single-target, visual (mounted 3/4 arena cleave, frost behind boss, claw left)
        MidnightFrostLongDismounted1FrontRight = 31716, // Boss->self, 8.0s cast, single-target, visual (dismounted (1) 7/8 arena cleave; frost facing boss, claw right)
        MidnightFrostLongDismounted1FrontLeft = 31717, // Boss->self, 8.0s cast, single-target, visual (dismounted (1) 7/8 arena cleave; frost facing boss, claw left)
        MidnightFrostLongDismounted1BackRight = 31718, // Boss->self, 8.0s cast, single-target, visual (dismounted (1) 7/8 arena cleave; frost behind boss, claw right)
        MidnightFrostLongDismounted1BackLeft = 31719, // Boss->self, 8.0s cast, single-target, visual (dismounted (1) 7/8 arena cleave; frost behind boss, claw left)
        MidnightFrostLongDismounted2FrontRight = 32522, // Boss->self, 8.0s cast, single-target, visual (dismounted (2) 7/8 arena cleave; frost facing boss, claw right)
        MidnightFrostLongDismounted2FrontLeft = 32523, // Boss->self, 8.0s cast, single-target, visual (dismounted (2) 7/8 arena cleave; frost facing boss, claw left)
        MidnightFrostLongDismounted2BackRight = 32524, // Boss->self, 8.0s cast, single-target, visual (dismounted (2) 7/8 arena cleave; frost behind boss, claw right)
        MidnightFrostLongDismounted2BackLeft = 32525, // Boss->self, 8.0s cast, single-target, visual (dismounted (2) 7/8 arena cleave; frost behind boss, claw left)
        MidnightFrostLongMountedFrontAOE = 31709, // Helper->self, 8.2s cast, range 60 180-degree cone (mounted, facing boss)
        MidnightFrostLongMountedBackAOE = 31710, // Helper->self, 8.2s cast, range 60 180-degree cone (mounted, behind boss)
        MidnightFrostLongDismountedFrontAOE = 31722, // Helper->self, 8.2s cast, range 60 180-degree cone (dismounted, facing boss)
        MidnightFrostLongDismountedBackAOE = 31723, // Helper->self, 8.2s cast, range 60 180-degree cone (dismounted, behind boss)
        WaxingClawRight = 31712, // Helper->self, 8.2s cast, range 60 180-degree cone
        WaxingClawLeft = 31713, // Helper->self, 8.2s cast, range 60 180-degree cone (left)
        PlayfulOrbit1 = 31714, // Boss->self, 2.6s cast, single-target, visual (dismount for double midnight frost)
        PlayfulOrbit2 = 31715, // Boss->self, 2.6s cast, single-target, visual (dismount for double midnight frost)
        Remount1 = 31750, // Boss->self, no cast, single-target, visual (end playful orbit 1)
        Remount2 = 31751, // Boss->self, no cast, single-target, visual (end playful orbit 2)

        SilverMirrorNormal = 31733, // Boss->self, 4.0s cast, single-target, visual (puddles)
        SilverMirrorMounted = 31402, // Boss->self, 4.0s cast, single-target, visual (puddles)
        SilverMirrorAOE = 31734, // Helper->self, 4.0s cast, range 7 circle puddle
        Moonset = 31688, // Boss->location, 4.0s cast, single-target, jump to first aoe
        MoonsetJump = 31689, // Boss->location, no cast, single-target, jump to subsequent aoes
        MoonsetAOE = 31690, // Helper->self, 5.0s cast, range 12 circle aoe
        MoonsetRays = 33017, // Boss->self, 5.0s cast, single-target, visual (stack)
        MoonsetRaysAOE = 33018, // Helper->players, 5.0s cast, range 6 circle stack

        WinterHaloShort = 31686, // Boss->self, 5.0s cast, single-target, visual (donut)
        WinterHaloShortAOE = 31687, // Helper->self, 5.3s cast, range 10-60 donut
        WinterHaloLongMountedRight = 31707, // Boss->self, 8.0s cast, single-target, visual (donut, claw right)
        WinterHaloLongMountedLeft = 31708, // Boss->self, 8.0s cast, single-target, visual (donut, claw left)
        WinterHaloLongDismounted1Right = 31720, // Boss->self, 8.0s cast, single-target, visual (dismounted (1) donut, claw right)
        WinterHaloLongDismounted1Left = 31721, // Boss->self, 8.0s cast, single-target, visual (dismounted (1) donut, claw left)
        WinterHaloLongDismounted2Right = 32526, // Boss->self, 8.0s cast, single-target, visual (dismounted (2) donut, claw right)
        WinterHaloLongDismounted2Left = 32527, // Boss->self, 8.0s cast, single-target, visual (dismounted (2) donut, claw left)
        WinterHaloLongMountedAOE = 31711, // Helper->self, 8.2s cast, range 10-60 donut
        WinterHaloLongDismountedAOE = 31724, // Helper->self, 8.2s cast, range 10-60 donut

        SelenainMysteria = 31420, // Boss->self, 3.0s cast, single-target, visual (add phase start)
        Recharge = 32053, // CeremonialPillar->Boss, no cast, single-target, visual (increase boss gauge)
        AncientBlizzard = 32870, // IceSprite->self, 4.0s cast, range 45 45-degree cone
        RiseOfTheTwinMoons = 32868, // Helper->self, 11.0s cast, range 60 circle, raidwide
        KeenMoonbeam = 31731, // Boss->self, 3.0s cast, single-target, visual (spreads)
        KeenMoonbeamAOE = 31732, // Helper->players, 5.0s cast, range 6 circle spread

        CrateringChill = 31726, // Boss->self, 3.0s cast, single-target, visual (two proximity aoes)
        CrateringChillAOE = 31727, // Helper->self, 6.0s cast, range 60 circle with 20 falloff
        WinterSolstice = 31725, // Boss->self, 3.0s cast, single-target, visual (slippery floor)
    };

    public enum IconID : uint
    {
        LunarKiss = 412, // player
        KeenMoonbeam = 139, // player
        MoonsetRays = 62, // player
    };

    public enum TetherID : uint
    {
        CeremonialPillar = 14, // CeremonialPillar->Boss
    };
}

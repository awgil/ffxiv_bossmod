namespace BossMod.Endwalker.Ultimate.TOP
{
    public enum OID : uint
    {
        Boss = 0x3D5C, // R12.006, x1
        Helper = 0x233C, // R0.500, x36
        OpticalUnit = 0x3D64, // R0.500, x1
        Tower1 = 0x1EB83C, // R0.500, EventObj type, spawn during fight (unlike Tower2, doesn't get eobjstate events on enter/exit)
        Tower2 = 0x1EB83D, // R0.500, EventObj type, spawn during fight
        OmegaM = 0x3D60, // R3.000-5.010, spawn during fight (starts as M, turns into F)
        OmegaF = 0x3D61, // R3.000-5.010, spawn during fight (starts as F, turns into M)
        OmegaMHelper = 0x3D62, // R5.010, spawn during fight (never targetable)
        OmegaFHelper = 0x3D63, // R5.010, spawn during fight (never targetable)
        BossP3 = 0x3D65, // R12.502, spawn during fight
        LeftArmUnit = 0x3D66, // R1.680, spawn during fight
        RightArmUnit = 0x3D67, // R1.680, spawn during fight
        P3IntermissionVoidzone = 0x1EB821, // R0.500, EventObj type, spawn during fight
        OmegaMP5 = 0x3D68, // R5.010, spawn during fight
        OmegaFP5 = 0x3D6A, // R5.010, spawn during fight
        //_Gen_RearPowerUnit = 0x3D6B, // R6.720, spawn during fight
        //_Gen_Omega = 0x3D6C, // R12.006, spawn during fight
        //_Gen_Omega = 0x394D, // R12.502, spawn during fight

        //_Gen_Actor1ea1a1 = 0x1EA1A1, // R2.000, x1, EventObj type
        //_Gen_Exit = 0x1E850B, // R0.500, x1, EventObj type

    };

    public enum AID : uint
    {
        // p1
        AutoAttackP1 = 31741, // Boss->player, no cast, single-target
        ProgramLoop = 31491, // Boss->self, 4.0s cast, single-target, visual (first mechanic start)
        StorageViolation = 31492, // Helper->self, no cast, range 3 circle tower
        StorageViolationObliteration = 31494, // Helper->self, no cast, range 100 circle if tower is unsoaked
        Blaster = 31495, // Boss->self, 7.9s cast, single-target, visual (first tethers explosion)
        BlasterRepeat = 31496, // Boss->self, no cast, single-target, visual (1-3 tether explosion)
        BlasterLast = 31497, // Boss->self, no cast, single-target, visual (4 tether explosion)
        BlasterAOE = 31498, // Helper->players, no cast, range 15 circle aoe around tether target
        Pantokrator = 31499, // Boss->self, 5.0s cast, single-target, visual (second mechanic start)
        BallisticImpact = 31500, // Helper->location, 3.0s cast, range 5 circle baited puddle
        FlameThrowerFirst = 31501, // Helper->self, 4.0s cast, range 65 60-degree cone
        FlameThrowerRest = 32368, // Helper->self, 4.0s cast, range 65 60-degree cone
        GuidedMissileKyrios = 31502, // Helper->player, no cast, range 5 circle aoe
        CondensedWaveCannonKyrios = 31503, // Helper->self, no cast, range 50 width 6 rect shared aoe
        DiffuseWaveCannonKyrios = 31504, // Helper->self, no cast, range 60 ?-degree cone tankbuster baited on 2 furthest targets
        WaveCannonKyrios = 31505, // Helper->self, no cast, range 50 width 6 rect aoe
        AtomicRay = 31480, // Boss->self, 5.0s cast, range 100 circle, enrage

        // p2
        AutoAttackM = 31742, // OmegaM->player, no cast, single-target
        AutoAttackF = 31743, // OmegaF->player, no cast, single-target
        FirewallM = 31552, // OmegaM->self, 3.0s cast, range 100 circle, visual (filter preventing damage from furthest 4 players)
        FirewallF = 31553, // OmegaF->self, 3.0s cast, range 100 circle, visual (filter preventing damage from furthest 4 players)
        SolarRayM = 32362, // OmegaM->player, 5.0s cast, range 5 circle, tankbuster
        SolarRayF = 32363, // OmegaF->player, 5.0s cast, range 5 circle, tankbuster
        PartySynergyM = 31550, // OmegaM->self, 3.0s cast, single-target, visual (big mechanic)
        PartySynergyF = 31551, // OmegaF->self, 3.0s cast, single-target, visual (big mechanic)
        SubjectSimulationF = 31515, // OmegaM->self, no cast, single-target, applies 'superfluid' buff
        SubjectSimulationM = 31516, // OmegaF->self, no cast, single-target, applies 'superfluid' buff
        BeyondStrength = 31525, // OmegaMHelper->self, 1.5s cast, range 10-40 donut aoe
        EfficientBladework = 31526, // OmegaMHelper/OmegaF->self, 1.5s cast, range 10 circle aoe
        SuperliminalSteel = 31530, // OmegaFHelper->self, 1.5s cast, single-target, visual (side cleaves)
        SuperliminalSteelL = 31531, // Helper->self, 1.5s cast, range 80 width 36 rect aoe (left side cleave)
        SuperliminalSteelR = 31532, // Helper->self, 1.5s cast, range 80 width 36 rect aoe (right side cleave)
        OptimizedBlizzard = 31533, // OmegaFHelper->self, 1.5s cast, range 100 width 10 cross aoe
        OptimizedFire = 31535, // Helper->players, no cast, range 7 circle spread
        OpticalLaser = 31521, // OpticalUnit->self, 1.3s cast, range 100 width 16 rect
        Discharger = 31534, // OmegaM->self, no cast, range 100 circle knockback 13
        Spotlight = 31536, // Helper->players, no cast, range 6 circle stack
        AnimationGrayishM = 31508, // Helper->self, no cast, single-target, visual (become 'grayish', model state 6)
        AnimationGrayishF = 31509, // Helper->self, no cast, single-target, visual (become 'grayish', model state 6)
        AnimationDisappearM = 31510, // OmegaM->self, no cast, single-target, visual (sink down, model state 11)
        AnimationDisappearF = 31511, // OmegaF->self, no cast, single-target, visual (sink down, model state 11)
        AnimationSwapFM = 31517, // OmegaF->self, no cast, single-target, visual (F becomes M)
        AnimationSwapMF = 31518, // OmegaM->self, no cast, single-target, visual (M becomes F)
        AnimationReappearM = 31519, // OmegaF->self, no cast, single-target, visual (reappear from ground, model state 11)
        AnimationReappearF = 31520, // OmegaM->self, no cast, single-target, visual (reappear from ground, model state 11)
        SyntheticShield = 32369, // OmegaF/OmegaMHelper->self, 1.0s cast, single-target, visual (equip shield)
        LimitlessSynergyM = 31544, // OmegaF->self, 5.0s cast, single-target, visual (?)
        LimitlessSynergyF = 31545, // OmegaM->self, 5.0s cast, single-target, visual (?)
        TeleportM = 31554, // OmegaF->location, no cast, ???
        TeleportF = 31555, // OmegaM->location, no cast, ???
        OptimizedPassageOfArms = 31556, // OmegaF->self, no cast, single-target, visual (invincibility)
        OptimizedSagittariusArrow = 31539, // OmegaMHelper->self, 8.0s cast, range 100 width 10 rect
        OptimizedBladedanceM = 31540, // OmegaMHelper->self, 7.9s cast, single-target, visual (tethered cone tankbuster)
        OptimizedBladedanceF = 31541, // OmegaFHelper->self, 7.9s cast, single-target, visual (tethered cone tankbuster)
        OptimizedBladedanceMVisual = 31542, // OmegaMHelper->self, no cast, range 100 ?-degree cone, visual
        OptimizedBladedanceFVisual = 31543, // OmegaFHelper->self, no cast, range 100 ?-degree cone, visual
        OptimizedBladedanceAOE = 32629, // Helper->self, no cast, range 100 90?-degree cone
        BeyondDefense = 31527, // OmegaMHelper->self, 4.9s cast, single-target, visual (jump on one of two closest)
        BeyondDefenseAOE = 31528, // OmegaMHelper->player, no cast, range 5 circle
        PilePitch = 31529, // OmegaMHelper->players, no cast, range 6 circle stack
        OptimizedMeteor = 31537, // OmegaFHelper->self, 8.0s cast, single-target, visual (flares)
        OptimizedMeteorAOE = 31538, // Helper->players, no cast, range 100 circle raidwide with ? falloff
        CosmoMemoryM = 31522, // OmegaMHelper->self, 5.0s cast, single-target, visual (raidwide)
        CosmoMemoryF = 31523, // OmegaFHelper->self, 5.0s cast, single-target, visual (raidwide)
        CosmoMemoryAOE = 31524, // OpticalUnit->self, 1.5s cast, range 100 circle, raidwide
        LaserShower = 31557, // OmegaM->self, 60.0s cast, range 100 circle, enrage
        DieM = 31506, // OmegaF->self, no cast, single-target, visual (death)
        DieF = 31507, // OmegaM->self, no cast, single-target, visual (death)
        IntermissionTeleportM = 31561, // OmegaM->location, no cast, ???, visual
        IntermissionTeleportF = 31562, // OmegaF->location, no cast, single-target, visual
        IntermissionMergeStart = 31563, // OmegaM->self, no cast, single-target, visual
        IntermissionMergeEnd = 31564, // BossP3->self, no cast, single-target

        // p3
        AutoAttackP3 = 31744, // BossP3->player, no cast, single-target
        TeleportP3 = 31558, // BossP3->location, no cast, ???
        WaveRepeater1 = 31567, // Helper->location, 5.0s cast, range 6 circle
        WaveRepeater2 = 31568, // Helper->self, no cast, range 6-12 donut
        WaveRepeater3 = 31569, // Helper->self, no cast, range 12-18 donut
        WaveRepeater4 = 31570, // Helper->self, no cast, range 18-24 donut
        SniperCannon = 31571, // Helper->players, no cast, range 6 circle spread
        HighPoweredSniperCannon = 31572, // Helper->players, no cast, range 6 circle 2-man stack
        ColossalBlow = 31566, // LeftArmUnit/RightArmUnit->self, 2.0s cast, range 11 circle
        HelloWorld = 31573, // BossP3->self, 5.0s cast, range 100 circle, raidwide + mechanic start
        LatentDefect = 31599, // BossP3->self, 9.0s cast, single-target, visual (towers)
        HWRedTowerExpireWipe = 31581, // Helper->self, no cast, range 100 circle - 'cascading latent defect', wipe if HWRedTower debuff expires without being cleansed by rot explosion
        HWBlueTowerExpireWipe = 31582, // Helper->self, no cast, range 100 circle - 'latent performance defect', wipe if HWBlueTower debuff expires without being cleansed by rot explosion
        HWRedTower = 31583, // Helper->self, 10.0s cast, range 6 circle - 'cascading latent defect', red tower
        HWBlueTower = 31584, // Helper->self, 10.0s cast, range 6 circle - 'latent performance defect', blue tower
        HWRedTowerUnsoakedWipe = 31585, // Helper->self, no cast, range 100 circle - 'cascading latent defect', wipe if red tower was not soaked
        HWBlueTowerUnsoakedWipe = 31586, // Helper->self, no cast, range 100 circle - 'latent performance defect', wipe if blue tower was not soaked
        HWStack = 31574, // Helper->players, no cast, range 5 circle - 'critical synchronization bug', 2-man stack
        HWDefamation = 31575, // Helper->players, no cast, range 20 circle - 'critical overflow bug', defamation
        HWDefamationExpireFail = 31577, // Helper->player, no cast, single-target - damage-down if defamation was not soaked in time
        HWRedRot = 31578, // Helper->players, no cast, range 5 circle - 'critical underflow bug' - aoe on red rot expiration
        HWBlueRot = 31579, // Helper->player, no cast, range 5 circle - 'critical performance bug' - aoe on blue rot expiration
        HWTetherBreak = 31587, // Helper->self, no cast, range 100 circle - 'patch' - raidwide on tether break (remote or any?)
        HWTetherFail = 32505, // Helper->self, no cast, range 100 circle - 'patch' - wipe if tether wasn't broken
        CriticalError = 31588, // BossP3->self, 8.0s cast, range 100 circle, raidwide
        OversampledWaveCannonR = 31595, // BossP3->self, 10.0s cast, single-target, visual (monitors cleaving right side)
        OversampledWaveCannonL = 31596, // BossP3->self, 10.0s cast, single-target, visual (monitors cleaving left side)
        OversampledWaveCannonAOE = 31597, // Helper->players, no cast, range 7 circle spread
        IonEfflux = 31560, // BossP3->self, 10.0s cast, range 100 circle enrage

        // p4
        P3End = 31559, // BossP3->self, no cast, single-target, visual (right after p3 end)
        P4Begin = 31610, // BossP3->self, no cast, single-target, visual (right before p4 begin)
        P4WaveCannonStackTarget = 22393, // Helper->player, no cast, single-target, visual (line stack marker)
        P4WaveCannonProtean = 31614, // Helper->self, no cast, range 100 width 6 rect, first immediate protean
        P4WaveCannonStack = 31615, // Helper->self, no cast, range 100 width 6 rect
        P4WaveCannonProteanAOE = 31616, // Helper->self, 5.3s cast, range 100 width 6 rect, second baited protean
        P4WaveCannonVisualStart = 31617, // BossP3->self, 5.0s cast, single-target, visual
        P4WaveCannonVisual1 = 31618, // BossP3->self, no cast, single-target, visual
        P4WaveCannonVisual2 = 32534, // BossP3->self, no cast, single-target, visual
        P4WaveCannonVisual3 = 31619, // BossP3->self, no cast, single-target, visual
        P4WaveCannonVisual4 = 31620, // BossP3->self, no cast, single-target, visual
        BlueScreen = 31611, // BossP3->self, 8.0s cast, single-target, visual (enrage)
        BlueScreenAOE = 31612, // Helper->self, 1.0s cast, range 100 circle, raidwide
        BlueScreenFail = 31613, // Helper->self, 1.0s cast, range 100 circle, wipe if hp is high enough

        // p5
        AutoAttackP5 = 31745, // OmegaMP5->player, no cast, single-target
        P5AppearM = 31621, // OmegaMP5->self, no cast, single-target, visual (appear)
        P5AppearF = 31622, // OmegaFP5->self, no cast, single-target, visual (appear)
        P5SolarRay = 33196, // OmegaMP5->player, 5.0s cast, range 5 circle tankbuster
        P5SolarRaySecond = 31489, // OmegaMP5->player, no cast, range 5 circle tankbuster second hit
        RunMiDeltaVersion = 31624, // OmegaMP5->self, 5.0s cast, range 100 circle, raidwide
        //_Ability_PeripheralSynthesis = 31628, // 3D6C->self, no cast, single-target
        //_Ability_ArchivePeripheral = 32630, // 394D->self, no cast, single-target
        //_Ability_Explosion = 31482, // 3D5D/3D5E->location, 3.0s cast, range 3 circle
        //_Ability_UnmitigatedExplosion = 31483, // 3D5E/3D5D->location, 3.0s cast, range 100 circle
        //_Ability_HyperPulse = 31600, // RightArmUnit/LeftArmUnit->self, 2.5s cast, range 100 width 8 rect
        //_Ability_HyperPulse = 31601, // RightArmUnit/LeftArmUnit->self, no cast, range 100 width 8 rect
        //_Ability_Hello, World = 31627, // Helper->self, no cast, range 100 circle
        //_Ability_OversampledWaveCannon = 31639, // 394D->self, 10.0s cast, single-target
    };

    public enum SID : uint
    {
        InLine1 = 3004, // none->player, extra=0x0
        InLine2 = 3005, // none->player, extra=0x0
        InLine3 = 3006, // none->player, extra=0x0
        InLine4 = 3451, // none->player, extra=0x0
        MidGlitch = 3427, // none->player, extra=0x0
        RemoteGlitch = 3428, // none->player, extra=0x0
        SniperCannonFodder = 3425, // none->player, extra=0x0
        HighPoweredSniperCannonFodder = 3426, // none->player, extra=0x0

        // hello world: all 'prep' statuses are replaced with actual statuses on expiration
        HWPrepStack = 3436, // none->player, extra=0x0 - 'synchronization code smell', stack preparation
        HWPrepDefamation = 3437, // none->player, extra=0x0 - 'overflow code smell', defamation preparation
        HWPrepRedRot = 3438, // none->player, extra=0x0 - 'underflow code smell', red rot preparation
        HWPrepBlueRot = 3439, // none->player, extra=0x0 - 'performance code smell', blue rot preparation
        HWPrepLocalTether = 3503, // none->player, extra=0x0 - 'local code smell', tether broken by moving close preparation
        HWPrepRemoteTether = 3441, // none->player, extra=0x0 - 'remote code smell', tether broken by moving away preparation
        HWStack = 3524, // none->player, extra=0x0 - 'critical synchronization bug', stack
        HWDefamation = 3525, // none->player, extra=0x0 - 'critical overflow bug', defamation
        HWRedRot = 3526, // none->player, extra=0x0 - 'critical underflow bug', red rot
        HWBlueRot = 3429, // none->player, extra=0x0 - 'critical performance bug', blue rot
        HWLocalTether = 3529, // none->player, extra=0x0 - 'local regression', tether broken by moving close
        HWRemoteTether = 3530, // none->player, extra=0x0 - 'remote regression', tether broken by moving away
        HWNeedDefamation = 3527, // none->player, extra=0x0 - 'latent defect', applied to first pair with 'local' tether, kills on expire, cleansed by defamation
        HWNeedStack = 3434, // none->player, extra=0x0 - 'latent synchronization bug', applied to everyone, cleansed (converted to stack) by sharing a stack, kills on expire
        HWRedTower = 3528, // Helper->player, extra=0x0 - 'cascading latent defect', applied by soaking red tower, prevents death by red rot expiration
        HWBlueTower = 3435, // Helper->player, extra=0x0 - 'latent performance defect', applied by soaking blue tower, prevents death by blue rot expiration
        HWImmuneStack = 3430, // none->player, extra=0x0 - 'synchronization debugger', replaces stack after resolve
        HWImmuneDefamation = 3431, // none->player, extra=0x0 - 'overflow debugger', replaces defamation after resolve
        HWImmuneRedRot = 3432, // none->player, extra=0x0 - 'underflow debugger', replaces red rot after resolve
        HWImmuneBlueRot = 3433, // none->player, extra=0x0 - 'performance debugger', replaces blue rot after resolve
        OversampledWaveCannonLoadingR = 3452, // none->player, extra=0x0, cleaves right side
        OversampledWaveCannonLoadingL = 3453, // none->player, extra=0x0, cleaves left side
    };

    public enum IconID : uint
    {
        WaveCannonKyrios = 23, // player
        SolarRay = 343, // player
        PartySynergyCircle = 416, // player
        PartySynergyTriangle = 417, // player
        PartySynergySquare = 418, // player
        PartySynergyCross = 419, // player
        Spotlight = 100, // player
        OptimizedMeteor = 346, // player
    };

    public enum TetherID : uint
    {
        Blaster = 89, // player->Boss
        PartySynergy = 222, // player->player
        OptimizedBladedance = 84, // OmegaFHelper/OmegaMHelper->player
        HWPrepLocalTether = 200, // player->player - tether broken by moving close (preparation)
        HWPrepRemoteTether = 201, // player->player - tether broken by moving away (preparation)
        HWLocalTether = 224, // player->player - tether broken by moving close
        HWRemoteTether = 225, // player->player - tether broken by moving away
    };
}

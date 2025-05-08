namespace BossMod.DRG;

public enum AID : uint
{
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    DragonsongDive = 4242, // LB3, 4.5s cast (0 charges), range 8, single-target, targets=Hostile, animLock=3.700s?
    TrueThrust = 75, // L1, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    VorpalThrust = 78, // L4, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    LifeSurge = 83, // L6, instant, 40.0s CD (group 14/70) (1-2 charges), range 0, single-target, targets=Self
    PiercingTalon = 90, // L15, instant, GCD (0 charges), range 20, single-target, targets=Hostile
    Disembowel = 87, // L18, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    FullThrust = 84, // L26, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    LanceCharge = 85, // L30, instant, 60.0s CD (group 9) (0 charges), range 0, single-target, targets=Self
    Jump = 92, // L30, instant, 30.0s CD (group 7) (0 charges), range 20, single-target, targets=Hostile, animLock=0.800s?
    ElusiveJump = 94, // L35, instant, 30.0s CD (group 5) (0 charges), range 0, single-target, targets=Self, animLock=0.800s?
    DoomSpike = 86, // L40, instant, GCD (0 charges), range 10, AOE 10+R width 4 rect, targets=Hostile
    WingedGlide = 36951, // L45, instant, 60.0s CD (group 19/71), range 20, single-target, targets=Hostile, animLock=???
    ChaosThrust = 88, // L50, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    DragonfireDive = 96, // L50, instant, 120.0s CD (group 20) (0 charges), range 20, AOE 5 circle, targets=Hostile, animLock=0.800s?
    BattleLitany = 3557, // L52, instant, 120.0s CD (group 21) (0 charges), range 0, AOE 30 circle, targets=Self
    FangAndClaw = 3554, // L56, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    WheelingThrust = 3556, // L58, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    Geirskogul = 3555, // L60, instant, 60.0s CD (group 10) (0 charges), range 15, AOE 15+R width 4 rect, targets=Hostile
    SonicThrust = 7397, // L62, instant, GCD (0 charges), range 10, AOE 10+R width 4 rect, targets=Hostile
    Drakesbane = 36952, // L64, instant, GCD (0 charges), range 3, single-target, targets=Hostile, animLock=???
    MirageDive = 7399, // L68, instant, 1.0s CD (group 0) (0 charges), range 20, single-target, targets=Hostile
    Nastrond = 7400, // L70, instant, 2.0s CD (group 3) (0 charges), range 15, AOE 15+R width 4 rect, targets=Hostile
    CoerthanTorment = 16477, // L72, instant, GCD (0 charges), range 10, AOE 10+R width 4 rect, targets=Hostile
    HighJump = 16478, // L74, instant, 30.0s CD (group 7) (0 charges), range 20, single-target, targets=Hostile, animLock=0.800s?
    RaidenThrust = 16479, // L76, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    Stardiver = 16480, // L80, instant, 30.0s CD (group 6) (0 charges), range 20, AOE 5 circle, targets=Hostile, animLock=1.500s?
    DraconianFury = 25770, // L82, instant, GCD (0 charges), range 10, AOE 10+R width 4 rect, targets=Hostile
    ChaoticSpring = 25772, // L86, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    HeavensThrust = 25771, // L86, instant, GCD (0 charges), range 3, single-target, targets=Hostile
    WyrmwindThrust = 25773, // L90, instant, 10.0s CD (group 4) (0 charges), range 15, AOE 15+R width 4 rect, targets=Hostile
    RiseOfTheDragon = 36953, // L92, instant, 1.0s CD (group 1) (0 charges), range 20, AOE 5 circle, targets=Hostile, animLock=???
    LanceBarrage = 36954, // L96, instant, GCD (0 charges), range 3, single-target, targets=Hostile, animLock=???
    SpiralBlow = 36955, // L96, instant, GCD (0 charges), range 3, single-target, targets=Hostile, animLock=???
    Starcross = 36956, // L100, instant, 1.0s CD (group 2) (0 charges), range 3, AOE 5 circle, targets=Hostile, animLock=???

    // Shared
    Braver = ClassShared.AID.Braver, // LB1, 2.0s cast (0 charges), range 8, single-target, targets=Hostile, animLock=3.860s?
    Bladedance = ClassShared.AID.Bladedance, // LB2, 3.0s cast (0 charges), range 8, single-target, targets=Hostile, animLock=3.860s?
    SecondWind = ClassShared.AID.SecondWind, // L8, instant, 120.0s CD (group 49) (0 charges), range 0, single-target, targets=Self
    LegSweep = ClassShared.AID.LegSweep, // L10, instant, 40.0s CD (group 43) (0 charges), range 3, single-target, targets=Hostile
    Bloodbath = ClassShared.AID.Bloodbath, // L12, instant, 90.0s CD (group 46) (0 charges), range 0, single-target, targets=Self
    Feint = ClassShared.AID.Feint, // L22, instant, 90.0s CD (group 47) (0 charges), range 10, single-target, targets=Hostile
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48) (0 charges), range 0, single-target, targets=Self
    TrueNorth = ClassShared.AID.TrueNorth, // L50, instant, 45.0s CD (group 45/50) (2 charges), range 0, single-target, targets=Self
}

public enum TraitID : uint
{
    None = 0,
    BloodOfTheDragon = 434, // L54
    LifeOfTheDragon = 163, // L70
    JumpMastery = 275, // L74
    LanceMastery2 = 247, // L76
    EnhancedCoerthanTorment = 435, // L82
    EnhancedWingedGlide = 580, // L84
    LanceMastery3 = 437, // L86
    EnhancedLifeSurge = 438, // L88
    LanceMastery4 = 508, // L90
    EnhancedDragonfireDive = 581, // L92
    EnhancedSecondWind = 642, // L94
    MeleeMastery = 653, // L94
    LanceMasteryIV = 582, // L96
    EnhancedFeint = 641, // L98
    EnhancedStardiver = 583, // L100
}

// TODO: regenerate
public enum SID : uint
{
    None = 0,
    LanceCharge = 1864, // applied by Lance Charge to self, damage buff
    LifeSurge = 116, // applied by Life Surge to self, forced crit for next gcd
    BattleLitany = 786, // applied by Battle Litany to self
    PowerSurge = 2720, // applied by Disembowel & Sonic Thrust to self, damage buff
    ChaosThrust = 118, // applied by Chaos Thrust to target, dot
    ChaoticSpring = 2719, // applied by Chaotic Spring to target, dot
    FangAndClawBared = 802, // applied by Full Thrust to self
    WheelInMotion = 803, // applied by Chaos Thrust to self
    DraconianFire = 1863, // applied by Fang and Claw, Wheeling Thrust to self
    RightEye = 1910, // applied by Dragon Sight to self
    DiveReady = 1243, // applied by Jump to self
    Bloodbath = 84, // applied by Bloodbath to self, lifesteal
    Stun = 2, // applied by Leg Sweep to target
    NastrondReady = 3844, // applied by Geirskogul to self
    DragonsFlight = 3845, // applied by Dragonfire Dive to self
    StarcrossReady = 3846, // applied by Stardiver to self
    EnhancedPiercingTalon = 1870, // applied by Elusive Jump to self

    //Shared
    Feint = ClassShared.SID.Feint, // applied by Feint to target
    TrueNorth = ClassShared.SID.TrueNorth, // applied by True North to self
}

public sealed class Definitions : IDisposable
{
    private readonly DRGConfig _config = Service.Config.Get<DRGConfig>();

    public Definitions(ActionDefinitions d)
    {
        d.RegisterSpell(AID.DragonsongDive, castAnimLock: 3.70f); // animLock=3.700s?
        d.RegisterSpell(AID.TrueThrust);
        d.RegisterSpell(AID.VorpalThrust);
        d.RegisterSpell(AID.LifeSurge);
        d.RegisterSpell(AID.PiercingTalon);
        d.RegisterSpell(AID.Disembowel);
        d.RegisterSpell(AID.FullThrust);
        d.RegisterSpell(AID.LanceCharge);
        d.RegisterSpell(AID.Jump, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.ElusiveJump, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.DoomSpike);
        d.RegisterSpell(AID.WingedGlide); // animLock=???
        d.RegisterSpell(AID.ChaosThrust);
        d.RegisterSpell(AID.DragonfireDive, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.BattleLitany);
        d.RegisterSpell(AID.FangAndClaw);
        d.RegisterSpell(AID.WheelingThrust);
        d.RegisterSpell(AID.Geirskogul);
        d.RegisterSpell(AID.SonicThrust);
        d.RegisterSpell(AID.Drakesbane); // animLock=???
        d.RegisterSpell(AID.MirageDive);
        d.RegisterSpell(AID.Nastrond);
        d.RegisterSpell(AID.CoerthanTorment);
        d.RegisterSpell(AID.HighJump, instantAnimLock: 0.80f); // animLock=0.800s?
        d.RegisterSpell(AID.RaidenThrust);
        d.RegisterSpell(AID.Stardiver, instantAnimLock: 1.50f); // animLock=1.500s?
        d.RegisterSpell(AID.DraconianFury);
        d.RegisterSpell(AID.ChaoticSpring);
        d.RegisterSpell(AID.HeavensThrust);
        d.RegisterSpell(AID.WyrmwindThrust);
        d.RegisterSpell(AID.RiseOfTheDragon); // animLock=???
        d.RegisterSpell(AID.LanceBarrage); // animLock=???
        d.RegisterSpell(AID.SpiralBlow); // animLock=???
        d.RegisterSpell(AID.Starcross); // animLock=???

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        // hardcoded mechanics
        d.RegisterChargeIncreaseTrait(AID.LifeSurge, TraitID.EnhancedLifeSurge);
        d.RegisterChargeIncreaseTrait(AID.WingedGlide, TraitID.EnhancedWingedGlide);

        // elusive jump aiming
        d.Spell(AID.ElusiveJump)!.TransformAngle = (ws, player, _, _) => _config.ElusiveJump switch
        {
            DRGConfig.ElusiveJumpBehavior.CharacterForward => player.Rotation + 180.Degrees(),
            DRGConfig.ElusiveJumpBehavior.CameraBackward => ws.Client.CameraAzimuth + 180.Degrees(),
            DRGConfig.ElusiveJumpBehavior.CameraForward => ws.Client.CameraAzimuth,
            _ => null
        };

        // upgrades (TODO: don't think we actually care...)
        //d.Spell(AID.FullThrust)!.TransformAction = d.Spell(AID.HeavensThrust)!.TransformAction = () => ActionID.MakeSpell(_state.BestHeavensThrust);
        //d.Spell(AID.ChaosThrust)!.TransformAction = d.Spell(AID.ChaoticSpring)!.TransformAction = () => ActionID.MakeSpell(_state.BestChaoticSpring);
        //d.Spell(AID.Jump)!.TransformAction = d.Spell(AID.HighJump)!.TransformAction = () => ActionID.MakeSpell(_state.BestJump);
        //d.Spell(AID.TrueThrust)!.TransformAction = d.Spell(AID.RaidenThrust)!.TransformAction = () => ActionID.MakeSpell(_state.BestTrueThrust);
        //d.Spell(AID.DoomSpike)!.TransformAction = d.Spell(AID.DraconianFury)!.TransformAction = () => ActionID.MakeSpell(_state.BestDoomSpike);
        //d.Spell(AID.Geirskogul)!.TransformAction = d.Spell(AID.Nastrond)!.TransformAction = () => ActionID.MakeSpell(_state.BestGeirskogul);

        d.Spell(AID.Stardiver)!.ForbidExecute =
            d.Spell(AID.DragonfireDive)!.ForbidExecute =
            d.Spell(AID.WingedGlide)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;
        d.Spell(AID.ElusiveJump)!.ForbidExecute = ActionDefinitions.DashFixedDistanceCheck(15, backwards: true);
    }

    public float EffectApplicationDelay(AID aid) => aid switch
    {
        AID.ChaoticSpring => 0.45f,          //Chaotic Spring delay
        AID.HighJump => 0.49f,               //High Jump delay
        AID.CoerthanTorment => 0.49f,        //Coerthan Torment delay
        AID.BattleLitany => 0.62f,           //Battle Litany delay
        AID.LanceBarrage => 0.62f,           //Lance Barrage delay
        AID.FangAndClaw => 0.62f,            //Fang and Claw delay
        AID.RaidenThrust => 0.62f,           //Raiden Thrust delay
        AID.Geirskogul => 0.67f,             //Geirskogul delay
        AID.WheelingThrust => 0.67f,         //Wheeling Thrust delay
        AID.HeavensThrust => 0.71f,          //Heavens Thrust delay
        AID.DraconianFury => 0.76f,          //Draconian Fury delay
        AID.Nastrond => 0.76f,               //Nastrond delay
        AID.TrueThrust => 0.76f,             //True Thrust delay
        AID.DragonfireDive => 0.8f,          //Dragonfire Dive delay
        AID.MirageDive => 0.8f,              //Mirage Dive delay
        AID.SonicThrust => 0.8f,             //Sonic Thrust delay
        AID.PiercingTalon => 0.85f,          //Piercing Talon delay
        AID.Starcross => 0.98f,              //Starcross delay
        AID.VorpalThrust => 1.02f,           //Vorpal Thrust delay
        AID.RiseOfTheDragon => 1.16f,        //Rise of the Dragon delay
        AID.WyrmwindThrust => 1.2f,          //Wyrmwind Thrust delay
        AID.DoomSpike => 1.29f,              //Doom Spike delay
        AID.Stardiver => 1.29f,              //Stardiver delay
        AID.SpiralBlow => 1.38f,             //Spiral Blow delay
        AID.Disembowel => 1.65f,             //Disembowel delay
        AID.DragonsongDive => 2.23f,         //Dragonsong Dive delay
        _ => 0.0f
    };

}

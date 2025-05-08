namespace BossMod.GNB;

public enum AID : uint
{
    #region PvE
    None = 0,
    Sprint = ClassShared.AID.Sprint,

    GunmetalSoul = 17105, // LB3, instant, range 0, AOE 50 circle, targets=self, animLock=3.860
    KeenEdge = 16137, // L1, instant, GCD, range 3, single-target, targets=hostile
    NoMercy = 16138, // L2, instant, 60.0s CD (group 10), range 0, single-target, targets=self
    BrutalShell = 16139, // L4, instant, GCD, range 3, single-target, targets=hostile
    Camouflage = 16140, // L6, instant, 90.0s CD (group 15), range 0, single-target, targets=self
    DemonSlice = 16141, // L10, instant, GCD, range 0, AOE 5 circle, targets=self
    RoyalGuard = 16142, // L10, instant, 2.0s CD (group 1), range 0, single-target, targets=self
    ReleaseRoyalGuard = 32068, // L10, instant, 1.0s CD (group 1), range 0, single-target, targets=self
    LightningShot = 16143, // L15, instant, GCD, range 20, single-target, targets=hostile
    DangerZone = 16144, // L18, instant, 30.0s CD (group 4), range 3, single-target, targets=hostile
    SolidBarrel = 16145, // L26, instant, GCD, range 3, single-target, targets=hostile
    BurstStrike = 16162, // L30, instant, GCD, range 3, single-target, targets=hostile
    Nebula = 16148, // L38, instant, 120.0s CD (group 21), range 0, single-target, targets=self
    DemonSlaughter = 16149, // L40, instant, GCD, range 0, AOE 5 circle, targets=self
    Aurora = 16151, // L45, instant, 60.0s CD (group 19/71), range 30, single-target, targets=self/party/alliance/friendly
    Superbolide = 16152, // L50, instant, 360.0s CD (group 24), range 0, single-target, targets=self
    SonicBreak = 16153, // L54, instant, 60.0s CD (group 13/57), range 3, single-target, targets=hostile
    Trajectory = 36934, // L56, instant, 30.0s CD (group 9/70) (2? charges), range 20, single-target, targets=hostile
    GnashingFang = 16146, // L60, instant, 30.0s CD (group 5/57), range 3, single-target, targets=hostile, animLock=0.700
    SavageClaw = 16147, // L60, instant, GCD, range 3, single-target, targets=hostile, animLock=0.500
    WickedTalon = 16150, // L60, instant, GCD, range 3, single-target, targets=hostile, animLock=0.770
    BowShock = 16159, // L62, instant, 60.0s CD (group 11), range 0, AOE 5 circle, targets=self
    HeartOfLight = 16160, // L64, instant, 90.0s CD (group 16), range 0, AOE 30 circle, targets=self
    HeartOfStone = 16161, // L68, instant, 25.0s CD (group 3), range 30, single-target, targets=self/party
    AbdomenTear = 16157, // L70, instant, 1.0s CD (group 0), range 5, single-target, targets=hostile
    JugularRip = 16156, // L70, instant, 1.0s CD (group 0), range 5, single-target, targets=hostile
    EyeGouge = 16158, // L70, instant, 1.0s CD (group 0), range 5, single-target, targets=hostile
    Continuation = 16155, // L70, instant, 1.0s CD (group 0), range 0, single-target, targets=self, animLock=???
    FatedCircle = 16163, // L72, instant, GCD, range 0, AOE 5 circle, targets=self
    Bloodfest = 16164, // L76, instant, 120.0s CD (group 14), range 25, single-target, targets=hostile
    BlastingZone = 16165, // L80, instant, 30.0s CD (group 4), range 3, single-target, targets=hostile
    HeartOfCorundum = 25758, // L82, instant, 25.0s CD (group 3), range 30, single-target, targets=self/party
    Hypervelocity = 25759, // L86, instant, 1.0s CD (group 0), range 5, single-target, targets=hostile
    DoubleDown = 25760, // L90, instant, 60.0s CD (group 12/57), range 0, AOE 5 circle, targets=self
    GreatNebula = 36935, // L92, instant, 120.0s CD, range 0, single-target, targeets=self
    FatedBrand = 36936, // L96, instant, 1.0s CD, (group 0), range 5, AOE, targets=hostile
    ReignOfBeasts = 36937, // L100, instant, GCD, range 3, single-target, targets=hostile
    NobleBlood = 36938, // L100, instant, GCD, range 3, single-target, targets=hostile
    LionHeart = 36939, // L100, instant, GCD, range 3, single-target, targets=hostile

    // Shared
    ShieldWall = ClassShared.AID.ShieldWall, // LB1, instant, range 0, AOE 50 circle, targets=self, animLock=1.930
    Stronghold = ClassShared.AID.Stronghold, // LB2, instant, range 0, AOE 50 circle, targets=self, animLock=3.860
    Rampart = ClassShared.AID.Rampart, // L8, instant, 90.0s CD (group 46), range 0, single-target, targets=self
    LowBlow = ClassShared.AID.LowBlow, // L12, instant, 25.0s CD (group 41), range 3, single-target, targets=hostile
    Provoke = ClassShared.AID.Provoke, // L15, instant, 30.0s CD (group 42), range 25, single-target, targets=hostile
    Interject = ClassShared.AID.Interject, // L18, instant, 30.0s CD (group 43), range 3, single-target, targets=hostile
    Reprisal = ClassShared.AID.Reprisal, // L22, instant, 60.0s CD (group 44), range 0, AOE 5 circle, targets=self
    ArmsLength = ClassShared.AID.ArmsLength, // L32, instant, 120.0s CD (group 48), range 0, single-target, targets=self
    Shirk = ClassShared.AID.Shirk, // L48, instant, 120.0s CD (group 49), range 25, single-target, targets=party
    #endregion

    #region PvP
    KeenEdgePvP = 29098, // instant, GCD, range 3, single-target, targets=hostile
    BrutalShellPvP = 29099, // instant, GCD, range 3, single-target, targets=hostile
    SolidBarrelPvP = 29100, // instant, GCD, range 3, single-target, targets=hostile
    BurstStrikePvP = 29101, // instant, GCD, range 5, single-target, targets=hostile
    GnashingFangPvP = 29102, // instant, GCD, range 5, single-target, targets=hostile
    FatedCirclePvP = 41511, // instant, GCD, range 0, AOE 5, targets=self
    ContinuationPvP = 29106, // instant, oGCD, range 5, single-target, targets=hostile
    RoughDividePvP = 29123, // instant, oGCD, range 20, single-target, targets=hostile
    BlastingZonePvP = 29128, // instant, oGCD, range 5, single-target, targets=hostile
    HeartOfCorundumPvP = 41443, // instant, oGCD, range 20, single-target, targets=party
    SavageClawPvP = 29103, // instant, GCD, range 5, single-target, targets=hostile
    WickedTalonPvP = 29104, // instant, GCD, range 5, single-target, targets=hostile
    HypervelocityPvP = 29107, // instant, oGCD, range 5, single-target, targets=hostile
    FatedBrandPvP = 41442, // instant, oGCD, range 5, single-target, targets=hostile
    JugularRipPvP = 29108, // instant, oGCD, range 5, single-target, targets=hostile
    AbdomenTearPvP = 29109, // instant, oGCD, range 5, single-target, targets=hostile
    EyeGougePvP = 29110, // instant, oGCD, range 5, single-target, targets=hostile

    //LBs
    RelentlessRushPvP = 29130,
    TerminalTriggerPvP = 29131,

    //Shared
    Elixir = ClassShared.AID.Elixir,
    Recuperate = ClassShared.AID.Recuperate,
    Purify = ClassShared.AID.Purify,
    Guard = ClassShared.AID.Guard,
    SprintPvP = ClassShared.AID.SprintPvP
    #endregion
}

public enum TraitID : uint
{
    None = 0,
    TankMastery = 320, // L1
    CartridgeCharge = 257, // L30
    EnhancedBrutalShell = 258, // L52
    DangerZoneMastery = 259, // L80
    HeartOfStoneMastery = 424, // L82
    EnhancedAurora = 425, // L84
    MeleeMastery = 507, // L84
    EnhancedContinuation = 426, // L86
    CartridgeChargeII = 427, // L88
    NebulaMastery = 574, // L92
    EnhancedContinuationII = 575,// L96
    EnhancedBloodfest = 576, // L100
}

// TODO: regenerate
public enum SID : uint
{
    #region PvE
    None = 0,
    BrutalShell = 1898, // applied by Brutal Shell to self
    NoMercy = 1831, // applied by No Mercy to self
    ReadyToRip = 1842, // applied by Gnashing Fang to self
    SonicBreak = 1837, // applied by Sonic Break to target
    BowShock = 1838, // applied by Bow Shock to target
    ReadyToTear = 1843, // applied by Savage Claw to self
    ReadyToGouge = 1844, // applied by Wicked Talon to self
    ReadyToBlast = 2686, // applied by Burst Strike to self
    Nebula = 1834, // applied by Nebula to self
    Rampart = 1191, // applied by Rampart to self
    Camouflage = 1832, // applied by Camouflage to self
    ArmsLength = 1209, // applied by Arm's Length to self
    HeartOfLight = 1839, // applied by Heart of Light to self
    Aurora = 1835, // applied by Aurora to self
    Superbolide = 1836, // applied by Superbolide to self
    HeartOfCorundum = 2683, // applied by Heart of Corundum to self
    ClarityOfCorundum = 2684, // applied by Heart of Corundum to self
    CatharsisOfCorundum = 2685, // applied by Heart of Corundum to self
    RoyalGuard = 1833, // applied by Royal Guard to self
    Stun = 2, // applied by Low Blow to target
    GreatNebula = 3838, // applied by Nebula to self
    ReadyToRaze = 3839, // applied by Fated Circle to self
    ReadyToBreak = 3886, // applied by No mercy to self
    ReadyToReign = 3840, // applied by Bloodfest to target

    //Shared
    Reprisal = ClassShared.SID.Reprisal, // applied by Reprisal to target
    #endregion PvE

    #region PvP
    ReadyToBlastPvP = 3041, // applied by Burst Strike to self
    ReadyToRazePvP = 4293, // applied by Fated Circle to self
    ReadyToRipPvP = 2002, // applied by Gnashing Fang to self
    ReadyToTearPvP = 2003, // applied by Savage Claw to self
    ReadyToGougePvP = 2004, // applied by Wicked Talon to self
    NebulaPvP = 3051, // applied by Nebula to self
    NoMercyPvP = 3042, // applied by No Mercy to self
    HeartOfCorundumPvP = 4295, // applied by Heart of Corundum to self
    CatharsisOfCorundumPvP = 4296, // applied by Heart of Corundum to self
    RelentlessRushPvP = 3052,
    RelentlessShrapnelPvP = 3053,

    //Shared
    Elixir = ClassShared.AID.Elixir,
    Recuperate = ClassShared.AID.Recuperate,
    Purify = ClassShared.AID.Purify,
    Guard = ClassShared.AID.Guard,
    SprintPvP = ClassShared.AID.SprintPvP,
    Silence = ClassShared.SID.Silence,
    Bind = ClassShared.SID.Bind,
    StunPvP = ClassShared.SID.StunPvP,
    HalfAsleep = ClassShared.SID.HalfAsleep,
    Sleep = ClassShared.SID.Sleep,
    DeepFreeze = ClassShared.SID.DeepFreeze,
    Heavy = ClassShared.SID.Heavy,
    Unguarded = ClassShared.SID.Unguarded,

    #endregion
}

public sealed class Definitions : IDisposable
{
    private readonly GNBConfig _config = Service.Config.Get<GNBConfig>();

    public Definitions(ActionDefinitions d)
    {
        #region PvE
        d.RegisterSpell(AID.GunmetalSoul, instantAnimLock: 3.86f);
        d.RegisterSpell(AID.KeenEdge);
        d.RegisterSpell(AID.NoMercy);
        d.RegisterSpell(AID.BrutalShell);
        d.RegisterSpell(AID.Camouflage);
        d.RegisterSpell(AID.DemonSlice);
        d.RegisterSpell(AID.RoyalGuard);
        d.RegisterSpell(AID.ReleaseRoyalGuard);
        d.RegisterSpell(AID.LightningShot);
        d.RegisterSpell(AID.DangerZone);
        d.RegisterSpell(AID.SolidBarrel);
        d.RegisterSpell(AID.BurstStrike);
        d.RegisterSpell(AID.Nebula);
        d.RegisterSpell(AID.DemonSlaughter);
        d.RegisterSpell(AID.Aurora);
        d.RegisterSpell(AID.Superbolide);
        d.RegisterSpell(AID.SonicBreak);
        d.RegisterSpell(AID.Trajectory);
        d.RegisterSpell(AID.GnashingFang, instantAnimLock: 0.70f);
        d.RegisterSpell(AID.SavageClaw, instantAnimLock: 0.50f);
        d.RegisterSpell(AID.WickedTalon, instantAnimLock: 0.77f);
        d.RegisterSpell(AID.BowShock);
        d.RegisterSpell(AID.HeartOfLight);
        d.RegisterSpell(AID.HeartOfStone);
        d.RegisterSpell(AID.AbdomenTear);
        d.RegisterSpell(AID.JugularRip);
        d.RegisterSpell(AID.EyeGouge);
        d.RegisterSpell(AID.Continuation); // animLock=???
        d.RegisterSpell(AID.FatedCircle);
        d.RegisterSpell(AID.Bloodfest);
        d.RegisterSpell(AID.BlastingZone);
        d.RegisterSpell(AID.HeartOfCorundum);
        d.RegisterSpell(AID.Hypervelocity);
        d.RegisterSpell(AID.DoubleDown);
        d.RegisterSpell(AID.FatedBrand);
        d.RegisterSpell(AID.ReignOfBeasts);
        d.RegisterSpell(AID.NobleBlood);
        d.RegisterSpell(AID.LionHeart);
        #endregion

        #region PvP
        d.RegisterSpell(AID.KeenEdgePvP);
        d.RegisterSpell(AID.BrutalShellPvP);
        d.RegisterSpell(AID.SolidBarrelPvP);
        d.RegisterSpell(AID.BurstStrikePvP);
        d.RegisterSpell(AID.GnashingFangPvP);
        d.RegisterSpell(AID.SavageClawPvP);
        d.RegisterSpell(AID.WickedTalonPvP);
        d.RegisterSpell(AID.AbdomenTearPvP);
        d.RegisterSpell(AID.JugularRipPvP);
        d.RegisterSpell(AID.EyeGougePvP);
        d.RegisterSpell(AID.ContinuationPvP);
        d.RegisterSpell(AID.FatedCirclePvP);
        d.RegisterSpell(AID.BlastingZonePvP);
        d.RegisterSpell(AID.HeartOfCorundumPvP);
        d.RegisterSpell(AID.HypervelocityPvP);
        d.RegisterSpell(AID.FatedBrandPvP);
        d.RegisterSpell(AID.RoughDividePvP);
        d.RegisterSpell(AID.RelentlessRushPvP);
        d.RegisterSpell(AID.TerminalTriggerPvP);
        #endregion

        Customize(d);
    }

    public void Dispose() { }

    private void Customize(ActionDefinitions d)
    {
        d.RegisterChargeIncreaseTrait(AID.Aurora, TraitID.EnhancedAurora);

        d.Spell(AID.LightningShot)!.ForbidExecute = (ws, player, _, _) => _config.ForbidEarlyLightningShot && !player.InCombat && ws.Client.CountdownRemaining > 0.7f;

        d.Spell(AID.Trajectory)!.ForbidExecute = ActionDefinitions.DashToTargetCheck;

        //d.Spell(AID.Aurora)!.ForbidExecute = (_, player, _, _) => player.HPMP.CurHP >= player.HPMP.MaxHP; // don't use at full hp

        // upgrades (TODO: don't think we actually care...)
        //d.Spell(AID.RoyalGuard)!.TransformAction = d.Spell(AID.ReleaseRoyalGuard)!.TransformAction = () => ActionID.MakeSpell(_state.HaveTankStance ? AID.ReleaseRoyalGuard : AID.RoyalGuard);
        //d.Spell(AID.DangerZone)!.TransformAction = d.Spell(AID.BlastingZone)!.TransformAction = () => ActionID.MakeSpell(_state.BestZone);
        //d.Spell(AID.HeartOfStone)!.TransformAction = d.Spell(AID.HeartOfCorundum)!.TransformAction = () => ActionID.MakeSpell(_state.BestHeart);
        //d.Spell(AID.Continuation)!.TransformAction = d.Spell(AID.JugularRip)!.TransformAction = d.Spell(AID.AbdomenTear)!.TransformAction = d.Spell(AID.EyeGouge)!.TransformAction = d.Spell(AID.Hypervelocity)!.TransformAction = () => ActionID.MakeSpell(_state.BestContinuation);
        //d.Spell(AID.GnashingFang)!.TransformAction = d.Spell(AID.SavageClaw)!.TransformAction = d.Spell(AID.WickedTalon)!.TransformAction = () => ActionID.MakeSpell(_state.BestGnash);
        //d.Spell(AID.Continuation)!.Condition = _ => _state.ReadyToRip ? ActionID.MakeSpell(AID.JugularRip) : ActionID.MakeSpell(AID.None);
        //d.Spell(AID.Continuation)!.Condition = _ => _state.ReadyToTear ? ActionID.MakeSpell(AID.AbdomenTear) : ActionID.MakeSpell(AID.None);
        //d.Spell(AID.Continuation)!.Condition = _ => _state.ReadyToGouge ? ActionID.MakeSpell(AID.EyeGouge) : ActionID.MakeSpell(AID.None);
        //d.Spell(AID.Continuation)!.Condition = _ => _state.ReadyToBlast ? ActionID.MakeSpell(AID.Hypervelocity) : ActionID.MakeSpell(AID.None);
        // combo replacement (TODO: don't think we actually care...)
        //d.Spell(AID.BrutalShell)!.TransformAction = config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextBrutalShellComboAction(ComboLastMove)) : null;
        //d.Spell(AID.SolidBarrel)!.TransformAction = config.STCombos ? () => ActionID.MakeSpell(Rotation.GetNextSTComboAction(ComboLastMove, AID.SolidBarrel)) : null;
        //d.Spell(AID.DemonSlaughter)!.TransformAction = config.AOECombos ? () => ActionID.MakeSpell(Rotation.GetNextAOEComboAction(ComboLastMove)) : null;
    }
}

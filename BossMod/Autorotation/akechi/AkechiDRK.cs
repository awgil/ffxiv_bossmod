using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.DRK;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiDRK(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    //Abilities tracked for Cooldown Planner & Autorotation execution
    public enum Track
    {
        Blood                //Blood abilities tracking
        = SharedTrack.Count, //Shared tracking
        MP,                  //MP actions tracking
        Carve,               //Carve and Spit & Abyssal Drain tracking
        ScarletCombo,        //Scarlet Combo ability tracking
        Potion,              //Potion item tracking
        Unmend,              //Ranged ability tracking
        BloodWeapon,         //Blood Weapon ability tracking
        Delirium,            //Delirium ability tracking
        SaltedEarth,         //Salted Earth ability tracking
        SaltAndDarkness,     //Salt and Darkness ability tracking
        LivingShadow,        //Living Shadow ability tracking
        Shadowbringer,       //Shadowbringer ability tracking
        Disesteem            //Disesteem ability tracking
    }

    public enum BloodStrategy
    {
        Automatic,           //Automatically decide when to use Burst Strike & Fated Circle
        OnlyBloodspiller,    //Use Bloodspiller optimally as Blood spender only, regardless of targets
        OnlyQuietus,         //Use Quietus optimally as Blood spender only, regardless of targets
        ForceBloodspiller,   //Force use of Bloodspiller
        ForceQuietus,        //Force use of Quietus
        Conserve             //Conserves all Blood-related abilities as much as possible
    }
    public enum MPStrategy
    {
        Auto9k,              //Automatically decide best MP action to use; Uses when at 9000+ MP
        Auto6k,              //Automatically decide best MP action to use; Uses when at 6000+ MP
        Auto3k,              //Automatically decide best MP action to use; Uses when at 3000+ MP
        AutoRefresh,         //Automatically decide best MP action to use
        Edge9k,              //Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 9000+ MP
        Edge6k,              //Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 6000+ MP
        Edge3k,              //Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 3000+ MP
        EdgeRefresh,         //Use Edge of Shadow as Darkside refresher only
        Flood9k,             //Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 9000+ MP
        Flood6k,             //Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 6000+ MP
        Flood3k,             //Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 3000+ MP
        FloodRefresh,        //Use Flood of Shadow as Darkside refresher only
        Delay                //Delay the use of MP actions for strategic reasons
    }
    public enum CarveStrategy
    {
        Automatic,           //Automatically decide when to use either Carve and Spit or Abyssal Drain
        CarveAndSpit,        //Force use of Carve and Spit
        AbyssalDrain,        //Force use of Abyssal Drain
        Delay                //Delay the use of Carve and Spit and Abyssal Drain for strategic reasons
    }
    public enum ScarletComboStrategy
    {
        Automatic,           //Automatically decide when to use Scarlet Combo
        ScarletDelirum,      //Force use of Scarlet Delirium
        Comeuppance,         //Force use of Comeuppance
        Torcleaver,          //Force use of Torcleaver
        Impalement,          //Force use of Impalement
        Delay                //Delay the use of Scarlet Combo for strategic reasons
    }
    public enum PotionStrategy
    {
        Manual,              //Manual potion usage
        AlignWithRaidBuffs,  //Align potion usage with raid buffs
        Immediate            //Use potions immediately when available
    }
    public enum UnmendStrategy
    {
        OpenerFar,           //Only use Unmend in pre-pull & out of melee range
        OpenerForce,         //Force use Unmend in pre-pull in any range
        Force,               //Force the use of Unmend in any range
        Allow,               //Allow the use of Unmend when out of melee range
        Forbid               //Prohibit the use of Unmend
    }
    #endregion

    #region Module Definitions & Strategies
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi DRK", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)", //Category
            "Akechi", //Contributor
            RotationModuleQuality.Basic, //Quality
            BitMask.Build((int)Class.DRK), //Job
            100); //Level supported

        #region Strategies

        res.DefineShared();
        res.Define(Track.Blood).As<BloodStrategy>("Blood", "Carts", uiPriority: 180)
            .AddOption(BloodStrategy.Automatic, "Automatic", "Automatically decide when to use Blood optimally")
            .AddOption(BloodStrategy.OnlyBloodspiller, "Only Bloodspiller", "Uses Bloodspiller optimally as Blood spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(BloodStrategy.OnlyQuietus, "Only Quietus", "Uses Quietus optimally as Blood spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(BloodStrategy.ForceBloodspiller, "Force Bloodspiller", "Force use of Bloodspiller", 0, 0, ActionTargets.Hostile, 62)
            .AddOption(BloodStrategy.ForceQuietus, "Force Quietus", "Force use of Quietus", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(BloodStrategy.Conserve, "Conserve", "Conserves all Blood-related abilities as much as possible");

        res.Define(Track.MP).As<MPStrategy>("MP", "MP", uiPriority: 170)
            .AddOption(MPStrategy.Auto3k, "Auto 3k", "Automatically decide best MP action to use; Uses when at 3000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Auto6k, "Auto 6k", "Automatically decide best MP action to use; Uses when at 6000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Auto9k, "Auto 9k", "Automatically decide best MP action to use; Uses when at 9000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.AutoRefresh, "Auto Refresh", "Automatically decide best MP action to use as Darkside refresher only", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Edge3k, "Edge 3k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 3000+ MP", 0, 0, ActionTargets.Self, 40)
            .AddOption(MPStrategy.Edge6k, "Edge 6k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 6000+ MP", 0, 0, ActionTargets.Self, 40)
            .AddOption(MPStrategy.Edge9k, "Edge 9k", "Use Edge of Shadow as Darkside refresher & MP spender; Uses when at 9000+ MP", 0, 0, ActionTargets.Self, 40)
            .AddOption(MPStrategy.EdgeRefresh, "Edge Refresh", "Use Edge of Shadow as Darkside refresher only", 0, 0, ActionTargets.Self, 40)
            .AddOption(MPStrategy.Flood3k, "Flood 3k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 3000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Flood6k, "Flood 6k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 6000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Flood9k, "Flood 9k", "Use Flood of Shadow as Darkside refresher & MP spender; Uses when at 9000+ MP", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.FloodRefresh, "Flood Refresh", "Use Flood of Shadow as Darkside refresher only", 0, 0, ActionTargets.Self, 30)
            .AddOption(MPStrategy.Delay, "Delay", "Delay the use of MP actions for strategic reasons", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(AID.EdgeOfDarkness, AID.EdgeOfShadow, AID.FloodOfDarkness, AID.FloodOfShadow);

        res.Define(Track.Carve).As<CarveStrategy>("Carve", "Carve", uiPriority: 160)
            .AddOption(CarveStrategy.Automatic, "Auto", "Automatically decide when to use either Carve and Spit or Abyssal Drain")
            .AddOption(CarveStrategy.CarveAndSpit, "Carve And Spit", "Force use of Carve and Spit", 60, 0, ActionTargets.Hostile, 60)
            .AddOption(CarveStrategy.AbyssalDrain, "Abyssal Drain", "Force use of Abyssal Drain", 60, 0, ActionTargets.Hostile, 56)
            .AddOption(CarveStrategy.Delay, "Delay", "Delay the use of Carve and Spit for strategic reasons", 0, 0, ActionTargets.None, 56)
            .AddAssociatedActions(AID.CarveAndSpit);

        res.Define(Track.ScarletCombo).As<ScarletComboStrategy>("Scarlet Combo", "ScarletCombo", uiPriority: 150)
            .AddOption(ScarletComboStrategy.Automatic, "Auto", "Automatically decide when to use Scarlet Combo", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(ScarletComboStrategy.ScarletDelirum, "Scarlet Delirium", "Force use of Scarlet Delirium", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(ScarletComboStrategy.Comeuppance, "Comeuppance", "Force use of Comeuppance", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(ScarletComboStrategy.Torcleaver, "Torcleaver", "Force use of Torcleaver", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(ScarletComboStrategy.Impalement, "Impalement", "Force use of Impalement", 0, 0, ActionTargets.Hostile, 96)
            .AddOption(ScarletComboStrategy.Delay, "Delay", "Delay use of Scarlet combo for strategic reasons", 0, 0, ActionTargets.Hostile, 96)
            .AddAssociatedActions(AID.ScarletDelirium, AID.Comeuppance, AID.Torcleaver, AID.Impalement);

        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 20)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with No Mercy & Bloodfest together (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        res.Define(Track.Unmend).As<UnmendStrategy>("Unmend", "Unmend", uiPriority: 30)
            .AddOption(UnmendStrategy.OpenerFar, "Far (Opener)", "Use Unmend in pre-pull & out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.OpenerForce, "Force (Opener)", "Force use Unmend in pre-pull in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Force, "Force", "Force use Unmend in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Allow, "Allow", "Allow use of Unmend when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(UnmendStrategy.Forbid, "Forbid", "Prohibit use of Unmend")
            .AddAssociatedActions(AID.Unmend);

        res.DefineOGCD(Track.BloodWeapon, "Blood Weapon", "B.Weapon", 60, 15, ActionTargets.Self, 35, 68).AddAssociatedActions(AID.BloodWeapon);
        res.DefineOGCD(Track.Delirium, "Delirium", "Delirium", 60, 15, ActionTargets.Self, 68).AddAssociatedActions(AID.BloodWeapon);
        res.DefineOGCD(Track.SaltedEarth, "Salted Earth", "S.Earth", 90, 15, ActionTargets.Self, 52).AddAssociatedActions(AID.SaltedEarth);
        res.DefineOGCD(Track.SaltAndDarkness, "Salt & Darkness", "S&D", 20, 0, ActionTargets.Self, 86).AddAssociatedActions(AID.SaltAndDarkness);
        res.DefineOGCD(Track.LivingShadow, "Living Shadow", "L.Shadow", 120, 20, ActionTargets.Self, 80).AddAssociatedActions(AID.LivingShadow);
        res.DefineOGCD(Track.Shadowbringer, "Shadowbringer", "S.bringer", 60, 0, ActionTargets.Hostile, 90).AddAssociatedActions(AID.Shadowbringer);
        res.DefineGCD(Track.Disesteem, "Disesteem", "Disesteem", supportedTargets: ActionTargets.Hostile, minLevel: 100).AddAssociatedActions(AID.Disesteem);
        #endregion

        return res;
    }
    #endregion

    #region Priorities
    public enum GCDPriority //priorities for GCDs (higher number = higher priority)
    {
        None = 0,           //default
        Combo123 = 350,     //combo actions
        Gauge = 500,        //Blood spender actions
        Reign = 525,        //Reign of Beasts
        comboNeed = 550,    //combo actions that need to be used
        GF23 = 575,         //Gnashing combo chain
        SonicBreak = 600,   //Sonic Break
        DoubleDown = 675,   //Double Down
        GF1 = 700,          //Gnashing Fang
        ForcedGCD = 900,    //Forced GCDs
    }
    public enum OGCDPriority //priorities for oGCDs (higher number = higher priority)
    {
        None = 0,           //default
        Continuation = 500, //Continuation procs
        Zone = 550,         //Blasting Zone
        BowShock = 600,     //Bow Shock
        Bloodfest = 700,    //Bloodfest
        NoMercy = 875,      //No Mercy
        Potion = 900,       //Potion
        ForcedOGCD = 900,   //Forced oGCDs
    }
    #endregion

    #region Upgrade Paths
    private AID BestEdge
        => Unlocked(AID.EdgeOfShadow)
        ? AID.EdgeOfShadow
        : Unlocked(AID.EdgeOfDarkness)
        ? AID.EdgeOfDarkness
        : AID.FloodOfDarkness;

    private AID BestFlood
        => Unlocked(AID.FloodOfShadow)
        ? AID.FloodOfShadow
        : AID.FloodOfDarkness;

    private AID BestMPSpender
        => ShouldUseAOE
        ? BestFlood
        : BestEdge;

    private AID BestAOEMPSpender
        => Unlocked(AID.FloodOfShadow)
        && ShouldUseAOECircle(5).OnThreeOrMore
        ? AID.FloodOfDarkness
        : Unlocked(AID.FloodOfDarkness)
        && ShouldUseAOECircle(5).OnFourOrMore
        ? AID.FloodOfDarkness
        : BestEdge;

    private AID BestQuietus
        => Unlocked(AID.Quietus)
        ? AID.Quietus
        : AID.Bloodspiller;

    private AID BestBloodSpender
        => ShouldUseAOE
        ? BestQuietus
        : AID.Bloodspiller;

    private AID BestDelirium
        => Unlocked(AID.Delirium)
        ? AID.Delirium
        : AID.BloodWeapon;

    private AID CarveOrDrain
        => ShouldUseAOE
        ? AID.AbyssalDrain
        : AID.CarveAndSpit;

    private AID BestSalt
        => Unlocked(AID.SaltAndDarkness)
        && PlayerHasEffect(SID.SaltedEarth, 15)
        ? AID.SaltAndDarkness
        : AID.SaltedEarth;

    private SID BestBloodWeapon
        => Unlocked(AID.Delirium)
        ? SID.Delirium
        : SID.BloodWeapon;
    #endregion

    #region Module Variables
    //Gauge
    public bool RiskingBlood;
    public byte Blood;
    public (byte State, bool IsActive) DarkArts;
    public (ushort Step, float Left, int Stacks, float CD, bool IsActive, bool IsReady) Delirium;
    public (ushort Timer, float CD, bool IsActive, bool IsReady) LivingShadow;

    private bool inOdd; //Checks if player is in an odd-minute window
    private bool ShouldUseAOE; //Checks if AOE rotation or abilities should be used
    public bool canWeaveIn; //Can weave in oGCDs
    public bool canWeaveEarly; //Can early weave oGCDs
    public bool canWeaveLate; //Can late weave oGCDs
    public float BurstWindowLeft; //Time left in current burst window (typically 20s-22s)
    public float BurstWindowIn; //Time until next burst window (typically 20s-22s)
    #endregion

    #region Module Helpers
    #endregion

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        //Gauge
        var gauge = World.Client.GetGauge<DarkKnightGauge>(); //Retrieve Gunbreaker gauge
        DarkArts.State = gauge.DarkArtsState; //Retrieve current Dark Arts state
        DarkArts.IsActive = DarkArts.State > 0; //Checks if Dark Arts is active

        Delirium.Step = gauge.DeliriumStep; //Retrieve current Delirium combo step
        Delirium.Left = StatusRemaining(Player, BestBloodWeapon, 15); //Retrieve current Delirium time left
        Delirium.Stacks = StacksRemaining(Player, BestBloodWeapon, 15); //Retrieve current Delirium stacks
        Delirium.CD = TotalCD(BestDelirium); //Retrieve current Delirium cooldown
        Delirium.IsActive = Delirium.Left > 0.1f; //Checks if Delirium is active
        Delirium.IsReady = Unlocked(BestDelirium) && Delirium.CD < 0.6f; //Delirium ability

        LivingShadow.Timer = gauge.ShadowTimer; //Retrieve current Living Shadow timer
        LivingShadow.CD = TotalCD(AID.LivingShadow); //Retrieve current Living Shadow cooldown
        LivingShadow.IsActive = LivingShadow.Timer > 0; //Checks if Living Shadow is active
        LivingShadow.IsReady = Unlocked(AID.LivingShadow) && LivingShadow.CD < 0.6f; //Living Shadow ability
        //Cooldowns

        //GCD & Weaving
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late

        //Misc
        inOdd =  is <= 90 and >= 30; //Checks if we are in an odd-minute window
        ShouldUseAOE = ShouldUseAOECircle(5).OnThreeOrMore;

        #region Minimal Requirements

        #endregion

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var carts = strategy.Option(Track.Blood); //Blood track
        var cartStrat = carts.As<Bloodtrategy>(); //Blood strategy
        var nm = strategy.Option(Track.NoMercy); //No Mercy track
        var nmStrat = nm.As<NoMercyStrategy>(); //No Mercy strategy
        var zone = strategy.Option(Track.Zone); //Zone track
        var zoneStrat = zone.As<OGCDStrategy>(); //Zone strategy
        var bow = strategy.Option(Track.BowShock); //Bow Shock track
        var bowStrat = bow.As<OGCDStrategy>(); //Bow Shock strategy
        var bf = strategy.Option(Track.Bloodfest); //Bloodfest track
        var bfStrat = bf.As<BloodfestStrategy>(); //Bloodfest strategy
        var dd = strategy.Option(Track.DoubleDown); //Double Down track
        var ddStrat = dd.As<GCDStrategy>(); //Double Down strategy
        var gf = strategy.Option(Track.GnashingFang); //Gnashing Fang track
        var gfStrat = gf.As<GnashingStrategy>(); //Gnashing Fang strategy
        var reign = strategy.Option(Track.Reign); //Reign of Beasts track
        var reignStrat = reign.As<ReignStrategy>(); //Reign of Beasts strategy
        var sb = strategy.Option(Track.SonicBreak); //Sonic Break track
        var sbStrat = sb.As<SonicBreakStrategy>(); //Sonic Break strategy
        var ls = strategy.Option(Track.Unmend); //Unmend track
        var lsStrat = ls.As<UnmendStrategy>(); //Unmend strategy
        var hold = strategy.Option(Track.Cooldowns).As<CooldownStrategy>() == CooldownStrategy.Hold; //Determine if holding resources
        var conserve = cartStrat == Bloodtrategy.Conserve; //Determine if conserving Blood
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotations (1-2-3 / 1-2)

        #region Force Execution
        if (AOEStrategy is AOEStrategy.ForceSTwithO) //if Single-target (with overcap protection) option is selected
            QueueGCD(STwithOvercap(), //queue the next single-target combo action with overcap protection
                ResolveTargetOverride(AOE.Value) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                GCDPriority.ForcedGCD); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceSTwithoutO) //if Single-target (without overcap protection) option is selected
            QueueGCD(STwithoutOvercap(), //queue the next single-target combo action without overcap protection
                ResolveTargetOverride(AOE.Value) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                GCDPriority.ForcedGCD); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceAOEwithO) //if AOE (with overcap protection) option is selected
            QueueGCD(AOEwithOvercap(), //queue the next AOE combo action with overcap protection
                Player, //on Self (no target needed)
                GCDPriority.ForcedGCD); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceAOEwithoutO) //if AOE (without overcap protection) option is selected
            QueueGCD(AOEwithoutOvercap(), //queue the next AOE combo action without overcap protection
                Player, //on Self (no target needed)
                GCDPriority.ForcedGCD);  //with priority for forced GCDs
        #endregion

        #region Logic for Cart Generation before Downtime
        //TODO: refactor this
        if (AOEStrategy == AOEStrategy.GenerateDowntime) //if Generate Downtime option is selected
        {
            if (downtimeIn == GCD * 2 && Ammo == 2 || //if 2 GCDs until downtime & has 2 Blood
                downtimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 Blood
                downtimeIn == GCD * 6 && Ammo == 0) //if 6 GCDs until downtime & has 0 Blood
                QueueGCD(AID.Unleash, //queue Demon Slice
                    Player, //on Self (no target needed)
                    GCDPriority.ForcedGCD); //with priority for forced GCDs

            if (downtimeIn == GCD * 3 && Ammo == 2 || //if 3 GCDs until downtime & has 2 Blood
                downtimeIn == GCD * 5 && Ammo == 1 || //if 5 GCDs until downtime & has 1 Blood
                downtimeIn == GCD * 8 && Ammo == 0 || //if 8 GCDs until downtime & has 0 Blood
                downtimeIn == GCD * 9 && Ammo == 0) //if 9 GCDs until downtime & has 0 Blood
                QueueGCD(AID.HardSlash, //queue Keen Edge
                    primaryTarget, //on the primary target
                    GCDPriority.ForcedGCD); //with priority for forced GCDs

            if (ComboLastMove == AID.Unleash && //if last move was Demon Slice
                (downtimeIn == GCD && Ammo == 2 || //if 1 GCD until downtime & has 2 Blood
                downtimeIn == GCD * 3 && Ammo == 1 || //if 3 GCDs until downtime & has 1 Blood
                downtimeIn == GCD * 5 && Ammo == 0)) //if 5 GCDs until downtime & has 0 Blood
                QueueGCD(AID.StalwartSoul, //queue Demon Slaughter
                    Player, //on Self (no target needed)
                    GCDPriority.ForcedGCD); //with priority for forced GCDs

            if (ComboLastMove == AID.HardSlash && //if last move was Keen Edge
                (downtimeIn == GCD * 2 && Ammo == 2 || //if 2 GCDs until downtime & has 2 Blood
                downtimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 Blood
                downtimeIn == GCD * 7 && Ammo == 2 || //if 7 GCDs until downtime & has 2 Blood
                downtimeIn == GCD * 8 && Ammo == 2)) //if 8 GCDs until downtime & has 2 Blood
                QueueGCD(AID.SyphonStrike, //queue Brutal Shell
                    primaryTarget, //on the primary target
                    GCDPriority.ForcedGCD); //with priority for forced GCDs

            if (ComboLastMove == AID.SyphonStrike) //if last move was Brutal Shell
            {
                if (downtimeIn == GCD && (Ammo == 2 || Ammo == 3) || //if 1 GCD until downtime & has 2 or 3 Blood
                    downtimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 Blood
                    downtimeIn == GCD * 7 && Ammo == 0) //if 7 GCDs until downtime & has 0 Blood
                    QueueGCD(AID.Souleater, //queue Solid Barrel
                        primaryTarget, //on the primary target
                        GCDPriority.ForcedGCD); //with priority for forced GCDs
            }

            if (Ammo == MaxBlood) //if at max Blood
                QueueGCD(STwithoutOvercap(), //queue the next single-target combo action without overcap protection to save resources for uptime
                    primaryTarget, //on the primary target
                    GCDPriority.ForcedGCD); //with priority for forced GCDs
        }
        #endregion

        #region Standard Execution
        if (AOEStrategy == AOEStrategy.AutoBreakCombo) //if Break Combo option is selected
        {
            if (ShouldUseAOE) //if AOE rotation should be used
                QueueGCD(AOEwithoutOvercap(), //queue the next AOE combo action
                    Player, //on Self (no target needed)
                    GCDPriority.Combo123); //with priority for 123/12 combo actions
            if (!ShouldUseAOE)
                QueueGCD(STwithoutOvercap(), //queue the next single-target combo action
                    ResolveTargetOverride(AOE.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    GCDPriority.Combo123); //with priority for 123/12 combo actions
        }
        if (AOEStrategy == AOEStrategy.AutoFinishCombo) //if Finish Combo option is selected
        {
            QueueGCD(NextBestRotation(), //queue the next single-target combo action only if combo is finished
                ResolveTargetOverride(AOE.Value) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                GCDPriority.Combo123); //with priority for 123/12 combo actions
        }
        #endregion

        #endregion

        #region OGCDs
        if (!hold) //if not holding cooldowns
        {
            //No Mercy execution
            if (ShouldUseNoMercy(nmStrat, primaryTarget)) //if No Mercy should be used
                QueueOGCD(AID.NoMercy, //queue No Mercy
                    Player, //on Self (no target needed, but desired to not waste)
                    nmStrat is NoMercyStrategy.Force //if strategy option is Force
                    or NoMercyStrategy.ForceW //or Force weave
                    or NoMercyStrategy.ForceQW //or Force last second weave
                    or NoMercyStrategy.Force1 //or Force with 1 Blood
                    or NoMercyStrategy.Force1W //or Force weave with 1 Blood
                    or NoMercyStrategy.Force1QW //or Force last second weave with 1 Blood
                    or NoMercyStrategy.Force2 //or Force with 2 Blood
                    or NoMercyStrategy.Force2W //or Force weave with 2 Blood
                    or NoMercyStrategy.Force2QW //or Force last second weave with 2 Blood
                    or NoMercyStrategy.Force3 //or Force with 3 Blood
                    or NoMercyStrategy.Force3W //or Force weave with 3 Blood
                    or NoMercyStrategy.Force3QW //or Force last second weave with 3 Blood
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.NoMercy); //otherwise, use intended priority

            //Zone execution (Blasting Zone / Danger Zone)
            if (ShouldUseZone(zoneStrat, primaryTarget)) //if Zone should be used
                QueueOGCD(BestZone, //queue the best Zone action
                    ResolveTargetOverride(zone.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    zoneStrat is OGCDStrategy.Force //if strategy option is Force
                    or OGCDStrategy.AnyWeave //or any Weave
                    or OGCDStrategy.EarlyWeave //or Early Weave
                    or OGCDStrategy.LateWeave //or Late Weave
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.Zone); //otherwise, use intended priority

            //Bow Shock execution
            if (ShouldUseBowShock(bowStrat, primaryTarget)) //if Bow Shock should be used
                QueueOGCD(AID.BowShock, //queue Bow Shock
                    Player, //on Self (no target needed)
                    bowStrat is OGCDStrategy.Force //if strategy option is Force
                    or OGCDStrategy.AnyWeave //or Any Weave
                    or OGCDStrategy.EarlyWeave //or Early Weave
                    or OGCDStrategy.LateWeave //or Late Weave
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.BowShock); //otherwise, use intended priority

            //Bloodfest execution
            if (ShouldUseBloodfest(bfStrat, primaryTarget)) //if Bloodfest should be used
                QueueOGCD(AID.Bloodfest, //queue Bloodfest
                    ResolveTargetOverride(bf.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    bfStrat is BloodfestStrategy.Force //if strategy option is Force
                    or BloodfestStrategy.ForceW //or Force weave
                    or BloodfestStrategy.Force0 //or Force with 0 Blood
                    or BloodfestStrategy.Force0W //or Force weave with 0 Blood
                    ? OGCDPriority.ForcedOGCD //use priority for forced oGCDs
                    : OGCDPriority.Bloodfest); //otherwise, use intended priority
        }

        //Continuation execution
        if (canContinue && //if Continuation is available
            (hasBlast || //and Ready To Blast buff is active
            hasRaze || //or Ready To Raze buff is active
            hasRip || //or Ready To Rip buff is active
            hasTear || //or Ready To Tear buff is active
            hasGouge)) //or Ready To Gouge buff is active
            QueueOGCD(BestContinuation, //queue the best Continuation action
                primaryTarget, //on the primary target
                canWeaveLate || GCD is 0 //if inside second weave slot & still havent used
                ? OGCDPriority.Continuation + 1201 //force the fuck out of this to prevent loss, any loss is very bad
                : OGCDPriority.Continuation); //otherwise, use intended priority

        #endregion

        #region GCDs
        if (!hold) //if not holding cooldowns
        {
            if (!conserve) //if not conserving Blood
            {
                //Double Down execution
                if (ShouldUseDoubleDown(ddStrat, primaryTarget)) //if Double Down should be used
                    QueueGCD(AID.DoubleDown, //queue Double Down
                        primaryTarget, //on the primary target
                        ddStrat == GCDStrategy.Force || //or Force Double Down is selected on Double Down strategy
                        Ammo == 1 //or only 1 Blood is available
                        ? GCDPriority.ForcedGCD //use priority for forced GCDs
                        : GCDPriority.DoubleDown); //otherwise, use intended priority
                //Gnashing Fang execution
                if (ShouldUseGnashingFang(gfStrat, primaryTarget)) //if Gnashing Fang should be used
                    QueueGCD(AID.GnashingFang, //queue Gnashing Fang
                        ResolveTargetOverride(gf.Value) //Get target choice
                        ?? primaryTarget, //if none, choose primary target
                        gfStrat == GnashingStrategy.ForceGnash //or Force Gnashing Fang is selected on Gnashing Fang strategy
                        ? GCDPriority.ForcedGCD //use priority for forced GCDs
                        : GCDPriority.GF1); //otherwise, use intended priority
                //Burst Strike & Fated Circle execution
                if (ShouldUseBlood(cartStrat, primaryTarget)) //if Blood should be used
                {
                    //Optimal targeting & execution for both gauge spenders
                    if (cartStrat == Bloodtrategy.Automatic) //if Automatic Blood strategy is selected
                        QueueGCD(BestCartSpender, //queue the best Blood spender
                            ResolveTargetOverride(carts.Value) //Get target choice
                            ?? primaryTarget, //if none, choose primary target
                            nmCD < 1 && Ammo == 3 //if No Mercy is imminent and 3 Blood are available
                            ? GCDPriority.ForcedGCD //use priority for forced GCDs
                            : GCDPriority.Gauge); //otherwise, use priority for gauge actions
                    //Burst Strike forced execution
                    if (cartStrat is Bloodtrategy.OnlyBS or Bloodtrategy.ForceBS) //if Burst Strike Blood strategies are selected
                        QueueGCD(AID.BurstStrike, //queue Burst Strike
                            ResolveTargetOverride(carts.Value) //Get target choice
                            ?? primaryTarget, //if none, choose primary target
                            GCDPriority.Gauge); //with priority for gauge actions
                    //Fated Circle forced execution
                    if (cartStrat is Bloodtrategy.ForceFC or Bloodtrategy.OnlyFC) //if Fated Circle Blood strategies are selected
                        QueueGCD(BestFatedCircle, //queue Fated Circle
                            primaryTarget ?? Player, //on Self (no target needed) if Fated Circle, on target if Burst Strike
                            GCDPriority.Gauge); //with priority for gauge actions
                }
            }

            //Sonic Break execution
            if (ShouldUseSonicBreak(sbStrat, primaryTarget)) //if Sonic Break should be used
                QueueGCD(AID.SonicBreak, //queue Sonic Break
                    ResolveTargetOverride(sb.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    sbStrat is SonicBreakStrategy.Force //if strategy option is Force
                    or SonicBreakStrategy.Early //or Early
                    ? GCDPriority.ForcedGCD //use priority for forced GCDs
                    : GCDPriority.SonicBreak); //otherwise, use intended priority
            //Reign of Beasts execution
            if (ShouldUseReign(reignStrat, primaryTarget)) //if Reign of Beasts should be used
                QueueGCD(AID.ReignOfBeasts, //queue Reign of Beasts
                    ResolveTargetOverride(reign.Value) //Get target choice
                    ?? primaryTarget, //if none, choose primary target
                    reignStrat == ReignStrategy.ForceReign //if Force Reign of Beasts is selected on Reign of Beasts strategy
                    ? GCDPriority.ForcedGCD //use priority for forced GCDs
                    : GCDPriority.Reign); //otherwise, use intended priority
        }

        //Gnashing Fang combo execution
        if (GunComboStep == 1) //if just used Gnashing Fang
            QueueGCD(AID.SavageClaw, //queue Savage Claw
                primaryTarget, //on the primary target
                gfStrat == GnashingStrategy.ForceClaw //if Force Savage Claw is selected on Gnashing Fang strategy
                ? GCDPriority.ForcedGCD //use priority for forced GCDs 
                : GCDPriority.GF23); //otherwise, use priority for Gnashing Fang combo steps
        if (GunComboStep == 2) //if just used Savage Claw
            QueueGCD(AID.WickedTalon, //queue Wicked Talon
                primaryTarget, //on the primary target
                gfStrat == GnashingStrategy.ForceTalon //if Force Wicked Talon is selected on Gnashing Fang strategy
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.GF23); //otherwise, use priority for Gnashing Fang combo steps
        //Reign of Beasts combo execution
        if (GunComboStep == 3) //if just used Wicked Talon
            QueueGCD(AID.NobleBlood, //queue Noble Blood
                primaryTarget, //on the primary target
                reignStrat == ReignStrategy.ForceNoble //if Force Noble Blood is selected on Reign of Beasts strategy
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.Reign); //otherwise, use priority for Reign of Beasts combo steps
        if (GunComboStep == 4) //if just used Noble Blood
            QueueGCD(AID.LionHeart, //queue Lion Heart
                primaryTarget, //on the primary target
                reignStrat == ReignStrategy.ForceLion //if Force Lion Heart is selected on Reign of Beasts strategy
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.Reign); //otherwise, use priority for Reign of Beasts combo steps
        //Unmend execution
        if (ShouldUseUnmend(primaryTarget, lsStrat)) //if Unmend should be used
            QueueGCD(AID.Unmend, //queue Unmend
                ResolveTargetOverride(ls.Value) //Get target choice
                ?? primaryTarget, //if none, choose primary target
                lsStrat is UnmendStrategy.Force //if strategy option is Force
                or UnmendStrategy.Allow //or Allow
                ? GCDPriority.ForcedGCD //use priority for forced GCDs
                : GCDPriority.Combo123); //otherwise, use priority for standard combo actions
        //Potion execution
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>())) //if Potion should be used
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, //queue the potion
                Player, //on Self (no target needed)
                ActionQueue.Priority.VeryHigh //with very high priority
                + (int)OGCDPriority.Potion, 0, GCD - 0.9f); //and the specified priority
        #endregion

        #endregion
    }


    #region Rotation Helpers
    private AID NextBestRotation() => ComboLastMove switch
    {
        //ST
        AID.Souleater => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
        AID.SyphonStrike => STwithoutOvercap(),
        AID.HardSlash => STwithoutOvercap(),
        //AOE
        AID.StalwartSoul => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
        AID.Unleash => AOEwithoutOvercap(),
        _ => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
    };

    #region Single-Target Helpers
    private AID STwithOvercap() => ComboLastMove switch
    {
        AID.SyphonStrike => RiskingBlood ? BestBloodSpender : AID.Souleater,
        AID.HardSlash => AID.SyphonStrike,
        _ => AID.HardSlash,
    };
    private AID STwithoutOvercap() => ComboLastMove switch
    {
        AID.SyphonStrike => AID.Souleater,
        AID.HardSlash => AID.SyphonStrike,
        _ => AID.HardSlash,
    };
    #endregion

    #region AOE Helpers
    private AID AOEwithOvercap() => ComboLastMove switch
    {
        AID.Unleash => RiskingBlood ? BestBloodSpender : AID.StalwartSoul,
        _ => AID.Unleash,
    };
    private AID AOEwithoutOvercap() => ComboLastMove switch
    {
        AID.Unleash => AID.StalwartSoul,
        _ => AID.Unleash,
    };
    #endregion

    #endregion

    #region Cooldown Helpers
    //No Mercy full strategy & conditions
    private bool ShouldUseNoMercy(NoMercyStrategy strategy, Actor? target) => strategy switch
    {
        NoMercyStrategy.Automatic =>
            //Standard conditions
            Player.InCombat && //In combat
            target != null && //Target exists
            canNM && //No Mercy is available
            ((Unlocked(AID.DoubleDown) && //Double Down is unlocked, indicating Lv90 or above
            (inOdd && Ammo >= 2) || //In Odd Window & conditions are met
            (!inOdd && Ammo < 3)) || //In Even Window & conditions are met
            (!Unlocked(AID.DoubleDown) && GCD < 0.9f && //Double Down is not unlocked, so we late weave it
            ((Unlocked(AID.Bloodfest) && //but Bloodfest is, indicating Lv80-89
            Ammo >= 1) || //Ammo is 1 or more
            (!Unlocked(AID.Bloodfest) && canGF) || //Bloodfest is not unlocked but Gnashing Fang is, indicating Lv60-79
            !Unlocked(AID.GnashingFang)))), //Gnashing Fang is not unlocked, indicating Lv59 and below
        NoMercyStrategy.Force => canNM, //Force No Mercy, regardless of correct weaving
        NoMercyStrategy.ForceW => canNM && canWeaveIn, //Force No Mercy into any weave slot 
        NoMercyStrategy.ForceQW => canNM && quarterWeave, //Force No Mercy into last possible second weave slot
        NoMercyStrategy.Force1 => canNM && Ammo == 1, //Force No Mercy if ready and 1 Blood, regardless of weaving
        NoMercyStrategy.Force1W => canNM && canWeaveIn && Ammo == 1, //Force No Mercy into any weave slot if ready and 1 Blood
        NoMercyStrategy.Force1QW => canNM && quarterWeave && Ammo == 1, //Force No Mercy into last possible second weave slot if ready and 1 Blood
        NoMercyStrategy.Force2 => canNM && Ammo == 2, //Force No Mercy if ready and 2 Blood, regardless of weaving
        NoMercyStrategy.Force2W => canNM && canWeaveIn && Ammo == 2, //Force No Mercy into any weave slot if ready and 2 Blood
        NoMercyStrategy.Force2QW => canNM && quarterWeave && Ammo == 2, //Force No Mercy into last possible second weave slot if ready and 2 Blood
        NoMercyStrategy.Force3 => canNM && Ammo == 3, //Force No Mercy if ready and 3 Blood, regardless of weaving
        NoMercyStrategy.Force3W => canNM && canWeaveIn && Ammo == 3, //Force No Mercy into any weave slot if ready and 3 Blood
        NoMercyStrategy.Force3QW => canNM && quarterWeave && Ammo == 3, //Force No Mercy into last possible second weave slot if ready and 3 Blood
        NoMercyStrategy.Delay => false, //Delay No Mercy 
        _ => false
    };

    //Bloodfest full strategy & conditions
    private bool ShouldUseBloodfest(BloodfestStrategy strategy, Actor? target) => strategy switch
    {
        BloodfestStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            canBF && Ammo == 0, //Bloodfest is available and empty on Blood
        BloodfestStrategy.Force => canBF, //Force Bloodfest, regardless of correct weaving
        BloodfestStrategy.ForceW => canBF && canWeaveIn, //Force Bloodfest into any weave slot
        BloodfestStrategy.Force0 => canBF && Ammo == 0, //Force Bloodfest if ready and 0 Blood, regardless of weaving
        BloodfestStrategy.Force0W => canBF && Ammo == 0 && canWeaveIn, //Force Bloodfest into any weave slot if ready and 0 Blood
        BloodfestStrategy.Delay => false, //Delay Bloodfest
        _ => false
    };

    //Zone full strategy & conditions
    private bool ShouldUseZone(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //In combat
            In3y(target) && //Target in melee range
            canZone && //Zone is available
            nmCD is < 57.55f and > 17, //No Mercy is active & not just used within 1GCD or CD is greater than 17s
        OGCDStrategy.Force => canZone, //Force Zone if available
        OGCDStrategy.AnyWeave => canZone && canWeaveIn, //Force Zone into any weave slot
        OGCDStrategy.EarlyWeave => canZone && canWeaveEarly, //Force weave Zone early
        OGCDStrategy.LateWeave => canZone && canWeaveLate, //Force weave Zone late
        OGCDStrategy.Delay => false,
        _ => false
    };

    //Bow Shock full strategy & conditions
    private bool ShouldUseBowShock(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic =>
            Player.InCombat && //In combat
            ActionReady(AID.BowShock) && //Bow Shock is available
            In5y(target) && //Target in range
            nmCD is < 57.55f and >= 40, //No Mercy is active, but not just used within 1GCD
        OGCDStrategy.Force => canBow, //Force Bow Shock if available, regardless of weaving
        OGCDStrategy.AnyWeave => canBow && canWeaveIn, //Force Bow Shock into any weave slot
        OGCDStrategy.EarlyWeave => canBow && canWeaveEarly, //Force weave Bow Shock early
        OGCDStrategy.LateWeave => canBow && canWeaveLate, //Force weave Bow Shock late
        OGCDStrategy.Delay => false,
        _ => false
    };

    //Gauge full strategy & conditions
    private bool ShouldUseBlood(Bloodtrategy strategy, Actor? target) => strategy switch
    {
        Bloodtrategy.Automatic =>
            ShouldUseFC //enough targets for optimal use of Fated Circle
            ? ShouldUseFatedCircle(Bloodtrategy.Automatic, target) //use Fated Circle
            : ShouldUseBurstStrike(Bloodtrategy.Automatic, target), //otherwise, use Burst Strike
        Bloodtrategy.OnlyBS => ShouldUseBurstStrike(Bloodtrategy.Automatic, target), //Optimally use Burst Strike
        Bloodtrategy.OnlyFC => ShouldUseFatedCircle(Bloodtrategy.Automatic, target), //Optimally use Fated Circle
        Bloodtrategy.ForceBS => canBS, //Force Burst Strike
        Bloodtrategy.ForceFC => canFC, //Force Fated Circle
        Bloodtrategy.Conserve => false, //Conserve Blood
        _ => false
    };

    //Double Down full strategy & conditions
    private bool ShouldUseDoubleDown(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In5y(target) && //Target in range
            canDD && //Double Down is available
            hasNM, //No Mercy is active
        GCDStrategy.Force => canDD, //Force Double Down if available
        GCDStrategy.Delay => false,
        _ => false
    };

    //Gnashing Fang & combo chain full strategy & conditions
    private bool ShouldUseGnashingFang(GnashingStrategy strategy, Actor? target) => strategy switch
    {
        GnashingStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In3y(target) && //Target in melee range
            canGF && //Gnashing Fang is available
            (nmLeft > 0 || hasNM || //No Mercy is active
            nmCD is < 35 and > 17), //or greater than 17s on No Mercy CD
        GnashingStrategy.ForceGnash => canGF, //Gnashing Fang is available
        GnashingStrategy.ForceClaw => Player.InCombat && GunComboStep == 1, //Force Savage Claw if available
        GnashingStrategy.ForceTalon => Player.InCombat && GunComboStep == 2, //Force Wicked Talon if available
        GnashingStrategy.Delay => false,
        _ => false
    };

    //Burst Strike & Fated Circle full strategy & conditions
    private bool ShouldUseBurstStrike(Bloodtrategy strategy, Actor? target) => strategy switch
    {
        Bloodtrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In3y(target) && //Target in melee range
            canBS && //Burst Strike is available
            (hasNM || //No Mercy is active
            (!(bfCD is <= 90 and >= 30) &&
            nmCD < 1 &&
            Ammo == 3)) || //No Mercy is almost ready and full carts
            Ammo == MaxBlood && ComboLastMove is AID.SyphonStrike or AID.Unleash, //Full carts and last move was Brutal Shell or Demon Slice
        _ => false
    };

    //Fated Circle full strategy & conditions
    private bool ShouldUseFatedCircle(Bloodtrategy strategy, Actor? target) => strategy switch
    {
        Bloodtrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            In5y(target) && //Target in range
            canFC && //Fated Circle is available
            (hasNM || //No Mercy is active
            (!(bfCD is <= 90 and >= 30) &&
            nmCD < 1 &&
            Ammo == 3)) || //No Mercy is almost ready and full carts
            Ammo == MaxBlood && ComboLastMove is AID.SyphonStrike or AID.Unleash, //Full carts and last move was Brutal Shell or Demon Slice
        _ => false
    };

    //Sonic Break full strategy & conditions
    private bool ShouldUseSonicBreak(SonicBreakStrategy strategy, Actor? target) => strategy switch
    {
        SonicBreakStrategy.Automatic =>
            Player.InCombat && //In combat
            In3y(target) && //Target in melee range
            canBreak, //Sonic Break is available
        SonicBreakStrategy.Force => canBreak, //Force Sonic Break
        SonicBreakStrategy.Early => nmCD > 55 || hasBreak, //Use Sonic Break early
        SonicBreakStrategy.Late => nmLeft <= GCDLength, //Use Sonic Break late
        SonicBreakStrategy.Delay => false,
        _ => false
    };

    //Reign of Beasts & combo chain full strategy & conditions
    private bool ShouldUseReign(ReignStrategy strategy, Actor? target) => strategy switch
    {
        ReignStrategy.Automatic =>
            Player.InCombat && //In combat
            target != null && //Target exists
            canReign && //Reign of Beasts is available
            hasNM && //No Mercy is active
            GunComboStep == 0, //not in GF combo
        ReignStrategy.ForceReign => canReign, //Force Reign of Beasts
        ReignStrategy.ForceNoble => Player.InCombat && GunComboStep == 3, //Force Noble Blood
        ReignStrategy.ForceLion => Player.InCombat && GunComboStep == 4, //Force Lion Heart
        ReignStrategy.Delay => false,
        _ => false
    };

    //Unmend full strategy & conditions
    private bool ShouldUseUnmend(Actor? target, UnmendStrategy strategy) => strategy switch
    {
        UnmendStrategy.OpenerFar =>
            (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && //Prepull or already in combat
            IsFirstGCD() && !In3y(target), //First GCD of fight and target is not in melee range
        UnmendStrategy.OpenerForce => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD(), //Prepull or already in combat and first GCD of fight
        UnmendStrategy.Force => true, //Force Unmend, regardless of any cooldowns or GCDs
        UnmendStrategy.Allow => !In3y(target), //Use Unmend if target is not in melee range
        UnmendStrategy.Forbid => false, //Do not use Unmend
        _ => false
    };

    //Potion full strategy & conditions
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => //Use potion when buffs are imminent
            nmCD < 5 && //No Mercy is almost ready
            bfCD < 15, //Bloodfest is almost ready
        PotionStrategy.Immediate => true, //Use potion immediately
        _ => false
    };
    #endregion
}

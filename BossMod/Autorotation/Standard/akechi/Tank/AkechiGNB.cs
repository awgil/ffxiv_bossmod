using static BossMod.AIHints;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using BossMod.GNB;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiGNB(RotationModuleManager manager, Actor player) : AkechiTools<AID, TraitID>(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track { AOE, Cooldowns, Cartridges, Potion, LightningShot, NoMercy, SonicBreak, GnashingFang, Reign, Bloodfest, DoubleDown, Zone, BowShock }
    public enum AOEStrategy { AutoFinishCombo, AutoBreakCombo, ForceSTwithO, ForceSTwithoutO, ForceAOEwithO, ForceAOEwithoutO, GenerateDowntime }
    public enum CooldownStrategy { Allow, Forbid }
    public enum CartridgeStrategy { Automatic, OnlyBS, OnlyFC, ForceBS, ForceFC, Conserve }
    public enum PotionStrategy { Manual, AlignWithRaidBuffs, Immediate }
    public enum LightningShotStrategy { OpenerFar, OpenerForce, Force, Allow, Forbid }
    public enum NoMercyStrategy { Automatic, Force, ForceW, ForceQW, Force1, Force1W, Force1QW, Force2, Force2W, Force2QW, Force3, Force3W, Force3QW, Delay }
    public enum SonicBreakStrategy { Automatic, Force, Early, Late, Delay }
    public enum GnashingStrategy { Automatic, ForceGnash, ForceClaw, ForceTalon, Delay }
    public enum ReignStrategy { Automatic, ForceReign, ForceNoble, ForceLion, Delay }
    public enum BloodfestStrategy { Automatic, Force, ForceW, Force0, Force0W, Delay }
    #endregion

    #region Module Definitions
    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi GNB", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)|Tank", //Category
            "Akechi", //Contributor
            RotationModuleQuality.Good, //Quality
            BitMask.Build((int)Class.GNB), //Job
            100); //Level supported

        res.Define(Track.AOE).As<AOEStrategy>("AOE", uiPriority: 200)
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto (Finish Combo)", "Auto-selects best rotation dependant on targets; Finishes combo first", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.AutoBreakCombo, "Auto (Break Combo)", "Auto-selects best rotation dependant on targets; Breaks combo if needed", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceSTwithO, "Force ST with Overcap", "Force single-target rotation with overcap protection", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceSTwithoutO, "Force ST without Overcap", "Force ST rotation without overcap protection", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOEwithO, "Force AOE with Overcap", "Force AOE rotation with overcap protection")
            .AddOption(AOEStrategy.ForceAOEwithoutO, "Force AOE without Overcap", "Force AOE rotation without overcap protection")
            .AddOption(AOEStrategy.GenerateDowntime, "Generate Downtime", "Generate cartridges before downtime");
        res.Define(Track.Cooldowns).As<CooldownStrategy>("Hold", uiPriority: 190)
            .AddOption(CooldownStrategy.Allow, "Allow", "Allows the use of all cooldowns & buffs; will use them optimally")
            .AddOption(CooldownStrategy.Forbid, "Hold", "Forbids the use of all cooldowns & buffs; will not use any actions with a cooldown timer");
        res.Define(Track.Cartridges).As<CartridgeStrategy>("Cartridges", "Carts", uiPriority: 180)
            .AddOption(CartridgeStrategy.Automatic, "Automatic", "Automatically decide when to use cartridges; uses them optimally")
            .AddOption(CartridgeStrategy.OnlyBS, "Only Burst Strike", "Uses Burst Strike optimally as cartridge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.OnlyFC, "Only Fated Circle", "Uses Fated Circle optimally as cartridge spender only, regardless of targets", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.ForceBS, "Force Burst Strike", "Force use of Burst Strike; consumes 1 cartridge", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(CartridgeStrategy.ForceFC, "Force Fated Circle", "Force use of Fated Circle; consumes 1 cartridge", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(CartridgeStrategy.Conserve, "Conserve", "Prohibit use of all cartridge-related abilities; will not use any of these actions listed above")
            .AddAssociatedActions(AID.BurstStrike, AID.FatedCircle);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 20)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with No Mercy & Bloodfest together (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);
        res.Define(Track.LightningShot).As<LightningShotStrategy>("Lightning Shot", "L.Shot", uiPriority: 30)
            .AddOption(LightningShotStrategy.OpenerFar, "Far (Opener)", "Use Lightning Shot in pre-pull & out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.OpenerForce, "Force (Opener)", "Force use Lightning Shot in pre-pull in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Force, "Force", "Force use Lightning Shot in any range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Allow, "Allow", "Allow use of Lightning Shot when out of melee range", supportedTargets: ActionTargets.Hostile)
            .AddOption(LightningShotStrategy.Forbid, "Forbid", "Prohibit use of Lightning Shot")
            .AddAssociatedActions(AID.LightningShot);
        res.Define(Track.NoMercy).As<NoMercyStrategy>("No Mercy", "N.Mercy", uiPriority: 170)
            .AddOption(NoMercyStrategy.Automatic, "Auto", "Normal use of No Mercy")
            .AddOption(NoMercyStrategy.Force, "Force", "Force use of No Mercy, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceW, "Force (Weave)", "Force use of No Mercy in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.ForceQW, "Force (Q.Weave)", "Force use of No Mercy in next possible last second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1, "Force (1 cart)", "Force use of No Mercy when 1 cartridge is available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1W, "Force (1 cart, Weave)", "Force use of No Mercy when 1 cartridge is available & in next weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force1QW, "Force (1 cart, Q.Weave)", "Force use of No Mercy when 1 cartridge is available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2, "Force (2 carts)", "Force use of No Mercy when 2 cartridges are available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2W, "Force (2 carts, Weave)", "Force use of No Mercy when 2 cartridges are available & in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force2QW, "Force (2 carts, Q.Weave)", "Force use of No Mercy when 2 cartridges are available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3, "Force (3 carts)", "Force use of No Mercy when 3 cartridges are available, regardless of weaving", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3W, "Force (3 carts, Weave)", "Force use of No Mercy when 3 cartridges are available & in next possible weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Force3QW, "Force (3 carts, Q.Weave)", "Force use of No Mercy when 3 cartridges are available & in next possible last-second weave slot", 60, 20, ActionTargets.Self, 2)
            .AddOption(NoMercyStrategy.Delay, "Delay", "Delay use of No Mercy", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(AID.NoMercy);
        res.Define(Track.SonicBreak).As<SonicBreakStrategy>("Sonic Break", "S.Break", uiPriority: 150)
            .AddOption(SonicBreakStrategy.Automatic, "Auto", "Normal use of Sonic Break")
            .AddOption(SonicBreakStrategy.Force, "Force", "Force use of Sonic Break", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Early, "Early Sonic Break", "Uses Sonic Break as the very first GCD when in No Mercy", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Late, "Late Sonic Break", "Uses Sonic Break as the very last GCD when in No Mercy", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(SonicBreakStrategy.Delay, "Delay", "Delay use of Sonic Break", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(AID.SonicBreak);
        res.Define(Track.GnashingFang).As<GnashingStrategy>("Gnashing Fang", "G.Fang", uiPriority: 160)
            .AddOption(GnashingStrategy.Automatic, "Auto", "Normal use of Gnashing Fang")
            .AddOption(GnashingStrategy.ForceGnash, "Force", "Force use of Gnashing Fang (Step 1)", 30, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceClaw, "Force", "Force use of Savage Claw (Step 2)", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.ForceTalon, "Force", "Force use of Wicked Talon (Step 3)", 0, 0, ActionTargets.Hostile, 60)
            .AddOption(GnashingStrategy.Delay, "Delay", "Delay use of Gnashing Fang", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(AID.GnashingFang, AID.SavageClaw, AID.WickedTalon);
        res.Define(Track.Reign).As<ReignStrategy>("Reign of Beasts", "Reign", uiPriority: 160)
            .AddOption(ReignStrategy.Automatic, "Auto", "Normal use of Reign of Beasts")
            .AddOption(ReignStrategy.ForceReign, "Force", "Force use of Reign of Beasts", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.ForceNoble, "Force", "Force use of Noble Blood", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.ForceLion, "Force", "Force use of Lion Heart", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(ReignStrategy.Delay, "Delay", "Delay use of Reign of Beasts", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(AID.ReignOfBeasts, AID.NobleBlood, AID.LionHeart);
        res.Define(Track.Bloodfest).As<BloodfestStrategy>("Bloodfest", "Fest", uiPriority: 170)
            .AddOption(BloodfestStrategy.Automatic, "Auto", "Normal use of Bloodfest")
            .AddOption(BloodfestStrategy.Force, "Force", "Force use of Bloodfest, regardless of ammo count & weaving", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.ForceW, "Force (Weave)", "Force use of Bloodfest in next possible weave slot, regardless of ammo count", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Force0, "Force (0 cart)", "Force use of Bloodfest only if empty on cartridges", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Force0W, "Force (0 cart, Weave)", "Force use of Bloodfest only if empty on cartridges & in next possible weave slot", 120, 0, ActionTargets.Hostile, 80)
            .AddOption(BloodfestStrategy.Delay, "Delay", "Delay use of Bloodfest", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(AID.Bloodfest);
        res.DefineGCD(Track.DoubleDown, AID.DoubleDown, "DoubleDown", "D.Down", uiPriority: 160, 60, 0, ActionTargets.Hostile, 90);
        res.DefineOGCD(Track.Zone, AID.DangerZone, "Zone", "Zone", uiPriority: 150, 30, 0, ActionTargets.Hostile, 18).AddAssociatedActions(AID.BlastingZone, AID.DangerZone);
        res.DefineOGCD(Track.BowShock, AID.BowShock, "BowShock", "B.Shock", uiPriority: 150, 60, 15, ActionTargets.Self, 62);

        return res;
    }
    #endregion

    #region Priorities
    public enum GCDPriority
    {
        None = 0,
        Standard = 100,
        ForcedCombo = 499,
        Gauge = 500,
        Reign = 525,
        comboNeed = 550,
        GF23 = 575,
        SonicBreak = 600,
        DoubleDown = 675,
        GF1 = 700,
        ForcedGCD = 900,
    }
    public enum OGCDPriority
    {
        None = 0,
        Continuation = 500,
        Zone = 550,
        BowShock = 600,
        Bloodfest = 700,
        NoMercy = 875,
        Potion = 900,
        ForcedOGCD = 1100, //Enough to put it past CDPlanner's "Automatic" priority, which is really only Medium priority
    }
    #endregion

    #region Upgrade Paths
    private AID BestZone => Unlocked(AID.BlastingZone) ? AID.BlastingZone : AID.DangerZone;
    private AID BestCartSpender => ShouldUseFC ? BestFatedCircle : canBS ? AID.BurstStrike : BestRotation();
    private AID BestFatedCircle => Unlocked(AID.FatedCircle) ? AID.FatedCircle : AID.BurstStrike;
    private AID BestContinuation => hasRaze ? AID.FatedBrand : hasBlast ? AID.Hypervelocity : hasGouge ? AID.EyeGouge : hasTear ? AID.AbdomenTear : hasRip ? AID.JugularRip : AID.Continuation;
    #endregion

    #region Module Variables
    public byte Ammo; //Range: 0-2 or 0-3 max; this counts current ammo count
    public byte GunComboStep; //0 = Gnashing Fang & Reign of Beasts, 1 = Savage Claw, 2 = Wicked Talon, 3 = NobleBlood, 4 = LionHeart
    public int MaxCartridges; //Maximum number of cartridges based on player level
    private float bfCD; //Time left on Bloodfest cooldown (120s base)
    private float nmLeft; //Time left on No Mercy buff (20s base)
    private float nmCD; //Time left on No Mercy cooldown (60s base)
    private bool inOdd; //Checks if player is in an odd-minute window
    private bool hasNM; //Checks self for No Mercy buff
    private bool hasBreak; //Checks self for Ready To Break buff
    private bool hasReign; //Checks self for Ready To Reign buff
    private bool hasBlast; //Checks self for Ready To Blast buff
    private bool hasRaze; //Checks self for Ready To Raze buff
    private bool hasRip; //Checks self for Ready To Rip buff
    private bool hasTear; //Checks self for Ready To Tear buff
    private bool hasGouge; //Checks self for Ready To Gouge buff
    private bool canNM; //Checks if No Mercy is completely available
    private bool canBS; //Checks if Burst Strike is completely available
    private bool canGF; //Checks if Gnashing Fang & its combo chain are completely available
    private bool canFC; //Checks if Fated Circle is completely available
    private bool canDD; //Checks if Double Down is completely available
    private bool canBF; //Checks if Bloodfest is completely available
    private bool canZone; //Checks if Danger / Blasting Zone is completely available
    private bool canBreak; //Checks if Sonic Break is completely available
    private bool canBow; //Checks if Bow Shock is completely available 
    private bool canContinue; //Checks if Continuation is completely available 
    private bool canReign; //Checks if Reign of Beasts & its combo chain are completely available
    private bool ShouldUseAOE; //Checks if AOE rotation should be used
    private bool ShouldUseFC; //Checks if Fated Circle should be used
    private int NumSplashTargets;
    private Enemy? BestSplashTargets;
    private Enemy? BestSplashTarget;
    #endregion

    public override void Execution(StrategyValues strategy, Enemy? primaryTarget)
    {
        #region Variables
        var gauge = World.Client.GetGauge<GunbreakerGauge>(); //Retrieve Gunbreaker gauge
        Ammo = gauge.Ammo; //Current cartridges
        GunComboStep = gauge.AmmoComboStep; //Combo step for Gnashing Fang or Reign of Beasts
        MaxCartridges = Unlocked(TraitID.CartridgeChargeII) ? 3 : 2; //Max cartridges based on level
        bfCD = TotalCD(AID.Bloodfest); //Bloodfest cooldown (120s)
        nmCD = TotalCD(AID.NoMercy); //No Mercy cooldown (60s)
        nmLeft = SelfStatusLeft(SID.NoMercy, 20); //Remaining time for No Mercy buff (20s)
        hasBreak = PlayerHasEffect(SID.ReadyToBreak, 30); //Checks for Ready To Break buff
        hasReign = PlayerHasEffect(SID.ReadyToReign, 30); //Checks for Ready To Reign buff
        hasNM = nmCD is >= 39.5f and <= 60; //Checks if No Mercy is active
        hasBlast = Unlocked(AID.Hypervelocity) && PlayerHasEffect(SID.ReadyToBlast, 10f) && !LastActionUsed(AID.Hypervelocity); //Checks for Ready To Blast buff
        hasRaze = Unlocked(AID.FatedBrand) && PlayerHasEffect(SID.ReadyToRaze, 10f) && !LastActionUsed(AID.FatedBrand); //Checks for Ready To Raze buff
        hasRip = PlayerHasEffect(SID.ReadyToRip, 10f) && !LastActionUsed(AID.JugularRip); //Checks for Ready To Rip buff
        hasTear = PlayerHasEffect(SID.ReadyToTear, 10f) && !LastActionUsed(AID.AbdomenTear); //Checks for Ready To Tear buff
        hasGouge = PlayerHasEffect(SID.ReadyToGouge, 10f) && !LastActionUsed(AID.EyeGouge); //Checks for Ready To Gouge buff
        inOdd = bfCD is <= 90 and >= 30; //Checks if we are in an odd-minute window
        ShouldUseAOE = Unlocked(TraitID.MeleeMastery) ? ShouldUseAOECircle(5).OnThreeOrMore : ShouldUseAOECircle(5).OnTwoOrMore;
        ShouldUseFC = ShouldUseAOECircle(5).OnTwoOrMore; //Determine if we should use Fated Circle
        (BestSplashTargets, NumSplashTargets) = GetBestTarget(primaryTarget, 3, IsSplashTarget);
        BestSplashTarget = Unlocked(AID.ReignOfBeasts) && NumSplashTargets > 1 ? BestSplashTargets : primaryTarget;

        #region Minimal Requirements
        canNM = TotalCD(AID.NoMercy) < 1; //No Mercy conditions
        canBS = Unlocked(AID.BurstStrike) && Ammo > 0; //Burst Strike conditions; -1 Ammo ST
        canGF = Unlocked(AID.GnashingFang) && ActionReady(AID.GnashingFang) && Ammo > 0; //Gnashing Fang conditions; -1 Ammo ST
        canFC = Unlocked(AID.FatedCircle) && Ammo > 0; //Fated Circle conditions; -1 Ammo AOE
        canDD = Unlocked(AID.DoubleDown) && ActionReady(AID.DoubleDown) && Ammo > 0; //Double Down conditions; -1 Ammo AOE
        canBF = Unlocked(AID.Bloodfest) && ActionReady(AID.Bloodfest); //Bloodfest conditions; +all Ammo (must have target)
        canZone = Unlocked(AID.DangerZone) && ActionReady(AID.DangerZone); //Zone conditions
        canBreak = hasBreak && Unlocked(AID.SonicBreak); //Sonic Break conditions
        canBow = Unlocked(AID.BowShock) && ActionReady(AID.BowShock); //Bow Shock conditions
        canContinue = Unlocked(AID.Continuation); //Continuation conditions
        canReign = Unlocked(AID.ReignOfBeasts) && hasReign; //Reign of Beasts conditions
        #endregion

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var carts = strategy.Option(Track.Cartridges); //Cartridges track
        var cartStrat = carts.As<CartridgeStrategy>(); //Cartridges strategy
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
        var ls = strategy.Option(Track.LightningShot); //Lightning Shot track
        var lsStrat = ls.As<LightningShotStrategy>(); //Lightning Shot strategy
        var hold = strategy.Option(Track.Cooldowns).As<CooldownStrategy>() == CooldownStrategy.Forbid; //Determine if holding resources
        var conserve = cartStrat == CartridgeStrategy.Conserve; //Determine if conserving cartridges
        #endregion

        #endregion

        #region Full Rotation Execution

        #region Standard Rotations

        #region Force Execution
        if (AOEStrategy is AOEStrategy.ForceSTwithO) //if Single-target (with overcap protection) option is selected
            QueueGCD(STwithOvercap(), //queue the next single-target combo action with overcap protection
                TargetChoice(AOE) //Get target choice
                ?? primaryTarget?.Actor, //if none, choose primary target
                GCDPriority.ForcedCombo); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceSTwithoutO) //if Single-target (without overcap protection) option is selected
            QueueGCD(STwithoutOvercap(), //queue the next single-target combo action without overcap protection
                TargetChoice(AOE) //Get target choice
                ?? primaryTarget?.Actor, //if none, choose primary target
                GCDPriority.ForcedCombo); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceAOEwithO) //if AOE (with overcap protection) option is selected
            QueueGCD(AOEwithOvercap(), //queue the next AOE combo action with overcap protection
                Player, //on Self (no target needed)
                GCDPriority.ForcedCombo); //with priority for forced GCDs
        if (AOEStrategy is AOEStrategy.ForceAOEwithoutO) //if AOE (without overcap protection) option is selected
            QueueGCD(AOEwithoutOvercap(), //queue the next AOE combo action without overcap protection
                Player, //on Self (no target needed)
                GCDPriority.ForcedCombo);  //with priority for forced GCDs
        #endregion

        #region Logic for Cart Generation before Downtime
        //TODO: refactor this
        if (AOEStrategy == AOEStrategy.GenerateDowntime) //if Generate Downtime option is selected
        {
            if (DowntimeIn == GCD * 2 && Ammo == 2 || //if 2 GCDs until downtime & has 2 cartridges
                DowntimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 cartridge
                DowntimeIn == GCD * 6 && Ammo == 0) //if 6 GCDs until downtime & has 0 cartridges
                QueueGCD(AID.DemonSlice, //queue Demon Slice
                    Player, //on Self (no target needed)
                    GCDPriority.ForcedCombo); //with priority for forced GCDs

            if (DowntimeIn == GCD * 3 && Ammo == 2 || //if 3 GCDs until downtime & has 2 cartridges
                DowntimeIn == GCD * 5 && Ammo == 1 || //if 5 GCDs until downtime & has 1 cartridge
                DowntimeIn == GCD * 8 && Ammo == 0 || //if 8 GCDs until downtime & has 0 cartridges
                DowntimeIn == GCD * 9 && Ammo == 0) //if 9 GCDs until downtime & has 0 cartridges
                QueueGCD(AID.KeenEdge, //queue Keen Edge
                    primaryTarget?.Actor, //on the primary target
                    GCDPriority.ForcedCombo); //with priority for forced GCDs

            if (ComboLastMove == AID.DemonSlice && //if last move was Demon Slice
                (DowntimeIn == GCD && Ammo == 2 || //if 1 GCD until downtime & has 2 cartridges
                DowntimeIn == GCD * 3 && Ammo == 1 || //if 3 GCDs until downtime & has 1 cartridge
                DowntimeIn == GCD * 5 && Ammo == 0)) //if 5 GCDs until downtime & has 0 cartridges
                QueueGCD(AID.DemonSlaughter, //queue Demon Slaughter
                    Player, //on Self (no target needed)
                    GCDPriority.ForcedCombo); //with priority for forced GCDs

            if (ComboLastMove == AID.KeenEdge && //if last move was Keen Edge
                (DowntimeIn == GCD * 2 && Ammo == 2 || //if 2 GCDs until downtime & has 2 cartridges
                DowntimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 cartridge
                DowntimeIn == GCD * 7 && Ammo == 2 || //if 7 GCDs until downtime & has 2 cartridges
                DowntimeIn == GCD * 8 && Ammo == 2)) //if 8 GCDs until downtime & has 2 cartridges
                QueueGCD(AID.BrutalShell, //queue Brutal Shell
                    primaryTarget?.Actor, //on the primary target
                    GCDPriority.ForcedCombo); //with priority for forced GCDs

            if (ComboLastMove == AID.BrutalShell) //if last move was Brutal Shell
            {
                if (DowntimeIn == GCD && (Ammo == 2 || Ammo == 3) || //if 1 GCD until downtime & has 2 or 3 cartridges
                    DowntimeIn == GCD * 4 && Ammo == 1 || //if 4 GCDs until downtime & has 1 cartridge
                    DowntimeIn == GCD * 7 && Ammo == 0) //if 7 GCDs until downtime & has 0 cartridges
                    QueueGCD(AID.SolidBarrel, //queue Solid Barrel
                        primaryTarget?.Actor, //on the primary target
                        GCDPriority.ForcedCombo); //with priority for forced GCDs
            }

            if (Ammo == MaxCartridges) //if at max cartridges
                QueueGCD(STwithoutOvercap(), //queue the next single-target combo action without overcap protection to save resources for uptime
                    primaryTarget?.Actor, //on the primary target
                    GCDPriority.ForcedCombo); //with priority for forced GCDs
        }
        #endregion

        #region Standard Execution
        if (AOEStrategy == AOEStrategy.AutoBreakCombo) //if Break Combo option is selected
        {
            if (ShouldUseAOE) //if AOE rotation should be used
                QueueGCD(AOEwithoutOvercap(), //queue the next AOE combo action
                    Player, //on Self (no target needed)
                    GCDPriority.Standard); //with priority for 123/12 combo actions
            if (!ShouldUseAOE)
                QueueGCD(STwithoutOvercap(), //queue the next single-target combo action
                    TargetChoice(AOE) //Get target choice
                    ?? primaryTarget?.Actor, //if none, choose primary target
                    GCDPriority.Standard); //with priority for 123/12 combo actions
        }
        if (AOEStrategy == AOEStrategy.AutoFinishCombo) //if Finish Combo option is selected
        {
            QueueGCD(BestRotation(), //queue the next single-target combo action only if combo is finished
                TargetChoice(AOE) //Get target choice
                ?? primaryTarget?.Actor, //if none, choose primary target
                GCDPriority.Standard); //with priority for 123/12 combo actions
        }
        #endregion

        #endregion

        #region Cooldowns
        if (!hold)
        {
            if (ShouldUseNoMercy(nmStrat, primaryTarget?.Actor))
                QueueOGCD(AID.NoMercy,
                    Player,
                    nmStrat is NoMercyStrategy.Force or NoMercyStrategy.ForceW or NoMercyStrategy.ForceQW or NoMercyStrategy.Force1 or NoMercyStrategy.Force1W or NoMercyStrategy.Force1QW or NoMercyStrategy.Force2 or NoMercyStrategy.Force2W or NoMercyStrategy.Force2QW or NoMercyStrategy.Force3 or NoMercyStrategy.Force3W or NoMercyStrategy.Force3QW
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.NoMercy);
            if (ShouldUseZone(zoneStrat, primaryTarget?.Actor))
                QueueOGCD(BestZone,
                    TargetChoice(zone) ?? primaryTarget?.Actor,
                    zoneStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Zone);
            if (ShouldUseBowShock(bowStrat, primaryTarget?.Actor))
                QueueOGCD(AID.BowShock,
                    Player,
                    bowStrat is OGCDStrategy.Force or OGCDStrategy.AnyWeave or OGCDStrategy.EarlyWeave or OGCDStrategy.LateWeave
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.BowShock);
            if (ShouldUseBloodfest(bfStrat, primaryTarget?.Actor))
                QueueOGCD(AID.Bloodfest,
                    TargetChoice(bf) ?? primaryTarget?.Actor,
                    bfStrat is BloodfestStrategy.Force or BloodfestStrategy.ForceW or BloodfestStrategy.Force0 or BloodfestStrategy.Force0W
                    ? OGCDPriority.ForcedOGCD : OGCDPriority.Bloodfest);
            if (ShouldUseSonicBreak(sbStrat, primaryTarget?.Actor))
                QueueGCD(AID.SonicBreak,
                    TargetChoice(sb) ?? primaryTarget?.Actor,
                    sbStrat is >= SonicBreakStrategy.Force
                    ? GCDPriority.ForcedGCD : GCDPriority.SonicBreak);
            if (ShouldUseReign(reignStrat, primaryTarget?.Actor))
                QueueGCD(AID.ReignOfBeasts,
                    TargetChoice(reign) ?? BestSplashTarget?.Actor,
                    reignStrat is ReignStrategy.ForceReign
                    ? GCDPriority.ForcedGCD : GCDPriority.Reign);

            if (!conserve)
            {
                if (ShouldUseDoubleDown(ddStrat, primaryTarget?.Actor))
                    QueueGCD(AID.DoubleDown,
                        primaryTarget?.Actor,
                        ddStrat is GCDStrategy.Force || Ammo == 1
                        ? GCDPriority.ForcedGCD : GCDPriority.DoubleDown);
                if (ShouldUseGnashingFang(gfStrat, primaryTarget?.Actor))
                    QueueGCD(AID.GnashingFang,
                        TargetChoice(gf) ?? primaryTarget?.Actor,
                        gfStrat is GnashingStrategy.ForceGnash
                        ? GCDPriority.ForcedGCD : GCDPriority.GF1);
                if (ShouldUseCartridges(cartStrat, primaryTarget?.Actor))
                {
                    if (cartStrat == CartridgeStrategy.Automatic)
                        QueueGCD(BestCartSpender,
                            TargetChoice(carts) ?? primaryTarget?.Actor,
                            nmCD < 1 && Ammo == 3
                            ? GCDPriority.ForcedGCD : GCDPriority.Gauge);
                    if (cartStrat is CartridgeStrategy.OnlyBS or CartridgeStrategy.ForceBS)
                        QueueGCD(AID.BurstStrike,
                            TargetChoice(carts) ?? primaryTarget?.Actor,
                            GCDPriority.Gauge);
                    if (cartStrat is CartridgeStrategy.ForceFC or CartridgeStrategy.OnlyFC)
                        QueueGCD(BestFatedCircle,
                            Unlocked(AID.FatedCircle) ? Player : primaryTarget?.Actor,
                            GCDPriority.Gauge);
                }
            }
        }
        if (canContinue && (hasBlast || hasRaze || hasRip || hasTear || hasGouge))
            QueueOGCD(BestContinuation,
                TargetChoice(gf) ?? primaryTarget?.Actor,
                CanLateWeaveIn || GCD is 0 ? OGCDPriority.Continuation + 1201 : OGCDPriority.Continuation);
        if (GunComboStep == 1)
            QueueGCD(AID.SavageClaw,
                TargetChoice(gf) ?? primaryTarget?.Actor,
                gfStrat is GnashingStrategy.ForceClaw ? GCDPriority.ForcedGCD : GCDPriority.GF23);
        if (GunComboStep == 2)
            QueueGCD(AID.WickedTalon,
                TargetChoice(gf) ?? primaryTarget?.Actor,
                gfStrat is GnashingStrategy.ForceTalon
                ? GCDPriority.ForcedGCD : GCDPriority.GF23);
        if (GunComboStep == 3)
            QueueGCD(AID.NobleBlood,
                TargetChoice(reign) ?? BestSplashTarget?.Actor,
                reignStrat is ReignStrategy.ForceNoble
                ? GCDPriority.ForcedGCD : GCDPriority.Reign);
        if (GunComboStep == 4)
            QueueGCD(AID.LionHeart,
                TargetChoice(reign) ?? BestSplashTarget?.Actor,
                reignStrat is ReignStrategy.ForceLion
                ? GCDPriority.ForcedGCD : GCDPriority.Reign);
        if (ShouldUseLightningShot(primaryTarget?.Actor, lsStrat))
            QueueGCD(AID.LightningShot,
                TargetChoice(ls) ?? primaryTarget?.Actor,
                lsStrat is >= LightningShotStrategy.Force
                ? GCDPriority.ForcedGCD : GCDPriority.Standard);
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr,
                Player,
                ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0,
                GCD - 0.9f);
        #endregion

        #endregion

        #region AI
        var goalST = primaryTarget?.Actor != null ? Hints.GoalSingleTarget(primaryTarget!.Actor, 3) : null; //Set goal for single target
        var goalAOE = primaryTarget?.Actor != null ? Hints.GoalAOECircle(5) : null; //Set goal for AOE
        var goal = AOEStrategy switch //Set goal based on AOE strategy
        {
            AOEStrategy.ForceSTwithoutO => goalST, //if forced single target
            AOEStrategy.ForceSTwithO => goalST, //if forced 123 combo
            AOEStrategy.ForceAOEwithoutO => goalAOE, //if forced buffs combo
            AOEStrategy.ForceAOEwithO => goalAOE, //if forced AOE action
            _ => goalST != null && goalAOE != null ? Hints.GoalCombined(goalST, goalAOE, 2) : goalAOE //otherwise, combine goals
        };
        if (goal != null) //if goal is set
            Hints.GoalZones.Add(goal); //add goal to zones
        #endregion
    }

    #region Rotation Helpers
    private AID BestRotation() => ComboLastMove switch
    {
        //ST
        AID.SolidBarrel => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
        AID.BrutalShell => STwithoutOvercap(),
        AID.KeenEdge => STwithoutOvercap(),
        //AOE
        AID.DemonSlaughter => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
        AID.DemonSlice => AOEwithoutOvercap(),
        _ => ShouldUseAOE ? AOEwithoutOvercap() : STwithoutOvercap(),
    };

    #region Single-Target Helpers
    private AID STwithOvercap() => ComboLastMove switch //with Overcap protection
    {
        AID.BrutalShell => Ammo == MaxCartridges ? BestCartSpender : AID.SolidBarrel, //if Brutal Shell is last move, use Solid Barrel regardless of ammo count
        AID.KeenEdge => AID.BrutalShell, //if Keen Edge is last move, use Brutal Shell
        _ => AID.KeenEdge, //start with Keen Edge
    };
    private AID STwithoutOvercap() => ComboLastMove switch //without Overcap protection
    {
        AID.BrutalShell => AID.SolidBarrel, //if Brutal Shell is last move, use Solid Barrel
        AID.KeenEdge => AID.BrutalShell, //if Keen Edge is last move, use Brutal Shell
        _ => AID.KeenEdge, //start with Keen Edge
    };
    #endregion

    #region AOE Helpers
    private AID AOEwithOvercap() => ComboLastMove switch //with Overcap protection
    {
        AID.DemonSlice => Ammo == MaxCartridges ? BestCartSpender : AID.DemonSlaughter, //if full ammo & Demon Slice is last move, use best cartridge spender
        _ => AID.DemonSlice, //start with Demon Slice
    };
    private AID AOEwithoutOvercap() => ComboLastMove switch //without Overcap protection
    {
        AID.DemonSlice => AID.DemonSlaughter, //if not at max ammo, use Demon Slaughter
        _ => AID.DemonSlice, //start with Demon Slice
    };
    #endregion

    #endregion

    #region Cooldown Helpers
    private bool ShouldUseNoMercy(NoMercyStrategy strategy, Actor? target) => strategy switch
    {
        NoMercyStrategy.Automatic => Player.InCombat && target != null && canNM &&
            ((Unlocked(AID.DoubleDown) && (inOdd && Ammo >= 2 || !inOdd && Ammo < 3)) ||
            (!Unlocked(AID.DoubleDown) && CanQuarterWeaveIn &&
            ((Unlocked(AID.Bloodfest) && Ammo >= 1) || (!Unlocked(AID.Bloodfest) && canGF) ||
            !Unlocked(AID.GnashingFang)))),
        NoMercyStrategy.Force => canNM,
        NoMercyStrategy.ForceW => canNM && CanWeaveIn,
        NoMercyStrategy.ForceQW => canNM && CanQuarterWeaveIn,
        NoMercyStrategy.Force1 => canNM && Ammo == 1,
        NoMercyStrategy.Force1W => canNM && CanWeaveIn && Ammo == 1,
        NoMercyStrategy.Force1QW => canNM && CanQuarterWeaveIn && Ammo == 1,
        NoMercyStrategy.Force2 => canNM && Ammo == 2,
        NoMercyStrategy.Force2W => canNM && CanWeaveIn && Ammo == 2,
        NoMercyStrategy.Force2QW => canNM && CanQuarterWeaveIn && Ammo == 2,
        NoMercyStrategy.Force3 => canNM && Ammo == 3,
        NoMercyStrategy.Force3W => canNM && CanWeaveIn && Ammo == 3,
        NoMercyStrategy.Force3QW => canNM && CanQuarterWeaveIn && Ammo == 3,
        NoMercyStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBloodfest(BloodfestStrategy strategy, Actor? target) => strategy switch
    {
        BloodfestStrategy.Automatic => Player.InCombat && target != null && canBF && Ammo == 0,
        BloodfestStrategy.Force => canBF,
        BloodfestStrategy.ForceW => canBF && CanWeaveIn,
        BloodfestStrategy.Force0 => canBF && Ammo == 0,
        BloodfestStrategy.Force0W => canBF && Ammo == 0 && CanWeaveIn,
        BloodfestStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseZone(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && In3y(target) && canZone && nmCD is < 57.55f and > 17,
        OGCDStrategy.Force => canZone,
        OGCDStrategy.AnyWeave => canZone && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canZone && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canZone && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBowShock(OGCDStrategy strategy, Actor? target) => strategy switch
    {
        OGCDStrategy.Automatic => Player.InCombat && ActionReady(AID.BowShock) && In5y(target) && nmCD is < 57.55f and >= 40,
        OGCDStrategy.Force => canBow,
        OGCDStrategy.AnyWeave => canBow && CanWeaveIn,
        OGCDStrategy.EarlyWeave => canBow && CanEarlyWeaveIn,
        OGCDStrategy.LateWeave => canBow && CanLateWeaveIn,
        OGCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseCartridges(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic => ShouldUseFC ? ShouldUseFatedCircle(CartridgeStrategy.Automatic, target) : ShouldUseBurstStrike(CartridgeStrategy.Automatic, target),
        CartridgeStrategy.OnlyBS => ShouldUseBurstStrike(CartridgeStrategy.Automatic, target),
        CartridgeStrategy.OnlyFC => ShouldUseFatedCircle(CartridgeStrategy.Automatic, target),
        CartridgeStrategy.ForceBS => canBS,
        CartridgeStrategy.ForceFC => canFC,
        CartridgeStrategy.Conserve => false,
        _ => false
    };
    private bool ShouldUseDoubleDown(GCDStrategy strategy, Actor? target) => strategy switch
    {
        GCDStrategy.Automatic => Player.InCombat && target != null && In5y(target) && canDD && hasNM,
        GCDStrategy.Force => canDD,
        GCDStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseGnashingFang(GnashingStrategy strategy, Actor? target) => strategy switch
    {
        GnashingStrategy.Automatic => Player.InCombat && target != null && In3y(target) && canGF && (nmLeft > 0 || hasNM || nmCD is < 35 and > 17),
        GnashingStrategy.ForceGnash => canGF,
        GnashingStrategy.ForceClaw => Player.InCombat && GunComboStep == 1,
        GnashingStrategy.ForceTalon => Player.InCombat && GunComboStep == 2,
        GnashingStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseBurstStrike(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic => Player.InCombat && target != null && In3y(target) && canBS &&
            (hasNM || (!(bfCD is <= 90 and >= 30) && nmCD < 1 && Ammo == 3)) ||
            Ammo == MaxCartridges && ComboLastMove is AID.BrutalShell or AID.DemonSlice,
        _ => false
    };
    private bool ShouldUseFatedCircle(CartridgeStrategy strategy, Actor? target) => strategy switch
    {
        CartridgeStrategy.Automatic => Player.InCombat && target != null && In5y(target) && canFC &&
            (hasNM || (!(bfCD is <= 90 and >= 30) && nmCD < 1 && Ammo == 3)) ||
            Ammo == MaxCartridges && ComboLastMove is AID.BrutalShell or AID.DemonSlice,
        _ => false
    };
    private bool ShouldUseSonicBreak(SonicBreakStrategy strategy, Actor? target) => strategy switch
    {
        SonicBreakStrategy.Automatic => Player.InCombat && In3y(target) && canBreak,
        SonicBreakStrategy.Force => canBreak,
        SonicBreakStrategy.Early => nmCD > 55 || hasBreak,
        SonicBreakStrategy.Late => nmLeft <= SkSGCDLength,
        SonicBreakStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseReign(ReignStrategy strategy, Actor? target) => strategy switch
    {
        ReignStrategy.Automatic => Player.InCombat && target != null && canReign && hasNM && GunComboStep == 0,
        ReignStrategy.ForceReign => canReign,
        ReignStrategy.ForceNoble => Player.InCombat && GunComboStep == 3,
        ReignStrategy.ForceLion => Player.InCombat && GunComboStep == 4,
        ReignStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseLightningShot(Actor? target, LightningShotStrategy strategy) => strategy switch
    {
        LightningShotStrategy.OpenerFar => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD() && !In3y(target),
        LightningShotStrategy.OpenerForce => (Player.InCombat || World.Client.CountdownRemaining < 0.8f) && IsFirstGCD(),
        LightningShotStrategy.Force => true,
        LightningShotStrategy.Allow => !In3y(target),
        LightningShotStrategy.Forbid => false,
        _ => false
    };
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs => nmCD < 5 && bfCD < 15,
        PotionStrategy.Immediate => true,
        _ => false
    };
    #endregion
}

using FFXIVClientStructs.FFXIV.Client.Game.Gauge;

namespace BossMod.Autorotation;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiPLD(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    // Actions tracked for Cooldown Planner execution
    public enum Track
    {
        AoE,              // AoE and single-target actions
        Burst,            // Burst damage actions
        Potion,           // Potion usage tracking
        Atonement,        // Tracking for Atonement
        FightOrFlight,    // Tracking for Fight or Flight
        Requiescat,       // Tracking for Requiescat
        GoringBlade,      // Tracking for Sonic Break
        BladeCombo,       // Tracking for Blade Combo actions
        HolySpirit,       // Tracking for Burst Strike
        HolyCircle,       // Tracking for Fated Circle
        SpiritsWithin,    // Tracking for Spirits Within
        CircleOfScorn,    // Tracking for Circle of Scorn
        BladeOfHonor,     // Tracking for Blade of Honor
        Dash,             // Tracking for Intervene
        Ranged,           // Tracking for ranged attacks
    }

    // Strategy definitions
    public enum AOEStrategy
    {
        ForceST,          // Force single-target actions without exceeding gauge
        ForceAoE,         // Force AoE actions without exceeding gauge
        Auto,             // Auto decision based on target count, may break combo
        AutoFinishCombo,  // Auto decision with combo finish if possible
    }

    public enum BurstStrategy
    {
        Automatic,        // Automatically execute burst actions
        Conserve,         // Conserve MP and cooldowns
        UnderRaidBuffs,   // Execute under raid buffs for maximum effect
        UnderPotion,      // Execute while under potion effects
    }

    public enum PotionStrategy
    {
        Manual,             // Manual potion usage
        AlignWithRaidBuffs, // Align potion usage with raid buffs
        Immediate           // Use potions immediately when available
    }

    public enum AtonementStrategy
    {
        Automatic,         // Automatically use Atonement
        ForceAtonement,    // Force use of Atonement
        ForceSupplication, // Force use of Savage Claw
        ForceSepulchre,    // Force use of Wicked Talon
        Delay              // Delay use of Atonement
    }

    public enum BladeComboStrategy
    {
        Automatic,        // Automatically execute Blade Combo
        ForceConfiteor,   // Force use of Confiteor
        ForceFaith,       // Force use of Blade of Faith
        ForceTruth,       // Force use of Blade of Truth
        ForceValor,       // Force use of Blade of Valor
        Delay             // Delay use of Confiteor & Blade Combo
    }

    public enum DashStrategy
    {
        Automatic,        // Automatically use Intervene
        Force,            // Force use of Intervene
        Conserve1,        // Conserve a use of Intervene
        GapClose,         // Use Intervene as a gap closer
        Delay             // Delay use of Intervene
    }

    public enum RangedStrategy
    {
        OpenerRanged,     // Use Shield Lob as part of the opener
        OpenerRangedCast, // Use Holy Spirit as part of the opener
        Opener,           // Use Shield Lob at the start of combat
        OpenerCast,       // Use Holy Spirit at the start of combat
        Force,            // Always use Shield Lob
        ForceCast,        // Always use Holy Spirit
        Ranged,           // Use Shield Lob for ranged attacks
        RangedCast,       // Use Holy Spirit for ranged attacks
        Forbid            // Prohibit use of Shield Lob
    }

    public enum OffensiveStrategy
    {
        Automatic,        // Automatically decide on offensive actions
        Force,            // Force the use of offensive actions
        Delay             // Delay offensive actions for strategic reasons
    }

    public static RotationModuleDefinition Definition()
    {
        // Module title & signature
        var res = new RotationModuleDefinition("PLD (Akechi)", "Standard Rotation Module", "Akechi", RotationModuleQuality.Ok, BitMask.Build((int)Class.PLD), 100);

        // AoE Strategy
        res.Define(Track.AoE).As<AOEStrategy>("AoE", uiPriority: 200)
            .AddOption(AOEStrategy.ForceST, "Force ST", "Use single-target rotation with overcap protection")
            .AddOption(AOEStrategy.ForceAoE, "Force AoE", "Use AoE rotation with overcap protection")
            .AddOption(AOEStrategy.Auto, "Auto", "Choose AoE if 3+ targets; otherwise, use single-target")
            .AddOption(AOEStrategy.AutoFinishCombo, "Auto Finish Combo", "Choose AoE if 3+ targets; otherwise, finish combo if possible");

        // Burst Strategy
        res.Define(Track.Burst).As<BurstStrategy>("Burst", uiPriority: 190)
            .AddOption(BurstStrategy.Automatic, "Automatic", "Spend cartridges optimally")
            .AddOption(BurstStrategy.Conserve, "Conserve", "Conserve MP and cooldowns")
            .AddOption(BurstStrategy.UnderRaidBuffs, "Under Raid Buffs", "Spend under raid buffs; conserve otherwise")
            .AddOption(BurstStrategy.UnderPotion, "Under Potion", "Spend under potion; conserve otherwise");

        // Potion Strategy
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 180)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use potions automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "Align With Raid Buffs", "Align potion usage with raid buffs", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use potions immediately when available", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionStr);

        // Atonement Strategy
        res.Define(Track.Atonement).As<AtonementStrategy>("Atonement", "Atone", uiPriority: 160)
            .AddOption(AtonementStrategy.Automatic, "Automatic", "Normal use of Atonement")
            .AddOption(AtonementStrategy.ForceAtonement, "Force Atonement", "Force use of Atonement", 30, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSupplication, "Force Supplication", "Force use of Savage Claw", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.ForceSepulchre, "Force Sepulchre", "Force use of Wicked Talon", 0, 0, ActionTargets.Hostile, 76)
            .AddOption(AtonementStrategy.Delay, "Delay", "Delay use of Atonement", 0, 0, ActionTargets.None, 60)
            .AddAssociatedActions(PLD.AID.Atonement, PLD.AID.Supplication, PLD.AID.Sepulchre);

        // Blade Combo Strategy
        res.Define(Track.BladeCombo).As<BladeComboStrategy>("Blade Combo", "Blades", uiPriority: 160)
            .AddOption(BladeComboStrategy.Automatic, "Automatic", "Normal use of Blade Combo")
            .AddOption(BladeComboStrategy.ForceConfiteor, "Force", "Force use of Confiteor", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(BladeComboStrategy.ForceFaith, "Force Faith", "Force use of Blade of Faith", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceTruth, "Force Truth", "Force use of Blade of Truth", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.ForceValor, "Force Valor", "Force use of Blade of Valor", 0, 0, ActionTargets.Hostile, 90)
            .AddOption(BladeComboStrategy.Delay, "Delay", "Delay use of Confiteor & Blade Combo", 0, 0, ActionTargets.None, 80)
            .AddAssociatedActions(PLD.AID.BladeOfFaith, PLD.AID.BladeOfTruth, PLD.AID.BladeOfValor);

        // Dash Strategy
        res.Define(Track.Dash).As<DashStrategy>("Intervene", "Dash", uiPriority: 150)
            .AddOption(DashStrategy.Automatic, "Automatic", "Normal use of Intervene")
            .AddOption(DashStrategy.Force, "Force", "Force use of Intervene", 30, 0, ActionTargets.Hostile, 74)
            .AddOption(DashStrategy.Conserve1, "Conserve 1", "Conserve one use of Intervene for manual usage", 30, 0, ActionTargets.Hostile, 74)
            .AddOption(DashStrategy.GapClose, "Gap Close", "Use as gap closer if outside melee range", 30, 0, ActionTargets.None, 74)
            .AddOption(DashStrategy.Delay, "Delay", "Delay use of Intervene", 30, 0, ActionTargets.None, 74)
            .AddAssociatedActions(PLD.AID.Intervene);

        // Ranged Strategy
        res.Define(Track.Ranged).As<RangedStrategy>("Ranged", uiPriority: 20)
            .AddOption(RangedStrategy.OpenerRanged, "Opener Ranged", "Use Shield Lob as the first GCD if outside melee range")
            .AddOption(RangedStrategy.OpenerRangedCast, "Opener Ranged Cast", "Use Holy Spirit as the first GCD if outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.Opener, "Opener", "Use Shield Lob as the first GCD regardless of range")
            .AddOption(RangedStrategy.OpenerCast, "Opener Cast", "Use Holy Spirit as the first GCD regardless of range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.Force, "Force", "Always use Shield Lob regardless of conditions")
            .AddOption(RangedStrategy.ForceCast, "Force Cast", "Always use Holy Spirit regardless of conditions", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.Ranged, "Ranged", "Use Shield Lob when outside melee range")
            .AddOption(RangedStrategy.RangedCast, "Ranged Cast", "Use Holy Spirit when outside melee range", 0, 0, ActionTargets.Hostile, 64)
            .AddOption(RangedStrategy.Forbid, "Forbid", "Prohibit use of Shield Lob")
            .AddAssociatedActions(PLD.AID.ShieldLob, PLD.AID.HolySpirit);

        // Fight or Flight Strategy
        res.Define(Track.FightOrFlight).As<OffensiveStrategy>("Fight or Flight", "FoF", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Fight or Flight normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Fight or Flight", 60, 20, ActionTargets.Self, 2)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Fight or Flight", 0, 0, ActionTargets.None, 2)
            .AddAssociatedActions(PLD.AID.FightOrFlight);

        // Spirits Within Strategy
        res.Define(Track.SpiritsWithin).As<OffensiveStrategy>("Spirits Within", "S.Within", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Spirits Within normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Spirits Within / Expiacion", 30, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Spirits Within / Expiacion", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(PLD.AID.SpiritsWithin, PLD.AID.Expiacion);

        // Circle of Scorn Strategy
        res.Define(Track.CircleOfScorn).As<OffensiveStrategy>("Circle of Scorn Strategy", "Circle of Scorn", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Circle of Scorn normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Circle of Scorn ASAP", 60, 15, ActionTargets.Hostile, 50)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Circle of Scorn", 0, 0, ActionTargets.None, 50)
            .AddAssociatedActions(PLD.AID.CircleOfScorn);

        // Goring Blade Strategy
        res.Define(Track.GoringBlade).As<OffensiveStrategy>("Goring Blade Strategy", "Sonic Break", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Goring Blade normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Goring Blade ASAP", 0, 30, ActionTargets.Hostile, 54)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Goring Blade", 0, 0, ActionTargets.None, 54)
            .AddAssociatedActions(PLD.AID.GoringBlade);

        // Holy Spirit Strategy
        res.Define(Track.HolySpirit).As<OffensiveStrategy>("Holy Spirit Strategy", "Burst Strike", uiPriority: 140)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Holy Spirit normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Holy Spirit ASAP", 0, 0, ActionTargets.Hostile, 30)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Holy Spirit", 0, 0, ActionTargets.None, 30)
            .AddAssociatedActions(PLD.AID.HolySpirit);

        // Requiescat Strategy
        res.Define(Track.Requiescat).As<OffensiveStrategy>("Requiescat Strategy", "Requiescat", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Requiescat normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Requiescat / Imperator", 120, 0, ActionTargets.Hostile, 68)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Requiescat / Imperator", 0, 0, ActionTargets.None, 68)
            .AddAssociatedActions(PLD.AID.Requiescat, PLD.AID.Imperator);

        // Holy Circle Strategy
        res.Define(Track.HolyCircle).As<OffensiveStrategy>("Holy Circle Strategy", "Fated Circle", uiPriority: 140)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Holy Circle normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Holy Circle ASAP", 0, 0, ActionTargets.Hostile, 72)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Holy Circle", 0, 0, ActionTargets.None, 72)
            .AddAssociatedActions(PLD.AID.HolyCircle);

        // Blade of Honor Strategy
        res.Define(Track.BladeOfHonor).As<OffensiveStrategy>("Blade of Honor Strategy", "Blade of Honor", uiPriority: 150)
            .AddOption(OffensiveStrategy.Automatic, "Automatic", "Use Blade of Honor normally")
            .AddOption(OffensiveStrategy.Force, "Force", "Force use of Blade of Honor ASAP", 0, 0, ActionTargets.Hostile, 100)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay use of Blade of Honor", 0, 0, ActionTargets.None, 100)
            .AddAssociatedActions(PLD.AID.BladeOfHonor);

        return res;
    }

    public enum GCDPriority //Priority for GCDs used
    {
        None = 0,
        Combo123 = 350,
        NormalHS = 600,
        NormalGB = 670,
        AC23 = 660,
        NormalBC = 680,
        AC1 = 690,
        NormalGCD = 700,
        ForcedShieldLob = 850,
        ForcedGoringBlade = 860,
        ForcedHolySpirit = 870,
        ForcedBladeCombo = 880,
        ForcedAtonement = 890,
        ForcedGCD = 900,
        MaxedOut = 980,
    }

    public enum OGCDPriority //Priority for oGCDs used
    {
        None = 0,
        NormalOGCD = 500,
        BladeOfHonor = 520,
        Intervene = 530,
        SpiritsWithin = 540,
        CircleOfScorn = 550,
        Requiescat = 675,
        FightOrFlight = 700,
        ForcedOGCD = 900,
        Potion = 910,
        MaxedOut = 980,
    }

    public int Oath; // Current oath gauge
    public int BladeComboStep; // Current combo step for Confiteor combo

    public float GCDLength; // Current GCD length, adjusted by skill speed/haste (baseline: 2.5s)

    public float PotionLeft; // Time left on potion buff (typically 30s)
    public float RaidBuffsLeft; // Time left on active raid-wide buffs (typically 20s-22s)
    public float RaidBuffsIn; // Time until raid-wide buffs are reapplied (typically 20s-22s)

    public float BurstWindowLeft; // Time left in the current burst window (typically 20s-22s)
    public float BurstWindowIn; // Time until the next burst window (typically 20s-22s)

    // Buff and cooldown checks
    public float fofCD; // Remaining cooldown of Fight or Flight ability
    public float fofLeft; // Remaining cooldown of Fight or Flight ability
    public float reqCD; // Remaining cooldown of Requiescat ability
    public (float Left, int Stacks) req; // Remaining cooldown of Fight or Flight ability
    public uint playerMP; // Current MP of the player

    // Buff presence checks
    public bool hasFoF; // Check if Fight or Flight buff is active
    public bool hasReq; // Check if Requiescat buff is active
    public bool hasMight; // Check if Divine Might buff is active
    public bool hasMPforMight; // Check if sufficient MP for Holy Spirit using Divine Might
    public bool hasMPforReq; // Check if sufficient MP for Holy Spirit under Requiescat

    // Phase and condition checks
    public bool isDivineMightExpiring; // Check if Divine Might is about to expire
    public bool isAtonementExpiring; // Check if any Atonement buffs are about to expire

    public PLD.AID NextGCD; //Next global cooldown action to be used (needed for cartridge management)
    private GCDPriority NextGCDPrio; //Priority of the next GCD, used for decision making on cooldowns

    //Check if the desired ability is unlocked
    private bool Unlocked(PLD.AID aid) => ActionUnlocked(ActionID.MakeSpell(aid));

    //Check if the desired trait is unlocked
    private bool Unlocked(PLD.TraitID tid) => TraitUnlocked((uint)tid);

    //Get remaining cooldown time for the specified action
    private float CD(PLD.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining;

    //Check if we can fit an additional GCD within the provided deadline
    private bool CanFitGCD(float deadline, int extraGCDs = 0) => GCD + GCDLength * extraGCDs < deadline;

    //Get the last action used in the combo sequence
    private PLD.AID ComboLastMove => (PLD.AID)World.Client.ComboState.Action;

    //Check if the target is within melee range (3 yalms)
    private bool In3y(Actor? target) => Player.DistanceToHitbox(target) <= 3;

    //Check if the target is within 5 yalms
    private bool In5y(Actor? target) => Player.DistanceToHitbox(target) <= 4.75;

    //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool ActionReady(PLD.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f;

    //Check if this is the first GCD in combat
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f;

    //Returns the number of targets hit by AoE within a 5-yalm radius around the player
    private int NumTargetsHitByAoE() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5);

    //Checks if the potion should be used before raid buffs expire
    private bool IsPotionBeforeRaidbuffs() => RaidBuffsLeft == 0 && PotionLeft > RaidBuffsIn + 17.5f;

    //Checks if Status effect is on self
    public bool HasEffect<SID>(SID sid) where SID : Enum => Player.FindStatus((uint)(object)sid, Player.InstanceID) != null;
    public float CombatTimer => (float)(World.CurrentTime - Manager.CombatStart).TotalSeconds;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {

        var gauge = World.Client.GetGauge<PaladinGauge>(); //Retrieve Paladin gauge
        Oath = gauge.OathGauge; //Current cartridges
        BladeComboStep = gauge.ConfiteorComboStep; //Combo step for Confiteor combo

        // Buff and cooldown checks
        fofCD = CD(PLD.AID.FightOrFlight); // Remaining cooldown of Fight or Flight ability
        fofLeft = SelfStatusLeft(PLD.SID.FightOrFlight); // Remaining duration of Fight or Flight buff
        reqCD = CD(PLD.AID.Requiescat); // Remaining cooldown of Requiescat ability
        playerMP = Player.HPMP.CurMP; // Current MP of the player

        // Buff presence checks
        hasFoF = fofCD is >= 40 and <= 60; // Check if Fight or Flight buff is active
        hasReq = HasEffect(PLD.AID.Requiescat); // Check if Requiescat buff is active
        hasMight = HasEffect(PLD.SID.DivineMight); // Check if Divine Might buff is active
        hasMPforMight = playerMP >= 1000; // Check if sufficient MP for Holy Spirit using Divine Might
        hasMPforReq = playerMP >= 1000 * 3.6; // Check if sufficient MP for Holy Spirit under Requiescat

        // Phase and condition checks
        isDivineMightExpiring = SelfStatusLeft(PLD.SID.DivineMight) < 6; // Check if Divine Might is about to expire
        isAtonementExpiring =
          (HasEffect(PLD.SID.AtonementReady) && SelfStatusLeft(PLD.SID.AtonementReady) < 6) ||
          (HasEffect(PLD.SID.SupplicationReady) && SelfStatusLeft(PLD.SID.SupplicationReady) < 6) ||
          (HasEffect(PLD.SID.SepulchreReady) && SelfStatusLeft(PLD.SID.SepulchreReady) < 6); // Check if any Atonement buffs are about to expire

        GCDLength = ActionSpeed.GCDRounded(World.Client.PlayerStats.SkillSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on skill speed and haste

        //Buff durations
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)

        //Raid buff timings
        (RaidBuffsLeft, RaidBuffsIn) = EstimateRaidBuffTimings(primaryTarget);

        //Next GCD action
        NextGCD = PLD.AID.None;
        NextGCDPrio = GCDPriority.None;

        //Define ST/AoE strategy and determine number of targets
        var AOEStrategy = strategy.Option(Track.AoE).As<AOEStrategy>();
        var AoETargets = AOEStrategy switch
        {
            AOEStrategy.ForceST => NumTargetsHitByAoE() > 0 ? 1 : 0,
            AOEStrategy.ForceAoE => NumTargetsHitByAoE() > 0 ? 100 : 0,
            _ => NumTargetsHitByAoE()
        };

        //Burst (raid buff) windows typically last 20s every 120s
        var burst = strategy.Option(Track.Burst);
        var burstStrategy = burst.As<BurstStrategy>();
        var hold = burstStrategy == BurstStrategy.Conserve; //Determine if conserving cartridges

        //Calculate the burst window based on the current strategy
        (BurstWindowIn, BurstWindowLeft) = burstStrategy switch
        {
            BurstStrategy.Automatic => (RaidBuffsIn, IsPotionBeforeRaidbuffs() ? 0 : Math.Max(PotionLeft, RaidBuffsLeft)),
            BurstStrategy.UnderRaidBuffs => (RaidBuffsIn, RaidBuffsLeft),
            BurstStrategy.UnderPotion => (PotionCD, PotionLeft),
            _ => (0, 0)
        };

        //GCD minimal conditions
        var canFoF = Unlocked(PLD.AID.FightOrFlight) && ActionReady(PLD.AID.FightOrFlight);
        var canReq = Unlocked(PLD.AID.Requiescat) && ActionReady(PLD.AID.Requiescat);
        var canScorn = Unlocked(PLD.AID.CircleOfScorn) && ActionReady(PLD.AID.CircleOfScorn);
        var canSpirit = Unlocked(PLD.AID.SpiritsWithin) && ActionReady(PLD.AID.SpiritsWithin);
        var canGB = Unlocked(PLD.AID.GoringBlade) && ActionReady(PLD.AID.GoringBlade);
        var canAtone = Unlocked(PLD.AID.Atonement);
        var canDash = Unlocked(PLD.AID.Intervene) && CD(PLD.AID.Intervene) > 29f;
        var canConfiteor = Unlocked(PLD.AID.Confiteor) && HasEffect(PLD.SID.ConfiteorReady) && BladeComboStep is 0;
        var canBlade = Unlocked(PLD.AID.BladeOfValor) && BladeComboStep is not 0;

        //Determine and queue combo action
        var (comboAction, comboPrio) = ComboActionPriority(AOEStrategy, AoETargets, burstStrategy, burst.Value.ExpireIn);
        QueueGCD(comboAction, comboAction is PLD.AID.TotalEclipse or PLD.AID.Prominence ? Player : primaryTarget, comboPrio);

        //Focused actions for AoE strategies
        if (AOEStrategy == AOEStrategy.ForceST)
            QueueGCD(NextComboSingleTarget(), primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy == AOEStrategy.ForceAoE)
            QueueGCD(NextComboAoE(), primaryTarget, GCDPriority.ForcedGCD);

        //Fight or Flight execution
        if (!hold && ShouldUseFightOrFlight(strategy.Option(Track.FightOrFlight).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(PLD.AID.FightOrFlight, Player, OGCDPriority.FightOrFlight);

        //SpiritsWithin execution
        if (!hold && ShouldUseSpiritsWithin(strategy.Option(Track.SpiritsWithin).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(Unlocked(PLD.AID.Expiacion) ? PLD.AID.Expiacion : PLD.AID.SpiritsWithin, primaryTarget, OGCDPriority.SpiritsWithin);

        //CircleOfScorn execution
        if (!hold && ShouldUseCircleOfScorn(strategy.Option(Track.CircleOfScorn).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(PLD.AID.CircleOfScorn, primaryTarget, OGCDPriority.CircleOfScorn);

        //Requiescat execution
        if (!hold && ShouldUseRequiescat(strategy.Option(Track.Requiescat).As<OffensiveStrategy>(), primaryTarget))
            QueueOGCD(Unlocked(PLD.AID.Imperator) ? PLD.AID.Imperator : PLD.AID.Requiescat, primaryTarget, OGCDPriority.Requiescat);

        //Atonement execution
        if (!hold && canAtone && ShouldUseAtonement(strategy.Option(Track.Atonement).As<AtonementStrategy>(), primaryTarget))
            QueueGCD(PLD.AID.Atonement, primaryTarget, GCDPriority.AC1);

        //Atonement's Combo execution
        if (BladeComboStep is 1)
            QueueGCD(PLD.AID.Supplication, primaryTarget, GCDPriority.AC23);
        if (BladeComboStep is 2)
            QueueGCD(PLD.AID.Sepulchre, primaryTarget, GCDPriority.AC23);

        //Confiteor execution
        if (canConfiteor && BladeComboStep is 0 && ShouldUseBladeCombo(strategy.Option(Track.BladeCombo).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(PLD.AID.Confiteor, primaryTarget, GCDPriority.NormalGCD);

        //Blade Combo execution
        if (BladeComboStep is 1)
            QueueGCD(PLD.AID.BladeOfFaith, primaryTarget, GCDPriority.NormalGCD);
        if (BladeComboStep is 2)
            QueueGCD(PLD.AID.BladeOfTruth, primaryTarget, GCDPriority.NormalGCD);
        if (BladeComboStep is 3)
            QueueGCD(PLD.AID.BladeOfValor, primaryTarget, GCDPriority.NormalGCD);

        //Sonic Break execution
        var forceBreak = strategy.Option(Track.GoringBlade).As<OffensiveStrategy>();
        if (canGB && hasFoF)
        {
            if (forceBreak == OffensiveStrategy.Force) //Force without breaking Autorot
            {
                QueueGCD(PLD.AID.GoringBlade, primaryTarget, GCDPriority.ForcedGoringBlade);
            }
            else if (ShouldUseGoringBlade(forceBreak, primaryTarget))
            {
                QueueGCD(PLD.AID.GoringBlade, primaryTarget, GCDPriority.NormalGB);
            }
        }

        //Burst Strike execution
        if (canBSlv80 && ShouldUseHolySpirit(strategy.Option(Track.HolySpirit).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(PLD.AID.HolySpirit, primaryTarget, GCDPriority.NormalHS);

        //Fated Circle execution
        if (canFC && ShouldUseHolyCircle(strategy.Option(Track.HolyCircle).As<OffensiveStrategy>(), primaryTarget))
            QueueGCD(PLD.AID.HolySpirit, primaryTarget, GCDPriority.NormalHS);
        else if (!canFC && canBSlv70)
        {
            var action = UseCorrectBS(AoETargets);
            QueueGCD(action, primaryTarget, GCDPriority.NormalHS);
        }

        //Lightning Shot execution
        if (ShouldUseShieldLob(primaryTarget, strategy.Option(Track.ShieldLob).As<RangedStrategy>()))
            QueueGCD(PLD.AID.ShieldLob, primaryTarget, GCDPriority.ForcedShieldLob);

        //Potion execution
        if (ShouldUsePotion(strategy.Option(Track.Potion).As<PotionStrategy>()))
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionStr, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
    }

    //QueueGCD execution
    private void QueueGCD(PLD.AID aid, Actor? target, GCDPriority prio)
    {
        if (prio != GCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, ActionQueue.Priority.High + (int)prio);
            if (prio > NextGCDPrio)
            {
                NextGCD = aid;
                NextGCDPrio = prio;
            }
        }
    }

    //QueueOGCD execution
    private void QueueOGCD(PLD.AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Medium)
    {
        if (prio != OGCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
        }
    }

    private PLD.AID NextComboSingleTarget() => ComboLastMove switch //Determines the next single-target action based on the last action used
    {
        PLD.AID.RiotBlade => Unlocked(PLD.AID.RoyalAuthority) ? PLD.AID.RoyalAuthority : PLD.AID.RageOfHalone,
        PLD.AID.FastBlade => PLD.AID.RiotBlade,
        _ => PLD.AID.FastBlade,
    };

    private PLD.AID NextComboAoE() => ComboLastMove switch //Determines the next AoE action based on the last action used
    {
        PLD.AID.TotalEclipse => PLD.AID.Prominence,
        _ => PLD.AID.TotalEclipse,
    };

    private (PLD.AID, GCDPriority) ComboActionPriority(AOEStrategy aoeStrategy, int AoETargets, BurstStrategy burstStrategy, float burstStrategyExpire)
    {
        //Determine how many combo steps are remaining based on the last action
        var comboStepsRemaining = ComboLastMove switch
        {
            PLD.AID.FastBlade => Unlocked(PLD.AID.RiotBlade) ? 2 : Unlocked(PLD.AID.RoyalAuthority) ? 1 : 0,
            PLD.AID.TotalEclipse => Unlocked(PLD.AID.Prominence) ? 1 : 0,
            _ => 0
        };

        //Check if we can fit the GCD based on remaining time
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining))
            comboStepsRemaining = 0;

        var doingAOECombo = ComboLastMove == PLD.AID.TotalEclipse;

        //Determine if an AoE action is desirable based on target count and strategy
        var wantAOEAction = Unlocked(PLD.AID.TotalEclipse) && aoeStrategy switch
        {
            AOEStrategy.ForceST => false,
            AOEStrategy.ForceAoE => true,
            AOEStrategy.Auto => AoETargets >= 3,
            AOEStrategy.AutoFinishCombo => comboStepsRemaining > 0
                ? doingAOECombo ? AoETargets >= 3 : AoETargets >= 2,
            _ => false
        };

        //Reset combo steps if the desired action does not match the current combo type
        if (comboStepsRemaining > 0 && wantAOEAction != doingAOECombo)
            comboStepsRemaining = 0;

        var nextAction = wantAOEAction ? NextComboAoE() : NextComboSingleTarget();

        //Return combo priority based on the ability to fit GCDs and remaining combo steps
        if (comboStepsRemaining > 0 && !CanFitGCD(World.Client.ComboState.Remaining, 1))
            return (nextAction, GCDPriority.Combo123);

        //Return normal combo action priority based on ammo risks
        return (nextAction, GCDPriority.Combo123);
    }

    //Determines when to use Lightning Shot
    private bool ShouldUseRanged(Actor? target, RangedStrategy strategy) => strategy switch
    {
        RangedStrategy.OpenerRanged => IsFirstGCD() && !In3y(target),
        RangedStrategy.Opener => IsFirstGCD(),
        RangedStrategy.Force => true,
        RangedStrategy.Ranged => !In3y(target),
        RangedStrategy.Forbid => false,
        _ => false
    };

    //Determines when to use Fight or Flight
    private bool ShouldUseFightOrFlight(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && target != null && ActionReady(PLD.AID.FightOrFlight),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Requiescat
    private bool ShouldUseRequiescat(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && target != null && ActionReady(PLD.AID.Requiescat) && hasFoF,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use SpiritsWithin
    private bool ShouldUseSpiritsWithin(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && fofCD is < 57.55f and > 17 &&
            ActionReady(Unlocked(PLD.AID.Expiacion) ? PLD.AID.Expiacion : PLD.AID.SpiritsWithin),
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use CircleOfScorn
    private bool ShouldUseCircleOfScorn(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && ActionReady(PLD.AID.CircleOfScorn) && In5y(target) && fofCD is < 57.55f and > 17,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Sonic Break
    private bool ShouldUseGoringBlade(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) && fofLeft <= GCDLength,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Double Down
    private bool ShouldUseBladeCombo(BladeComboStrategy strategy, Actor? target) => strategy switch
    {
        BladeComboStrategy.Automatic =>
            Player.InCombat && ActionReady(PLD.AID.BladeCombo) && In5y(target) && hasFoF && Ammo >= 2,
        BladeComboStrategy.Force => true,
        BladeComboStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Atonement
    private bool ShouldUseAtonement(AtonementStrategy strategy, Actor? target)
    {
        return strategy switch
        {
            AtonementStrategy.Automatic =>
                Player.InCombat && In3y(target) && HasEffect(PLD.SID.AtonementReady),
            AtonementStrategy.ForceAtonement => HasEffect(PLD.SID.AtonementReady),
            AtonementStrategy.ForceSupplication => HasEffect(PLD.SID.SupplicationReady),
            AtonementStrategy.ForceSepulchre => HasEffect(PLD.SID.SepulchreReady),
            AtonementStrategy.Delay => false,
            _ => false
        };
    }

    //Determines when to use Burst Strike
    private bool ShouldUseHolySpirit(OffensiveStrategy strategy, Actor? target) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(target) &&
            hasMight && hasMPforMight && !hasReq,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    //Determines when to use Fated Circle
    private bool ShouldUseHolyCircle(OffensiveStrategy strategy, Actor? AoETargets) => strategy switch
    {
        OffensiveStrategy.Automatic =>
            Player.InCombat && In3y(AoETargets) &&
            hasMight && hasMPforMight && NumTargetsHitByAoE() >= 3 && !hasReq,
        OffensiveStrategy.Force => true,
        OffensiveStrategy.Delay => false,
        _ => false
    };

    private bool ShouldUseDash(DashStrategy strategy, Actor? target) => strategy switch
    {
        DashStrategy.Force => true,
        DashStrategy.Conserve1 => CD(PLD.AID.Intervene) > 30,
        DashStrategy.GapClose => !In3y(target),
        _ => Player.InCombat && target != null & hasFoF,
    };

    //Potion Helpers
    //Determines if potions are aligned with Fight or Flight
    private bool IsPotionAlignedWithNM()
    {
        //Use potion before Solid Barrel in opener
        //Use for 6m window
        return (ActionReady(PLD.AID.FightOrFlight) &&
                ActionReady(PLD.AID.Requiescat) || //Opener
                (reqCD < 15 || ActionReady(PLD.AID.Requiescat)));
    }

    //Determines when to use a potion based on strategy
    private bool ShouldUsePotion(PotionStrategy strategy) => strategy switch
    {
        PotionStrategy.AlignWithRaidBuffs =>
            (IsPotionAlignedWithNM() || (fofCD < 5 && reqCD < 15)),
        PotionStrategy.Immediate => true,
        _ => false
    };
}


using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using AID = BossMod.BLM.AID;
using SID = BossMod.BLM.SID;
using TraitID = BossMod.BLM.TraitID;

namespace BossMod.Autorotation.akechi;
//Contribution by Akechi
//Discord: @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class AkechiBLM(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track
    {
        AOE,                 //ST&AOE rotations tracking
        Thunder,             //Thunder tracking
        Polyglot,            //Polyglot tracking
        Manafont,            //Manafont tracking
        Potion,              //Potion item tracking
        Transpose,           //Transpose tracking
        Triplecast,          //Triplecast tracking
        LeyLines,            //Ley Lines tracking
        Amplifier,           //Amplifier tracking
    }
    public enum AOEStrategy
    {
        Auto,                //Automatically decide when to use ST or AOE rotation based on targets nearby
        ForceST,             //Force ST rotation only
        ForceAOE,            //Force AOE rotation only
    }
    public enum ThunderStrategy
    {
        Thunder3,            //Force use of Thunder if target has 3s or less remaining on DOT effect
        Thunder6,            //Force use of Thunder if target has 6s or less remaining on DOT effect
        Thunder9,            //Force use of Thunder if target has 9s or less remaining on DOT effect
        Thunder0,            //Force use of Thunder if target has does not have DOT effect
        Force,               //Force use of Thunder regardless of DOT effect
        Delay                //Delay the use of Thunder for manual or strategic usage
    }
    public enum PolyglotStrategy
    {
        Automatic,           //Automatically decide when to use Polyglot based on targets nearby
        OnlyXeno,            //Automatically use Xenoglossy optimal spender, regardless of targets nearby
        OnlyFoul,            //Automatically use Foul optimal spender, regardless of targets nearby
        ForceXeno,           //Force use of Xenoglossy
        ForceFoul,           //Force use of Foul
        Delay                //Delay the use of Polyglot abilities for manual or strategic usage
    }
    public enum ManafontStrategy
    {
        Automatic,           //Automatically decide when to use Manafont
        Force,               //Force the use of Manafont (180s CD), regardless of weaving conditions
        ForceWeave,          //Force the use of Manafont (180s CD) in any next possible weave slot
        ForceEX,             //Force the use of Manafont (100s CD), regardless of weaving conditions
        ForceWeaveEX,        //Force the use of Manafont (100s CD) in any next possible weave slot
        Delay                //Delay the use of Manafont for strategic reasons
    }
    public enum PotionStrategy
    {
        Manual,              //Manual potion usage
        AlignWithRaidBuffs,  //Align potion usage with raid buffs
        Immediate            //Use potions immediately when available
    }
    public enum OffensiveStrategy
    {
        Automatic,           //Automatically decide when to use off-global offensive abilities
        Force,               //Force the use of off-global offensive abilities, regardless of weaving conditions
        AnyWeave,            //Force the use of off-global offensive abilities in any next possible weave slot
        EarlyWeave,          //Force the use of off-global offensive abilities in very next FIRST weave slot only
        LateWeave,           //Force the use of off-global offensive abilities in very next LAST weave slot only
        Delay                //Delay the use of offensive abilities for strategic reasons
    }
    #endregion

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Akechi BLM", //Title
            "Standard Rotation Module", //Description
            "Standard rotation (Akechi)", //Category
            "Akechi", //Contributor
            RotationModuleQuality.Ok, //Quality
            BitMask.Build(Class.THM, Class.BLM), //Job
            100); //Level supported

        #region Custom strategies
        res.Define(Track.AOE).As<AOEStrategy>("AOE", "AOE", uiPriority: 200)
            .AddOption(AOEStrategy.Auto, "Auto", "Automatically decide when to use ST or AOE abilities")
            .AddOption(AOEStrategy.ForceST, "Force ST", "Force use of ST abilities only", supportedTargets: ActionTargets.Hostile)
            .AddOption(AOEStrategy.ForceAOE, "Force AOE", "Force use of AOE abilities only", supportedTargets: ActionTargets.Hostile);
        res.Define(Track.Thunder).As<ThunderStrategy>("Damage Over Time", "Thunder", uiPriority: 190)
            .AddOption(ThunderStrategy.Thunder3, "Thunder3", "Use Thunder if target has 3s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Thunder6, "Thunder6", "Use Thunder if target has 6s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Thunder9, "Thunder9", "Use Thunder if target has 9s or less remaining on DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Thunder0, "Thunder0", "Use Thunder if target does not have DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Force, "Force", "Force use of Thunder regardless of DoT effect", 0, 30, ActionTargets.Hostile, 6)
            .AddOption(ThunderStrategy.Delay, "Delay", "Delay the use of Thunder for manual or strategic usage", 0, 0, ActionTargets.Hostile, 6)
            .AddAssociatedActions(AID.Thunder1, AID.Thunder2, AID.Thunder3, AID.Thunder4, AID.HighThunder, AID.HighThunder2);
        res.Define(Track.Polyglot).As<PolyglotStrategy>("Polyglot", "Polyglot", uiPriority: 180)
            .AddOption(PolyglotStrategy.Automatic, "Auto", "Automatically decide when to use Polyglot based on targets nearby", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.OnlyXeno, "Only Xenoglossy", "Automatically use Xenoglossy optimal spender, regardless of targets nearby", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.OnlyFoul, "Only Foul", "Automatically use Foul optimal spender, regardless of targets nearby", 0, 0, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.ForceXeno, "Force Xenoglossy", "Force use of Xenoglossy", 0, 0, ActionTargets.Hostile, 80)
            .AddOption(PolyglotStrategy.ForceFoul, "Force Foul", "Force use of Foul", 0, 30, ActionTargets.Hostile, 70)
            .AddOption(PolyglotStrategy.Delay, "Delay", "Delay the use of Polyglot abilities for manual or strategic usage", 0, 0, ActionTargets.Hostile, 2)
            .AddAssociatedActions(AID.Xenoglossy, AID.Foul);
        res.Define(Track.Manafont).As<ManafontStrategy>("Manafont", "Manafont", uiPriority: 170)
            .AddOption(ManafontStrategy.Automatic, "Auto", "Automatically decide when to use Manafont", 0, 0, ActionTargets.Self, 30)
            .AddOption(ManafontStrategy.Force, "Force", "Force the use of Manafont (180s CD), regardless of weaving conditions", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(ManafontStrategy.ForceWeave, "ForceWeave", "Force the use of Manafont (180s CD) in any next possible weave slot", 180, 0, ActionTargets.Self, 30, 83)
            .AddOption(ManafontStrategy.ForceEX, "ForceEX", "Force the use of Manafont (100s CD), regardless of weaving conditions", 100, 0, ActionTargets.Self, 84)
            .AddOption(ManafontStrategy.ForceWeaveEX, "ForceWeaveEX", "Force the use of Manafont (100s CD) in any next possible weave slot", 100, 0, ActionTargets.Self, 84)
            .AddOption(ManafontStrategy.Delay, "Delay", "Delay the use of Manafont for strategic reasons", 0, 0, ActionTargets.Self, 30)
            .AddAssociatedActions(AID.Manafont);
        res.Define(Track.Potion).As<PotionStrategy>("Potion", uiPriority: 180)
            .AddOption(PotionStrategy.Manual, "Manual", "Do not use automatically")
            .AddOption(PotionStrategy.AlignWithRaidBuffs, "AlignWithRaidBuffs", "Align with No Mercy & Bloodfest together (to ensure use on 2-minute windows)", 270, 30, ActionTargets.Self)
            .AddOption(PotionStrategy.Immediate, "Immediate", "Use ASAP, regardless of any buffs", 270, 30, ActionTargets.Self)
            .AddAssociatedAction(ActionDefinitions.IDPotionInt);
        #endregion

        #region Offensive Strategies
        res.Define(Track.Transpose).As<OffensiveStrategy>("Transpose", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Automatically decide when to use Transpose", 5, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Force, "Force", "Force the use of Transpose, regardless of weaving conditions", 5, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.AnyWeave, "AnyWeave", "Force the use of Transpose in any next possible weave slot", 5, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.EarlyWeave, "EarlyWeave", "Force the use of Transpose in very next FIRST weave slot only", 5, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.LateWeave, "LateWeave", "Force the use of Transpose in very next LAST weave slot only", 0, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay the use of Transpose", 0, 0, ActionTargets.Self, 4)
            .AddAssociatedActions(AID.Transpose);
        res.Define(Track.Triplecast).As<OffensiveStrategy>("Triplecast", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Automatically decide when to use Triplecast", 60, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Force, "Force", "Force the use of Triplecast, regardless of weaving conditions", 60, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.AnyWeave, "AnyWeave", "Force the use of Triplecast in any next possible weave slot", 60, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.EarlyWeave, "EarlyWeave", "Force the use of Triplecast in very next FIRST weave slot only", 60, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.LateWeave, "LateWeave", "Force the use of Triplecast in very next LAST weave slot only", 60, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay the use of Triplecast", 0, 0, ActionTargets.Self, 4)
            .AddAssociatedActions(AID.Triplecast);
        res.Define(Track.LeyLines).As<OffensiveStrategy>("LeyLines", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Automatically decide when to use Ley Lines", 90, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Force, "Force", "Force the use of Ley Lines, regardless of weaving conditions", 90, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.AnyWeave, "AnyWeave", "Force the use of Ley Lines in any next possible weave slot", 90, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.EarlyWeave, "EarlyWeave", "Force the use of Ley Lines in very next FIRST weave slot only", 90, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.LateWeave, "LateWeave", "Force the use of Ley Lines in very next LAST weave slot only", 90, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay the use of Ley Lines", 0, 0, ActionTargets.Self, 4)
            .AddAssociatedActions(AID.LeyLines);
        res.Define(Track.Amplifier).As<OffensiveStrategy>("Amplifier", uiPriority: 170)
            .AddOption(OffensiveStrategy.Automatic, "Auto", "Automatically decide when to use Amplifier", 30, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Force, "Force", "Force the use of Amplifier, regardless of weaving conditions", 30, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.AnyWeave, "AnyWeave", "Force the use of Amplifier in any next possible weave slot", 30, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.EarlyWeave, "EarlyWeave", "Force the use of Amplifier in very next FIRST weave slot only", 30, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.LateWeave, "LateWeave", "Force the use of Amplifier in very next LAST weave slot only", 30, 0, ActionTargets.Self, 4)
            .AddOption(OffensiveStrategy.Delay, "Delay", "Delay the use of Amplifier", 0, 0, ActionTargets.Self, 4)
            .AddAssociatedActions(AID.Amplifier);

        #endregion

        return res;
    }

    #region Priorities
    public enum GCDPriority //priorities for GCDs (higher number = higher priority)
    {
        None = 0,             //default
        Standard = 300,       //standard abilities
        DOT = 350,            //damage-over-time abilities
        Despair = 400,        //Despair
        F3P = 450,            //Fire III proc
        NeedB3 = 460,         //Need to use Blizzard III
        Polyglot = 475,       //Polyglots
        Paradox = 500,        //Paradox

        NeedDOT = 600,        //Need to apply DOTs
        NeedF3P = 625,        //Need to use Fire III proc
        NeedDespair = 640,    //Need to use Despair
        NeedPolyglot = 650,   //Need to use Polyglots
        Moving = 700,         //Moving
        ForcedGCD = 900,      //Forced GCDs
    }
    public enum OGCDPriority //priorities for oGCDs (higher number = higher priority)
    {
        None = 0,             //default
        Transpose = 400,      //Transpose
        Manafont = 450,       //Manafont
        LeyLines = 500,       //Ley Lines
        Amplifier = 550,      //Amplifier
        Triplecast = 600,     //Triplecast
        Potion = 800,         //Potion
        ForcedOGCD = 900,     //Forced oGCDs
    }
    #endregion

    #region Placeholders for Variables
    private uint MP; //Current MP
    private bool NoStance; //No stance
    private bool InAstralFire; //In Astral Fire
    private bool InUmbralIce; //In Umbral Ice
    private sbyte ElementStance; //Elemental Stance
    private byte Polyglots; //Polyglot Stacks
    private int PolyglotTimer; //Polyglot timer
    private int MaxPolyglots; // 
    private byte UmbralHearts; //Umbral Hearts
    private int MaxUmbralHearts; //Max Umbral Hearts
    private int UmbralStacks; //Umbral Ice Stacks
    private int AstralStacks; //Astral Fire Stacks
    private int AstralSoulStacks; //Stacks for Flare Star (Lv100)
    private int EnochianTimer; //Enochian timer
    private float ElementTimer; //Time remaining on Enochian
    private bool EnochianActive; //Enochian is active
    private bool ParadoxActive; //Paradox is active
    private bool canFoul; //Can use Foul
    private bool canXeno; //Can use Xenoglossy
    private bool canLL; //Can use Ley Lines
    private bool canAmp; //Can use Amplifier
    private bool canTC; //Can use Triplecast
    private bool canMF; //Can use Manafont
    private bool hasFirestarter; //Has Firestarter buff
    private bool hasThunderhead; //Has Thunderhead buff
    private float ThunderLeft; //Time left on DOT effect (30s base)
    private bool ShouldUseAOE; //Checks if AOE should be used
    public bool canWeaveIn; //Can weave in oGCDs
    public bool canWeaveEarly; //Can early weave oGCDs
    public bool canWeaveLate; //Can late weave oGCDs
    public bool quarterWeave; //Can last second weave oGCDs
    public float PotionLeft; //Time left on potion buff (30s base)
    public float RaidBuffsLeft; //Time left on raid-wide buffs (typically 20s-22s)
    public float RaidBuffsIn; //Time until raid-wide buffs are applied again (typically 20s-22s)
    public float SpS; //Current GCD length, adjusted by spell speed/haste (2.5s baseline)
    public float BurstWindowLeft; //Time left in current burst window (typically 20s-22s)
    public float BurstWindowIn; //Time until next burst window (typically 20s-22s)
    public AID NextGCD; //Next global cooldown action to be used
    #endregion

    #region Module Helpers
    private bool Unlocked(AID aid) => ActionUnlocked(ActionID.MakeSpell(aid)); //Check if the desired ability is unlocked
    private bool Unlocked(TraitID tid) => TraitUnlocked((uint)tid); //Check if the desired trait is unlocked
    private float CD(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining; //Get remaining cooldown time for the specified action
    private bool In25y(Actor? target) => Player.DistanceToHitbox(target) <= 24.99f; //Check if the target is within 25 yalms
    private bool ActionReady(AID aid) => Unlocked(aid) && CD(aid) < 0.6f; //Check if the desired action is ready (cooldown less than 0.6 seconds)
    private bool IsFirstGCD() => !Player.InCombat || (World.CurrentTime - Manager.CombatStart).TotalSeconds < 0.1f; //Check if this is the first GCD in combat
    private int TargetsInRange() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 25); //Returns the number of targets hit by AOE within a 25-yalm radius around the player
    public bool PlayerHasEffect(SID sid, float duration) => SelfStatusLeft(sid, duration) > GCD; //Checks if Status effect is on self
    public bool JustUsed(AID aid, float variance)
    {
        if (Manager?.LastCast == null)
            return false;

        return Manager.LastCast.Data?.IsSpell(aid) == true
               && (World.CurrentTime - Manager.LastCast.Time).TotalSeconds <= variance;
    }
    private float GetCastTime(AID aid) => ActionDefinitions.Instance.Spell(aid)!.CastTime * SpS / 2.5f;
    private float CastTime(AID aid)
    {
        if (PlayerHasEffect(SID.Triplecast, 15) ||
            PlayerHasEffect(SID.Swiftcast, 10))
            return 0;

        var aspect = ActionDefinitions.Instance.Spell(aid)!.Aspect;

        if (aid == AID.Fire3 && hasFirestarter
            || aid == AID.Foul && Unlocked(TraitID.EnhancedFoul)
            || aspect == ActionAspect.Thunder && hasThunderhead)
            return 0;

        var castTime = GetCastTime(aid);
        if (castTime == 0)
            return 0;

        if (ElementStance == -3 && aspect == ActionAspect.Fire || ElementStance == 3 && aspect == ActionAspect.Ice)
            castTime *= 0.5f;

        return castTime;

    }
    private int TargetsInPlayerAOE(Actor primary) => Hints.NumPriorityTargetsInAOERect( //Use Hints to count number of targets in AOE rectangle
            Player.Position, //from Player's position
            (primary.Position - Player.Position) //direction of AOE rectangle
            .Normalized(), //normalized direction
            25, //AOE rectangle length
            5); //AOE rectangle width
    public Actor? TargetChoice(StrategyValues.OptionRef strategy) => ResolveTargetOverride(strategy.Value); //Resolves the target choice based on the strategy
    #endregion

    #region Upgrade Paths
    private AID BestFire1
        => Unlocked(AID.Paradox) ? AID.Paradox
        : AID.Fire1;
    private AID BestBlizzard1
        => Unlocked(AID.Blizzard4) ? AID.Blizzard4
        : AID.Blizzard1;
    private AID swiftcastB3
        => Player.InCombat && ActionReady(AID.Swiftcast) ? AID.Swiftcast
        : AID.Blizzard3;
    private AID BestThunderST
        => Unlocked(AID.HighThunder) ? AID.HighThunder
        : Unlocked(AID.Thunder3) ? AID.Thunder3
        : AID.Thunder1;
    private AID BestThunderAOE
        => Unlocked(AID.HighThunder2) ? AID.HighThunder2
        : Unlocked(AID.Thunder4) ? AID.Thunder4
        : AID.Thunder2;
    private AID BestThunder
        => ShouldUseAOE ? BestThunderAOE : BestThunderST;
    private AID BestPolyglot
        => ShouldUseAOE ? AID.Foul : BestXenoglossy;
    private AID BestXenoglossy
        => Unlocked(AID.Xenoglossy) ? AID.Xenoglossy : AID.Foul;
    #endregion

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        var gauge = World.Client.GetGauge<BlackMageGauge>(); //Retrieve BLM gauge
        NoStance = ElementStance is 0; //No stance
        ElementStance = gauge.ElementStance; //Elemental Stance
        InAstralFire = ElementStance is 1 or 2 or 3; //In Astral Fire
        InUmbralIce = ElementStance is -1 or -2 or -3; //In Umbral Ice
        Polyglots = gauge.PolyglotStacks; //Polyglot Stacks
        UmbralHearts = gauge.UmbralHearts; //Umbral Hearts
        MaxUmbralHearts = Unlocked(TraitID.UmbralHeart) ? 3 : 0;
        UmbralStacks = gauge.UmbralStacks; //Umbral Ice Stacks
        AstralStacks = gauge.AstralStacks; //Astral Fire Stacks
        AstralSoulStacks = gauge.AstralSoulStacks; //Stacks for Flare Star (Lv100)
        EnochianActive = gauge.EnochianActive; //Enochian is active
        ParadoxActive = gauge.ParadoxActive; //Paradox is active
        EnochianTimer = gauge.EnochianTimer; //Enochian timer
        ElementTimer = gauge.ElementTimeRemaining / 1000f; //Time remaining on current element
        PolyglotTimer = Math.Max(0, ((MaxPolyglots - Polyglots) * 30000) + (EnochianTimer - 30000));
        MaxPolyglots = Unlocked(TraitID.EnhancedPolyglotII) ? 3 : Unlocked(TraitID.EnhancedPolyglot) ? 2 : 1;
        canFoul = Unlocked(AID.Foul) && Polyglots > 0; //Can use Foul
        canXeno = Unlocked(AID.Xenoglossy) && Polyglots > 0; //Can use Xenoglossy
        canLL = Unlocked(AID.LeyLines) && CD(AID.LeyLines) <= 120 && SelfStatusLeft(SID.LeyLines, 30) == 0; //Can use Ley Lines
        canAmp = ActionReady(AID.Amplifier); //Can use Amplifier
        canTC = Unlocked(AID.Triplecast) && CD(AID.Triplecast) <= 60 && SelfStatusLeft(SID.Triplecast) == 0; //Can use Triplecast
        canMF = ActionReady(AID.Manafont); //Can use Manafont
        hasFirestarter = PlayerHasEffect(SID.Firestarter, 15); //Has Firestarter buff
        hasThunderhead = PlayerHasEffect(SID.Thunderhead, 30); //Has Thunderhead buff
        ThunderLeft = Utils.MaxAll(
            StatusDetails(primaryTarget, SID.Thunder, Player.InstanceID, 24).Left,
            StatusDetails(primaryTarget, SID.ThunderII, Player.InstanceID, 18).Left,
            StatusDetails(primaryTarget, SID.ThunderIII, Player.InstanceID, 27).Left,
            StatusDetails(primaryTarget, SID.ThunderIV, Player.InstanceID, 21).Left,
            StatusDetails(primaryTarget, SID.HighThunder, Player.InstanceID, 30).Left,
            StatusDetails(primaryTarget, SID.HighThunderII, Player.InstanceID, 24).Left);
        MP = Player.HPMP.CurMP; //Current MP
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        SpS = ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on spell speed and haste
        NextGCD = AID.None; //Next global cooldown action to be used
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)
        ShouldUseAOE = TargetsInRange() > 2; //otherwise, use AOE if 2+ targets would be hit

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var thunder = strategy.Option(Track.Thunder); //Thunder track
        var thunderStrat = thunder.As<ThunderStrategy>(); //Thunder strategy
        var polyglot = strategy.Option(Track.Polyglot); //Polyglot track
        var polyglotStrat = polyglot.As<PolyglotStrategy>(); //Polyglot strategy
        var mf = strategy.Option(Track.Manafont); //Manafont track
        var mfStrat = mf.As<ManafontStrategy>(); //Manafont strategy
        var tc = strategy.Option(Track.Triplecast); //Triplecast track
        var tcStrat = tc.As<OffensiveStrategy>(); //Triplecast strategy
        var ll = strategy.Option(Track.LeyLines); //Ley Lines track
        var llStrat = ll.As<OffensiveStrategy>(); //Ley Lines strategy
        var amp = strategy.Option(Track.Amplifier); //Amplifier track
        var ampStrat = amp.As<OffensiveStrategy>(); //Amplifier strategy
        var potion = strategy.Option(Track.Potion).As<PotionStrategy>(); //Potion strategy
        #endregion

        #endregion

        #region Force Execution
        if (AOEStrategy is AOEStrategy.Auto)
            STLv72toLv89(TargetChoice(AOE) ?? primaryTarget);
        if (AOEStrategy is AOEStrategy.ForceST)
            BestST(TargetChoice(AOE) ?? primaryTarget);
        if (AOEStrategy is AOEStrategy.ForceAOE)
            BestAOE(TargetChoice(AOE) ?? primaryTarget);
        #endregion

        #region Standard Execution
        //Out of combat
        if (Unlocked(AID.UmbralSoul))
        {
            if (primaryTarget == null &&
                (!Player.InCombat || Player.InCombat && TargetsInRange() is 0))
            {
                if (InAstralFire)
                    QueueOGCD(AID.Transpose, Player, OGCDPriority.Transpose);
                if (InUmbralIce &&
                    (ElementTimer <= 14 || UmbralStacks < 3 || UmbralHearts != MaxUmbralHearts))
                    QueueGCD(AID.UmbralSoul, Player, GCDPriority.Standard);
            }
        }
        //Thunder
        if (ShouldUseThunder(primaryTarget, thunderStrat)) //if Thunder should be used based on strategy
            QueueGCD(BestThunder, TargetChoice(thunder) ?? primaryTarget, ThunderLeft < 3 ? GCDPriority.NeedDOT : GCDPriority.DOT); //Queue Thunder
        //Polyglots
        if (ShouldUsePolyglot(primaryTarget, polyglotStrat)) //if Polyglot should be used based on strategy
            QueueGCD(BestPolyglot, TargetChoice(polyglot) ?? primaryTarget, polyglotStrat is PolyglotStrategy.ForceFoul or PolyglotStrategy.ForceXeno ? GCDPriority.ForcedGCD : EnochianTimer < 5000 ? GCDPriority.NeedPolyglot : GCDPriority.Paradox); //Queue Polyglot
        //LeyLines
        if (ShouldUseLeyLines(primaryTarget, llStrat))
            QueueOGCD(AID.LeyLines, Player, OGCDPriority.LeyLines);
        //Triplecast
        if (ShouldUseTriplecast(primaryTarget, tcStrat))
            QueueOGCD(AID.Triplecast, Player, OGCDPriority.Triplecast);
        //Amplifier
        if (ShouldUseAmplifier(primaryTarget, ampStrat))
            QueueOGCD(AID.Amplifier, Player, OGCDPriority.Amplifier);
        //Manafont
        if (ShouldUseManafont(primaryTarget, mfStrat))
            QueueOGCD(AID.Manafont, Player, OGCDPriority.Manafont);
        //Potion
        if (potion is PotionStrategy.AlignWithRaidBuffs && CD(AID.LeyLines) < 5 ||
            potion is PotionStrategy.Immediate)
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionInt, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
        #endregion
    }

    #region Core Execution Helpers
    public void QueueGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueGCD(aid, target, (int)(object)priority, delay);

    public void QueueGCD(AID aid, Actor? target, int priority = 8, float delay = 0)
    {
        var NextGCDPrio = 0;

        if (priority == 0)
            return;

        if (QueueAction(aid, target, ActionQueue.Priority.High + priority, delay) && priority > NextGCDPrio)
        {
            NextGCD = aid;
        }
    }

    public void QueueOGCD<P>(AID aid, Actor? target, P priority, float delay = 0) where P : Enum
        => QueueOGCD(aid, target, (int)(object)priority, delay);

    public void QueueOGCD(AID aid, Actor? target, int priority = 4, float delay = 0)
    {
        if (priority == 0)
            return;

        QueueAction(aid, target, ActionQueue.Priority.Medium + priority, delay);
    }

    public bool QueueAction(AID aid, Actor? target, float priority, float delay)
    {
        if ((uint)(object)aid == 0)
            return false;

        var def = ActionDefinitions.Instance.Spell(aid);
        if (def == null)
            return false;

        if (def.Range != 0 && target == null)
        {
            return false;
        }

        Vector3 targetPos = default;

        if (def.AllowedTargets.HasFlag(ActionTargets.Area))
        {
            if (def.Range == 0)
                targetPos = Player.PosRot.XYZ();
            else if (target != null)
                targetPos = target.PosRot.XYZ();
        }

        Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, priority, delay: delay, targetPos: targetPos);
        return true;
    }
    #endregion
    private void STLv1toLv34(Actor? target)
    {
        //Fire
        if (Unlocked(AID.Fire1) && //if Fire is unlocked
            NoStance && MP >= 800 || //if no stance is active and MP is 800 or more
            InAstralFire && MP >= 1600) //or if Astral Fire is active and MP is 1600 or more
            QueueGCD(AID.Fire1, target, GCDPriority.Standard); //Queue Fire
        //Ice
        if (InUmbralIce && MP != 10000) //if Umbral Ice is active and MP is not max
            QueueGCD(AID.Blizzard1, target, GCDPriority.Standard); //Queue Blizzard
        //Transpose 
        if (ActionReady(AID.Transpose) && //if Transpose is unlocked & off cooldown
            InAstralFire && MP < 1600 || //if Astral Fire is active and MP is less than 1600
            InUmbralIce && MP == 10000) //or if Umbral Ice is active and MP is max
            QueueOGCD(AID.Transpose, Player, OGCDPriority.Transpose); //Queue Transpose
    }
    private void STLv35toLv59(Actor? target)
    {
        //Ice
        if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
            (NoStance && MP == 10000 || //and if no stance is active and MP is max (opener)
            (NoStance && MP < 10000 && Player.InCombat) || //or if in combat and no stance is active and MP is less than max (died or stopped attacking)
            InAstralFire && MP < 1600)) //or if Astral Fire is active and MP is less than 1600
            QueueGCD(AID.Blizzard3, target, GCDPriority.Standard);
        if (UmbralStacks >= 1)
            QueueGCD(BestBlizzard1, target, GCDPriority.Standard);
        //Fire
        if (CastTime(AID.Fire3) < 2.5f &&
            (hasFirestarter || JustUsed(BestBlizzard1, 5)))
            QueueGCD(AID.Fire3, target, GCDPriority.F3P);
        if (InAstralFire && MP >= 1600)
            QueueGCD(AID.Fire1, target, GCDPriority.Standard);
    }
    private void STLv60toLv71(Actor? target)
    {
        //Ice
        if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
            (NoStance && MP == 10000 || //and if no stance is active and MP is max (opener)
            (NoStance && MP < 10000 && Player.InCombat) || //or if in combat and no stance is active and MP is less than max (died or stopped attacking)
            InAstralFire && MP < 1600)) //or if Astral Fire is active and MP is less than 1600
            QueueGCD(AID.Blizzard3, target, GCDPriority.Standard);
        if (UmbralStacks >= 1 ||
            Unlocked(AID.Blizzard4) && UmbralHearts != MaxUmbralHearts)
            QueueGCD(BestBlizzard1, target, GCDPriority.Standard);
        //Fire
        if (CastTime(AID.Fire3) < 2.5f &&
            (hasFirestarter || JustUsed(BestBlizzard1, 5)))
            QueueGCD(AID.Fire3, target, GCDPriority.F3P);
        if (InAstralFire && MP >= 1600)
            QueueGCD(AID.Fire4, target, GCDPriority.Standard);
        if (InAstralFire && ElementTimer <= 6)
            QueueGCD(BestFire1, target, GCDPriority.Paradox);
    }

    private void STLv72toLv89(Actor? target)
    {
        //Ice
        if (Unlocked(AID.Blizzard3) && //if Blizzard III is unlocked
            (NoStance && MP == 10000 || //and if no stance is active and MP is max (opener)
            (NoStance && MP < 10000 && Player.InCombat) || //or if in combat and no stance is active and MP is less than max (died or stopped attacking)
            InAstralFire && MP < 1600)) //or if Astral Fire is active and MP is less than 400
            QueueGCD(AID.Blizzard3, target, InAstralFire && MP is 0 ? GCDPriority.NeedB3 : GCDPriority.Standard);
        if (InUmbralIce && UmbralHearts != MaxUmbralHearts)
            QueueGCD(BestBlizzard1, target, GCDPriority.Standard);
        //Fire
        if (CastTime(AID.Fire3) < SpS && !JustUsed(AID.Fire3, 5) &&
            (hasFirestarter || UmbralHearts == MaxUmbralHearts && UmbralStacks == 3))
            QueueGCD(AID.Fire3, target, UmbralHearts == MaxUmbralHearts && UmbralStacks == 3 ? GCDPriority.NeedF3P : GCDPriority.F3P);
        if (InAstralFire && MP >= 1600)
            QueueGCD(AID.Fire4, target, GCDPriority.Standard);
        if (InAstralFire && ElementTimer <= 6 && MP >= 2800)
            QueueGCD(BestFire1, target, GCDPriority.Paradox);
        if (InAstralFire && MP is < 1600 and >= 800)
            QueueGCD(AID.Despair, target, ElementTimer <= 4 && MP <= 2800 ? GCDPriority.NeedDespair : GCDPriority.Despair);
    }
    private void STLv90toLv99()
    {
        // TODO: Implement single-target rotation for level 90-99
    }
    private void STLv100()
    {
        // TODO: Implement single-target rotation for level 100
    }
    private void BestST(Actor? target)
    {
        if (Player.Level is >= 1 and <= 34)
        {
            STLv1toLv34(target);
        }
        if (Player.Level is >= 35 and <= 59)
        {
            STLv35toLv59(target);
        }
        if (Player.Level is >= 60 and <= 71)
        {
            STLv60toLv71(target);
        }
        if (Player.Level is >= 72 and <= 89)
        {
            //STLv72toLv89(target);
        }
        if (Player.Level is >= 90 and <= 99)
        {
            //STLv90toLv99(target);
        }
        if (Player.Level is 100)
        {
            //STLv100(target);
        }
    }
    private void AOELv12toLv34()
    {
        // TODO: Implement AOE rotation for level 12-34
    }
    private void AOELv35toLv39()
    {
        // TODO: Implement AOE rotation for level 35-39
    }
    private void AOELv40toLv49()
    {
        // TODO: Implement AOE rotation for level 40-49
    }
    private void AOELv50toLv57()
    {
        // TODO: Implement AOE rotation for level 50-57
    }
    private void AOELv58toLv81()
    {
        // TODO: Implement AOE rotation for level 58-81
    }
    private void AOELv82toLv99()
    {
        // TODO: Implement AOE rotation for level 82-99
    }
    private void AOELv100()
    {
        // TODO: Implement AOE rotation for level 100
    }
    private void BestAOE(Actor? target)
    {
        if (In25y(target))
        {
            if (Player.Level is >= 12 and <= 34)
            {
                AOELv12toLv34();
            }
            if (Player.Level is >= 35 and <= 39)
            {
                AOELv35toLv39();
            }
            if (Player.Level is >= 40 and <= 49)
            {
                AOELv40toLv49();
            }
            if (Player.Level is >= 50 and <= 57)
            {
                AOELv50toLv57();
            }
            if (Player.Level is >= 58 and <= 81)
            {
                AOELv58toLv81();
            }
            if (Player.Level is >= 82 and <= 99)
            {
                AOELv82toLv99();
            }
            if (Player.Level is 100)
            {
                AOELv100();
            }
        }
    }

    private void BestRotation(Actor? target)
    {
        if (ShouldUseAOE)
        {
            BestAOE(target);
        }
        if (!ShouldUseAOE)
        {
            BestST(target);
        }
    }

    #region Cooldown Helpers
    private bool ShouldUseThunder(Actor? target, ThunderStrategy strategy) => strategy switch
    {
        ThunderStrategy.Thunder3 => Player.InCombat && target != null && hasThunderhead && ThunderLeft <= 3 && In25y(target),
        ThunderStrategy.Thunder6 => Player.InCombat && target != null && hasThunderhead && ThunderLeft <= 6 && In25y(target),
        ThunderStrategy.Thunder9 => Player.InCombat && target != null && hasThunderhead && ThunderLeft <= 9 && In25y(target),
        ThunderStrategy.Thunder0 => Player.InCombat && target != null && hasThunderhead && ThunderLeft is 0 && In25y(target),
        ThunderStrategy.Force => hasThunderhead,
        ThunderStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUsePolyglot(Actor? target, PolyglotStrategy strategy) => strategy switch
    {
        PolyglotStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        Polyglots > 0 &&
        ((CD(AID.Triplecast) <= 60 || CD(AID.LeyLines) <= 120) || (CD(AID.Manafont) < 0.6f && JustUsed(AID.Despair, 5) && MP < 1600) ||
        (Polyglots == MaxPolyglots && PolyglotTimer <= 5000)), //Overcap
        PolyglotStrategy.OnlyXeno => ShouldUseXenoglossy(target, PolyglotStrategy.Automatic),
        PolyglotStrategy.OnlyFoul => ShouldUseFoul(target, PolyglotStrategy.Automatic),
        PolyglotStrategy.ForceXeno => canXeno,
        PolyglotStrategy.ForceFoul => canFoul,
        PolyglotStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseXenoglossy(Actor? target, PolyglotStrategy strategy) => strategy switch
    {
        PolyglotStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canXeno &&
        Polyglots > 0 &&
        ((CD(AID.Triplecast) <= 60 || CD(AID.LeyLines) <= 120) || (CD(AID.Manafont) < 0.6f && JustUsed(AID.Despair, 5) && MP < 1600) ||
        (Polyglots == MaxPolyglots && PolyglotTimer <= 5000)), //Overcap
        _ => false
    };
    private bool ShouldUseFoul(Actor? target, PolyglotStrategy strategy) => strategy switch
    {
        PolyglotStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canFoul &&
        Polyglots > 0 &&
        ((CD(AID.Triplecast) <= 60 || CD(AID.LeyLines) <= 120) || (CD(AID.Manafont) < 0.6f && JustUsed(AID.Despair, 5) && MP < 1600) ||
        (Polyglots == MaxPolyglots && PolyglotTimer <= 5000)), //Overcap
        _ => false
    };
    private bool ShouldUseLeyLines(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canLL &&
        canWeaveIn,
        OffensiveStrategy.Force => canLL,
        OffensiveStrategy.AnyWeave => canLL && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canLL && canWeaveEarly,
        OffensiveStrategy.LateWeave => canLL && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseAmplifier(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canAmp &&
        canWeaveIn &&
        Polyglots != MaxPolyglots,
        OffensiveStrategy.Force => canAmp,
        OffensiveStrategy.AnyWeave => canAmp && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canAmp && canWeaveEarly,
        OffensiveStrategy.LateWeave => canAmp && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseManafont(Actor? target, ManafontStrategy strategy) => strategy switch
    {
        ManafontStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canMF &&
        InAstralFire &&
        (JustUsed(BestXenoglossy, 5) && MP < 1600),
        ManafontStrategy.Force => canMF,
        ManafontStrategy.ForceWeave => canMF && canWeaveIn,
        ManafontStrategy.ForceEX => canMF,
        ManafontStrategy.ForceWeaveEX => canMF && canWeaveIn,
        ManafontStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseTriplecast(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic
        => Player.InCombat &&
        target != null &&
        canTC &&
        canWeaveIn,
        OffensiveStrategy.Force => canWeaveIn,
        OffensiveStrategy.AnyWeave => canWeaveIn,
        OffensiveStrategy.EarlyWeave => canWeaveEarly,
        OffensiveStrategy.LateWeave => canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    #endregion
}

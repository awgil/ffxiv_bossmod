using BossMod.AST;
using FFXIVClientStructs.FFXIV.Client.Game.Gauge;
using AID = BossMod.BLM.AID;
using SID = BossMod.BLM.SID;

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
            BitMask.Build((int)Class.BLM), //Job
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
        DOT = 400,            //damage-over-time abilities
        ForcedGCD = 900,      //Forced GCDs
    }
    public enum OGCDPriority //priorities for oGCDs (higher number = higher priority)
    {
        None = 0,             //default
        Potion = 800,         //Potion
        ForcedOGCD = 900,     //Forced oGCDs
    }
    #endregion

    #region Placeholders for Variables
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
    private int TargetsInAOERange() => Hints.NumPriorityTargetsInAOECircle(Player.Position, 5); //Returns the number of targets hit by AOE within a 5-yalm radius around the player
    public bool PlayerHasEffect(SID sid, float duration) => SelfStatusLeft(sid, duration) > 0; //Checks if Status effect is on self
    #endregion

    #region Upgrade Paths
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
        var gauge = World.Client.GetGauge<BlackMageGauge>(); //Retrieve BLMolar gauge
        canWeaveIn = GCD is <= 2.5f and >= 0.1f; //Can weave in oGCDs
        canWeaveEarly = GCD is <= 2.5f and >= 1.25f; //Can weave in oGCDs early
        canWeaveLate = GCD is <= 1.25f and >= 0.1f; //Can weave in oGCDs late
        SpS = ActionSpeed.GCDRounded(World.Client.PlayerStats.SpellSpeed, World.Client.PlayerStats.Haste, Player.Level); //GCD based on spell speed and haste
        NextGCD = AID.None; //Next global cooldown action to be used
        PotionLeft = PotionStatusLeft(); //Remaining time for potion buff (30s)
        ShouldUseAOE = TargetsInAOERange() > 1; //otherwise, use AOE if 2+ targets would be hit

        #region Strategy Definitions
        var AOE = strategy.Option(Track.AOE); //AOE track
        var AOEStrategy = AOE.As<AOEStrategy>(); //AOE strategy
        var Thunder = strategy.Option(Track.Thunder); //Thunder track
        var ThunderStrategy = Thunder.As<ThunderStrategy>(); //Thunder strategy
        var potion = strategy.Option(Track.Potion).As<PotionStrategy>(); //Potion strategy


        #endregion

        #endregion

        #region Force Execution
        if (AOEStrategy is AOEStrategy.Auto)
            QueueGCD(BestRotation(), ResolveTargetOverride(AOE.Value) ?? primaryTarget, GCDPriority.Standard);
        if (AOEStrategy is AOEStrategy.ForceST)
            QueueGCD(BestST(), ResolveTargetOverride(AOE.Value) ?? primaryTarget, GCDPriority.ForcedGCD);
        if (AOEStrategy is AOEStrategy.ForceAOE)
            QueueGCD(BestAOE(), Player, GCDPriority.ForcedGCD);
        #endregion

        #region Standard Execution
        if (AOEStrategy == AOEStrategy.Auto)
        {
            var STtarget = ResolveTargetOverride(AOE.Value) ?? primaryTarget;
            if (In25y(STtarget))
            {
                if (ShouldUseAOE)
                    QueueGCD(BestAOE(), Player, GCDPriority.Standard);
                if (In25y(STtarget) &&
                    (!ShouldUseAOE || IsFirstGCD()))
                    QueueGCD(BestST(), STtarget, GCDPriority.Standard);

            }
        }

        if (potion is PotionStrategy.AlignWithRaidBuffs && CD(AID.LeyLines) < 5 ||
            potion is PotionStrategy.Immediate)
            Hints.ActionsToExecute.Push(ActionDefinitions.IDPotionMnd, Player, ActionQueue.Priority.VeryHigh + (int)OGCDPriority.Potion, 0, GCD - 0.9f);
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
    private void STLv1toLv34()
    {
        // TODO: Implement single-target rotation for level 1-34
    }
    private void STLv35toLv59()
    {
        // TODO: Implement single-target rotation for level 35-59
    }
    private void STLv60toLv71()
    {
        // TODO: Implement single-target rotation for level 60-71
    }
    private void STLv72toLv89()
    {
        // TODO: Implement single-target rotation for level 72-89
    }
    private void STLv90toLv99()
    {
        // TODO: Implement single-target rotation for level 90-99
    }
    private void STLv100()
    {
        // TODO: Implement single-target rotation for level 100
    }
    private void BestST()
    {
        if (Player.Level is >= 1 and <= 34)
            STLv1toLv34();
        if (Player.Level is >= 35 and <= 59)
            STLv35toLv59();
        if (Player.Level is >= 60 and <= 71)
            STLv60toLv71();
        if (Player.Level is >= 72 and <= 89)
            STLv72toLv89();
        if (Player.Level is >= 90 and <= 99)
            STLv90toLv99();
        if (Player.Level is 100)
            STLv100();
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
    private void BestAOE()
    {
        if (Player.Level is >= 12 and <= 34)
            AOELv12toLv34();
        if (Player.Level is >= 35 and <= 39)
            AOELv35toLv39();
        if (Player.Level is >= 40 and <= 49)
            AOELv40toLv49();
        if (Player.Level is >= 50 and <= 57)
            AOELv50toLv57();
        if (Player.Level is >= 58 and <= 81)
            AOELv58toLv81();
        if (Player.Level is >= 82 and <= 99)
            AOELv82toLv99();
        if (Player.Level is 100)
            AOELv100();
    }

    private void BestRotation()
    {
        if (ShouldUseAOE)
        {
            BestAOE();
        }
        if (!ShouldUseAOE)
        {
            BestST();
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
    private bool ShouldUseLeyLines(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && target != null && canLL && canWeaveIn,
        OffensiveStrategy.Force => canLL,
        OffensiveStrategy.AnyWeave => canLL && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canLL && canWeaveEarly,
        OffensiveStrategy.LateWeave => canLL && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseAmplifier(Actor? target, OffensiveStrategy strategy) => strategy switch
    {
        OffensiveStrategy.Automatic => Player.InCombat && target != null && canAmp && canWeaveIn,
        OffensiveStrategy.Force => canAmp,
        OffensiveStrategy.AnyWeave => canAmp && canWeaveIn,
        OffensiveStrategy.EarlyWeave => canAmp && canWeaveEarly,
        OffensiveStrategy.LateWeave => canAmp && canWeaveLate,
        OffensiveStrategy.Delay => false,
        _ => false
    };
    private bool ShouldUseManafont(Actor? target, ManafontStrategy strategy) => strategy switch
    {
        ManafontStrategy.Automatic => Player.InCombat && target != null && canMF && canWeaveIn,
        ManafontStrategy.Force => canMF,
        ManafontStrategy.ForceWeave => canMF && canWeaveIn,
        ManafontStrategy.ForceEX => canMF,
        ManafontStrategy.ForceWeaveEX => canMF && canWeaveIn,
        ManafontStrategy.Delay => false,
        _ => false
    };
    #endregion
}

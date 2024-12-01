using AID = BossMod.GNB.AID;
using SID = BossMod.GNB.SID;

namespace BossMod.Autorotation.Utility;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class RolePvPUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    #region Enums: Abilities / Strategies
    public enum Track
    {
        Elixir,           //Solid Barrel combo tracking
        Recuperate,    //Gnashing Fang action tracking
        Guard,     //Rough Divide ability tracking
        Purify,     //Fated Circle ability tracking
        Sprint,           //Burst tracking
    }
    public enum ElixirStrategy
    {
        Automatic,
        Close,
        Mid,
        Far,
        Force,
        Hold
    }

    public enum RecuperateStrategy
    {
        Automatic,        //Automatically execute based on conditions
        Seventy,          //Execute at 70% HP
        Fifty,            //Execute at 50% HP
        Thirty,           //Execute at 30% HP
        Force,            //Force burst actions regardless of conditions
        Hold              //Conserve resources and cooldowns
    }

    public enum GuardStrategy
    {
        Automatic,        //Automatically execute based on conditions
        Seventy,          //Execute at 70% HP
        Fifty,            //Execute at 50% HP
        Thirty,           //Execute at 30% HP
        Force,            //Force burst actions regardless of conditions
        Hold              //Conserve resources and cooldowns
    }

    public enum DefensiveStrategy
    {
        Automatic,      //Automatically decide when to use Defensive abilities
        Force,          //Force the use of Defensive abilities regardless of conditions
        Delay           //Delay the use of Defensive abilities for strategic reasons
    }

    #endregion

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("PvP: Utility", "PvP Rotation Module", "Utility Actions (PvP)", "Akechi", RotationModuleQuality.Excellent,
            BitMask.Build(
                Class.PLD, Class.WAR, Class.DRK, Class.GNB, //Tank
                Class.WHM, Class.SCH, Class.AST, Class.SGE, //Healer    
                Class.MNK, Class.DRG, Class.NIN, Class.SAM, Class.RPR, Class.VPR, //Melee
                Class.BRD, Class.MCH, Class.DNC, //Ranged
                Class.BLM, Class.SMN, Class.RDM, Class.PCT), 30); //Caster

        res.Define(Track.Elixir).As<ElixirStrategy>("Elixir", uiPriority: 150)
            .AddOption(ElixirStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(ElixirStrategy.Close, "Close", "Use when target is close (within 10y or further)")
            .AddOption(ElixirStrategy.Mid, "Mid", "Use when target is mid-range (within 20y or further)")
            .AddOption(ElixirStrategy.Far, "Far", "Use when target is far (within 30y or further)")
            .AddOption(ElixirStrategy.Force, "Force", "Force")
            .AddOption(ElixirStrategy.Hold, "Hold", "Hold")
            .AddAssociatedActions(AID.Elixir);
        res.Define(Track.Recuperate).As<RecuperateStrategy>("Recuperate", uiPriority: 150)
            .AddOption(RecuperateStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(RecuperateStrategy.Seventy, "Seventy", "Use at 70% HP")
            .AddOption(RecuperateStrategy.Fifty, "Fifty", "Use at 50% HP")
            .AddOption(RecuperateStrategy.Thirty, "Thirty", "Use at 30% HP")
            .AddOption(RecuperateStrategy.Force, "Force", "Force")
            .AddOption(RecuperateStrategy.Hold, "Hold", "Hold")
            .AddAssociatedActions(AID.Recuperate);
        res.Define(Track.Guard).As<GuardStrategy>("Guard", uiPriority: 150)
            .AddOption(GuardStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(GuardStrategy.Seventy, "Seventy", "Use at 70% HP")
            .AddOption(GuardStrategy.Fifty, "Fifty", "Use at 50% HP")
            .AddOption(GuardStrategy.Thirty, "Thirty", "Use at 30% HP")
            .AddOption(GuardStrategy.Force, "Force", "Force")
            .AddOption(GuardStrategy.Hold, "Hold", "Hold")
            .AddAssociatedActions(AID.Guard);
        res.Define(Track.Purify).As<DefensiveStrategy>("Purify", uiPriority: 150)
            .AddOption(DefensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(DefensiveStrategy.Force, "Force", "Force")
            .AddOption(DefensiveStrategy.Delay, "Delay", "Delay")
            .AddAssociatedActions(AID.Purify);
        res.Define(Track.Sprint).As<DefensiveStrategy>("Sprint", uiPriority: 150)
            .AddOption(DefensiveStrategy.Automatic, "Automatic", "Use normally")
            .AddOption(DefensiveStrategy.Force, "Force", "Force")
            .AddOption(DefensiveStrategy.Delay, "Delay", "Delay")
            .AddAssociatedActions(AID.Sprint);
        return res;

    }

    #region Priorities
    //Priority for GCDs used
    public enum GCDPriority
    {
        None = 0,
        Elixir = 500,
        ForcedGCD = 900,
    }
    //Priority for oGCDs used
    public enum OGCDPriority
    {
        None = 0,
        Sprint = 300,
        Recuperate = 400,
        Guard = 600,
        Purify = 700,
        ForcedOGCD = 900,
    }
    #endregion

    #region Placeholders for Variables
    //Cooldown Related
    private bool hasSprint; //Checks if Sprint is active
    private bool canElixir; //can Keen Edge
    private bool canRecuperate; //can Brutal Shell
    private bool canGuard; //can Solid Barrel
    private bool canPurify; //can Burst Strike
    private bool canSprint; //can Gnashing Fang

    //Misc
    public float GCDLength; //Current GCD length, adjusted by skill speed/haste (2.5s baseline)
    public AID NextGCD; //Next global cooldown action to be used (needed for cartridge management)
    private GCDPriority NextGCDPrio; //Priority of the next GCD, used for decision making on cooldowns
    #endregion

    #region Module Helpers
    private bool In10y(Actor? target) => Player.DistanceToHitbox(target) <= 9.9; //Check if the target is within 10 yalms
    private bool In20y(Actor? target) => Player.DistanceToHitbox(target) <= 19.9; //Check if the target is within 20 yalms
    private bool In30y(Actor? target) => Player.DistanceToHitbox(target) <= 29.9; //Check if the target is within 30 yalms
    private bool IsOffCooldown(AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f; //Check if the desired action is ready (cooldown less than 0.6 seconds)
    public bool HasEffect(SID sid) => SelfStatusLeft(sid) > 0; //Check if the player has the specified status effect
    public bool TargetHasEffect(SID sid, Actor? target) => StatusDetails(target, sid, Player.InstanceID, 1000).Left > 0; //Check if the target has the specified status effect
    #endregion
    public float DebuffsLeft(Actor? target)
    {
        return target == null ? 0f
            : Utils.MaxAll(
            StatusDetails(target, SID.Silence, Player.InstanceID, 5).Left,
            StatusDetails(target, SID.Stun, Player.InstanceID, 5).Left,
            StatusDetails(target, SID.Bind, Player.InstanceID, 5).Left,
            StatusDetails(target, SID.Heavy, Player.InstanceID, 5).Left,
            StatusDetails(target, SID.Sleep, Player.InstanceID, 5).Left,
            StatusDetails(target, SID.HalfAsleep, Player.InstanceID, 5).Left
        );
    }
    public bool HasAnyDebuff(Actor? target) => DebuffsLeft(target) > 0;

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving) //Executes our actions
    {
        #region Variables
        hasSprint = HasEffect(SID.SprintPvP); //Checks if Sprint is active

        #region Minimal Requirements
        canElixir = IsOffCooldown(AID.Elixir) && strategy.Option(Track.Elixir).As<ElixirStrategy>() != ElixirStrategy.Hold;
        canRecuperate = Player.HPMP.CurMP >= 2500 && strategy.Option(Track.Recuperate).As<RecuperateStrategy>() != RecuperateStrategy.Hold;
        canGuard = IsOffCooldown(AID.Guard) && strategy.Option(Track.Guard).As<GuardStrategy>() != GuardStrategy.Hold;
        canPurify = IsOffCooldown(AID.Purify) && strategy.Option(Track.Purify).As<DefensiveStrategy>() != DefensiveStrategy.Delay;
        canSprint = !hasSprint && strategy.Option(Track.Sprint).As<DefensiveStrategy>() != DefensiveStrategy.Delay;
        #endregion
        #endregion

        //Elixir execution
        var elixirStrat = strategy.Option(Track.Elixir).As<ElixirStrategy>();
        if (ShouldUseElixir(elixirStrat, primaryTarget))
            QueueGCD(AID.Elixir, Player, elixirStrat == ElixirStrategy.Force ? GCDPriority.ForcedGCD : GCDPriority.Elixir);

        //Recuperate execution
        var recuperateStrat = strategy.Option(Track.Recuperate).As<RecuperateStrategy>();
        if (ShouldUseRecuperate(recuperateStrat))
            QueueOGCD(AID.Recuperate, Player, recuperateStrat == RecuperateStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Recuperate);

        //Guard execution
        var guardStrat = strategy.Option(Track.Guard).As<GuardStrategy>();
        if (ShouldUseGuard(guardStrat, primaryTarget))
            QueueOGCD(AID.Guard, Player, guardStrat == GuardStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Guard);

        //Purify execution
        var purifyStrat = strategy.Option(Track.Purify).As<DefensiveStrategy>();
        if (ShouldUsePurify(purifyStrat, primaryTarget))
            QueueOGCD(AID.Purify, Player, purifyStrat == DefensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Purify);

        //Sprint execution
        var sprintStrat = strategy.Option(Track.Sprint).As<DefensiveStrategy>();
        if (ShouldUseSprint(sprintStrat, primaryTarget))
            QueueOGCD(AID.SprintPvP, Player, sprintStrat == DefensiveStrategy.Force ? OGCDPriority.ForcedOGCD : OGCDPriority.Sprint);
    }

    #region Core Execution Helpers
    //QueueGCD execution
    private void QueueGCD(AID aid, Actor? target, GCDPriority prio)
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
    private void QueueOGCD(AID aid, Actor? target, OGCDPriority prio, float basePrio = ActionQueue.Priority.Medium)
    {
        if (prio != OGCDPriority.None)
        {
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(aid), target, basePrio + (int)prio);
        }
    }
    #endregion

    public bool ShouldUseElixir(ElixirStrategy strategy, Actor? target) => strategy switch
    {
        ElixirStrategy.Automatic =>
            canElixir &&
            Player.HPMP.CurHP <= 2500 &&
            (In20y(target) || target != null),
        ElixirStrategy.Close => Player.HPMP.CurHP <= 4000 && In10y(target),
        ElixirStrategy.Mid => Player.HPMP.CurHP <= 4000 && In20y(target),
        ElixirStrategy.Far => Player.HPMP.CurHP <= 4000 && In30y(target),
        ElixirStrategy.Force => canElixir,
        ElixirStrategy.Hold => false,
        _ => false,
    };

    public bool ShouldUseRecuperate(RecuperateStrategy strategy) => strategy switch
    {
        RecuperateStrategy.Automatic =>
            canRecuperate &&
            Player.HPMP.CurHP <= 4000,
        RecuperateStrategy.Seventy => canRecuperate && Player.HPMP.CurHP <= 7000,
        RecuperateStrategy.Fifty => canRecuperate && Player.HPMP.CurHP <= 5000,
        RecuperateStrategy.Thirty => canRecuperate && Player.HPMP.CurHP <= 3000,
        RecuperateStrategy.Force => canRecuperate,
        RecuperateStrategy.Hold => false,
        _ => false,
    };

    public bool ShouldUseGuard(GuardStrategy strategy, Actor? target) => strategy switch
    {
        GuardStrategy.Automatic =>
            canGuard &&
            Player.HPMP.CurHP <= 3500,
        GuardStrategy.Seventy => canGuard && Player.HPMP.CurHP <= 7000,
        GuardStrategy.Fifty => canGuard && Player.HPMP.CurHP <= 5000,
        GuardStrategy.Thirty => canGuard && Player.HPMP.CurHP <= 3000,
        GuardStrategy.Force => canGuard,
        GuardStrategy.Hold => false,
        _ => false,
    };

    public bool ShouldUsePurify(DefensiveStrategy strategy, Actor? target) => strategy switch
    {
        DefensiveStrategy.Automatic =>
            canPurify &&
            HasAnyDebuff(target),
        DefensiveStrategy.Force => canPurify,
        DefensiveStrategy.Delay => false,
        _ => false,
    };

    public bool ShouldUseSprint(DefensiveStrategy strategy, Actor? target) => strategy switch
    {
        DefensiveStrategy.Automatic =>
            !Player.InCombat &&
            canSprint,
        DefensiveStrategy.Force => true,
        DefensiveStrategy.Delay => false,
        _ => false,
    };
}

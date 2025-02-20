namespace BossMod.Autorotation.Utility;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class RolePvPUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Elixir, Recuperate, Guard, Purify, Sprint }
    public enum ElixirStrategy { Automatic, Close, Far, Force, Delay }
    public enum RecuperateStrategy { Automatic, Seventy, Fifty, Thirty, Force, Delay }
    public enum GuardStrategy { Automatic, Seventy, Fifty, Thirty, Force, Delay }
    public enum DefensiveStrategy { Automatic, Force, Delay }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PvP", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic,
            BitMask.Build(
                Class.PLD, Class.WAR, Class.DRK, Class.GNB,
                Class.WHM, Class.SCH, Class.AST, Class.SGE,
                Class.MNK, Class.DRG, Class.NIN, Class.SAM, Class.RPR, Class.VPR,
                Class.BRD, Class.MCH, Class.DNC,
                Class.BLM, Class.SMN, Class.RDM, Class.PCT), 100, 30);

        res.Define(Track.Elixir).As<ElixirStrategy>("Elixir", uiPriority: 150)
            .AddOption(ElixirStrategy.Automatic, "Automatic", "Automatically use Elixir when no targets are nearby within 30 yalms")
            .AddOption(ElixirStrategy.Close, "Close", "Automatically use Elixir when no targets are nearby within 15 yalms")
            .AddOption(ElixirStrategy.Far, "Far", "Automatically use Elixir when no targets are nearby within 45 yalms")
            .AddOption(ElixirStrategy.Force, "Force", "Force use Elixir")
            .AddOption(ElixirStrategy.Delay, "Delay", "Forbids use of Elixir")
            .AddAssociatedActions(ClassShared.AID.Elixir);
        res.Define(Track.Recuperate).As<RecuperateStrategy>("Recuperate", uiPriority: 150)
            .AddOption(RecuperateStrategy.Automatic, "Automatic", "Automatically use Recuperate when HP% is under 40%")
            .AddOption(RecuperateStrategy.Seventy, "Seventy", "Automatically use Recuperate when HP% is under 70%")
            .AddOption(RecuperateStrategy.Fifty, "Fifty", "Automatically use Recuperate when HP% is under 50%")
            .AddOption(RecuperateStrategy.Thirty, "Thirty", "Automatically use Recuperate when HP% is under 30%")
            .AddOption(RecuperateStrategy.Force, "Force", "Force use Recuperate")
            .AddOption(RecuperateStrategy.Delay, "Delay", "Forbids use of Recuperate")
            .AddAssociatedActions(ClassShared.AID.Recuperate);
        res.Define(Track.Guard).As<GuardStrategy>("Guard", uiPriority: 150)
            .AddOption(GuardStrategy.Automatic, "Automatic", "Automatically use Guard when HP% is under 35%")
            .AddOption(GuardStrategy.Seventy, "Seventy", "Automatically use Guard when HP% is under 70%")
            .AddOption(GuardStrategy.Fifty, "Fifty", "Automatically use Guard when HP% is under 50%")
            .AddOption(GuardStrategy.Thirty, "Thirty", "Automatically use Guard when HP% is under 30%")
            .AddOption(GuardStrategy.Force, "Force", "Force use Guard")
            .AddOption(GuardStrategy.Delay, "Delay", "Forbids use of Guard")
            .AddAssociatedActions(ClassShared.AID.Guard);
        res.Define(Track.Purify).As<DefensiveStrategy>("Purify", uiPriority: 150)
            .AddOption(DefensiveStrategy.Automatic, "Automatic", "Automatically use Purify when under any debuff that can be cleansed")
            .AddOption(DefensiveStrategy.Force, "Force", "Force use Purify")
            .AddOption(DefensiveStrategy.Delay, "Delay", "Forbids use of Purify")
            .AddAssociatedActions(ClassShared.AID.Purify);
        res.Define(Track.Sprint).As<DefensiveStrategy>("Sprint", uiPriority: 150)
            .AddOption(DefensiveStrategy.Automatic, "Automatic", "Automatically uses Sprint when no target is nearby within 15 yalms")
            .AddOption(DefensiveStrategy.Force, "Force", "Force use Sprint")
            .AddOption(DefensiveStrategy.Delay, "Delay", "Forbids use of Sprint")
            .AddAssociatedActions(ClassShared.AID.Sprint);
        return res;
    }

    #region Priorities
    public enum GCDPriority
    {
        None = 0,
        Elixir = 500,
        ForcedGCD = 900,
    }
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

    #region Module Helpers
    public float PlayerHPP() => (float)Player.HPMP.CurHP / Player.HPMP.MaxHP * 100;
    public float DebuffsLeft(Actor? target)
    {
        return target == null ? 0f
            : Utils.MaxAll(
            StatusDetails(target, ClassShared.SID.Silence, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.StunPvP, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.Bind, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.Heavy, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.Sleep, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.HalfAsleep, Player.InstanceID, 5).Left
        );
    }
    public bool HasAnyDebuff(Actor? target) => DebuffsLeft(target) > 0;
    private bool IsOffCooldown(ClassShared.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining < 0.6f;
    #endregion

    private bool hasSprint;
    private bool canElixir;
    private bool canRecuperate;
    private bool canGuard;
    private bool canPurify;
    private bool canSprint;

    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        hasSprint = Player.FindStatus(ClassShared.SID.SprintPvP) != null;
        canElixir = IsOffCooldown(ClassShared.AID.Elixir) && strategy.Option(Track.Elixir).As<ElixirStrategy>() != ElixirStrategy.Delay;
        canRecuperate = Player.HPMP.CurMP >= 2500 && strategy.Option(Track.Recuperate).As<RecuperateStrategy>() != RecuperateStrategy.Delay;
        canGuard = IsOffCooldown(ClassShared.AID.Guard) && strategy.Option(Track.Guard).As<GuardStrategy>() != GuardStrategy.Delay;
        canPurify = IsOffCooldown(ClassShared.AID.Purify) && strategy.Option(Track.Purify).As<DefensiveStrategy>() != DefensiveStrategy.Delay;
        canSprint = !hasSprint && strategy.Option(Track.Sprint).As<DefensiveStrategy>() != DefensiveStrategy.Delay;

        var elixirStrat = strategy.Option(Track.Elixir).As<ElixirStrategy>();
        if (ShouldUseElixir(elixirStrat))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Elixir), Player, strategy.Option(Track.Elixir).Priority(), strategy.Option(Track.Elixir).Value.ExpireIn);

        var recuperateStrat = strategy.Option(Track.Recuperate).As<RecuperateStrategy>();
        if (ShouldUseRecuperate(recuperateStrat))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Recuperate), Player, strategy.Option(Track.Recuperate).Priority(), strategy.Option(Track.Recuperate).Value.ExpireIn);

        var guardStrat = strategy.Option(Track.Guard).As<GuardStrategy>();
        if (ShouldUseGuard(guardStrat))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Guard), Player, strategy.Option(Track.Guard).Priority(), strategy.Option(Track.Guard).Value.ExpireIn);

        var purifyStrat = strategy.Option(Track.Purify).As<DefensiveStrategy>();
        if (ShouldUsePurify(purifyStrat))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Purify), Player, strategy.Option(Track.Purify).Priority(), strategy.Option(Track.Purify).Value.ExpireIn);

        var sprintStrat = strategy.Option(Track.Sprint).As<DefensiveStrategy>();
        if (ShouldUseSprint(sprintStrat))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.Sprint), Player, strategy.Option(Track.Sprint).Priority(), strategy.Option(Track.Sprint).Value.ExpireIn);
    }

    public bool ShouldUseElixir(ElixirStrategy strategy) => strategy switch
    {
        ElixirStrategy.Automatic => canElixir && PlayerHPP() <= 60 && Hints.NumPriorityTargetsInAOECircle(Player.Position, 30) == 0,
        ElixirStrategy.Close => canElixir && PlayerHPP() <= 60 && Hints.NumPriorityTargetsInAOECircle(Player.Position, 15) == 0,
        ElixirStrategy.Far => canElixir && PlayerHPP() <= 60 && Hints.NumPriorityTargetsInAOECircle(Player.Position, 45) == 0,
        ElixirStrategy.Force => canElixir,
        ElixirStrategy.Delay => false,
        _ => false,
    };
    public bool ShouldUseRecuperate(RecuperateStrategy strategy) => strategy switch
    {
        RecuperateStrategy.Automatic => canRecuperate && PlayerHPP() <= 40,
        RecuperateStrategy.Seventy => canRecuperate && PlayerHPP() <= 70,
        RecuperateStrategy.Fifty => canRecuperate && PlayerHPP() <= 50,
        RecuperateStrategy.Thirty => canRecuperate && PlayerHPP() <= 30,
        RecuperateStrategy.Force => canRecuperate,
        RecuperateStrategy.Delay => false,
        _ => false,
    };
    public bool ShouldUseGuard(GuardStrategy strategy) => strategy switch
    {
        GuardStrategy.Automatic => canGuard && PlayerHPP() <= 35,
        GuardStrategy.Seventy => canGuard && PlayerHPP() <= 70,
        GuardStrategy.Fifty => canGuard && PlayerHPP() <= 50,
        GuardStrategy.Thirty => canGuard && PlayerHPP() <= 30,
        GuardStrategy.Force => canGuard,
        GuardStrategy.Delay => false,
        _ => false,
    };
    public bool ShouldUsePurify(DefensiveStrategy strategy) => strategy switch
    {
        DefensiveStrategy.Automatic => canPurify && HasAnyDebuff(Player),
        DefensiveStrategy.Force => canPurify,
        DefensiveStrategy.Delay => false,
        _ => false,
    };
    public bool ShouldUseSprint(DefensiveStrategy strategy) => strategy switch
    {
        DefensiveStrategy.Automatic => Hints.NumPriorityTargetsInAOECircle(Player.Position, 15) == 0 && canSprint,
        DefensiveStrategy.Force => canSprint,
        DefensiveStrategy.Delay => false,
        _ => false,
    };
}

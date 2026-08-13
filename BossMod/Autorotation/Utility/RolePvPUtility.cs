namespace BossMod.Autorotation.Utility;

public sealed class RolePvPUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Elixir, Recuperate, Guard, GuardEnd, Purify, Sprint }
    public enum ElixirStrategy { Far, Close, Forbid }
    public enum ThresholdStrategy { Seventy, Fifty, Thirty, Forbid }
    public enum GuardStrategy { Auto, Two, Three, Four, Seventy, Fifty, Thirty, Forbid }
    public enum GuardEndStrategy { Normal, Point25, Point5, One }
    public enum DefensiveStrategy { Allow, Forbid }

    public static RotationModuleDefinition Definition()
    {
        var res = new RotationModuleDefinition("Utility: PvP", "PvP Rotation Module", "PvP", "Akechi", RotationModuleQuality.Basic, BitMask.Build(
                Class.PLD, Class.WAR, Class.DRK, Class.GNB,
                Class.WHM, Class.SCH, Class.AST, Class.SGE,
                Class.MNK, Class.DRG, Class.NIN, Class.SAM, Class.RPR, Class.VPR,
                Class.BRD, Class.MCH, Class.DNC,
                Class.BLM, Class.SMN, Class.RDM, Class.PCT), 100, 30, PvP: PvPCompatibility.PvPOnly);

        res.Define(Track.Elixir).As<ElixirStrategy>("Elixir", uiPriority: 150)
            .AddOption(ElixirStrategy.Far, "Allows use of Elixir if resources are low and no targets are nearby within 50 yalms")
            .AddOption(ElixirStrategy.Close, "Allows use of Elixir if resources are low and no targets are nearby within 30 yalms")
            .AddOption(ElixirStrategy.Forbid, "Forbid use of Elixir")
            .AddAssociatedActions(ClassShared.AID.ElixirPvP);

        res.Define(Track.Recuperate).As<ThresholdStrategy>("Recuperate", uiPriority: 150)
            .AddOption(ThresholdStrategy.Seventy, "Automatically use Recuperate when HP% is under 70%")
            .AddOption(ThresholdStrategy.Fifty, "Automatically use Recuperate when HP% is under 50%")
            .AddOption(ThresholdStrategy.Thirty, "Automatically use Recuperate when HP% is under 30%")
            .AddOption(ThresholdStrategy.Forbid, "Forbid use of Recuperate")
            .AddAssociatedActions(ClassShared.AID.RecuperatePvP);

        res.Define(Track.Guard).As<GuardStrategy>("Guard", uiPriority: 150)
            .AddOption(GuardStrategy.Auto, "Automatically use Guard when HP% is under 75% and two or more targets are targeting you, or when HP% is below 33%")
            .AddOption(GuardStrategy.Two, "Automatically use Guard when HP is not full and two or more targets are targeting you")
            .AddOption(GuardStrategy.Three, "Automatically use Guard when HP is not full and three or more targets are targeting you")
            .AddOption(GuardStrategy.Four, "Automatically use Guard when HP is not full and four or more targets are targeting you")
            .AddOption(GuardStrategy.Seventy, "Automatically use Guard when HP% is under 70%")
            .AddOption(GuardStrategy.Fifty, "Automatically use Guard when HP% is under 50%")
            .AddOption(GuardStrategy.Thirty, "Automatically use Guard when HP% is under 30%")
            .AddOption(GuardStrategy.Forbid, "Forbid use of Guard")
            .AddAssociatedActions(ClassShared.AID.GuardPvP);

        res.Define(Track.GuardEnd).As<GuardEndStrategy>("Guard End", uiPriority: 150)
            .AddOption(GuardEndStrategy.Normal, "Do not end Guard early")
            .AddOption(GuardEndStrategy.Point25, "End Guard 0.25 seconds early")
            .AddOption(GuardEndStrategy.Point5, "End Guard 0.5 seconds early")
            .AddOption(GuardEndStrategy.One, "End Guard 1 second early")
            .AddAssociatedActions(ClassShared.AID.GuardPvP);

        res.Define(Track.Purify).As<DefensiveStrategy>("Purify", uiPriority: 150)
            .AddOption(DefensiveStrategy.Allow, "Allow use of Purify when under any debuff that can be cleansed")
            .AddOption(DefensiveStrategy.Forbid, "Forbid use of Purify")
            .AddAssociatedActions(ClassShared.AID.PurifyPvP);

        res.Define(Track.Sprint).As<DefensiveStrategy>("Sprint", uiPriority: 150)
            .AddOption(DefensiveStrategy.Allow, "Allow use of Sprint when no target is nearby within 30 yalms")
            .AddOption(DefensiveStrategy.Forbid, "Forbid use of Sprint")
            .AddAssociatedActions(ClassShared.AID.Sprint);

        return res;
    }

    private bool IsReady(ClassShared.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining <= 0.2f;
    private int EnemiesTargetingPlayer => Hints.PotentialTargets.Count(x => !x.Actor.IsDeadOrDestroyed && x.Actor.TargetID == Player.InstanceID);
    private bool TargetsNearby(float range) => Hints.PotentialTargets.Any(h => !h.Actor.IsDeadOrDestroyed && h.Actor.DistanceToHitbox(Player) <= range);

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var inGuard = strategy.Option(Track.GuardEnd).As<GuardEndStrategy>() switch
        {
            GuardEndStrategy.Normal => Player.FindStatus(ClassShared.SID.GuardPvP) != null,
            GuardEndStrategy.Point25 => SelfStatusLeft(ClassShared.SID.GuardPvP) > 0.25f,
            GuardEndStrategy.Point5 => SelfStatusLeft(ClassShared.SID.GuardPvP) > 0.5f,
            GuardEndStrategy.One => SelfStatusLeft(ClassShared.SID.GuardPvP) > 1f,
            _ => false
        };
        if (Player.IsDeadOrDestroyed || Player.MountId != 0 || inGuard)
            return;

        if (IsReady(ClassShared.AID.GuardPvP) && strategy.Option(Track.Guard).As<GuardStrategy>() switch
        {
            GuardStrategy.Auto => (Player.PendingHPRatio is < 0.75f and not 0.0f && EnemiesTargetingPlayer >= 2) || Player.PendingHPRatio is < 0.33f and not 0.0f,
            GuardStrategy.Two => EnemiesTargetingPlayer >= 2 && Player.PendingHPRatio is < 1.0f and not 0.0f,
            GuardStrategy.Three => EnemiesTargetingPlayer >= 3 && Player.PendingHPRatio is < 1.0f and not 0.0f,
            GuardStrategy.Four => EnemiesTargetingPlayer >= 4 && Player.PendingHPRatio is < 1.0f and not 0.0f,
            GuardStrategy.Seventy => Player.PendingHPRatio is < 0.7f and not 0.0f,
            GuardStrategy.Fifty => Player.PendingHPRatio is < 0.5f and not 0.0f,
            GuardStrategy.Thirty => Player.PendingHPRatio is < 0.3f and not 0.0f,
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.GuardPvP), Player, (int)ActionQueue.Priority.VeryHigh + 2);

        if (Player.HPMP.CurMP >= 2500 && strategy.Option(Track.Recuperate).As<ThresholdStrategy>() switch
        {
            ThresholdStrategy.Seventy => Player.PendingHPRatio is < 0.7f and not 0.0f,
            ThresholdStrategy.Fifty => Player.PendingHPRatio is < 0.5f and not 0.0f,
            ThresholdStrategy.Thirty => Player.PendingHPRatio is < 0.3f and not 0.0f,
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.RecuperatePvP), Player, (int)ActionQueue.Priority.VeryHigh + 1);

        if (IsReady(ClassShared.AID.PurifyPvP) &&
            strategy.Option(Track.Purify).As<DefensiveStrategy>() == DefensiveStrategy.Allow &&
            (Player.FindStatus(ClassShared.SID.StunPvP) != null ||
            Player.FindStatus(ClassShared.SID.HeavyPvP) != null ||
            Player.FindStatus(ClassShared.SID.BindPvP) != null ||
            Player.FindStatus(ClassShared.SID.SilencePvP) != null ||
            Player.FindStatus(ClassShared.SID.DeepFreezePvP) != null ||
            Player.FindStatus(WHM.SID.MiracleOfNaturePvP) != null))
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.PurifyPvP), Player, (int)ActionQueue.Priority.VeryHigh);

        if (IsReady(ClassShared.AID.SprintPvP) && Player.MountId == 0 && Player.FindStatus(ClassShared.SID.SprintPvP) == null &&
            !TargetsNearby(32) && strategy.Option(Track.Sprint).As<DefensiveStrategy>() == DefensiveStrategy.Allow)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SprintPvP), Player, (int)ActionQueue.Priority.High);

        if ((Player.HPMP.CurHP != Player.HPMP.MaxHP || Player.HPMP.CurMP != Player.HPMP.MaxMP) && strategy.Option(Track.Elixir).As<ElixirStrategy>() switch
        {
            ElixirStrategy.Close => !TargetsNearby(32),
            ElixirStrategy.Far => !TargetsNearby(52),
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ElixirPvP), Player, (int)ActionQueue.Priority.High);
    }
}

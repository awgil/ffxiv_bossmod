namespace BossMod.Autorotation.Utility;
//Contribution by Akechi
//Discord @akechdz or 'Akechi' on Puni.sh for maintenance

public sealed class RolePvPUtility(RotationModuleManager manager, Actor player) : RotationModule(manager, player)
{
    public enum Track { Elixir, Recuperate, Guard, Purify, Sprint }
    public enum ElixirStrategy { Far, Close, Forbid }
    public enum ThresholdStrategy { Seventy, Fifty, Thirty, Forbid }
    public enum DefensiveStrategy { Allow, Forbid }

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
            .AddOption(ElixirStrategy.Far, "Far", "Allows use of Elixir if resources are low and no targets are nearby within 50 yalms")
            .AddOption(ElixirStrategy.Close, "Close", "Allows use of Elixir if resources are low and no targets are nearby within 30 yalms")
            .AddOption(ElixirStrategy.Forbid, "Forbid", "Forbid use of Elixir")
            .AddAssociatedActions(ClassShared.AID.ElixirPvP);
        res.Define(Track.Recuperate).As<ThresholdStrategy>("Recuperate", uiPriority: 150)
            .AddOption(ThresholdStrategy.Seventy, "Seventy", "Automatically use Recuperate when HP% is under 70%")
            .AddOption(ThresholdStrategy.Fifty, "Fifty", "Automatically use Recuperate when HP% is under 50%")
            .AddOption(ThresholdStrategy.Thirty, "Thirty", "Automatically use Recuperate when HP% is under 30%")
            .AddOption(ThresholdStrategy.Forbid, "Forbid", "Forbids use of Recuperate")
            .AddAssociatedActions(ClassShared.AID.RecuperatePvP);
        res.Define(Track.Guard).As<ThresholdStrategy>("Guard", uiPriority: 150)
            .AddOption(ThresholdStrategy.Seventy, "Seventy", "Automatically use Guard when HP% is under 70%")
            .AddOption(ThresholdStrategy.Fifty, "Fifty", "Automatically use Guard when HP% is under 50%")
            .AddOption(ThresholdStrategy.Thirty, "Thirty", "Automatically use Guard when HP% is under 30%")
            .AddOption(ThresholdStrategy.Forbid, "Forbid", "Forbids use of Guard")
            .AddAssociatedActions(ClassShared.AID.GuardPvP);
        res.Define(Track.Purify).As<DefensiveStrategy>("Purify", uiPriority: 150)
            .AddOption(DefensiveStrategy.Allow, "Allow", "Allows use Purify when under any debuff that can be cleansed")
            .AddOption(DefensiveStrategy.Forbid, "Forbid", "Forbids use of Purify")
            .AddAssociatedActions(ClassShared.AID.PurifyPvP);
        res.Define(Track.Sprint).As<DefensiveStrategy>("Sprint", uiPriority: 150)
            .AddOption(DefensiveStrategy.Allow, "Allow", "Allows uses Sprint when no target is nearby within 30 yalms")
            .AddOption(DefensiveStrategy.Forbid, "Forbid", "Forbids use of Sprint")
            .AddAssociatedActions(ClassShared.AID.Sprint);
        return res;
    }

    private bool IsReady(ClassShared.AID aid) => World.Client.Cooldowns[ActionDefinitions.Instance.Spell(aid)!.MainCooldownGroup].Remaining <= 0.2f;
    public float PlayerHPP => (float)Player.HPMP.CurHP / Player.HPMP.MaxHP * 100;
    public bool IsMounted => Player.MountId != 0;
    public float DebuffsLeft(Actor? target)
    {
        return target == null ? 0f
            : Utils.MaxAll(
            StatusDetails(target, ClassShared.SID.SilencePvP, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.StunPvP, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.BindPvP, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.HeavyPvP, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.SleepPvP, Player.InstanceID, 5).Left,
            StatusDetails(target, ClassShared.SID.HalfAsleepPvP, Player.InstanceID, 5).Left);
    }
    private bool NoTargetsNearby(float range) => !Hints.PriorityTargets.Any(h =>
            !h.Actor.IsDeadOrDestroyed &&
            !h.Actor.IsFriendlyNPC &&
            !h.Actor.IsAlly &&
            h.Actor.Position.InCircle(Player.Position, range));
    public override void Execute(StrategyValues strategy, ref Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        if (Player.IsDeadOrDestroyed || IsMounted)
            return;

        if (DebuffsLeft(Player) > 0 && IsReady(ClassShared.AID.PurifyPvP) &&
            strategy.Option(Track.Purify).As<DefensiveStrategy>() == DefensiveStrategy.Allow)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.PurifyPvP), Player, (int)ActionQueue.Priority.VeryHigh);
        if (IsReady(ClassShared.AID.SprintPvP) && !IsMounted && Player.FindStatus(ClassShared.SID.SprintPvP) == null &&
            NoTargetsNearby(32) && strategy.Option(Track.Sprint).As<DefensiveStrategy>() == DefensiveStrategy.Allow)
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.SprintPvP), Player, (int)ActionQueue.Priority.High);
        if ((Player.HPMP.CurHP != Player.HPMP.MaxHP || Player.HPMP.CurMP != Player.HPMP.MaxMP) && strategy.Option(Track.Elixir).As<ElixirStrategy>() switch
        {
            ElixirStrategy.Close => NoTargetsNearby(32),
            ElixirStrategy.Far => NoTargetsNearby(52),
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.ElixirPvP), Player, (int)ActionQueue.Priority.High);
        if (Player.HPMP.CurMP >= 2500 && strategy.Option(Track.Recuperate).As<ThresholdStrategy>() switch
        {
            ThresholdStrategy.Seventy => PlayerHPP is < 70 and not 0,
            ThresholdStrategy.Fifty => PlayerHPP is < 50 and not 0,
            ThresholdStrategy.Thirty => PlayerHPP is < 30 and not 0,
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.RecuperatePvP), Player, (int)ActionQueue.Priority.VeryHigh);
        if (IsReady(ClassShared.AID.GuardPvP) && Player.FindStatus(ClassShared.SID.GuardPvP) == null && strategy.Option(Track.Guard).As<ThresholdStrategy>() switch
        {
            ThresholdStrategy.Seventy => PlayerHPP is < 70 and not 0,
            ThresholdStrategy.Fifty => PlayerHPP is < 50 and not 0,
            ThresholdStrategy.Thirty => PlayerHPP is < 30 and not 0,
            _ => false
        })
            Hints.ActionsToExecute.Push(ActionID.MakeSpell(ClassShared.AID.GuardPvP), Player, (int)ActionQueue.Priority.VeryHigh);
    }
}

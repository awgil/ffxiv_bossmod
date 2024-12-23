namespace BossMod.Autorotation.xan;

public class DeepDungeonAI(RotationModuleManager manager, Actor player) : AIBase(manager, player)
{
    public enum Track { Potion }

    public enum PotionStrategy
    {
        Disabled,
        Always,
        Boss,
        BossOrHigh
    }

    public static RotationModuleDefinition Definition()
    {
        var def = new RotationModuleDefinition("Deep Dungeon AI", "Utilities for deep dungeon - potion/pomander user", "AI (xan)", "xan", RotationModuleQuality.Basic, new BitMask(~0ul), 100, CanUseWhileRoleplaying: true);

        def.Define(Track.Potion).As<PotionStrategy>("Potion")
            .AddOption(PotionStrategy.Disabled, "Do not use")
            .AddOption(PotionStrategy.Always, "Use below 80% HP if status is not present")
            .AddOption(PotionStrategy.Boss, "Use during boss fights")
            .AddOption(PotionStrategy.BossOrHigh, "Use during boss fights or at high floors");

        return def;
    }

    enum Transformation : uint
    {
        None,
        Manticore,
        Succubus,
        Kuribu
    }

    enum SID : uint
    {
        Transfiguration = 565,
        ItemPenalty = 1094,
    }

    public override void Execute(StrategyValues strategy, Actor? primaryTarget, float estimatedAnimLockDelay, bool isMoving)
    {
        var transformation = Transformation.None;
        if (Player.FindStatus(SID.Transfiguration) is { } status)
        {
            transformation = (status.Extra & 0xFF) switch
            {
                42 => Transformation.Manticore,
                43 => Transformation.Succubus,
                49 => Transformation.Kuribu,
                _ => Transformation.None
            };
        }

        if (transformation != Transformation.None)
        {
            DoTransformActions(strategy, primaryTarget, transformation);
            return;
        }

        if (Player.FindStatus(SID.ItemPenalty) != null)
            return;

        var (regenAction, potAction) = World.DeepDungeon.Type switch
        {
            DeepDungeonState.DungeonType.POTD => (ActionDefinitions.IDSustainingPotion, ActionDefinitions.IDMaxPotion),
            DeepDungeonState.DungeonType.HOH => (ActionDefinitions.IDEmpyreanPotion, ActionDefinitions.IDSuperPotion),
            _ => (default, default)
        };

        if (regenAction != default && ShouldPotion(strategy))
            Hints.ActionsToExecute.Push(regenAction, Player, ActionQueue.Priority.Medium);

        if (potAction != default && HPRatio() <= 0.3f)
            Hints.ActionsToExecute.Push(potAction, Player, ActionQueue.Priority.Medium);
    }

    private void DoTransformActions(StrategyValues strategy, Actor? primaryTarget, Transformation t)
    {
        if (primaryTarget == null)
            return;

        Func<WPos, float> goal;
        ActionID attack;
        int numTargets;
        var castTime = 0f;

        switch (t)
        {
            case Transformation.Manticore:
                goal = Hints.GoalSingleTarget(primaryTarget, 3);
                numTargets = 1;
                attack = ActionID.MakeSpell(Roleplay.AID.Pummel);
                break;
            case Transformation.Succubus:
                goal = Hints.GoalSingleTarget(primaryTarget, 25);
                numTargets = Hints.NumPriorityTargetsInAOECircle(primaryTarget.Position, 5);
                attack = ActionID.MakeSpell(Roleplay.AID.VoidFireII);
                castTime = 2.5f;
                break;
            case Transformation.Kuribu:
                // heavenly judge is ground targeted
                goal = Hints.GoalSingleTarget(primaryTarget.Position, 25);
                numTargets = Hints.NumPriorityTargetsInAOECircle(primaryTarget.Position, 6);
                attack = ActionID.MakeSpell(Roleplay.AID.HeavenlyJudge);
                castTime = 2.5f;
                break;
            default:
                return;
        }

        if (numTargets == 0)
            return;

        Hints.GoalZones.Add(goal);
        if (castTime == 0 || Hints.MaxCastTimeEstimate >= (castTime - 0.5f))
            Hints.ActionsToExecute.Push(attack, primaryTarget, ActionQueue.Priority.High, targetPos: primaryTarget.PosRot.XYZ());
    }

    private bool ShouldPotion(StrategyValues strategy)
    {
        var ratio = Player.ClassCategory is ClassCategory.Tank ? 0.6f : 0.8f;
        var use = PendingHPRatio(Player) < ratio && Player.FindStatus(648) == null && Player.InCombat;
        return use && strategy.Option(Track.Potion).As<PotionStrategy>() switch
        {
            PotionStrategy.Always => true,
            PotionStrategy.Boss => World.DeepDungeon.Progress.Floor % 10 == 0,
            PotionStrategy.BossOrHigh => IsHighFloor(World.DeepDungeon) || World.DeepDungeon.Progress.Floor % 10 == 0,
            _ => false
        };
    }

    private bool IsHighFloor(DeepDungeonState st) => st.Progress.Floor > (st.Type == DeepDungeonState.DungeonType.POTD ? 100 : 50);
}
